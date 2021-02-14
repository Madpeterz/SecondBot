using BSB.bottypes;
using Newtonsoft.Json;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Static
{
    public class InventoryMapFolder
    {
        public string id;
        public string name;
        public List<InventoryMapFolder> subfolders = new List<InventoryMapFolder>();
    }
    public class InventoryMapItem
    {
        public string id;
        public string name;
        public string typename;
    }

    public static class HelperInventory
    {
        public static InventoryItem getItemByInventoryUUID(CommandsBot bot, UUID target)
        {
            return bot.GetClient.Inventory.FetchItem(target, bot.GetClient.Self.AgentID, 3000);
        }
        public static UUID GetAssetUUID(CommandsBot bot, UUID target)
        {
            InventoryItem A = getItemByInventoryUUID(bot, target);
            if (A != null)
            {
                return A.AssetUUID;
            }
            return UUID.Zero;
        }
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

        public static List<InventoryMapItem> getfolderinventory(CommandsBot bot, UUID folder)
        {
            List<InventoryMapItem> entrys = new List<InventoryMapItem>();
            InventoryNode node = bot.GetClient.Inventory.Store.GetNodeFor(folder);
            InventoryMapItem mapitem = new InventoryMapItem();
            mapitem.name = node.Data.Name;
            mapitem.id = node.Data.UUID.ToString();
            mapitem.typename = "Folder";
            entrys.Add(mapitem);
            List<InventoryBase> contents = bot.GetClient.Inventory.FolderContents(folder, bot.GetClient.Self.AgentID, true, true, InventorySortOrder.ByDate, 40 * 1000, true);
            if (contents != null)
            {
                if (contents.Count > 0)
                {
                    foreach (InventoryBase item in contents)
                    {
                        mapitem = new InventoryMapItem();
                        mapitem.name = item.Name;
                        mapitem.id = item.UUID.ToString();
                        mapitem.typename = item.GetType().Name;
                        entrys.Add(mapitem);
                    }
                }
            }
            return entrys;
        }
        public static string MapFolderInventoryJson(CommandsBot bot,UUID folder)
        {
            return JsonConvert.SerializeObject(getfolderinventory(bot, folder));
        }

        public static KeyValuePair<string,string> MapFolderInventoryHumanReadable(CommandsBot bot, UUID folder)
        {
            StringBuilder output = new StringBuilder();
            string foldername = "Unknown";
            foreach(InventoryMapItem mitem in getfolderinventory(bot, folder))
            {
                if(mitem.typename == "Folder")
                {
                    foldername = mitem.name;
                }
                output.Append("ID: ");
                output.Append(mitem.id);
                output.Append(" | ");
                output.Append("Name: ");
                output.Append(mitem.name);
                output.Append(" | ");
                output.Append("Type: ");
                output.Append(mitem.typename);
                output.Append("\n\r");
            }
            return new KeyValuePair<string,string>(foldername,output.ToString());
        }

        public static string MapFolderJson(CommandsBot bot)
        {
            if (bot.GetClient.Inventory.Store != null)
            {
                if (bot.GetClient.Inventory.Store.RootFolder != null)
                {
                    return JsonConvert.SerializeObject(DoMapFolderJson(bot, bot.GetClient.Inventory.Store.RootFolder));
                }
            }
            return null;
        }

        public static InventoryMapFolder DoMapFolderJson(CommandsBot bot, InventoryFolder folder)
        {
            InventoryMapFolder ReplyFolder = new InventoryMapFolder();
            ReplyFolder.id = folder.UUID.ToString();
            ReplyFolder.name = folder.Name;
            Dictionary<string, string> foldernode = new Dictionary<string, string>();
            List<InventoryBase> T = bot.GetClient.Inventory.Store.GetContents(folder);
            foreach (InventoryBase R in T)
            {
                if (R.GetType() == typeof(InventoryFolder))
                {
                    ReplyFolder.subfolders.Add(DoMapFolderJson(bot, (InventoryFolder)R));
                }
            }
            return ReplyFolder;
        }



        public static string MapFolderHumanReadable(CommandsBot bot)
        {
            StringBuilder B = new StringBuilder();
            DoMapFolderHumanReadable(bot, 0, bot.GetClient.Inventory.Store.RootFolder, B);
            return B.ToString();
        }
        public static void DoMapFolderHumanReadable(CommandsBot bot,int level, InventoryFolder folder, StringBuilder reply)
        {
            List<InventoryBase> T = bot.GetClient.Inventory.Store.GetContents(folder);
            foreach (InventoryBase R in T)
            {
                if (R.GetType() == typeof(InventoryFolder))
                {
                    reply.Append("" + Spaces(level) + "|- " + R.Name + " [" + R.UUID.ToString() + "]\n\r");
                    DoMapFolderHumanReadable(bot, level + 1, (InventoryFolder)R, reply);
                }
            }
        }
        public static string Spaces(int counter)
        {
            StringBuilder reply = new StringBuilder();
            while (counter > 0)
            {
                reply.Append("      ");
                counter--;
            }
            return reply.ToString();
        }
    }
}
