using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Inventory
{
    public class Attach : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Text", "Text" }; } }
        public override string[] ArgHints { get { return new[] { "Sub folder", "Item name" }; } }
        public override string Helpfile { get { return "Attachs a item found inside the attachments / [ARG1] folder<br/>that matchs the item name [ARG2]"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                // attachments/[arg1]/[arg2]
                // arg1 = sub folder
                // arg2 = itemname
                InventoryFolder AA = bot.GetClient.Inventory.Store.RootFolder;
                List<InventoryBase> T = bot.GetClient.Inventory.Store.GetContents(AA);
                AA = null;
                foreach (InventoryBase R in T)
                {
                    if (R.Name == "attachments")
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
                        List<InventoryItem> items = new List<InventoryItem>();
                        List<InventoryBase> contents = bot.GetClient.Inventory.FolderContents(AA.UUID, bot.GetClient.Self.AgentID, true, true, InventorySortOrder.ByName, 20 * 1000);
                        foreach (InventoryBase item in contents)
                        {
                            if ((item is InventoryWearable) || (item is InventoryObject))
                            {
                                if (item.Name == args[1])
                                {
                                    items.Add((InventoryItem)item);
                                    break;
                                }
                            }
                        }
                        if (items.Count() > 0)
                        {
                            bot.GetClient.Appearance.AddAttachments(items, false, false);
                            return true;
                        }
                        else
                        {
                            return Failed("Unable to find attachment: " + args[1] + " in sub folder " + args[0] + "");
                        }
                    }
                    else
                    {
                        return Failed("Unable to find subfolder " + args[0] + "");
                    }
                }
                else
                {
                    return Failed("Unable to find attachements folder");
                }
            }
            return false;
        }
    }
}
