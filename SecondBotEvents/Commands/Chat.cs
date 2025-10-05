using System.Text.Json;
using OpenMetaverse;
using SecondBotEvents.Services;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Text based interaction,message status and history")]
    public class Chat(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("fetchs the last localchat messages")]
        [ReturnHints("json encoded array")]
        [CmdTypeGet()]
        public object LocalChatHistory()
        {
            return BasicReply(JsonSerializer.Serialize(master.DataStoreService.GetLocalChat(), JsonOptions.UnsafeRelaxed));
        }

        [About("sends a message to localchat (Normal chat)")]
        [ArgHints("channel", "the channel to output on", "Number", "123")]
        [ArgHints("message", "the message to send", "Text", "Hi Mom im on TV")]
        [ReturnHints("array string")]
        [ReturnHintsFailure("Message empty")]
        [CmdTypeDo()]
        public object Say(string channel, string message)
        {
            if (SecondbotHelpers.IsEmpty(message) == true)
            {
                return Failure("Message empty", [channel, message]);
            }
            if(int.TryParse(channel,out int channelnum) == false)
            {
                return Failure("Invaild channel", [channel, message]);
            }
            master.DataStoreService.BotRecordLocalchatReply(message);
            GetClient().Self.Chat(message, channelnum, ChatType.Normal);
            return BasicReply("ok");
        }

        [About("sends a message to localchat (as a Shout)")]
        [ArgHints("channel", "the channel to output on", "Number", "123")]
        [ArgHints("message", "the message to send", "Text", "Hi Mom im on TV")]
        [ReturnHints("array string")]
        [ReturnHintsFailure("Message empty")]
        [CmdTypeDo()]
        public object Shout(string channel, string message)
        {
            if (SecondbotHelpers.IsEmpty(message) == true)
            {
                return Failure("Message empty", [channel, message]);
            }
            if (int.TryParse(channel, out int channelnum) == false)
            {
                return Failure("Invaild channel", [channel, message]);
            }
            master.DataStoreService.BotRecordLocalchatReply(message);
            GetClient().Self.Chat(message, channelnum, ChatType.Shout);
            return BasicReply("ok");
        }

        [About("sends a message to localchat (as a Whisper)")]
        [ArgHints("channel", "the channel to output on", "Number", "123")]
        [ArgHints("message", "the message to send", "Text", "Hi Mom im on TV")]
        [ReturnHints("array string")]
        [ReturnHintsFailure("Message empty")]
        [CmdTypeDo()]
        public object Whisper(string channel, string message)
        {
            if (SecondbotHelpers.IsEmpty(message) == true)
            {
                return Failure("Message empty", [channel, message]);
            }
            if (int.TryParse(channel, out int channelnum) == false)
            {
                return Failure("Invaild channel", [channel, message]);
            }
            master.DataStoreService.BotRecordLocalchatReply(message);
            GetClient().Self.Chat(message, channelnum, ChatType.Whisper);
            return BasicReply("ok");
        }



        [About("sends a im to the selected avatar")]
        [ArgHints("avatar", "Who to send the messsage to", "AVATAR")]
        [ArgHints("message", "the message to send", "Text", "Hi there how are you today?")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHintsFailure("avatar lookup")]
        [CmdTypeDo()]
        public object IM(string avatar, string message)
        {
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", [avatar, message]);
            }
            if (SecondbotHelpers.IsEmpty(message) == true)
            {
                return Failure("Message empty", [avatar, message]);
            }
            master.DataStoreService.OpenChatWindow(false, avataruuid, avataruuid);
            master.BotClient.SendIM(avataruuid, message);
            return BasicReply("ok",  [avatar, message]);
        }

        [About("gets a full list of all avatar chat windows")]
        [ReturnHints("array UUID = Name")]
        [CmdTypeGet()]
        public object chatwindows()
        {
            return BasicReply(JsonSerializer.Serialize(master.DataStoreService.GetAvatarImWindows(), JsonOptions.UnsafeRelaxed));
        }

        [About("gets a list of chat windows from avatars with unread messages")]
        [ReturnHints("array of UUID")]
        [CmdTypeGet()]
        public object listwithunread()
        {
            return BasicReply(JsonSerializer.Serialize(master.DataStoreService.GetAvatarImWindowsUnread(), JsonOptions.UnsafeRelaxed));
        }

        [About("gets if there are any unread im messages from avatars at all")]
        [ReturnHints("True|False")]
        [CmdTypeGet()]
        public object haveunreadims()
        {
            return BasicReply(master.DataStoreService.GetAvatarImWindowsUnreadAny().ToString());
        }

        [About("gets the chat from the selected window for avatar")]
        [ArgHints("window", "avatar UUID you wish to view the chat from", "UUID")]
        [ReturnHintsFailure("avatar UUID invaild")]
        [ReturnHints("Array of text")]
        [CmdTypeGet()]
        public object getimchat(string avatarid)
        {
            UUID avatarUUID = UUID.Zero;
            if (UUID.TryParse(avatarid, out avatarUUID) == false)
            {
                return Failure("avatar UUID invaild");
            }
            return BasicReply(JsonSerializer.Serialize(master.DataStoreService.GetAvatarImWindow(avatarUUID), JsonOptions.UnsafeRelaxed));
        }

    }
}
