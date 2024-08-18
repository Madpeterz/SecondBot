using Newtonsoft.Json;
using OpenMetaverse;
using OpenMetaverse.ImportExport.Collada14;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.Static
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
        public static InventoryItem getItemByInventoryUUID(GridClient bot, UUID target)
        {
            return bot.Inventory.FetchItem(target, bot.Self.AgentID, 3000);
        }
        public static UUID GetAssetUUID(GridClient bot, UUID target)
        {
            InventoryItem A = getItemByInventoryUUID(bot, target);
            if (A != null)
            {
                return A.AssetUUID;
            }
            return UUID.Zero;
        }

        public static InventoryFolder FindFolderByPath(GridClient bot, string path)
        {
            return DigFolderForPath(bot, path, 0, bot.Inventory.Store.RootFolder.UUID);
        }

        private static InventoryFolder DigFolderForPath(GridClient bot, string path, int level, UUID folder)
        {
            string[] bits = path.Split('/');
            List<InventoryBase> T = bot.Inventory.Store.GetContents(folder);
            InventoryFolder foundFolder = null;
            bool found = false;
            foreach (InventoryBase t in T)
            {
                if (t.GetType() == typeof(InventoryFolder))
                {
                    if (t.Name == bits[level])
                    {
                        folder = t.UUID;
                        if (bits.Length > (level+1))
                        {
                            // need to go deeper
                            foundFolder = DigFolderForPath(bot, path, level + 1, folder);
                            if (foundFolder != null)
                            {
                                found = true;
                            }
                        }
                        else
                        {
                            // we are here
                            foundFolder = (InventoryFolder)t;
                            found = true;
                            break;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
            }
            if(found == true)
            { 
                return foundFolder;
            }
            else
            {
                return null;
            }
        }
        public static InventoryBase FindFolder(GridClient bot, InventoryFolder current,UUID target)
        {
            List<InventoryBase> T = bot.Inventory.Store.GetContents(current);
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

        public static List<InventoryMapItem> getfolderinventory(GridClient bot, UUID folder)
        {
            List<InventoryMapItem> entrys = new List<InventoryMapItem>();
            InventoryNode node = bot.Inventory.Store.GetNodeFor(folder);
            InventoryMapItem mapitem = new InventoryMapItem();
            mapitem.name = node.Data.Name;
            mapitem.id = node.Data.UUID.ToString();
            mapitem.typename = "Folder";
            entrys.Add(mapitem);
            List<InventoryBase> contents = bot.Inventory.FolderContents(folder, bot.Self.AgentID, true, true, InventorySortOrder.ByDate, 40 * 1000, true);
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
        public static string MapFolderInventoryJson(GridClient bot,UUID folder)
        {
            return JsonConvert.SerializeObject(getfolderinventory(bot, folder));
        }

        public static KeyValuePair<string,string> MapFolderInventoryHumanReadable(GridClient bot, UUID folder)
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

        public static string MapFolderJson(GridClient bot,string targetFolder="root", bool subfolders = true)
        {
            if (bot.Inventory.Store != null)
            {
                if (bot.Inventory.Store.RootFolder != null)
                {
                    InventoryFolder topLevel = null;
                    if (targetFolder == "root")
                    {
                        topLevel = bot.Inventory.Store.RootFolder;
                    }
                    else
                    {
                        if(UUID.TryParse(targetFolder, out UUID folderUUID) == true)
                        {
                            topLevel = new InventoryFolder(folderUUID);
                        }
                    }
                    if (topLevel != null)
                    {
                        return JsonConvert.SerializeObject(DoMapFolderJson(bot, topLevel, subfolders));
                    }
                }
            }
            return null;
        }

        public static InventoryMapFolder DoMapFolderJson(GridClient bot, InventoryFolder folder, bool subfolders=true)
        {
            InventoryMapFolder ReplyFolder = new InventoryMapFolder();
            ReplyFolder.id = folder.UUID.ToString();
            ReplyFolder.name = folder.Name;
            Dictionary<string, string> foldernode = new Dictionary<string, string>();
            List<InventoryBase> T = bot.Inventory.Store.GetContents(folder);
            foreach (InventoryBase R in T)
            {
                if (R.GetType() == typeof(InventoryFolder))
                {
                    if (subfolders == true)
                    {
                        ReplyFolder.subfolders.Add(DoMapFolderJson(bot, (InventoryFolder)R, subfolders));
                    }
                }
            }
            return ReplyFolder;
        }



        public static string MapFolderHumanReadable(GridClient bot)
        {
            StringBuilder B = new StringBuilder();
            DoMapFolderHumanReadable(bot, 0, bot.Inventory.Store.RootFolder, B);
            return B.ToString();
        }
        public static void DoMapFolderHumanReadable(GridClient bot,int level, InventoryFolder folder, StringBuilder reply)
        {
            List<InventoryBase> T = bot.Inventory.Store.GetContents(folder);
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
