﻿using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Services;
using System.Collections.Generic;

namespace SecondBotEvents.Commands
{
    public class Chat : CommandsAPI
    {
        public Chat(EventsSecondBot setmaster) : base(setmaster)
        {
        }
        [About("fetchs the last 20 localchat messages")]
        [ReturnHints("array string")]
        [Route(HttpVerbs.Get, "/LocalChatHistory/{token}")]
        public object LocalChatHistory(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            return Failure("@todo add local chat history");
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
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (SecondbotHelpers.isempty(message) == true)
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
            getClient().Self.Chat(message, channelnum, ChatType.Normal);
            if (channelnum == 0)
            {
                // @todo add message to localchat
            }
            return Failure("@todo get localchat history");
            
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
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", "IM", new [] { avatar, message });
            }
            if (SecondbotHelpers.isempty(message) == true)
            {
                return Failure("Message empty", "IM", new [] { avatar, message });
            }
            master.botClient.SendIM(avataruuid, message);
            return BasicReply("ok", "IM", new [] { avatar, message });
        }

        [About("gets a full list of all chat windows")]
        [ReturnHints("array UUID = Name")]
        [Route(HttpVerbs.Get, "/chatwindows/{token}")]
        public object chatwindows(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            return Failure("@todo chat windows list");
        }

        [About("gets a list of chat windows with unread messages")]
        [ReturnHints("array of UUID")]
        [Route(HttpVerbs.Get, "/listwithunread/{token}")]
        public object listwithunread(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            List<UUID> unreadimwindows = new List<UUID>();
            return Failure("@todo chat windows list unread");
        }

        [About("gets if there are any unread im messages at all")]
        [ReturnHints("True|False")]
        [Route(HttpVerbs.Get, "/haveunreadims/{token}")]
        public object haveunreadims(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            return Failure("@todo have any unread ims");
        }

        [About("gets the chat from the selected window")]
        [ArgHints("window", "URLARG", "the UUID of the chat window")]
        [ReturnHintsFailure("Window UUID invaild")]
        [ReturnHints("Array of text")]
        [Route(HttpVerbs.Get, "/getimchat/{window}/{token}")]
        public object getimchat(string window, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(window, out UUID windowUUID) == false)
            {
                return Failure("Window UUID invaild", "getimchat", new[] { window });
            }
            return Failure("@todo chat window fetch");
        }

    }


}
