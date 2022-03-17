using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using OpenMetaverse;
using OpenMetaverse.Assets;
using SecondBotEvents.Services;
using System.Collections.Generic;
using System.IO;

namespace SecondBotEvents.Commands
{
    public class Inventory : CommandsAPI
    {
        public Inventory(EventsSecondBot setmaster) : base(setmaster)
        {
        }

        [About("Attachs an event for inventory changes")]
        [ReturnHints("cleared")]
        [ReturnHints("No action")]
        [ReturnHints("Event added")]
        [ArgHints("inventoryType", "URLARG", "Types: texture,sound,callcard,landmark,clothing,object,notecard,lsltext,lslbyte,animatn,gesture,mesh")]
        [ArgHints("outputTarget", "text", "HTTP url, channel, avatar UUID or clear to remove all events for the selected type")]
        [Route(HttpVerbs.Post, "/SetInventoryUpdate/{inventoryType}/{token}")]
        public object SetInventoryUpdate(string inventoryType, [FormField] string outputTarget, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            return BasicReply(bot.addInventoryUpdateEvent(inventoryType, outputTarget), "SetInventoryUpdate");
        }

        [About("Uploads a new sound file to inventory")]
        [ReturnHints("ok")]
        [ArgHints("sourcePath", "Text", "accepts a file path to a wave PCM file @ 44100")]
        [ArgHints("inventoryName", "URLARG", "the name in secondlife")]
        [Route(HttpVerbs.Post, "/UploadMediaWave/{inventoryName}/{token}")]
        public object UploadMediaWave([FormField] string sourcePath, string inventoryName, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            byte[] audioData = File.ReadAllBytes(sourcePath);
            InventoryFolder AA = getClient().Inventory.Store.RootFolder;
            List<InventoryBase> T = getClient().Inventory.Store.GetContents(AA);
            AA = null;
            foreach (InventoryBase R in T)
            {
                if (R.Name == "Sounds")
                {
                    AA = (InventoryFolder)R;
                    break;
                }
            }
            AssetSound S = new AssetSound();
            S.AssetData = audioData;
            S.Encode();

            getClient().Inventory.RequestCreateItemFromAsset(S.AssetData, inventoryName, "", AssetType.Sound, InventoryType.Sound, AA.UUID, null);
            return BasicReply("ok", "UploadMediaWave");
        }


