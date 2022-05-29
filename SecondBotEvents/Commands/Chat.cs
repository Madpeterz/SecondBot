using EmbedIO;
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

        [About("fetchs the last localchat messages")]
        [ReturnHints("json encoded array")]
        [Route(HttpVerbs.Get, "/LocalChatHistory/{token}")]
        public object LocalChatHistory(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.getLocalChat()));
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
            return BasicReply("ok");
            
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

        [About("gets a full list of all avatar chat windows")]
        [ReturnHints("array UUID = Name")]
        [Route(HttpVerbs.Get, "/chatwindows/{token}")]
        public object chatwindows(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.getAvatarImWindows()));
        }

        [About("gets a list of chat windows from avatars with unread messages")]
        [ReturnHints("array of UUID")]
        [Route(HttpVerbs.Get, "/listwithunread/{token}")]
        public object listwithunread(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.getAvatarImWindowsUnread()));
        }

        [About("gets if there are any unread im messages from avatars at all")]
        [ReturnHints("True|False")]
        [Route(HttpVerbs.Get, "/haveunreadims/{token}")]
        public object haveunreadims(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            return BasicReply(master.DataStoreService.getAvatarImWindowsUnreadAny().ToString());
        }

        [About("gets the chat from the selected window for avatar")]
        [ArgHints("window", "URLARG", "the UUID of the avatar you wish to view the chat from")]
        [ReturnHintsFailure("avatar UUID invaild")]
        [ReturnHints("Array of text")]
        [Route(HttpVerbs.Get, "/getimchat/{avatarid}/{token}")]
        public object getimchat(string avatarid, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            UUID avatarUUID = UUID.Zero;
            if (UUID.TryParse(avatarid, out avatarUUID) == false)
            {
                return Failure("avatar UUID invaild");
            }
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.getAvatarImWindow(avatarUUID)));
        }

    }
}
