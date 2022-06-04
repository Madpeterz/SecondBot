using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Services;

namespace SecondBotEvents.Commands
{
    public class Chat : CommandsAPI
    {
        public Chat(EventsSecondBot setmaster) : base(setmaster)
        {
        }

        [About("fetchs the last localchat messages")]
        [ReturnHints("json encoded array")]
        public object LocalChatHistory()
        {
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetLocalChat()));
        }

        [About("sends a message to localchat")]
        [ArgHints("channel", "the channel to output on (>=0)")]
        [ArgHints("message", "the message to send")]
        [ReturnHints("array string")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHintsFailure("Invaild channel")]
        public object Say(string channel, string message)
        {
            if (SecondbotHelpers.isempty(message) == true)
            {
                return Failure("Message empty", new [] { channel, message });
            }
            if(int.TryParse(channel,out int channelnum) == false)
            {
                return Failure("Invaild channel", new [] { channel, message });
            }
            if(channelnum < 0)
            {
                return Failure("Invaild channel", new [] { channel, message });
            }
            GetClient().Self.Chat(message, channelnum, ChatType.Normal);
            return BasicReply("ok");
            
        }

        [About("sends a im to the selected avatar")]
        [ArgHints("avatar", "a UUID or Firstname Lastname")]
        [ArgHints("message", "the message to send")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHintsFailure("avatar lookup")]
        public object IM(string avatar, string message)
        {
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", new [] { avatar, message });
            }
            if (SecondbotHelpers.isempty(message) == true)
            {
                return Failure("Message empty", new [] { avatar, message });
            }
            master.BotClient.SendIM(avataruuid, message);
            return BasicReply("ok",  new [] { avatar, message });
        }

        [About("gets a full list of all avatar chat windows")]
        [ReturnHints("array UUID = Name")]
        public object chatwindows()
        {
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetAvatarImWindows()));
        }

        [About("gets a list of chat windows from avatars with unread messages")]
        [ReturnHints("array of UUID")]
        public object listwithunread()
        {
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetAvatarImWindowsUnread()));
        }

        [About("gets if there are any unread im messages from avatars at all")]
        [ReturnHints("True|False")]
        public object haveunreadims()
        {
            return BasicReply(master.DataStoreService.GetAvatarImWindowsUnreadAny().ToString());
        }

        [About("gets the chat from the selected window for avatar")]
        [ArgHints("window", "the UUID of the avatar you wish to view the chat from")]
        [ReturnHintsFailure("avatar UUID invaild")]
        [ReturnHints("Array of text")]
        public object getimchat(string avatarid)
        {
            UUID avatarUUID = UUID.Zero;
            if (UUID.TryParse(avatarid, out avatarUUID) == false)
            {
                return Failure("avatar UUID invaild");
            }
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetAvatarImWindow(avatarUUID)));
        }

    }
}
