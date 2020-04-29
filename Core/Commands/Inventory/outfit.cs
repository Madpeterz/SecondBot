using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Inventory
{
    public class Outfit : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Text" }; } }
        public override string[] ArgHints { get { return new[] { "Folder name" }; } }
        public override string Helpfile { get { return "Replaces the current avatar outfit with the Clothing/[ARG1] folder<br/>Please note: This does not use the outfits folder!"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                // uses the Clothing folder
                // must be a full outfit (shapes/eyes ect)
                InventoryFolder AA = bot.GetClient.Inventory.Store.RootFolder;
                List<InventoryBase> T = bot.GetClient.Inventory.Store.GetContents(AA);
                AA = null;
                foreach (InventoryBase R in T)
                {
                    if (R.Name == "Clothing")
                    {
                        AA = (InventoryFolder)R;
                        break;
                    }
                }
                if (AA != null)
                {
                    T = bot.GetClient.Inventory.Store.GetContents(AA);
                    AA = null;
                    foreach (InventoryBase R in T)
                    {
                        if (R.Name == args[0])
                        {
                            AA = (InventoryFolder)R;
                            break;
                        }
                    }
                    if (AA != null)
                    {
                        List<InventoryBase> contents = bot.GetClient.Inventory.FolderContents(AA.UUID, bot.GetClient.Self.AgentID, true, true, InventorySortOrder.ByName, 20 * 1000);
                        List<InventoryItem> wareables = new List<InventoryItem>();
                        if (contents != null)
                        {
                            foreach (InventoryBase item in contents)
                            {
                                if ((item is InventoryWearable) || (item is InventoryObject))
                                {
                                    wareables.Add((InventoryItem)item);
                                }
                            }
                            bot.GetClient.Appearance.ReplaceOutfit(wareables, false);
                            return true;
                        }
                        else
                        {
                            return Failed("target folder is empty or so full I cant get it in 20 secs...");
                        }
                    }
                    else
                    {
                        return Failed("Cant find target folder");
                    }
                }
                else
                {
                    return Failed("Cant find Clothing folder");
                }
            }
            return false;
        }
    }
}
