using BetterSecondBot.Static;
using OpenMetaverse;
using OpenMetaverse.Assets;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecondBotEvents.Commands
{
    public class InventoryCommands : CommandsAPI
    {
        public InventoryCommands(EventsSecondBot setmaster) : base(setmaster)
        {
        }

        [About("Attachs an event for inventory changes")]
        [ReturnHints("cleared")]
        [ReturnHints("No action")]
        [ReturnHints("Event added")]
        [ArgHints("inventoryType", "Types: texture,sound,callcard,landmark,clothing,object,notecard,lsltext,lslbyte,animatn,gesture,mesh")]
        [ArgHints("outputTarget", "HTTP url, channel, avatar UUID or clear to remove all events for the selected type")]
        public object SetInventoryUpdate(string inventoryType, string outputTarget)
        {
            return Failure("@todo inventory update events");
        }

        [About("Uploads a new sound file to inventory")]
        [ReturnHints("ok")]
        [ArgHints("sourcePath", "accepts a file path to a wave PCM file @ 44100")]
        [ArgHints("inventoryName", "the name in secondlife")]
        public object UploadMediaWave(string sourcePath, string inventoryName)
        {
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
        [ArgHints("item", "UUID of item to rez")]
        public object RezObject(string item)
        {
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
        [ArgHints("item", "UUID of item/folder to name")]
        [ArgHints("newname", "What we are changing it to")]
        public object RenameInventory(string item, string newname)
        {
            if (UUID.TryParse(item, out UUID target) == false)
            {
                return Failure("invaild item uuid", "RenameInventory", new [] { item, newname });
            }
            if (newname.Length < 3)
            {
                return Failure("Item name is to short", "RenameInventory", new [] { item, newname });
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(getClient(), target);
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
        [ArgHints("item", "UUID of item")]
        public object DeleteInventoryItem(string item)
        {
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
        [ArgHints("folder", "UUID of folder")]
        public object DeleteInventoryFolder(string folder)
        {
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
        [ArgHints("item", "UUID of item")]
        public object Attach(string item)
        {
            if (UUID.TryParse(item,out UUID itemuuid) == false)
            {
                return Failure("invaild item uuid", "Attach", new [] { item });
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(getClient(), itemuuid);
            getClient().Appearance.AddAttachments(new List<InventoryItem>() { realitem }, false, false);
            return BasicReply("Ok", "Attach", new [] { item });
        }

        [About("Attempts to Remove the given inventory folder")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")]
        [ArgHints("item", "UUID of item")]
        public object Detach(string item)
        {
            if (UUID.TryParse(item, out UUID itemuuid) == false)
            {
                return Failure("invaild item uuid", "Detach", new [] { item });
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(getClient(), itemuuid);
            getClient().Appearance.RemoveFromOutfit(realitem);
            return BasicReply("Ok", "Detach", new [] { item });
        }

        [About("Replaces the current avatar outfit with the Clothing/[NAME] folder<br/>Please note: This does not use the outfits folder!<br/>Please do not use links in the folder!")]
        [ReturnHints("ok")]
        [ReturnHints("Named folder value is empty")]
        [ReturnHints("Cant find Clothing folder")]
        [ReturnHints("Cant find target folder")]
        [ReturnHints("target folder is empty or so full I cant get it in 5 secs...")]
        [ArgHints("name", "Name of the folder")]
        public object Outfit(string name)
        {
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
        public object InventoryPurgeNotecards()
        {
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
        [ArgHints("item", "inventory level UUID of item")]
        public object getRealUUID(string item)
        {
            if (UUID.TryParse(item, out UUID itemUUID) == false)
            {
                return Failure("Invaild item uuid", "getRealUUID", new [] { item });
            }
            UUID reply = HelperInventory.GetAssetUUID(getClient(), itemUUID);
            return BasicReply(reply.ToString(), "getRealUUID", new [] { item });
        }

        [About("sends a item to an avatar")]
        [ReturnHints("ok")]
        [ReturnHints("Failed")]
        [ReturnHints("Invaild avatar uuid")]
        [ReturnHints("Invaild item uuid")]
        [ReturnHints("Unable to find item")]
        [ArgHints("item", "UUID of item")]
        [ArgHints("avatar", "a UUID or Firstname Lastname")]
        public object SendItem(string item, string avatar)
        {
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
        [ArgHints("item", "UUID of item")]
        [ArgHints("avatar", "a UUID or Firstname Lastname")]
        public object SendFolder(string folder, string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", "SendFolder", new [] { folder, avatar });
            }
            if (UUID.TryParse(folder, out UUID targetfolder) == true)
            {
                return Failure("Invaild folder uuid", "SendFolder", new [] { folder, avatar });
            }
            InventoryBase FindFolderHelper = HelperInventory.FindFolder(getClient(), getClient().Inventory.Store.RootFolder, targetfolder);
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
        [ArgHints("item",  "UUID of item")]
        [ArgHints("object", "the uuid of the object")]
        [ArgHints("running", "true if you wish the transfered script to be running otherwise false")]
        public object TransferInventoryToObject(string item, string objectuuid, string running)
        {
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
        public object InventoryFolders()
        {
            string reply = HelperInventory.MapFolderJson(getClient());
            if (reply != null) return BasicReply(reply, "InventoryFolders");
            return Failure("Error", "InventoryFolders");
        }

        [About("Requests folders limited to selected folder")]
        [ArgHints("targetfolder", "the UUID of the folder or root")]
        [ReturnHints("single InventoryMapFolder")]
        [ReturnHints("Error")]
        public object InventoryFoldersLimited(string targetfolder)
        {
            string reply = HelperInventory.MapFolderJson(getClient(), targetfolder,false);
            if (reply != null) return BasicReply(reply, "InventoryFoldersLimited");
            return Failure("Error", "InventoryFoldersLimited");
        }

        [About("Requests the contents of a folder as an array of InventoryMapItem<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>typename: String</li></ul>")]
        [ReturnHints("array of InventoryMapItem")]
        [ReturnHints("Invaild folder UUID")]
        [ArgHints("folderUUID", "the folder to fetch (Found via: inventory/folders)")]
        public object InventoryContents(string folderUUID)
        {
            if (UUID.TryParse(folderUUID, out UUID folder) == false)
            {
                return Failure("Invaild folder UUID", "InventoryContents", new [] { folderUUID });
            }
            return BasicReply(HelperInventory.MapFolderInventoryJson(getClient(), folder), "InventoryContents", new [] { folderUUID });
        }

        [About("creates a folder in a folder")]
        [ReturnHints("Ok")]
        [ReturnHintsFailure("invaild parent folder UUID")]
        [ReturnHintsFailure("new folder name is too short, must be longer than 3 characters.")]
        [ArgHints("parentFolder", "UUID of folder to create a subfolder in")]
        [ArgHints("folderName", "name of the new folder")]
        public object CreateInventoryFolder(string parentFolder, string folderName, string token)
        {
            if (UUID.TryParse(parentFolder, out UUID parentFolderUUID) == false)
            {
                return Failure("invaild parent folder UUID", "CreateInventoryFolder", new[] { parentFolder });
            }
            if (folderName.Length < 3)
            {
                return Failure("new folder name is too short, must be longer than 3 characters.", "CreateInventoryFolder", new[] { folderName });
            }
            UUID folder = getClient().Inventory.CreateFolder(parentFolderUUID, folderName);
            string uuid = folder.ToString();
            return BasicReply("Ok", "CreateInventoryFolder", new string[] { uuid });
        }


        [About("moves an item to another folder")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("invaild item uuid")]
        [ReturnHintsFailure("invaild folder uuid")]
        [ArgHints("item", "UUID of item to move")]
        [ArgHints("folder", "UUID of destination folder")]
        public object MoveInventoryItem(string item, string folder)
        {
            if (UUID.TryParse(item, out UUID itemUUID) == false)
            {
                return Failure("invaild item uuid", "MoveInventoryItem", new[] { item, folder });
            }
            if (UUID.TryParse(folder, out UUID folderUUID) == false)
            {
                return Failure("invaild folder uuid", "MoveInventoryItem", new[] { item, folder });
            }
            getClient().Inventory.MoveItem(itemUUID, folderUUID);
            return BasicReply("Ok", "MoveInventoryItem", new string[] { item });
        }

        [About("moves a folder to another folder")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("invaild source folder uuid")]
        [ReturnHintsFailure("invaild dest folder uuid")]
        [ArgHints("sourceFolder", "UUID of folder to move")]
        [ArgHints("destFolder", "UUID of destination folder")]
        public object MoveInventoryFolder(string sourceFolder, string destFolder)
        {
            if (UUID.TryParse(sourceFolder, out UUID sourceFolderUUID) == false)
            {
                return Failure("invaild source folder uuid", "MoveInventoryFolder", new[] { sourceFolder, destFolder });
            }
            if (UUID.TryParse(destFolder, out UUID destFolderUUID) == false)
            {
                return Failure("invaild dest folder uuid", "MoveInventoryFolder", new[] { sourceFolder, destFolder });
            }
            getClient().Inventory.MoveFolder(sourceFolderUUID, destFolderUUID);
            return BasicReply("Ok", "MoveInventoryFolder", new string[] { sourceFolder, destFolder });
        }
    }
}
