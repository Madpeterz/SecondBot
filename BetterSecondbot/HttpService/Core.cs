using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using BetterSecondBot.bottypes;
using OpenMetaverse;
using System.Threading.Tasks;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace BetterSecondBot.HttpService
{
    public class HttpApiCore : WebApiControllerWithTokens
    {
        
        protected JsonConfig config;
        public HttpApiCore(JsonConfig myconfig, SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens)
        {
            config = myconfig;
        }


        [About("Requests a new token (Vaild for 10 mins) <br/>to use with all other requests")]
        [NeedsToken(false)]
        [ReturnHints("A new token with full system scope")]
        [ArgHints("authcode", "text", "the first 10 chars of SHA1(unixtime+WebUIkey)<br/>unixtime can be +- 30 of the bots time.")]
        [ArgHints("unixtimegiven", "number", "the unixtime you made this request")]
        [Route(HttpVerbs.Post, "/GetToken")]
        public Object GetToken([FormField] string authcode, [FormField] string unixtimegiven)
        {
            long now = helpers.UnixTimeNow();
            long dif = 0;
            if(long.TryParse(unixtimegiven,out long given) == true)
            {
                dif = now - given;
                now = given;
            }
            if((dif < 30) && (dif > -30))
            {
                var raw = now.ToString() + config.Security_WebUIKey;
                string hash = helpers.GetSHA1(raw).Substring(0, 10);
                if (hash == authcode)
                {
                    string newtoken = tokens.CreateToken(getClientIP());
                    if(newtoken != null) return BasicReply(newtoken);
                }
            }
            return Failure("Authcode not accepted");
        }




        [About("uses the AutoPilot to move to a location")]
        [ReturnHints("Error Unable to AutoPilot to location")]
        [ReturnHints("Accepted")]
        [ArgHints("x", "URLARG", "X location to AutoPilot to")]
        [ArgHints("y", "URLARG", "y location to AutoPilot to")]
        [ArgHints("z", "URLARG", "z location to AutoPilot to")]
        [Route(HttpVerbs.Get, "/WalkTo/{x}/{y}/{z}/{token}")]
        public object WalkTo(string x, string y, string z, string token)
        {
            if (tokens.Allow(token, "core", "walkto", getClientIP()) == true)
            {
                bool status = bot.GetCommandsInterface.Call("AutoPilot", "<"+x+","+y+","+z+">");
                if (status == true)
                {
                    return BasicReply("Accepted");
                }
                return Failure("Error Unable to AutoPilot to location");
            }
            return Failure("Token not accepted");
        }



        [About("Attempt to teleport to a new region")]
        [ReturnHints("Error Unable to Teleport to location")]
        [ReturnHints("Accepted")]
        [ArgHints("region", "URLARG", "the name of the region we are going to")]
        [ArgHints("x", "URLARG", "X location to teleport to")]
        [ArgHints("y", "URLARG", "y location to teleport to")]
        [ArgHints("z", "URLARG", "z location to teleport to")]
        [Route(HttpVerbs.Get, "/Teleport/{region}/{x}/{y}/{z}/{token}")]
        public object Teleport(string region,string x, string y, string z, string token)
        {
            if (tokens.Allow(token, "core", "teleport", getClientIP()) == true)
            {
                bool status = bot.GetCommandsInterface.Call("Teleport", ""+region+"/" + x + "/" + y + "/" + z + "");
                if (status == true)
                {
                    return BasicReply("Accepted");
                }
                return Failure("Error Unable to Teleport to location");
            }
            return Failure("Token not accepted");
        }


        [About("Used to check HTTP connections")]
        [ReturnHints("world")]
        [NeedsToken(false)]
        [Route(HttpVerbs.Get, "/Hello")]
        public object Hello()
        {
            return BasicReply("world");
        }


        [About("Removes the given token from the accepted token pool")]
        [ReturnHints("Failed to remove token")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/LogoutUI/{token}")]
        public object LogoutUI(string token)
        {
            if (tokens.Allow(token, "core", "LogoutUI", getClientIP()) == true)
            {
                return tokens.Expire(token);
            }
            return Failure("Token not accepted");
        }






        [About("Makes a request to the core commands lib<br/>See LSL example on how to create a core command auth code")]
        [ArgHints("Request Body", "JsonObject", "Command: string<br/>Args: string[]<br/>AuthCode: string")]
        [ReturnHints("accepted")]
        [ReturnHints("Auth not accepted")]
        [ReturnHints("Failed")]
        [Route(HttpVerbs.Post, "/Command/{token}")]
        public async Task<object> Command(string token)
        {
            if (tokens.Allow(token, "core", "command", getClientIP()) == true)
            {
                CommandLibCall data = await HttpContext.GetRequestDataAsync<CommandLibCall>();
                string compressedArgs = string.Join("~#~", data.Args);
                string raw = data.Command + compressedArgs;
                string cooked = helpers.GetSHA1(raw + config.Security_SignedCommandkey);
                if (cooked == data.AuthCode)
                {
                    bool status = bot.GetCommandsInterface.Call(data.Command.ToLowerInvariant(), compressedArgs, UUID.Zero, "~#~");
                    if (status == true)
                    {
                        return BasicReply("accepted");
                    }
                    return Failure("Failed");
                }
                return Failure("Auth not accepted");
            }
            return Failure("Token not accepted");
        }
    }

    public class CommandLibCall
    {
        public string Command { get; set; }
        public string[] Args { get; set; }
        public string AuthCode { get; set; }
    }

    public class NearMeDetails
    {
        public string id { get; set; }
        public string name { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int range { get; set; }

    }


}
