using BSB.Static;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.Inventory
{
    public class SendFolder : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar","UUID" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]", "FOLDER" }; } }
        public override string Helpfile { get { return "Sends a folder [ARG 2] to an avatar [ARG 1]"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID targetavatar) == true)
                {
                    if (UUID.TryParse(args[1], out UUID targetfolder) == true)
                    {
                        InventoryBase FindFolderHelper = HelperInventory.FindFolder(bot, bot.GetClient.Inventory.Store.RootFolder, targetfolder);
                        if (FindFolderHelper != null)
                        {
                            bot.GetClient.Inventory.GiveFolder(FindFolderHelper.UUID, FindFolderHelper.Name, targetavatar, false);
                            return true;
                        }
                        else
                        {
                            return Failed("Unable to find folder");
                        }
                    }
                    else
                    {
                        return Failed("Invaild folder UUID");
                    }
                }
                else
                {
                    return Failed("Invaild avatar UUID");
                }
            }
            return false;
        }
    }
}
