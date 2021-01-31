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
                    Thread.Sleep(1000);
                }
            }
        }

        private WebServer CreateWebServer(string url)
        {
            Tokens = new TokenStorage();
            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithIPBanning(o => o
                    .WithMaxRequestsPerSecond(10)
                    .WithRegexRules("HTTP exception 404")
                )
                .WithWebApi("/inventory", m => m
                    .WithController(() => new SecondbotInventory(Bot, Tokens)))
                .WithWebApi("/core", m => m
                    .WithController(() => new SecondbotCoreWebAPi(Config, Bot, Tokens)))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" })));
            return server;
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
                if (info.IP == IPaddress)
                {
                    return true;
                }
            }
            return false;
        }

        public string CreateToken(IPAddress IP)
        {
            PurgeExpired();
            bool used = true;
            string last = "";
            while(used == true)
            {
                last = helpers.GetSHA1(last + helpers.UnixTimeNow() + new Random().Next(13256).ToString()).Substring(0, 10);
                used = tokens.ContainsKey(last);
                if (used == false)
                {
                    tokenInfo Info = new tokenInfo();
                    Info.IP = IP;
                    Info.Expires = helpers.UnixTimeNow() + (60 * 10);
                }
            }
            return last;
        }
    }

    public class WebApiControllerWithTokens : WebApiController
    {
        protected TokenStorage tokens;
        protected SecondBot bot;
        
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
