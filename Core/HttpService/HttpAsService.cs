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
using OpenMetaverse;
using System.Reflection;
using System.Linq;
using System.Threading.Tasks;
using Core.Static;

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
                if (helpers.notempty(Environment.GetEnvironmentVariable("ScopedToken" + loop.ToString())) == true)
                {
                    processScopedTokenRaw(Environment.GetEnvironmentVariable("ScopedToken" + loop.ToString()));
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
                ScopedTokens = new [] { "t:[XXX],ws:core" }
            };
            string targetfile = "scoped_tokens.json";
            SimpleIO io = new SimpleIO();
            io.ChangeRoot(Bot.use_folder);
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
                    io.MarkOld(targetfile);
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
                    Swan.Logging.Logger.UnregisterLogger<ConsoleLogger>();
                }
                WebServer server = CreateWebServer(Config.Http_Host);
                // Once we've registered our modules and configured them, we call the RunAsync() method.
                LogFormater.Info("Starting HTTP service: " + Config.Http_Host);
                server.RunAsync();
                while (Bot.KillMe == false)
                {
                    Thread.Sleep(3000);
                }
            }
            catch (Exception e)
            {
                LogFormater.Info("HTTP service has ended because: " + e.Message);
            }
        }

        private WebServer CreateWebServer(string url)
        {

            var server = new WebServer(o => o
                    .WithUrlPrefix(url)
                    .WithMode(HttpListenerMode.EmbedIO))
                .WithCors()
                .WithWebApi("/animation", m => m.WithController(() => new HTTP_Animation(Bot, Tokens)))
                .WithWebApi("/avatars", m => m.WithController(() => new HTTP_Avatars(Bot, Tokens)))
                .WithWebApi("/chat", m => m.WithController(() => new HTTP_Chat(Bot, Tokens)))
                .WithWebApi("/core", m => m.WithController(() => new HTTP_Core(Bot, Tokens)))
                .WithWebApi("/dialogs", m => m.WithController(() => new HTTP_Dialogs(Bot, Tokens)))
                .WithWebApi("/discord", m => m.WithController(() => new HTTP_Discord(Bot, Tokens)))
                .WithWebApi("/estate", m => m.WithController(() => new HTTP_Estate(Bot, Tokens)))
                .WithWebApi("/friends", m => m.WithController(() => new HTTP_Friends(Bot, Tokens)))
                .WithWebApi("/funds", m => m.WithController(() => new Http_Funds(Bot, Tokens)))
                .WithWebApi("/group", m => m.WithController(() => new HTTP_Group(Bot, Tokens)))
                .WithWebApi("/info", m => m.WithController(() => new HTTP_Info(Bot, Tokens)))
                .WithWebApi("/inventory", m => m.WithController(() => new HTTP_Inventory(Bot, Tokens)))
                .WithWebApi("/movement", m => m.WithController(() => new HTTP_Movement(Bot, Tokens)))
                .WithWebApi("/notecard", m => m.WithController(() => new HTTP_Notecard(Bot, Tokens)))
                .WithWebApi("/parcel", m => m.WithController(() => new HTTP_Parcel(Bot, Tokens)))
                .WithWebApi("/self", m => m.WithController(() => new HTTP_Self(Bot, Tokens)))
                .WithWebApi("/streamadmin", m => m.WithController(() => new HTTP_StreamAdmin(Bot, Tokens)))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" }))
            );
            return server;
        }
 
    }






    public class TokenStorage
    {
        // global access tokens
        protected Dictionary<string, tokenInfo> tokens = new Dictionary<string, tokenInfo>();

        // scoped access tokens
        protected Dictionary<string, scopedTokenInfo> scopedtokens = new Dictionary<string, scopedTokenInfo>();




        protected List<string> OneTimeTokens = new List<string>();

        public string OneTimeToken()
        {
            bool known = true;
            string last = "";
            while (known == true)
            {
                last = helpers.GetSHA1(last + helpers.UnixTimeNow() + new Random().Next(13256).ToString()).Substring(0, 10);
                if(OneTimeTokens.Contains(last) == false)
                {
                    known = false;
                    OneTimeTokens.Add(last);
                }
            }
            return last;
        }

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
            if(OneTimeTokens.Contains(code) == true)
            {
                OneTimeTokens.Remove(code);
                return true;
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
            if ((info.AllowWorkgroups.Contains(workgroup) == true) || (info.AllowWorkgroups.Contains("+ALL") == true))
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
        protected UUID avataruuid = UUID.Zero;
        protected Parcel targetparcel = null;

        protected bool disableIPchecks = false;
        public void disableIPlockout()
        {
            disableIPchecks = true;
        }

        public IPAddress handleGetClientIP()
        {
            try
            {
                if (disableIPchecks == true)
                {
                    return IPAddress.Parse("127.0.0.1");
                }
                else
                {
                    return HttpContext.Request.RemoteEndPoint.Address;
                }
            }
            catch
            {
                Console.WriteLine("Error getting client ip");
                return IPAddress.Parse("0.0.0.0");
            }
        }



        protected KeyValuePair<bool,string> SetupCurrentParcel(string token,string usenamespace,string command)
        {
            if (tokens.Allow(token, usenamespace, command, handleGetClientIP()) == false)
            {
                return new KeyValuePair<bool, string>(false, "Token not accepted");
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return new KeyValuePair<bool, string>(false, "Error not in a sim");
            }
            int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
            if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == false)
            {
                Thread.Sleep(2000);
                localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == false)
                {
                    return new KeyValuePair<bool, string>(false, "Parcel data not ready");
                }
            }
            targetparcel = bot.GetClient.Network.CurrentSim.Parcels[localid];
            return new KeyValuePair<bool, string>(true, "");
        }


        protected void ProcessAvatar(string avatar)
        {
            string[] bits = avatar.Split(' ');
            avataruuid = UUID.Zero;
            string findavatar = avatar;
            if ((bits.Length == 2) || (avatar.Length != UUID.Zero.ToString().Length))
            {
                findavatar = bot.FindAvatarName2Key(avatar);
                if(findavatar == "lookup")
                {
                    int loops = 0;
                    int secstowait = 7;
                    int delaysize = 2;
                    while(loops < (secstowait * delaysize))
                    {
                        Thread.Sleep(1000/ delaysize);
                        findavatar = bot.FindAvatarName2Key(avatar);
                        if(findavatar != "lookup")
                        {
                            loops = 55;
                        }
                        loops++;
                    }
                    
                    
                }
            }
            UUID.TryParse(findavatar, out avataruuid);

        }

        protected void SuccessNoReturn(string command)
        {
            SuccessNoReturn(command, new string[] { });
        }
        protected void SuccessNoReturn(string command, string[] args)
        {
            bot.CommandHistoryAdd(command, String.Join("~#~", args), true, "");
        }

        protected Object BasicReply(string input, string command)
        {
            return BasicReply(input, command, new string[] { });
        }

        protected Object BasicReply(string input,string command, string[] args)
        {
            bot.CommandHistoryAdd(command, String.Join("~#~", args), true, "");
            return new { reply = input, status=true };
        }

        protected Object Failure(string input, string command, string[] args)
        {
            bot.CommandHistoryAdd(command, String.Join("~#~",args), false, input);
            return new { reply = input, status = false };
        }

        protected Object Failure(string input, string command, string[] args, string whyfailed)
        {
            bot.CommandHistoryAdd(command, String.Join("~#~", args), false, whyfailed);
            return new { reply = input, status = false };
        }

        protected Object Failure(string input, string command)
        {
            return Failure(input, command, new string[] { });
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
                    "chat/IM",
                    "chat/LocalChatHistory",
                    "chat/Say",
                    "group/Groupchat",
                    "group/GetGroupList",
                    "group/GroupchatListAllUnreadGroups",
                    "im/getimchat",
                    "im/chatwindows",
                    "im/haveunreadims",
                    "im/listwithunread"
                };
            }
            else if (permsgroupname == "giver")
            {
                allowed = new List<string>(){
                    "inventory/SendItem",
                    "inventory/SendFolder",
                    "inventory/InventoryFolders",
                    "inventory/InventoryContents"
                };
            }
            else if (permsgroupname == "movement")
            {
                allowed = new List<string>(){
                    "movement/AutoPilot",
                    "movement/AutoPilotStop",
                    "movement/Fly",
                    "movement/RequestTeleport",
                    "movement/RotateTo",
                    "movement/RotateToFace",
                    "movement/RotateToFaceVector",
                    "movement/SendTeleportLure",
                    "movement/Teleport",
                };
            }
            return allowed.Contains(build);
        }
    }

}
