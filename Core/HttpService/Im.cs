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
    public class HttpApiIM : WebApiControllerWithTokens
    {
        public HttpApiIM(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("gets a full list of all chat windows")]
        [ReturnHints("array UUID = Name")]
        [Route(HttpVerbs.Get, "/chatwindows/{token}")]
        public object chatwindows(string token)
        {
            if (tokens.Allow(token, "im", "chatwindows", getClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            return BasicReply(JsonConvert.SerializeObject(bot.GetIMChatWindowKeyNames()));
        }

        [About("gets a list of chat windows with unread messages")]
        [ReturnHints("array of UUID")]
        [Route(HttpVerbs.Get, "/listwithunread/{token}")]
        public object listwithunread(string group, string token)
        {
            if (tokens.Allow(token, "im", "listwithunread", getClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            List<UUID> unreadimwindows = new List<UUID>();
            foreach (UUID window in bot.IMChatWindows())
            {
                if (bot.ImChatWindowHasUnread(window) == true)
                {
                    unreadimwindows.Add(window);
                }
            }
            return BasicReply(JsonConvert.SerializeObject(unreadimwindows));
        }

        [About("gets if there are any unread im messages at all")]
        [ReturnHints("True|False")]
        [Route(HttpVerbs.Get, "/haveunreadims/{token}")]
        public object haveunreadims(string token)
        {
            if (tokens.Allow(token, "im", "haveunreadims", getClientIP()) == false)
            {
                return BasicReply("Token not accepted");
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
            return BasicReply(reply.ToString());
        }

        [About("gets the chat from the selected window")]
        [ArgHints("window", "URLARG", "the UUID of the chat window")]
        [ReturnHints("Window UUID invaild")]
        [ReturnHints("Array of text")]
        [Route(HttpVerbs.Get, "/getimchat/{window}/{token}")]
        public object getimchat(string window, string token)
        {
            if (tokens.Allow(token, "im", "getimchat", getClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            if (UUID.TryParse(window, out UUID windowUUID) == false)
            {
                return BasicReply("Window UUID invaild");
            }
            return BasicReply(JsonConvert.SerializeObject(bot.GetIMChatWindow(windowUUID)));
        }
    }
}
