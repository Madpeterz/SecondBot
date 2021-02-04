using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using BSB.bottypes;
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

    public class SecondbotCoreWebAPi : WebApiControllerWithTokens
    {
        
        protected JsonConfig config;
        public SecondbotCoreWebAPi(JsonConfig myconfig, SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens)
        {
            config = myconfig;
        }

        [Route(HttpVerbs.Post, "/gettoken")]
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

        [Route(HttpVerbs.Get, "/friends/{token}")]
        public object Friends(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return BasicReply(bot.getJsonFriendlist());
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/walkto/{x}/{y}/{z}/{token}")]
        public object WalkTo(string x, string y, string z, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
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

        
        [Route(HttpVerbs.Get, "/gesture/{gesture}/{token}")]
        public object Gesture(string gesture, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                bool status = bot.GetCommandsInterface.Call("PlayGesture", gesture);
                if (status == true)
                {
                    return BasicReply("Accepted");
                }
                return BasicReply("Error with gesture");
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/teleport/{region}/{x}/{y}/{z}/{token}")]
        public object Teleport(string region,string x, string y, string z, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
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

        

        [Route(HttpVerbs.Get, "/regiontile/{regionname}/{token}")]
        public object Regiontile(string regionname, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
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

        [Route(HttpVerbs.Get, "/nearmewithdetails/{token}")]
        public object NearmeWithDetails(string token)
        {
            if (bot.GetClient.Network.CurrentSim != null)
            {
                List<NearMeDetails> BetterNearMe = new List<NearMeDetails>();

                Dictionary<uint, Avatar> avcopy = bot.GetClient.Network.CurrentSim.ObjectsAvatars.Copy();
                foreach (Avatar av in avcopy.Values)
                {
                    if (av.ID != bot.GetClient.Self.AgentID)
                    {
                        NearMeDetails details = new NearMeDetails();
                        details.id = av.ID.ToString();
                        details.name = av.Name;
                        details.x = (int)Math.Round(av.Position.X);
                        details.y = (int)Math.Round(av.Position.Y);
                        details.z = (int)Math.Round(av.Position.Z);
                        details.range = (int)Math.Round(Vector3.Distance(av.Position,bot.GetClient.Self.SimPosition));
                        BetterNearMe.Add(details);
                    }
                }
                return BasicReply(JsonConvert.SerializeObject(BetterNearMe));
            }
            return BasicReply("Token not accepted");
        }


        [Route(HttpVerbs.Get, "/nearme/{token}")]
        public object Nearme(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                if (bot.GetClient.Network.CurrentSim != null)
                {
                    Dictionary<UUID, string> NearMe = new Dictionary<UUID, string>();
                    Dictionary<uint, Avatar> avcopy = bot.GetClient.Network.CurrentSim.ObjectsAvatars.Copy();
                    foreach (Avatar av in avcopy.Values)
                    {
                        if (av.ID != bot.GetClient.Self.AgentID)
                        {
                            NearMe.Add(av.ID, av.Name);
                        }
                    }
                    return BasicReply(JsonConvert.SerializeObject(NearMe));
                }
                return BasicReply("Error not in a sim");
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/hello")]
        public object Hello()
        {
            return BasicReply("world");
        }

        [Route(HttpVerbs.Get, "/logout/{token}")]
        public object Logout(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return tokens.Expire(token);
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/version/{token}")]
        public object Version(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return BasicReply(bot.MyVersion);
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/name/{token}")]
        public object Name(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return BasicReply(bot.GetClient.Self.FirstName + " " + bot.GetClient.Self.LastName);
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/regionname/{token}")]
        public object Regionname(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return BasicReply(bot.GetClient.Network.CurrentSim.Name);
            }
            return BasicReply("Token not accepted");
        }


        [Route(HttpVerbs.Get, "/location/{token}")]
        public object Location(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                Dictionary<string, int> pos = new Dictionary<string, int>();
                pos.Add("x",(int)Math.Round(bot.GetClient.Self.SimPosition.X));
                pos.Add("y", (int)Math.Round(bot.GetClient.Self.SimPosition.Y));
                pos.Add("z", (int)Math.Round(bot.GetClient.Self.SimPosition.Z));
                return BasicReply(JsonConvert.SerializeObject(pos));
            }
            return BasicReply("Token not accepted");
        }


        [Route(HttpVerbs.Post, "/command/{token}")]
        public async Task<object> command(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                CommandLibCall data = await HttpContext.GetRequestDataAsync<CommandLibCall>();
                string compressedArgs = string.Join("~#~", data.Args);
                string raw = data.Command + compressedArgs;
                string cooked = helpers.GetSHA1(raw + config.Security_SignedCommandkey);
                if (cooked == data.AuthCode)
                {
                    bool status = bot.GetCommandsInterface.Call(data.Command, compressedArgs, UUID.Zero, "~#~");
                    if (status == true)
                    {
                        return BasicReply("accepted");
                    }
                }
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
