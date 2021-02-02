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
    public class SecondbotIm : WebApiControllerWithTokens
    {
        public SecondbotIm(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [Route(HttpVerbs.Get, "/chatwindows/{token}")]
        public object chatwindows(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return BasicReply(JsonConvert.SerializeObject(bot.GetIMChatWindowKeyNames()));
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/listwithunread/{token}")]
        public object listwithunread(string group, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
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

        [Route(HttpVerbs.Get, "/haveunreadims/{token}")]
        public object haveunreadims(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
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

        [Route(HttpVerbs.Get, "/getimchat/{window}/{token}")]
        public object getimchat(string window, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                if (UUID.TryParse(window, out UUID windowUUID) == true)
                {
                    return BasicReply(JsonConvert.SerializeObject(bot.GetIMChatWindow(windowUUID)));
                }
                return BasicReply("Window UUID invaild");
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Post, "/sendimchat/{avatar}/{token}")]
        public object sendgroupchat(string avatar, string token, [FormField] string message)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                bot.GetCommandsInterface.Call("im", avatar + "~#~" + message, UUID.Zero, "~#~");
                return BasicReply("ok");
            }
            return BasicReply("Token not accepted");
        }


    }
}
