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
            return BasicReply("Authcode not accepted");
        }


        [About("Gets the friendslist <br/>Formated as follows<br/>friendreplyobject<br/><ul><li>name: String</li><li>id: String</li><li>online: bool</li></ul>")]
        [ReturnHints("array UUID = friendreplyobject")]
        [Route(HttpVerbs.Get, "/Friends/{token}")]
        public object Friends(string token)
        {
            if (tokens.Allow(token, "core", "friends", getClientIP()) == true)
            {
                return BasicReply(bot.getJsonFriendlist());
            }
            return BasicReply("Token not accepted");
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
                return BasicReply("Error Unable to AutoPilot to location");
            }
            return BasicReply("Token not accepted");
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
                return BasicReply("Error Unable to Teleport to location");
            }
            return BasicReply("Token not accepted");
        }

        [About("Fetchs the regions map tile")]
        [ReturnHints("Unable to find region")]
        [ReturnHints("Texture UUID")]
        [ArgHints("regionname", "URLARG", "the name of the region we are fetching")]
        [Route(HttpVerbs.Get, "/Regiontile/{regionname}/{token}")]
        public object Regiontile(string regionname, string token)
        {
            if (tokens.Allow(token, "core", "regiontile", getClientIP()) == true)
            {
                if (bot.GetClient.Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region))
                {
                    return BasicReply(region.MapImageID.ToString());
                }
                else
                {
                    return BasicReply("Unable to find region");
                }
            }
            return BasicReply("Token not accepted");
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
        [Route(HttpVerbs.Get, "/Logout/{token}")]
        public object Logout(string token)
        {
            if (tokens.Allow(token, "core", "logout", getClientIP()) == true)
            {
                return tokens.Expire(token);
            }
            return BasicReply("Token not accepted");
        }



        [About("Fetchs the current bot")]
        [ReturnHints("The build ID of the bot")]
        [Route(HttpVerbs.Get, "/Version/{token}")]
        public object Version(string token)
        {
            if (tokens.Allow(token, "core", "version", getClientIP()) == true)
            {
                return BasicReply(bot.MyVersion);
            }
            return BasicReply("Token not accepted");
        }

        [About("Fetchs the name of the bot")]
        [ReturnHints("Firstname Lastname")]
        [Route(HttpVerbs.Get, "/Name/{token}")]
        public object Name(string token)
        {
            if (tokens.Allow(token, "core", "name", getClientIP()) == true)
            {
                return BasicReply(bot.GetClient.Self.FirstName + " " + bot.GetClient.Self.LastName);
            }
            return BasicReply("Token not accepted");
        }

        [About("Fetchs the current region name")]
        [ReturnHints("Regionname")]
        [ReturnHints("Error not in a sim")]
        [Route(HttpVerbs.Get, "/Regionname/{token}")]
        public object Regionname(string token)
        {
            if (tokens.Allow(token, "core", "regionname", getClientIP()) == true)
            {
                if(bot.GetClient.Network.CurrentSim != null)
                {
                    return BasicReply(bot.GetClient.Network.CurrentSim.Name);
                }
                return BasicReply("Error not in a sim");
            }
            return BasicReply("Token not accepted");
        }

        [About("Fetchs the current location of the bot")]
        [ReturnHints("array of X,Y,Z values")]
        [ReturnHints("Error not in a sim")]
        [Route(HttpVerbs.Get, "/Location/{token}")]
        public object Location(string token)
        {
            if (tokens.Allow(token, "core", "location", getClientIP()) == true)
            {
                if (bot.GetClient.Network.CurrentSim != null)
                {
                    Dictionary<string, int> pos = new Dictionary<string, int>();
                    pos.Add("x", (int)Math.Round(bot.GetClient.Self.SimPosition.X));
                    pos.Add("y", (int)Math.Round(bot.GetClient.Self.SimPosition.Y));
                    pos.Add("z", (int)Math.Round(bot.GetClient.Self.SimPosition.Z));
                    return BasicReply(JsonConvert.SerializeObject(pos));
                }
                return BasicReply("Error not in a sim");
            }
            return BasicReply("Token not accepted");
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
                    return BasicReply("Failed");
                }
                return BasicReply("Auth not accepted");
            }
            return BasicReply("Token not accepted");
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
