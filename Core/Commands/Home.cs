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
    public class HTTP_Home : WebApiControllerWithTokens
    {
        public HTTP_Home(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Makes the bot teleport to its home region")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/GoHome/{token}")]
        public object GoHome(string group, string token)
        {
            if (tokens.Allow(token, "groups", "GetGroupMembers", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            bot.GotoNextHomeRegion();
            return BasicReply("ok");
        }

    }
}
