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
        public object friends(string token)
        {
            return BasicReply(bot.getJsonFriendlist());
        }


        [Route(HttpVerbs.Get, "/nearme/{token}")]
        public object nearme(string token)
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
            return BasicReply("Error");
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


}
