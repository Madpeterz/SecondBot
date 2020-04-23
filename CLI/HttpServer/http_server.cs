using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using BSB;
using BSB.bottypes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSecondBot.HttpServer
{
    public class http_server
    {
        protected HttpListener listener;
        protected SecondBot Bot;
        public SecondBot GetBot { get { return Bot; } }
        protected JsonConfig Config;
        public JsonConfig GetConfig { get { return Config; } }
        protected HTTPCommandsInterfaceGet get_controler;
        protected HTTPCommandsInterfacePost post_controler;
        protected string url = "";
        protected bool require_signed_commands;
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
        public void start_http_server(JsonConfig setConfig)
        {
            start_http_server(setConfig, null);
        }
        public void start_http_server(SecondBot LinkBot, JsonConfig setConfig)
        {
            start_http_server(setConfig, LinkBot);
        }
        protected void start_http_server(JsonConfig setConfig, SecondBot LinkBot)
        {
            Config = setConfig;
            require_signed_commands = Config.HttpRequireSigned;
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
            open_http_service();
        }

        protected async Task HandleIncomingConnections()
        {
            while(Bot.KillMe == false)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                string test = req.Url.AbsolutePath.Substring(1);
                string[] http_args = test.Split('/', StringSplitOptions.RemoveEmptyEntries);
                byte[] data = Encoding.UTF8.GetBytes("totaly fucked");
                resp.StatusCode = 404;
                if (req.HttpMethod == "GET")
                {
                    resp.StatusCode = 200;
                    string command = "status";
                    string arg = "";
                    if (helpers.notempty(http_args) == true)
                    {
                        if (http_args.Length >= 1)
                        {
                            command = http_args[0];
                        }
                        if (http_args.Length == 2)
                        {
                            arg = http_args[1];
                        }
                    }
                    string content = String.Join("{@}", get_controler.Call(command, arg));
                    data = Encoding.UTF8.GetBytes(content);
                }
                else if (req.HttpMethod == "POST")
                {
                    
                    string text;
                    using (var reader = new StreamReader(req.InputStream,req.ContentEncoding))
                    {
                        text = reader.ReadToEnd();
                    }
                    //"command = logout & args = &passkey = Kp83VZaEhcy"
                    string[] pairs = text.Split('&', StringSplitOptions.RemoveEmptyEntries);
                    Dictionary<string, string> post_args = new Dictionary<string, string>();
                    foreach(string B in pairs)
                    {
                        string[] bits = B.Split('=', StringSplitOptions.RemoveEmptyEntries);
                        if(bits.Length == 2) post_args.Add(bits[0].Trim(), bits[1].Trim());
                    }
                    if (post_args.ContainsKey("command") == true)
                    {
                        if (post_args.ContainsKey("passkey") == true)
                        {
                            if (Config.Httpkey == post_args["passkey"])
                            {
                                string command = post_args["command"];
                                resp.StatusCode = 200;
                                string arg = "";
                                if (post_args.ContainsKey("args") == true)
                                {
                                    arg = post_args["args"];
                                }
                                data = Encoding.UTF8.GetBytes(String.Join("###", post_controler.Call(command, arg)));
                            }
                            else
                            {
                                data = Encoding.UTF8.GetBytes("totaly fucked");
                            }
                        }
                        else
                        {
                            data = Encoding.UTF8.GetBytes("post request missing command");
                        }
                    }
                    else
                    {
                        data = Encoding.UTF8.GetBytes("post request missing command");
                    }
                }
                
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;
                await resp.OutputStream.WriteAsync(data, 0, data.Length);
                resp.Close();
            }
        }
        protected void open_http_service()
        {
            get_controler = new HTTPCommandsInterfaceGet(Bot,this);
            post_controler = new HTTPCommandsInterfacePost(Bot,this);
            Thread t = new Thread(delegate (object unused)
            {
                listener = new HttpListener();
                listener.Prefixes.Add(url);
                listener.Start();
                ConsoleLog.Status("Listening for connections on " + url + "");
                Task listenTask = HandleIncomingConnections();
                listenTask.GetAwaiter().GetResult();
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
