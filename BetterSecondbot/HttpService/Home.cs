using BetterSecondBot.bottypes;
using BetterSecondBot.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Newtonsoft.Json;
using BetterSecondBotShared.Static;

namespace BetterSecondBot.HttpService
{
    public class Http_Home : WebApiControllerWithTokens
    {
        public Http_Home(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Makes the bot teleport to its home region")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/GoHome/{token}")]
        public object GoHome(string group, string token)
        {
            if (tokens.Allow(token, "groups", "GetGroupMembers", getClientIP()) == true)
            {
                return Failure("Token not accepted");
            }
            bot.GotoNextHomeRegion();
            return BasicReply("ok");
        }

    }
}
