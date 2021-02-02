using BSB.bottypes;
using BSB.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Newtonsoft.Json;

namespace BetterSecondBot.HttpService
{
    public class SecondbotChat : WebApiControllerWithTokens
    {
        public SecondbotChat(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [Route(HttpVerbs.Get, "/localchathistory/{token}")]
        public object localchatHistory(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return BasicReply(JsonConvert.SerializeObject(bot.getLocalChatHistory()));
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Post, "/localchatsay/{token}")]
        public object localchatSay(string token, [FormField] string message)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                bot.GetCommandsInterface.Call("say", message);
                return BasicReply(JsonConvert.SerializeObject(bot.getLocalChatHistory()));
            }
            return BasicReply("Token not accepted");
        }

    }


}
