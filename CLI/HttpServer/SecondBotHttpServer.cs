using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using BSB;
using BSB.bottypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSecondBot.HttpServer
{
    public class SecondBotHttpServer
    {
        protected HttpListener listener;
        protected SecondBot Bot;
        public SecondBot GetBot { get { return Bot; } }
        protected JsonConfig Config;
        public JsonConfig GetConfig { get { return Config; } }
        protected HTTPCommandsInterfaceGet get_controler;
        protected HTTPCommandsInterfacePost post_controler;
        protected string url = "";
        public bool ShutdownHTTP { get; set; }
        public bool HTTPCnCmode { get; set; }

        public string GetStatus()
        {
            string reply = "";
            if(Bot == null)
            {
                reply += "- No bot";
            }
            else
            {
                Bot.LastStatusMessage = Bot.GetStatus();
                reply += Bot.LastStatusMessage;
            }
            if(listener == null)
            {
                reply += " - No HTTP listener";
            }
            return reply;
        }
        public void NewBot()
        {
            Bot = new SecondBot();
        }
        public void KillBot()
        {
            Bot.GetClient.Network.Logout();
            Bot.GetClient.Network.Shutdown(OpenMetaverse.NetworkManager.DisconnectType.ClientInitiated);
            Bot = null;
        }
        public void StartHttpServer(JsonConfig setConfig)
        {
            StartHttpServer(setConfig, null);
        }
        public void StartHttpServer(SecondBot LinkBot, JsonConfig setConfig)
        {
            StartHttpServer(setConfig, LinkBot);
        }
        protected void StartHttpServer(JsonConfig setConfig, SecondBot LinkBot)
        {
            Config = setConfig;
            Bot = LinkBot;
            int port = Math.Abs(Config.Httpport);
            if (Config.Httpkey.Length < 12)
            {
                Config.Httpkey = helpers.GetSHA1("" + helpers.UnixTimeNow().ToString() + "" + new Random().Next(4, 9999).ToString()).Substring(0, 12);
            }
            if (Config.HttpHost == "docker")
            {
                url = "http://*:" + port.ToString() + "/";
            }
            else
            {
                if (url.StartsWith("http://") == false)
                {
                    url = "http://localhost:" + port.ToString() + "";
                }
                if (url.EndsWith("/") == false)
                {
                    url = "" + url + "/";
                }
            }
            OpenHttpService();
        }

        protected async Task HandleIncomingConnections()
        {
            while (Bot.KillMe == false)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                string test = req.Url.AbsolutePath.Substring(1);
                test = test.Replace(Config.HttpPublicUrlBase, "");
                List<string> http_args = test.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
                byte[] data;
                resp.StatusCode = 200;
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                if (req.HttpMethod == "GET")
                {
                    string command = "status";
                    if (http_args.Count > 0)
                    {
                        command = http_args.Last();
                    }
                    string content = String.Join("{@}", get_controler.Call(command, ""));
                    data = Encoding.UTF8.GetBytes(content);
                    resp.ContentLength64 = data.LongLength;
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
                else if (req.HttpMethod == "POST")
                {
                    string text;
                    using (var reader = new StreamReader(req.InputStream, req.ContentEncoding))
                    {
                        text = reader.ReadToEnd();
                    }
                    //"command = logout & args = &passkey = Kp83VZaEhcy"
                    string[] pairs = text.Split('&', StringSplitOptions.RemoveEmptyEntries);
                    Dictionary<string, string> post_args = new Dictionary<string, string>();
                    foreach (string B in pairs)
                    {
                        string[] bits = B.Split('=', StringSplitOptions.RemoveEmptyEntries);
                        if (bits.Length == 2)
                        {
                            post_args.Add(bits[0].Trim(), bits[1].Trim());
                        }
                    }

                    bool accept_request = false;
                    string why_request_failed = "Bad signing request";

                    string command = "";
                    string arg = "";
                    string signing_code;
                    resp.StatusCode = 200;
                    if (post_args.ContainsKey("command") == true)
                    {
                        command = post_args["command"];
                        if (post_args.ContainsKey("args") == true)
                        {
                            arg = post_args["args"];
                        }
                        if (post_args.ContainsKey("code") == true)
                        {
                            signing_code = post_args["code"];
                            string raw = "" + command + "" + arg + "" + Config.Httpkey + "";
                            string hashcheck = helpers.GetSHA1(raw);
                            if (hashcheck == signing_code)
                            {
                                accept_request = true;
                            }
                        }
                        else
                        {
                            why_request_failed = "Post missing arg value";
                        }
                    }
                    else
                    {
                        why_request_failed = "Post missing command value";
                    }
                    if (accept_request == true)
                    {
                        data = Encoding.UTF8.GetBytes(String.Join("###", post_controler.Call(command, arg)));
                    }
                    else
                    {
                        resp.StatusCode = 417;
                        data = Encoding.UTF8.GetBytes(why_request_failed);
                    }
                    resp.ContentLength64 = data.LongLength;
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
            }
        }
        protected void OpenHttpService()
        {
            get_controler = new HTTPCommandsInterfaceGet(Bot,this);
            post_controler = new HTTPCommandsInterfacePost(Bot,this);
            Thread t = new Thread(delegate (object _)
            {
                bool ok = true;
                try
                {
                    listener = new HttpListener();
                    listener.Prefixes.Add(url);
                    listener.Start();
                }
                catch (Exception e)
                {
                    ok = false;
                    ConsoleLog.Crit("Unable to setup http service: "+e.Message+"");
                    ConsoleLog.Crit("if running on windows please check: https://docs.microsoft.com/en-us/dotnet/framework/wcf/feature-details/configuring-http-and-https?redirectedfrom=MSDN");
                }
                if (ok == true)
                {
                    ConsoleLog.Status("Listening for connections on " + url + "");
                    Task listenTask = HandleIncomingConnections();
                    listenTask.GetAwaiter().GetResult();
                }
                listener.Close();
            })
            {
                Name = "http thead",
                IsBackground = true
            };
            t.Start();
        }
    }
}
