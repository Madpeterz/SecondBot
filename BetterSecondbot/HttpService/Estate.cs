using BetterSecondBotShared.Json;
using BetterSecondBot.bottypes;
using EmbedIO;
using EmbedIO.Routing;
using System;
using Newtonsoft.Json;

namespace BetterSecondBot.HttpService
{



    public class HttpAPIEstate : WebApiControllerWithTokens
    {
        public HttpAPIEstate(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens)
        {

        }

        [About("Requests the estate banlist")]
        [ReturnHints("")]
        [Route(HttpVerbs.Get, "/banlist/{token}")]
        public Object Banlist(string token)
        {
            if (tokens.Allow(token, "estate", "banlist", getClientIP()) == true)
            {
                return BasicReply(JsonConvert.SerializeObject(bot._lastUpdatedSimBlacklist));
            }
            return BasicReply("Token not accepted");
        }
    }
}
