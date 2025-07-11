﻿using OpenMetaverse;
using SecondBotEvents.Services;


namespace SecondBotEvents.Commands
{
    [ClassInfo("Script dialog box interaction / events when displayed")]
    public class Dialogs(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("adds a avatar dialog relay target [or removes if it exists]")]
        [ReturnHints("added")]
        [ReturnHints("removed")]
        [ReturnHintsFailure("unable to find avatar")]
        [ReturnHintsFailure("looking up avatar please try again")]
        [ArgHints("avatar", "Who to send the dialog relay to", "AVATAR")]
        [CmdTypeSet()]
        public object DialogRelayAvatarTarget(string avatar)
        {
            if(UUID.TryParse(avatar, out UUID avUUID) == false)
            {
                string avataruuid = master.DataStoreService.GetAvatarUUID(avatar);
                if (avataruuid == "lookup")
                {
                    return Failure("looking up avatar please try again");
                }
                if (UUID.TryParse(avataruuid, out avUUID) == false)
                {
                    return Failure("unable to find avatar");
                }
            }
            return BasicReply(master.DialogService.AvatarRelayTarget(avUUID));
        }

        [About("adds a chat dialog relay target [or removes if it exists]")]
        [ReturnHints("added")]
        [ReturnHints("removed")]
        [ReturnHintsFailure("channel must be zero or more")]
        [ReturnHintsFailure("channel is not vaild")]
        [ArgHints("channel", "what channel number to send the reply to (must be zero or higher)","Number", "123")]
        [CmdTypeSet()]
        public object DialogRelayChatTarget(string channel)
        {
            if (int.TryParse(channel, out int channelnum) == false)
            {
                return Failure("channel is not vaild");
            }
            if(channelnum < 0)
            {
                return Failure("channel must be zero or more");
            }
            return BasicReply(master.DialogService.ChannelRelayTarget(channelnum));
        }

        [About("adds a http dialog relay target [or removes if it exists]")]
        [ReturnHints("added")]
        [ReturnHints("removed")]
        [ReturnHintsFailure("url must start with http")]
        [ArgHints("url", "the URL to send the replys to","URL","http://mycoolsite.com/botdialog.php")]
        [CmdTypeSet()]
        public object DialogRelayHttpTarget(string url)
        {
            if(url.StartsWith("http") == false)
            {
                return Failure("url must start with http");
            }
            return BasicReply(master.DialogService.HttpRelayTarget(url));
        }


        [About("Makes the bot interact with the dialog [dialogid] with the button [buttontext]")]
        [ReturnHints("action")]
        [ReturnHintsFailure("Invaild dialog window")]
        [ReturnHintsFailure("Invaild dialog button")]
        [ReturnHintsFailure("bad dialog id")]
        [ArgHints("dialogid", "The ID for the dialog","Number","442")]
        [ArgHints("buttontext", "The button text to push","Text","Unlock")]
        [CmdTypeDo()]
        public object DialogResponce(string dialogid, string buttontext)
        {
            if (int.TryParse(dialogid, out int dialogidnum) == false)
            {
                return Failure("bad dialog id", [dialogid, buttontext]);
            }
            return BasicReply(master.DialogService.DialogAction(dialogidnum, buttontext));
        }

    }
}