        [About("rezs the item at the bots current location")]
        [ReturnHints("UUID of rezzed item")]
        [ReturnHints("Invaild item UUID")]
        [ReturnHints("Unable to find item")]
        [ArgHints("item", "URLARG", "UUID of item to rez")]
        [Route(HttpVerbs.Get, "/RezObject/{item}/{token}")]
        public object RezObject(string item, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID targetitem) == false)
            {
                return Failure("Invaild item UUID: "+ item, "RezObject", new [] { item });
            }
            InventoryItem itm = getClient().Inventory.FetchItem(targetitem, getClient().Self.AgentID, (5 * 1000));
            if (itm == null)
            {
                return Failure("Unable to find item: " + item, "RezObject", new [] { item });
            }
            UUID rezedobject = getClient().Inventory.RequestRezFromInventory(getClient().Network.CurrentSim, getClient().Self.SimRotation, getClient().Self.RelativePosition, itm);
            return BasicReply(rezedobject.ToString(), "RezObject", new [] { item });
        }


        [About("renames a folder or inventory item")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")]
        [ReturnHints("Item name is to short")]
        [ReturnHints("Unable to find inventory item")]
        [ArgHints("item", "URLARG", "UUID of item/folder to name")]
        [ArgHints("newname", "Text", "What we are changing it to")]
        [Route(HttpVerbs.Post, "/RenameInventory/{item}/{token}")]
        public object RenameInventory(string item, [FormField] string newname, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID target) == false)
            {
                return Failure("invaild item uuid", "RenameInventory", new [] { item, newname });
            }
            if (newname.Length < 3)
            {
                return Failure("Item name is to short", "RenameInventory", new [] { item, newname });
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(bot, target);
            if (realitem == null)
            {
                return Failure("Unable to find inventory item", "RenameInventory", new [] { item, newname });
            }
            realitem.Name = newname;
            getClient().Inventory.RequestUpdateItem(realitem);
            return BasicReply("Ok", "RenameInventory", new [] { item, newname });
        }

        [About("Attempts to Remove the given inventory item")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")]
        [ArgHints("item", "URLARG", "UUID of item")]
        [Route(HttpVerbs.Get, "/DeleteInventoryItem/{item}/{token}")]
        public object DeleteInventoryItem(string item, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID target) == false)
            {
                return Failure("invaild item uuid", "DeleteInventoryItem", new [] { item });
            }
            getClient().Inventory.Remove(new List<UUID>() { target }, new List<UUID>());
            return BasicReply("Ok", "DeleteInventoryItem", new [] { item });
        }

        [About("Attempts to Remove the given inventory folder")]
        [ReturnHints("ok")]
        [ReturnHints("invaild folder uuid")]
        [ArgHints("folder", "URLARG", "UUID of folder")]
        [Route(HttpVerbs.Get, "/DeleteInventoryFolder/{folder}/{token}")]
        public object DeleteInventoryFolder(string folder, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(folder, out UUID target) == false)
            {
                return Failure("invaild folder uuid", "DeleteInventoryFolder", new [] { folder });
            }
            getClient().Inventory.Remove(new List<UUID>(), new List<UUID>() { target });
            return BasicReply("Ok", "DeleteInventoryFolder", new [] { folder });
        }

        [About("Attempts to attach the given inventory item")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")] 
        [ArgHints("item", "URLARG", "UUID of item")]
        [Route(HttpVerbs.Get, "/Attach/{item}/{token}")]
        public object Attach(string item, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item,out UUID itemuuid) == false)
            {
                return Failure("invaild item uuid", "Attach", new [] { item });
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(bot, itemuuid);
            getClient().Appearance.AddAttachments(new List<InventoryItem>() { realitem }, false, false);
            return BasicReply("Ok", "Attach", new [] { item });
        }

        [About("Attempts to Remove the given inventory folder")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")]
        [ArgHints("item", "URLARG", "UUID of item")]
        [Route(HttpVerbs.Get, "/Detach/{item}/{token}")]
        public object Detach(string item, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID itemuuid) == false)
            {
                return Failure("invaild item uuid", "Detach", new [] { item });
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(bot, itemuuid);
            getClient().Appearance.RemoveFromOutfit(realitem);
            return BasicReply("Ok", "Detach", new [] { item });
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
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (SecondbotHelpers.notempty(name) == false)
            {
                return Failure("Named folder value is empty", "Outfit", new [] { name });
            }
            // uses the Clothing folder
            // must be a full outfit (shapes/eyes ect)
            InventoryFolder AA = getClient().Inventory.Store.RootFolder;
            List<InventoryBase> T = getClient().Inventory.Store.GetContents(AA);
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
                return Failure("Cant find Clothing folder", "Outfit", new [] { name });
            }
            T = getClient().Inventory.Store.GetContents(AA);
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
                return Failure("Cant find target folder", "Outfit", new [] { name });
            }
            List<InventoryBase> contents = getClient().Inventory.FolderContents(AA.UUID, getClient().Self.AgentID, true, true, InventorySortOrder.ByName, 5 * 1000);
            List<InventoryItem> wareables = new List<InventoryItem>();
            if (contents == null)
            {
                return Failure("target folder is empty or so full I cant get it in 5 secs...", "Outfit", new [] { name });
            }
            foreach (InventoryBase item in contents)
            {
                if ((item is InventoryWearable) || (item is InventoryObject))
                {
                    wareables.Add((InventoryItem)item);
                }
            }
            getClient().Appearance.ReplaceOutfit(wareables, false);
            return BasicReply("ok", "Outfit", new [] { name });
        }



        [About("Searchs the notecards folder for notecards, any older than 31 days are deleted.<br/>Depending on the number of notecards this might require multiple calls!")]
        [ReturnHints("ok")]
        [ReturnHints("Unable to find notecard folder")]
        [Route(HttpVerbs.Get, "/InventoryPurgeNotecards/{token}")]
        public object InventoryPurgeNotecards(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            List<InventoryBase> T = getClient().Inventory.Store.GetContents(getClient().Inventory.Store.RootFolder);
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
                return Failure("Unable to find notecard folder", "InventoryPurgeNotecards");
            }
            List<UUID> purge_notecards = new List<UUID>();
            List<InventoryBase> contents = getClient().Inventory.FolderContents(NotecardFolder.UUID, getClient().Self.AgentID, true, true, InventorySortOrder.ByDate, 40 * 1000);
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
                getClient().Inventory.Remove(purge_notecards, new List<UUID>());
            }
            return BasicReply("Ok", "InventoryPurgeNotecards");
        }

        [About("converts a inventory uuid to a realworld uuid<br/>Needed for texture preview")]
        [ReturnHints("Asset UUID or UUID zero")]
        [ReturnHints("Invaild item uuid")]
        [ArgHints("item", "URLARG", "inventory level UUID of item")]
        [Route(HttpVerbs.Get, "/getRealUUID/{item}/{token}")]
        public object getRealUUID(string item, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID itemUUID) == false)
            {
                return Failure("Invaild item uuid", "getRealUUID", new [] { item });
            }
            UUID reply = HelperInventory.GetAssetUUID(bot, itemUUID);
            return BasicReply(reply.ToString(), "getRealUUID", new [] { item });
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
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", "SendItem", new [] { item, avatar });
            }
            if (UUID.TryParse(item, out UUID targetitem) == false)
            {
                return Failure("Invaild item uuid", "SendItem", new [] { item, avatar });
            }
            InventoryItem itm = getClient().Inventory.FetchItem(targetitem, getClient().Self.AgentID, (3 * 1000));
            if (itm == null)
            {
                return Failure("Unable to find item", "SendItem", new [] { item, avatar });
            }
            getClient().Inventory.GiveItem(itm.UUID, itm.Name, itm.AssetType, avataruuid, false);
            return BasicReply("ok", "SendItem", new [] { item, avatar });
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
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", "SendFolder", new [] { folder, avatar });
            }
            if (UUID.TryParse(folder, out UUID targetfolder) == true)
            {
                return Failure("Invaild folder uuid", "SendFolder", new [] { folder, avatar });
            }
            InventoryBase FindFolderHelper = HelperInventory.FindFolder(bot, getClient().Inventory.Store.RootFolder, targetfolder);
            if (FindFolderHelper == null)
            {
                return Failure("Unable to find folder", "SendFolder", new [] { folder, avatar });
            }
            getClient().Inventory.GiveFolder(FindFolderHelper.UUID, FindFolderHelper.Name, avataruuid, false);
            return BasicReply("ok", "SendFolder", new [] { folder, avatar });
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
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(item, out UUID itemuuid) == false)
            {
                return Failure("Invaild item uuid", "TransferInventoryToObject", new [] { item, objectuuid, running });
            }
            if (UUID.TryParse(objectuuid, out UUID objectUUID) == false)
            {
                return Failure("Invaild object uuid", "TransferInventoryToObject", new [] { item, objectuuid, running });
            }
            if (bool.TryParse(running, out bool runscript) == false)
            {
                return Failure("Invaild running", "TransferInventoryToObject", new [] { item, objectuuid, running });
            }

            InventoryItem itm = getClient().Inventory.FetchItem(itemuuid, getClient().Self.AgentID, (3 * 1000));
            if (itm == null)
            {
                return Failure("Unable to find inventory", "TransferInventoryToObject", new [] { item, objectuuid, running });
            }
            Dictionary<uint, Primitive> objects_copy = getClient().Network.CurrentSim.ObjectsPrimitives.Copy();
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
                return Failure("Unable to find object", "TransferInventoryToObject", new [] { item, objectuuid, running });
            }
            bool scriptState = runscript;
            if (itm.AssetType != AssetType.LSLText)
            {
                scriptState = false;
            }
            if (itm.AssetType == AssetType.LSLText)
            {
                getClient().Inventory.CopyScriptToTask(RealObject.Key, itm, scriptState);
                return BasicReply("Transfering script [state: "+scriptState.ToString()+"]", "TransferInventoryToObject", new [] { item, objectuuid, running });
            }
            getClient().Inventory.UpdateTaskInventory(RealObject.Key, itm);
            return BasicReply("Transfering inventory", "TransferInventoryToObject", new [] { item, objectuuid, running });
        }

        [About("Requests the inventory folder layout as a json object InventoryMapFolder<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>subfolders: InventoryMapFolder[]</li></ul>")]
        [ReturnHints("array of InventoryMapFolder")]
        [ReturnHints("Error")]
        [Route(HttpVerbs.Get, "/InventoryFolders/{token}")]
        public object InventoryFolders(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            string reply = HelperInventory.MapFolderJson(bot);
            if (reply != null) return BasicReply(reply, "InventoryFolders");
            return Failure("Error", "InventoryFolders");
        }

        [About("Requests folders limited to selected folder")]
        [ArgHints("targetfolder", "URLARG", "the UUID of the folder or root")]
        [ReturnHints("single InventoryMapFolder")]
        [ReturnHints("Error")]
        [Route(HttpVerbs.Get, "/InventoryFoldersLimited/{targetfolder}/{token}")]
        public object InventoryFoldersLimited(string targetfolder,string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            string reply = HelperInventory.MapFolderJson(bot, targetfolder,false);
            if (reply != null) return BasicReply(reply, "InventoryFoldersLimited");
            return Failure("Error", "InventoryFoldersLimited");
        }

        [About("Requests the contents of a folder as an array of InventoryMapItem<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>typename: String</li></ul>")]
        [ReturnHints("array of InventoryMapItem")]
        [ReturnHints("Invaild folder UUID")]
        [ArgHints("folderUUID", "URLARG", "the folder to fetch (Found via: inventory/folders)")]
        [Route(HttpVerbs.Get, "/InventoryContents/{folderUUID}/{token}")]
        public object InventoryContents(string folderUUID, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(folderUUID, out UUID folder) == false)
            {
                return Failure("Invaild folder UUID", "InventoryContents", new [] { folderUUID });
            }
            return BasicReply(HelperInventory.MapFolderInventoryJson(bot, folder), "InventoryContents", new [] { folderUUID });
        }
    }
}
