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
            if (tokens.Allow(token, "im", "chatwindows", getClientIP()) == true)
            {
                return BasicReply(JsonConvert.SerializeObject(bot.GetIMChatWindowKeyNames()));
            }
            return BasicReply("Token not accepted");
        }

        [About("gets a list of chat windows with unread messages")]
        [ReturnHints("array of UUID")]
        [Route(HttpVerbs.Get, "/listwithunread/{token}")]
        public object listwithunread(string group, string token)
        {
            if (tokens.Allow(token, "im", "listwithunread", getClientIP()) == true)
            {
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
            return BasicReply("Token not accepted");
        }

        [About("gets if there are any unread im messages at all")]
        [ReturnHints("True|False")]
        [Route(HttpVerbs.Get, "/haveunreadims/{token}")]
        public object haveunreadims(string token)
        {
            if (tokens.Allow(token, "im", "haveunreadims", getClientIP()) == true)
            {
                bool reply = false;
                foreach(UUID window in bot.IMChatWindows())
                {
                    if(bot.ImChatWindowHasUnread(window) == true)
                    {
                        reply = true;
                        break;
                    }
                }
                return BasicReply(reply.ToString());
            }
            return BasicReply("Token not accepted");
        }

        [About("gets the chat from the selected window")]
        [ArgHints("window", "URLARG", "the UUID of the chat window")]
        [ReturnHints("Window UUID invaild")]
        [ReturnHints("Array of text")]
        [Route(HttpVerbs.Get, "/getimchat/{window}/{token}")]
        public object getimchat(string window, string token)
        {
            if (tokens.Allow(token, "im", "getimchat", getClientIP()) == true)
            {
                if (UUID.TryParse(window, out UUID windowUUID) == true)
                {
                    return BasicReply(JsonConvert.SerializeObject(bot.GetIMChatWindow(windowUUID)));
                }
                return BasicReply("Window UUID invaild");
            }
            return BasicReply("Token not accepted");
        }

        [About("sends a im to the selected avatar")]
        [ArgHints("avatar", "URLARG", "a UUID or Firstname Lastname")]
        [ArgHints("message", "Text", "the message to send")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Post, "/sendimchat/{avatar}/{token}")]
        public object sendimchat(string avatar, string token, [FormField] string message)
        {
            if (tokens.Allow(token, "im", "sendimchat", getClientIP()) == true)
            {
                bot.GetCommandsInterface.Call("im", avatar + "~#~" + message, UUID.Zero, "~#~");
                return BasicReply("ok");
            }
            return BasicReply("Token not accepted");
        }


    }
}
