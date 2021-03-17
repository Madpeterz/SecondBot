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
            if (tokens.Allow(token, "chat", "LocalChatHistory", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "LocalChatHistory", new string[] { });
            }
            return BasicReply(JsonConvert.SerializeObject(bot.getLocalChatHistory()), "LocalChatHistory", new string[] { });
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
            if (tokens.Allow(token, "chat", "Say", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Say", new string[] { channel, message });
            }
            if (helpers.isempty(message) == true)
            {
                return Failure("Message empty", "Say", new string[] { channel, message });
            }
            if(int.TryParse(channel,out int channelnum) == false)
            {
                return Failure("Invaild channel", "Say", new string[] { channel, message });
            }
            if(channelnum < 0)
            {
                return Failure("Invaild channel", "Say", new string[] { channel, message });
            }
            bot.GetClient.Self.Chat(message, channelnum, ChatType.Normal);
            return BasicReply(JsonConvert.SerializeObject(bot.getLocalChatHistory()), "Say", new string[] { channel, message });
            
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
            if (tokens.Allow(token, "chat", "IM", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "IM", new string[] { avatar, message });
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", "IM", new string[] { avatar, message });
            }
            if (helpers.isempty(message) == true)
            {
                return Failure("Message empty", "IM", new string[] { avatar, message });
            }
            bot.SendIM(avataruuid, message);
            return BasicReply("ok", "IM", new string[] { avatar, message });
        }

    }


}
