using System;
using OpenMetaverse;
using BetterSecondBot.Static;
using BetterSecondBotShared.Static;
using System.Collections.Generic;

namespace BetterSecondBot.Commands.Inventory
{

    public class InventoryContents : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart","UUID" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply target: Channel,Avatar,HTTP","Folder" }; } }
        public override string Helpfile { get { return "Maps the given inventory folder [ARG 2] for contents<br/>" +
                    "Arg 1 *Avatar UUID* sends it via notecard to [ARG 1]<hr/>" +
                    "Arg 1 *Anything else*<br/>as csv packed as follows folderinventory=UUID~|~entry~|~entry..." +
                    "entry is formated as follows: itemname###uuid###type<br/>" +
                    "sent to smart reply target"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[1], out UUID targetfolder) == true)
                {
                    if (UUID.TryParse(args[0], out UUID targetavatar) == true)
                    {
                        KeyValuePair<string,string> content = HelperInventory.MapFolderInventoryHumanReadable(bot, targetfolder);
                        if (content.Value != null)
                        {
                            string notecardname = ""+content.Key+" Contents @" + helpers.UnixTimeNow().ToString() + "";
                            bot.SendNotecard(notecardname, content.Value, targetavatar);
                        }
                        else
                        {
                            bot.GetClient.Self.InstantMessage(targetavatar, "Unable to load folder or it is empty");
                        }
                        return true;
                    }
                    else
                    {
                        return bot.GetCommandsInterface.SmartCommandReply(true, args[0], HelperInventory.MapFolderInventoryJson(bot,targetfolder), CommandName);
                    }
                }
                else
                {
                    return Failed("Invaild folder UUID");
                }
            }
            return false;
        }
    }
}
