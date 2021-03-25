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
    public class HTTP_IM : WebApiControllerWithTokens
    {
        public HTTP_IM(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("gets a full list of all chat windows")]
        [ReturnHints("array UUID = Name")]
        [Route(HttpVerbs.Get, "/chatwindows/{token}")]
        public object chatwindows(string token)
        {
            if (tokens.Allow(token, "im", "chatwindows", handleGetClientIP()) == false)
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
            if (tokens.Allow(token, "im", "listwithunread", handleGetClientIP()) == false)
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
            if (tokens.Allow(token, "im", "haveunreadims", handleGetClientIP()) == false)
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
        [ReturnHints("Window UUID invaild")]
        [ReturnHints("Array of text")]
        [Route(HttpVerbs.Get, "/getimchat/{window}/{token}")]
        public object getimchat(string window, string token)
        {
            if (tokens.Allow(token, "im", "getimchat", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "getimchat", new [] { window });
            }
            if (UUID.TryParse(window, out UUID windowUUID) == false)
            {
                return Failure("Window UUID invaild", "getimchat", new [] { window });
            }
            return BasicReply(JsonConvert.SerializeObject(bot.GetIMChatWindow(windowUUID)), "getimchat", new [] { window });
        }
    }
}
