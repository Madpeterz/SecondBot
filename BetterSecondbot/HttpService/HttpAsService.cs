using BetterSecondBotShared.Json;
using BetterSecondBot.bottypes;
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
using BetterSecondBotShared.logs;
using BetterSecondBotShared.IO;
using Newtonsoft.Json;

namespace BetterSecondBot.HttpService
{
    public class HttpAsService
    {
        protected JsonConfig Config;
        protected SecondBot Bot;
        protected TokenStorage Tokens;
        protected bool killedLogger = false;

        protected void loadScopedTokensFromENV()
        {
            int loop = 1;
            bool found = true;
            while (found == true)
            {
                if (helpers.notempty(Environment.GetEnvironmentVariable("scoped_token_" + loop.ToString())) == true)
                {
                    processScopedTokenRaw(Environment.GetEnvironmentVariable("scoped_token_" + loop.ToString()));
                }
                else
                {
                    found = false;
                }
                loop++;
            }
        }

        protected void loadScopedTokensFromFile()
        {
            JsonScopedTokens LoadedTokens = new JsonScopedTokens
            {
                ScopedTokens = new string[] { "t:[XXX],ws:core" }
            };
            string targetfile = "scoped_tokens.json";
            SimpleIO io = new SimpleIO();
            if (SimpleIO.FileType(targetfile, "json") == false)
            {
                io.WriteJsonTokens(LoadedTokens, targetfile);
                return;
            }
            if (io.Exists(targetfile) == false)
            {
                io.WriteJsonTokens(LoadedTokens, targetfile);
                return;
            }
            string json = io.ReadFile(targetfile);
            if (json.Length > 0)
            {
                try
                {
                    LoadedTokens = JsonConvert.DeserializeObject<JsonScopedTokens>(json);
                    foreach (string loaded in LoadedTokens.ScopedTokens)
                    {
                        processScopedTokenRaw(loaded);
                    }
                }
                catch
                {
                    io.makeOld(targetfile);
                    io.WriteJsonTokens(LoadedTokens, targetfile);
                }
                return;
            }
        }

        protected void processScopedTokenRaw(string raw)
        {
            raw = raw.Replace(" ", "");
            string[] bits = raw.Split(',');
            string code = null;
            int accessids = 0;
            scopedTokenInfo stinfo = new scopedTokenInfo();
            foreach(string bit in bits)
            {
                string[] subbits = bit.Split(':');
                if(subbits[0] == "t")
                {
                    code = subbits[1];
                }
                else if (subbits[0] == "cm")
                {
                    stinfo.AllowCommands.Add(subbits[1]);
                    accessids++;
                }
                else if (subbits[0] == "cg")
                {
                    stinfo.AllowAccessGroups.Add(subbits[1]);
                    accessids++;
                }
                else if (subbits[0] == "ws")
                {
                    stinfo.AllowWorkgroups.Add(subbits[1]);
                    accessids++;
                }
            }
            if((code != null) && (accessids > 0))
            {
                Tokens.AddScopedToken(code, stinfo);
                LogFormater.Info("Adding scopped command " + code + " with: " + accessids.ToString() + " access types");
            }
            else
            {
                LogFormater.Warn("Scoped access code: "+raw+" did not pass checks");
            }

        }
        public HttpAsService(SecondBot MainBot, JsonConfig BotConfig, bool running_in_docker)
        {
            Bot = MainBot;
            Config = BotConfig;

            Tokens = new TokenStorage();
            // Load scoped tokens
            if(running_in_docker == true)
            {
                // using ENV values
                loadScopedTokensFromENV();
            }
            else
            {
                // from file
                loadScopedTokensFromFile();
            }

            // Our web server is disposable
            try
            {
                if (killedLogger == false)
                {
                    killedLogger = true;
                    Logger.UnregisterLogger<ConsoleLogger>();
                }
                using (var server = CreateWebServer(Config.Http_Host))
                {
                    // Once we've registered our modules and configured them, we call the RunAsync() method.
                    server.RunAsync();
                    while (Bot.KillMe == false)
                    {
                        Thread.Sleep(3000);
                    }
                }
            }
            catch
            { 

            }
        }

        private WebServer CreateWebServer(string url)
        {
            
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
        // global access tokens
        protected Dictionary<string, tokenInfo> tokens = new Dictionary<string, tokenInfo>();

        // scoped access tokens
        protected Dictionary<string, scopedTokenInfo> scopedtokens = new Dictionary<string, scopedTokenInfo>();


        public void AddScopedToken(string code, scopedTokenInfo info)
        {
            scopedtokens.Add(code, info);
        }

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

        public bool Allow(string code, string workgroup, string command, IPAddress IPaddress)
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
            return ScopeAllow(code, workgroup, command);
        }

        public bool ScopeAllow(string code, string workgroup, string command)
        {
            if (scopedtokens.ContainsKey(code) == false)
            {
                return false;
            }
            scopedTokenInfo info = scopedtokens[code];
            string build = workgroup + "/" + command;
            if (info.AllowWorkgroups.Contains(workgroup) == true)
            {
                return true;
            }
            else if(info.AllowCommands.Contains(build) == true)
            {
                return true;
            }
            bool allowed = false;
            foreach(string A in info.AllowAccessGroups)
            {
                allowed = accessgroupcontrol.isAllowed(A, workgroup, command);
                if(allowed == true)
                {
                    break;
                }
            }
            return allowed;
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

    public class scopedTokenInfo
    {
        public List<string> AllowWorkgroups = new List<string>();
        public List<string> AllowAccessGroups = new List<string>();
        public List<string> AllowCommands = new List<string>();
    }

    public static class accessgroupcontrol
    {
        public static bool isAllowed(string permsgroupname, string workgroup, string command)
        {
            string build = workgroup + "/" + command;
            List<string> allowed = new List<string>();
            if (permsgroupname == "chat")
            {
                allowed = new List<string>(){
                    "chat/localchathistory",
                    "chat/localchatsay",
                    "groups/getgroupchat",
                    "groups/sendgroupchat",
                    "groups/listgroups",
                    "im/chatwindows",
                    "im/listwithunread",
                    "im/getimchat",
                    "im/sendimchat"
                };
            }
            else if (permsgroupname == "giver")
            {
                allowed = new List<string>(){
                    "inventory/send",
                    "inventory/folders",
                    "inventory/contents"
                };
            }
            else if (permsgroupname == "movement")
            {
                allowed = new List<string>(){
                    "core/walkto",
                    "core/teleport",
                    "core/gesture"
                };
            }
            return allowed.Contains(build);
        }
    }

}
