using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using BSB.bottypes;
using OpenMetaverse;
using System.Threading.Tasks;

using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;


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
        public string GetToken([FormField] string authcode)
        {
            long now = helpers.UnixTimeNow() + 120;
            int loop = 0;
            bool accepted = false;
            while(loop <= 30)
            {
                string hashA = helpers.GetSHA1((now + loop) + "" + config.Security_WebUIKey).Substring(0, 10);
                string hashB = helpers.GetSHA1((now - loop) + "" + config.Security_WebUIKey).Substring(0, 10);
                if ((authcode == hashA) || (authcode == hashB))
                {
                    accepted = true;
                    break;
                }
                loop++;
            }
            if(accepted == true)
            {
                return tokens.CreateToken(HttpContext.Request.RemoteEndPoint.Address);
            }
            return "Authcode not accepted";
        }

        [Route(HttpVerbs.Get, "/logout/{token}")]
        public string Logout(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return tokens.Expire(token);
            }
            return "Token not accepted";
        }

        [Route(HttpVerbs.Get, "/version/{token}")]
        public string Version(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return bot.MyVersion;
            }
            return "Token not accepted";
        }

        [Route(HttpVerbs.Get, "/name/{token}")]
        public string Name(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return bot.GetClient.Self.FirstName + " " + bot.GetClient.Self.LastName;
            }
            return "Token not accepted";
        }

        [Route(HttpVerbs.Post, "/command/{token}")]
        public async Task<string> command(string token)
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
                        return "accepted";
                    }
                }
            }
            return "Token not accepted";
        }
    }

    public class CommandLibCall
    {
        public string Command { get; set; }
        public string[] Args { get; set; }
        public string AuthCode { get; set; }
    }


}
