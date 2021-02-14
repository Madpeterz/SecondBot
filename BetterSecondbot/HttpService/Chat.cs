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

namespace BetterSecondBot.HttpService
{
    public class HttpApiLocalchat : WebApiControllerWithTokens
    {
        public HttpApiLocalchat(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("fetchs the last 20 localchat messages")]
        [ReturnHints("array string")]
        [Route(HttpVerbs.Get, "/localchathistory/{token}")]
        public object localchatHistory(string token)
        {
            if (tokens.Allow(token, "chat", "localchathistory", getClientIP()) == true)
            {
                return BasicReply(JsonConvert.SerializeObject(bot.getLocalChatHistory()));
            }
            return BasicReply("Token not accepted");
        }

        [About("sends a message to localchat")]
        [ArgHints("message","Text", "the message to send")]
        [ReturnHints("array string")]
        [Route(HttpVerbs.Post, "/localchatsay/{token}")]
        public object localchatSay(string token, [FormField] string message)
        {
            if (tokens.Allow(token, "chat", "localchatsay", getClientIP()) == true)
            {
                bot.GetCommandsInterface.Call("say", message);
                return BasicReply(JsonConvert.SerializeObject(bot.getLocalChatHistory()));
            }
            return BasicReply("Token not accepted");
        }

    }


}
