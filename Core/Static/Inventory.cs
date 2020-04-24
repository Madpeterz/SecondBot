using BSB.bottypes;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Static
{
    public static class HelperInventory
    {
        public static InventoryBase FindFolder(CommandsBot bot, InventoryFolder current,UUID target)
        {
            List<InventoryBase> T = bot.GetClient.Inventory.Store.GetContents(current);
            InventoryBase Found = null;
            foreach (InventoryBase R in T)
            {
                if(R.UUID == target)
                {
                    Found = R;
                    break;
                }
                else
                {
                    InventoryBase FoundSub = FindFolder(bot, (InventoryFolder)R, target);
                    if(FoundSub != null)
                    {
                        Found = FoundSub;
                        break;
                    }
                }
            }
            return Found;
        }
        public static string[] MapFolderInventory(CommandsBot bot,UUID folder)
        {
            List<string> entrys = new List<string>();
            entrys.Add("folderinventory=" + folder.ToString() + "");
            List<InventoryBase> contents = bot.GetClient.Inventory.FolderContents(folder, bot.GetClient.Self.AgentID, true, true, InventorySortOrder.ByDate, 40 * 1000);
            if (contents != null)
            {
                if (contents.Count > 0)
                {
                    StringBuilder B = new StringBuilder();
                    foreach (InventoryBase item in contents)
                    {
                        B.Append("|||");
                        B.Append(item.Name);
                        B.Append("###");
                        B.Append(item.UUID.ToString());
                        B.Append("###");
                        B.Append(item.GetType().Name);
                    }
                    entrys.Add(B.ToString());
                }
            }
            return entrys.ToArray();
        }
        public static string MapFolder(CommandsBot bot, bool as_csv)
        {
            StringBuilder B = new StringBuilder();
            DoMapFolder(bot, 0, bot.GetClient.Inventory.Store.RootFolder, as_csv, "", B);
            return B.ToString();
        }
        public static void DoMapFolder(CommandsBot bot,int level, InventoryFolder folder,bool as_csv,string addon, StringBuilder reply)
        {
            List<InventoryBase> T = bot.GetClient.Inventory.Store.GetContents(folder);
            foreach (InventoryBase R in T)
            {
                if (as_csv == false)
                {
                    reply.Append("" + Spaces(level) + "|- " + R.Name + " [" + R.UUID.ToString() + "]\n\r");
                }
                else
                {
                    reply.Append(addon);
                    reply.Append(R.Name);
                    reply.Append("###");
                    reply.Append(level.ToString());
                    reply.Append("###");
                    reply.Append(R.UUID.ToString());
                    addon = ",";
                }
                DoMapFolder(bot, level + 1, (InventoryFolder)R, as_csv, addon, reply);
            }
        }
        public static string Spaces(int counter)
        {
            string reply = "";
            while (counter > 0)
            {
                reply += "      ";
                counter--;
            }
            return reply;
        }
    }
}
