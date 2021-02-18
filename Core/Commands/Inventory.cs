using BetterSecondBot.bottypes;
using BetterSecondBot.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using System.Linq;
using BetterSecondBotShared.Static;

namespace BetterSecondBot.HttpService
{
    public class HTTP_Inventory : WebApiControllerWithTokens
    {
        public HTTP_Inventory(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("rezs the item at the bots current location")]
        [ReturnHints("True|False")]
        [ArgHints("item", "URLARG", "UUID of item to rez")]
        [Route(HttpVerbs.Get, "/RezObject/{item}/{token}")]
        public object RezObject(string item, string token)
        {
            if (tokens.Allow(token, "inventory", "RezObject", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            return Failure("Broken awaiting repairs");
        }


        [About("renames a folder or inventory item")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")]
        [ReturnHints("Item name is to short")]
        [ReturnHints("Unable to find inventory item")]
        [ArgHints("item", "URLARG", "UUID of item/folder to name")]
        [ArgHints("newname", "Text", "What we are changing it to")]
        [Route(HttpVerbs.Post, "/RenameInventory/{item}/{token}")]
        public object RenameInventory(string item, string token, [FormField] string newname)
        {
            if (tokens.Allow(token, "inventory", "RenameInventory", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID target) == false)
            {
                return Failure("invaild item uuid");
            }
            if (newname.Length < 3)
            {
                return Failure("Item name is to short");
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(bot, target);
            if (realitem == null)
            {
                return Failure("Unable to find inventory item");
            }
            realitem.Name = newname;
            bot.GetClient.Inventory.RequestUpdateItem(realitem);
            return BasicReply("Ok");
        }

        [About("Attempts to Remove the given inventory item")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")]
        [ArgHints("item", "URLARG", "UUID of item")]
        [Route(HttpVerbs.Get, "/DeleteInventoryItem/{item}/{token}")]
        public object DeleteInventoryItem(string item, string token)
        {
            if (tokens.Allow(token, "inventory", "DeleteInventoryItem", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID target) == false)
            {
                return Failure("invaild item uuid");
            }
            bot.GetClient.Inventory.Remove(new List<UUID>() { target }, new List<UUID>());
            return BasicReply("Ok");
        }

        [About("Attempts to Remove the given inventory folder")]
        [ReturnHints("ok")]
        [ReturnHints("invaild folder uuid")]
        [ArgHints("folder", "URLARG", "UUID of folder")]
        [Route(HttpVerbs.Get, "/DeleteInventoryFolder/{folder}/{token}")]
        public object DeleteInventoryFolder(string folder, string token)
        {
            if (tokens.Allow(token, "inventory", "DeleteInventoryFolder", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(folder, out UUID target) == false)
            {
                return Failure("invaild folder uuid");
            }
            bot.GetClient.Inventory.Remove(new List<UUID>(), new List<UUID>() { target });
            return BasicReply("Ok");
        }

        [About("Attempts to attach the given inventory item")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")] 
        [ArgHints("item", "URLARG", "UUID of item")]
        [Route(HttpVerbs.Get, "/Attach/{item}/{token}")]
        public object Attach(string item, string token)
        {
            if (tokens.Allow(token, "inventory", "Attach", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if(UUID.TryParse(item,out UUID itemuuid) == false)
            {
                return Failure("invaild item uuid");
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(bot, itemuuid);
            bot.GetClient.Appearance.AddAttachments(new List<InventoryItem>() { realitem }, false, false);
            return BasicReply("Ok");
        }

        [About("Attempts to Remove the given inventory folder")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")]
        [ArgHints("item", "URLARG", "UUID of item")]
        [Route(HttpVerbs.Get, "/Detach/{item}/{token}")]
        public object Detach(string item, string token)
        {
            if (tokens.Allow(token, "inventory", "Detach", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID itemuuid) == false)
            {
                return Failure("invaild item uuid");
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(bot, itemuuid);
            bot.GetClient.Appearance.RemoveFromOutfit(realitem);
            return BasicReply("Ok");
        }

        [About("Replaces the current avatar outfit with the Clothing/[NAME] folder<br/>Please note: This does not use the outfits folder!<br/>Please do not use links in the folder!")]
        [ReturnHints("ok")]
        [ReturnHints("Named folder value is empty")]
        [ReturnHints("Cant find Clothing folder")]
        [ReturnHints("Cant find target folder")]
        [ReturnHints("target folder is empty or so full I cant get it in 5 secs...")]
        [ArgHints("name", "URLARG", "Name of the folder")]
        [Route(HttpVerbs.Get, "/Outfit/{name}/{token}")]
        public object Outfit(string name, string token)
        {
            if (tokens.Allow(token, "inventory", "Outfit", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (helpers.notempty(name) == false)
            {
                return Failure("Named folder value is empty");
            }
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
            if (AA == null)
            {
                return Failure("Cant find Clothing folder");
            }
            T = bot.GetClient.Inventory.Store.GetContents(AA);
            AA = null;
            foreach (InventoryBase R in T)
            {
                if (R.Name == name)
                {
                    AA = (InventoryFolder)R;
                    break;
                }
            }
            if (AA == null)
            {
                return Failure("Cant find target folder");
            }
            List<InventoryBase> contents = bot.GetClient.Inventory.FolderContents(AA.UUID, bot.GetClient.Self.AgentID, true, true, InventorySortOrder.ByName, 5 * 1000);
            List<InventoryItem> wareables = new List<InventoryItem>();
            if (contents == null)
            {
                return Failure("target folder is empty or so full I cant get it in 5 secs...");
            }
            foreach (InventoryBase item in contents)
            {
                if ((item is InventoryWearable) || (item is InventoryObject))
                {
                    wareables.Add((InventoryItem)item);
                }
            }
            bot.GetClient.Appearance.ReplaceOutfit(wareables, false);
            return BasicReply("ok");
        }



        [About("Searchs the notecards folder for notecards, any older than 31 days are deleted.<br/>Depending on the number of notecards this might require multiple calls!")]
        [ReturnHints("ok")]
        [ReturnHints("Unable to find notecard folder")]
        [Route(HttpVerbs.Get, "/InventoryPurgeNotecards/{token}")]
        public object InventoryPurgeNotecards(string token)
        {
            if (tokens.Allow(token, "inventory", "InventoryPurgeNotecards", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            List<InventoryBase> T = bot.GetClient.Inventory.Store.GetContents(bot.GetClient.Inventory.Store.RootFolder);
            InventoryBase NotecardFolder = null;
            foreach (InventoryBase R in T)
            {
                if (R.Name == "Notecards")
                {
                    NotecardFolder = R;
                    break;
                }
            }
            if (NotecardFolder != null)
            {
                return Failure("Unable to find notecard folder");
            }
            List<UUID> purge_notecards = new List<UUID>();
            List<InventoryBase> contents = bot.GetClient.Inventory.FolderContents(NotecardFolder.UUID, bot.GetClient.Self.AgentID, true, true, InventorySortOrder.ByDate, 40 * 1000);
            foreach (InventoryBase C in contents)
            {
                InventoryItem A = (InventoryItem)C;
                if (A.AssetType == AssetType.Notecard)
                {
                    TimeSpan Dif = DateTime.Now - A.CreationDate;
                    if (Dif.Days >= 31)
                    {
                        purge_notecards.Add(C.UUID);
                    }
                }
            }
            if (purge_notecards.Count() > 0)
            {
                bot.GetClient.Inventory.Remove(purge_notecards, new List<UUID>());
            }
            return BasicReply("Ok");
        }

        [About("converts a inventory uuid to a realworld uuid<br/>Needed for texture preview")]
        [ReturnHints("Asset UUID or UUID zero")]
        [ReturnHints("Invaild item uuid")]
        [ArgHints("item", "URLARG", "inventory level UUID of item")]
        [Route(HttpVerbs.Get, "/getRealUUID/{item}/{token}")]
        public object getRealUUID(string item, string token)
        {
            if (tokens.Allow(token, "inventory", "getRealUUID", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID itemUUID) == false)
            {
                return Failure("Invaild item uuid");
            }
            UUID reply = HelperInventory.GetAssetUUID(bot, itemUUID);
            return BasicReply(reply.ToString());
        }

        [About("sends a item to an avatar")]
        [ReturnHints("ok")]
        [ReturnHints("Failed")]
        [ReturnHints("Invaild avatar uuid")]
        [ReturnHints("Invaild item uuid")]
        [ReturnHints("Unable to find item")]
        [ArgHints("item", "URLARG", "UUID of item")]
        [ArgHints("avatar", "URLARG", "a UUID or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/SendItem/{item}/{avatar}/{token}")]
        public object SendItem(string item, string avatar, string token)
        {
            if (tokens.Allow(token, "inventory", "SendItem", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid");
            }
            if (UUID.TryParse(item, out UUID targetitem) == true)
            {
                return Failure("Invaild item uuid");
            }
            InventoryItem itm = bot.GetClient.Inventory.FetchItem(targetitem, bot.GetClient.Self.AgentID, (3 * 1000));
            if (itm == null)
            {
                return Failure("Unable to find item");
            }
            bot.GetClient.Inventory.GiveItem(itm.UUID, itm.Name, itm.AssetType, avataruuid, false);
            return BasicReply("ok");
        }

        [About("Sends a folder to an avatar")]
        [ReturnHints("ok")]
        [ReturnHints("Failed")]
        [ReturnHints("Invaild avatar uuid")]
        [ReturnHints("Invaild folter uuid")]
        [ReturnHints("Unable to find folder")]
        [ArgHints("item", "URLARG", "UUID of item")]
        [ArgHints("avatar", "URLARG", "a UUID or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/SendFolder/{folder}/{avatar}/{token}")]
        public object SendFolder(string folder, string avatar, string token)
        {
            if (tokens.Allow(token, "inventory", "SendFolder", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid");
            }
            if (UUID.TryParse(folder, out UUID targetfolder) == true)
            {
                return Failure("Invaild folder uuid");
            }
            InventoryBase FindFolderHelper = HelperInventory.FindFolder(bot, bot.GetClient.Inventory.Store.RootFolder, targetfolder);
            if (FindFolderHelper == null)
            {
                return Failure("Unable to find folder");
            }
            bot.GetClient.Inventory.GiveFolder(FindFolderHelper.UUID, FindFolderHelper.Name, avataruuid, false);
            return BasicReply("ok");
        }



        [About("Transfers a item [ARG 2] to a objects inventory [ARG 1] (And if set with the script running state [ARG 3])")]
        [ReturnHints("Transfering running script")]
        [ReturnHints("Transfering inventory")]
        [ReturnHints("Invaild item uuid")]
        [ReturnHints("Invaild object uuid")]
        [ReturnHints("Unable to find inventory")]
        [ReturnHints("Unable to find object")]
        [ReturnHints("Invaild running")]
        [ArgHints("item", "URLARG", "UUID of item")]
        [ArgHints("object", "URLARG", "the uuid of the object")]
        [ArgHints("running", "URLARG", "true if you wish the transfered script to be running otherwise false")]

        [Route(HttpVerbs.Get, "/TransferInventoryToObject/{item}/{objectuuid}/{running}/{token}")]
        public object TransferInventoryToObject(string item, string objectuuid, string running, string token)
        {
            if (tokens.Allow(token, "inventory", "TransferInventoryToObject", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID itemuuid) == false)
            {
                return Failure("Invaild item uuid");
            }
            if (UUID.TryParse(objectuuid, out UUID objectUUID) == false)
            {
                return Failure("Invaild object uuid");
            }
            if (bool.TryParse(running, out bool runscript) == false)
            {
                return Failure("Invaild running");
            }

            InventoryItem itm = bot.GetClient.Inventory.FetchItem(itemuuid, bot.GetClient.Self.AgentID, (3 * 1000));
            if (itm == null)
            {
                return Failure("Unable to find inventory");
            }
            Dictionary<uint, Primitive> objects_copy = bot.GetClient.Network.CurrentSim.ObjectsPrimitives.Copy();
            KeyValuePair<uint, Primitive> RealObject = new KeyValuePair<uint, Primitive>(0,null);
            foreach (KeyValuePair<uint, Primitive> Obj in objects_copy)
            {
                if (Obj.Value.ID == objectUUID)
                {
                    RealObject = Obj;
                    break;
                }
            }
            if (RealObject.Value == null)
            {
                return Failure("Unable to find object");
            }
            bool scriptState = runscript;
            if (itm.AssetType != AssetType.LSLText)
            {
                scriptState = false;
            }
            if (itm.AssetType == AssetType.LSLText)
            {
                bot.GetClient.Inventory.CopyScriptToTask(RealObject.Key, itm, scriptState);
                return BasicReply("Transfering script [state: "+scriptState.ToString()+"]");
            }
            bot.GetClient.Inventory.UpdateTaskInventory(RealObject.Key, itm);
            return BasicReply("Transfering inventory");
        }

        [About("Requests the inventory folder layout as a json object InventoryMapFolder<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>subfolders: InventoryMapFolder[]</li></ul>")]
        [ReturnHints("array of InventoryMapFolder")]
        [ReturnHints("Error")]
        [Route(HttpVerbs.Get, "/InventoryFolders/{token}")]
        public object InventoryFolders(string token)
        {
            if (tokens.Allow(token, "inventory", "InventoryFolders", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            string reply = HelperInventory.MapFolderJson(bot);
            if (reply != null) return BasicReply(reply);
            return Failure("Error");
        }

        [About("Requests the contents of a folder as an array of InventoryMapItem<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>typename: String</li></ul>")]
        [ReturnHints("array of InventoryMapItem")]
        [ReturnHints("Invaild folder UUID")]
        [ArgHints("folderUUID", "URLARG", "the folder to fetch (Found via: inventory/folders)")]
        [Route(HttpVerbs.Get, "/InventoryContents/{folderUUID}/{token}")]
        public object InventoryContents(string folderUUID, string token)
        {
            if (tokens.Allow(token, "inventory", "InventoryContents", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(folderUUID, out UUID folder) == false)
            {
                return Failure("Invaild folder UUID");
            }
            return BasicReply(HelperInventory.MapFolderInventoryJson(bot, folder));
        }
    }
}
