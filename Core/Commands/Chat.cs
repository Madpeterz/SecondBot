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
    public class HTTP_Chat : WebApiControllerWithTokens
    {
        public HTTP_Chat(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("fetchs the last 20 localchat messages")]
        [ReturnHints("array string")]
        [Route(HttpVerbs.Get, "/LocalChatHistory/{token}")]
        public object LocalChatHistory(string token)
        {
            if (tokens.Allow(token, "chat", "localchathistory", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            return BasicReply(JsonConvert.SerializeObject(bot.getLocalChatHistory()));
        }

        [About("sends a message to localchat")]
        [ArgHints("channel", "URLARG", "the channel to output on (>=0)")]
        [ArgHints("message", "Text", "the message to send")]
        [ReturnHints("array string")]
        [ReturnHints("Message empty")]
        [ReturnHints("Invaild channel")]
        [Route(HttpVerbs.Post, "/Say/{channel}/{token}")]
        public object Say(string channel,[FormField] string message,string token)
        {
            if (tokens.Allow(token, "chat", "localchatsay", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            if (helpers.notempty(message) == false)
            {
                return BasicReply("Message empty");
            }
            if(int.TryParse(channel,out int channelnum) == false)
            {
                return BasicReply("Invaild channel");
            }
            if(channelnum < 0)
            {
                return BasicReply("Invaild channel");
            }
            bot.GetClient.Self.Chat(message, channelnum, ChatType.Normal);
            return BasicReply(JsonConvert.SerializeObject(bot.getLocalChatHistory()));
            
        }

        [About("sends a im to the selected avatar")]
        [ArgHints("avatar", "URLARG", "a UUID or Firstname Lastname")]
        [ArgHints("message", "Text", "the message to send")]
        [ReturnHints("ok")]
        [ReturnHints("Message empty")]
        [ReturnHints("avatar lookup")]
        [Route(HttpVerbs.Post, "/IM/{avatar}/{token}")]
        public object IM(string avatar, [FormField] string message, string token)
        {
            if (tokens.Allow(token, "chat", "im", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return BasicReply("avatar lookup");
            }
            if (helpers.notempty(message) == false)
            {
                return BasicReply("Message empty");
            }
            bot.SendIM(avataruuid, message);
            return BasicReply("ok");
        }

    }


}
