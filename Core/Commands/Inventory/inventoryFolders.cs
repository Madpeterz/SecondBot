using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using BSB.Static;
using BetterSecondBotShared.Static;

namespace BSB.Commands.Inventory
{
    public class InventoryFolders : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Smart" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply target: Channel,Avatar,HTTP url" }; } }
        public override string Helpfile { get { return "Fully maps the avatars inventory folders<br/>" +
                    "*[Arg 1 UUID] sends you a notecard with it<br/>" +
                    "*[Arg 1 Anything else] Sends it as a CSV via the smart target<br/>" +
                    "Formated as follows<br>" +
                    "entry,entry...<br/>" +
                    "Where entry is formated foldername###level###UUID"; } }
        public override int MinArgs { get { return 1; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID targetavatar) == true)
                {
                    bot.GetClient.Self.InstantMessage(targetavatar, "Building inventory folder map now [This might take a while]");
                    string reply = HelperInventory.MapFolder(bot, false);
                    bot.SendNotecard("Inventory Folders", reply, targetavatar);
                    return true;
                }
                else
                {
                    string reply = HelperInventory.MapFolder(bot, true);
                    return bot.GetCommandsInterface.SmartCommandReply(args[0], reply, CommandName);
                }
            }
            return false;
        }
    }
}
