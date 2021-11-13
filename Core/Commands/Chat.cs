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
                return Failure("Token not accepted", "LocalChatHistory");
            }
            return BasicReply(JsonConvert.SerializeObject(bot.getLocalChatHistory()), "LocalChatHistory");
        }

        [About("sends a message to localchat")]
        [ArgHints("channel", "URLARG", "the channel to output on (>=0)")]
        [ArgHints("message", "Text", "the message to send")]
        [ReturnHints("array string")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHintsFailure("Invaild channel")]
        [Route(HttpVerbs.Post, "/Say/{channel}/{token}")]
        public object Say(string channel,[FormField] string message,string token)
        {
            if (tokens.Allow(token, "chat", "Say", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Say", new [] { channel, message });
            }
            if (helpers.isempty(message) == true)
            {
                return Failure("Message empty", "Say", new [] { channel, message });
            }
            if(int.TryParse(channel,out int channelnum) == false)
            {
                return Failure("Invaild channel", "Say", new [] { channel, message });
            }
            if(channelnum < 0)
            {
                return Failure("Invaild channel", "Say", new [] { channel, message });
            }
            bot.GetClient.Self.Chat(message, channelnum, ChatType.Normal);
            if (channelnum == 0)
            {
                bot.AddToLocalChat(bot.GetClient.Self.Name, message);
            }
            return BasicReply(JsonConvert.SerializeObject(bot.getLocalChatHistory()), "Say", new [] { channel, message });
            
        }

        [About("sends a im to the selected avatar")]
        [ArgHints("avatar", "URLARG", "a UUID or Firstname Lastname")]
        [ArgHints("message", "Text", "the message to send")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHintsFailure("avatar lookup")]
        [Route(HttpVerbs.Post, "/IM/{avatar}/{token}")]
        public object IM(string avatar, [FormField] string message, string token)
        {
            if (tokens.Allow(token, "chat", "IM", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "IM", new [] { avatar, message });
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", "IM", new [] { avatar, message });
            }
            if (helpers.isempty(message) == true)
            {
                return Failure("Message empty", "IM", new [] { avatar, message });
            }
            bot.SendIM(avataruuid, message);
            return BasicReply("ok", "IM", new [] { avatar, message });
        }

        [About("gets a full list of all chat windows")]
        [ReturnHints("array UUID = Name")]
        [Route(HttpVerbs.Get, "/chatwindows/{token}")]
        public object chatwindows(string token)
        {
            if (tokens.Allow(token, "chat", "chatwindows", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted", "chatwindows");
            }
            return BasicReply(JsonConvert.SerializeObject(bot.GetIMChatWindowKeyNames()), "chatwindows");
        }

        [About("gets a list of chat windows with unread messages")]
        [ReturnHints("array of UUID")]
        [Route(HttpVerbs.Get, "/listwithunread/{token}")]
        public object listwithunread(string token)
        {
            if (tokens.Allow(token, "chat", "listwithunread", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted", "listwithunread");
            }
            List<UUID> unreadimwindows = new List<UUID>();
            foreach (UUID window in bot.IMChatWindows())
            {
                if (bot.ImChatWindowHasUnread(window) == true)
                {
                    unreadimwindows.Add(window);
                }
            }
            return BasicReply(JsonConvert.SerializeObject(unreadimwindows), "listwithunread");
        }

        [About("gets if there are any unread im messages at all")]
        [ReturnHints("True|False")]
        [Route(HttpVerbs.Get, "/haveunreadims/{token}")]
        public object haveunreadims(string token)
        {
            if (tokens.Allow(token, "chat", "haveunreadims", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted", "haveunreadims");
            }
            bool reply = false;
            foreach (UUID window in bot.IMChatWindows())
            {
                if (bot.ImChatWindowHasUnread(window) == true)
                {
                    reply = true;
                    break;
                }
            }
            return BasicReply(reply.ToString(), "haveunreadims");
        }

        [About("gets the chat from the selected window")]
        [ArgHints("window", "URLARG", "the UUID of the chat window")]
        [ReturnHintsFailure("Window UUID invaild")]
        [ReturnHints("Array of text")]
        [Route(HttpVerbs.Get, "/getimchat/{window}/{token}")]
        public object getimchat(string window, string token)
        {
            if (tokens.Allow(token, "chat", "getimchat", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "getimchat", new[] { window });
            }
            if (UUID.TryParse(window, out UUID windowUUID) == false)
            {
                return Failure("Window UUID invaild", "getimchat", new[] { window });
            }
            return BasicReply(JsonConvert.SerializeObject(bot.GetIMChatWindow(windowUUID)), "getimchat", new[] { window });
        }

    }


}
