using BetterSecondBotShared.Json;
using BSB.bottypes;
using System;
using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using System.Threading;
using Swan.Logging;
using System.Collections.Generic;
using EmbedIO.Security;
using BetterSecondBotShared.Static;
using System.Net;
using EmbedIO.Routing;

namespace BetterSecondBot.HttpService
{
    public class HttpAsService
    {
        protected JsonConfig Config;
        protected SecondBot Bot;
        protected TokenStorage Tokens;
        public HttpAsService(SecondBot MainBot, JsonConfig BotConfig)
        {
            Bot = MainBot;
            Config = BotConfig;

            // Our web server is disposable
            Logger.UnregisterLogger<ConsoleLogger>();
            using (var server = CreateWebServer(Config.Http_Host))
            {
                // Once we've registered our modules and configured them, we call the RunAsync() method.
                server.RunAsync();
                while(Bot.KillMe == false)
                {
                    Thread.Sleep(3000);
                }
            }
        }

        private WebServer CreateWebServer(string url)
        {
            Tokens = new TokenStorage();
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithCors()
                //.WithIPBanning(o => o
                //    .WithMaxRequestsPerSecond(30)
                //    .WithRegexRules("HTTP exception 404")
                //)
                .WithWebApi("/inventory", m => m
                    .WithController(() => new HttpApiInventory(Bot, Tokens))
                )
                .WithWebApi("/chat", m => m
                    .WithController(() => new HttpApiLocalchat(Bot, Tokens))
                )
                .WithWebApi("/groups", m => m
                    .WithController(() => new HttpApiGroup(Bot, Tokens))
                )
                .WithWebApi("/ims", m => m
                    .WithController(() => new HttpApiIM(Bot, Tokens))
                )
                .WithWebApi("/core", m => m
                    .WithController(() => new HttpApiCore(Config, Bot, Tokens))
                )
                .WithWebApi("/parcelestate", m => m
                    .WithController(() => new HttpApiParcelEstate(Bot,Tokens))
                )
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));
            return server;
        }
 
    }



    [AttributeUsage(AttributeTargets.Method)]
    public class About : Attribute
    {
        public string about = "";
        public About(string about)
        {
            this.about = about;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class NeedsToken : Attribute
    {
        public bool needsToken = true;
        public NeedsToken(bool needsToken)
        {
            this.needsToken = needsToken;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReturnHints : Attribute
    {
        public string hint = "";
        public ReturnHints(string hint)
        {
            this.hint = hint;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ArgHints : Attribute
    {
        public string name = "";
        public string typename = "";
        public string about = "";

        public ArgHints(string name,string typename, string about)
        {
            this.name = name;
            this.typename = typename;
            this.about = about;
        }
    }



    public class TokenStorage
    {
        protected Dictionary<string, tokenInfo> tokens = new Dictionary<string, tokenInfo>();

        public void PurgeExpired()
        {
            List<string> oldTokens = new List<string>();
            long now = helpers.UnixTimeNow();
            foreach (KeyValuePair<string, tokenInfo> entry in tokens)
            {
                if(entry.Value.Expires <= now)
                {
                    oldTokens.Add(entry.Key);
                }
            }
            foreach(string A in oldTokens)
            {
                tokens.Remove(A);
            }
        }

        public string Expire(string token)
        {
            if (tokens.ContainsKey(token) == true)
            {
                if(tokens.Remove(token) == true)
                {
                    return "ok";
                }
            }
            return "Failed to remove token";
        }

        public bool Allow(string code, IPAddress IPaddress)
        {
            if(tokens.ContainsKey(code) == true)
            {
                tokenInfo info = tokens[code];
                if (info.Expires <= helpers.UnixTimeNow())
                {
                    PurgeExpired();
                    return false;
                }
                if (IPaddress.Equals(info.IP) == true)
                {
                    return true;
                }
            }
            return false;
        }

        public string CreateToken(IPAddress IP)
        {
            if (IP != null)
            {
                PurgeExpired();
                bool used = true;
                string last = "";
                while (used == true)
                {
                    last = helpers.GetSHA1(last + helpers.UnixTimeNow() + new Random().Next(13256).ToString()).Substring(0, 10);
                    used = tokens.ContainsKey(last);
                    if (used == false)
                    {
                        tokenInfo Info = new tokenInfo();
                        Info.IP = IP;
                        Info.Expires = helpers.UnixTimeNow() + (60 * 10);
                        tokens.Add(last, Info);
                        break;
                    }
                }
                return last;
            }
            return null;
        }
    }

    public class WebApiControllerWithTokens : WebApiController
    {
        protected TokenStorage tokens;
        protected SecondBot bot;
        public bool needsToken { get; set; }


        protected Object BasicReply(string input)
        {
            return new { reply = input };
        }
        public WebApiControllerWithTokens(SecondBot mainbot, TokenStorage setuptokens)
        {
            bot = mainbot;
            tokens = setuptokens;
        }

        /*
         * not yet supported but will repace the current way soon(TM)
        [Route(HttpVerbs.Post, "/command/{token}")]
        public string CommandLib(string token, [JsonData] CommandLibCall request)
        {
            return "ok";
        }
        */

        public IPAddress getClientIP()
        {
            return HttpContext.Request.RemoteEndPoint.Address;
        }

    }

    public class tokenInfo
    {
        public long Expires = 0;
        public IPAddress IP = null;
    }

}
