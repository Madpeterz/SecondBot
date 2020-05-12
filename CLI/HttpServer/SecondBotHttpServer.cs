using BetterSecondBot.HttpWebUi;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using BSB;
using BSB.bottypes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSecondBot.HttpServer
{
    public class json_http_reply
    {
        public bool status;
        public string message;
        public string redirect;
    }
    public class SecondBotHttpServer
    {
        protected HttpListener listener;
        protected SecondBot Bot;
        public SecondBot GetBot { get { return Bot; } }
        protected JsonConfig Config;
        public JsonConfig GetConfig { get { return Config; } }
        protected HTTPCommandsInterface post_controler;
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
            int port = Math.Abs(Config.Http_Port);
            if (Config.Http_Host == "docker")
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
            webui myUI = new webui(Config,Bot);
            while (Bot.KillMe == false)
            {
                HttpListenerContext ctx = await listener.GetContextAsync().ConfigureAwait(true);
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                resp.Cookies = req.Cookies;
                string test = req.Url.AbsolutePath.Substring(1);
                if (helpers.notempty(Config.Http_PublicUrl) == true)
                {
                    test = test.Replace(Config.Http_PublicUrl, "");
                }
                json_http_reply reply = new json_http_reply();
                List<string> http_args = test.Split('/', StringSplitOptions.RemoveEmptyEntries).ToList();
                byte[] data;
                resp.StatusCode = 200;
                resp.ContentType = "text/html";
                if (req.HttpMethod == "GET")
                {
                    KeyValuePair<string, byte[]> replydata = myUI.Get_Process(req, resp, http_args);
                    if (replydata.Value != null)
                    {
                        resp.ContentType = replydata.Key;
                        data = replydata.Value;
                        resp.ContentLength64 = data.LongLength;
                        await resp.OutputStream.WriteAsync(data, 0, data.Length).ConfigureAwait(true);
                        resp.Close();
                    }
                    else
                    {
                        data = Encoding.UTF8.GetBytes("error with service");
                        resp.ContentLength64 = data.LongLength;
                        resp.ContentType = "text/html";
                        await resp.OutputStream.WriteAsync(data, 0, data.Length).ConfigureAwait(true);
                        resp.Close();
                    }
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
                    resp.StatusCode = 200;
                    if (myUI.Post_process(reply, req, resp, http_args, post_args) == true)
                    {
                        bool accept_request = false;
                        string why_request_failed = "Bad signing request";

                        string command = "";
                        string arg = "";
                        string signing_code;

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
                                string raw = "" + command + "" + arg + "" + Config.Security_SignedCommandkey + "";
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
                        reply.message = why_request_failed;
                        reply.status = accept_request;
                        if (accept_request == true)
                        {
                            reply.message = String.Join("###", post_controler.Call(command, arg));
                        }
                    }
                    if(reply.redirect != "")
                    {
                        if(reply.redirect == "/")
                        {
                            reply.redirect = "";
                        }
                        reply.redirect = "" + Config.Http_PublicUrl + "" + reply.redirect + "";
                    }
                    data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reply));
                    resp.ContentLength64 = data.LongLength;
                    await resp.OutputStream.WriteAsync(data, 0, data.Length).ConfigureAwait(true);
                    resp.Close();
                }
            }
        }
        protected void OpenHttpService()
        {
            post_controler = new HTTPCommandsInterface(Bot,this);
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
                    try
                    {
                        ConsoleLog.Status("Listening for connections on " + url + "");
                        Task listenTask = HandleIncomingConnections();
                        listenTask.GetAwaiter().GetResult();
                    }
                    catch(Exception e)
                    {
                        ConsoleLog.Status("Http interface killed itself: "+e.ToString()+"");
                    }
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
