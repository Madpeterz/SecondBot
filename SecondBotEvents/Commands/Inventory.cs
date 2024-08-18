using BetterSecondBot.Static;
using Discord;
using LibreMetaverse;
using OpenMetaverse;
using OpenMetaverse.Assets;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Inventory controls for sending items and so on")]
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
            if (master.EventsService.isRunning() == true)
            {
                master.EventsService.AddInventoryEvent(inventoryType, outputTarget);
                return BasicReply("Ok");
            }
            return Failure("Events service is not running");
        }

        [About("Uploads a new sound file to inventory")]
        [ReturnHints("ok")]
        [ArgHints("sourcePath", "accepts a file path to a wave PCM file @ 44100")]
        [ArgHints("inventoryName", "the name in secondlife")]
        public object UploadMediaWave(string sourcePath, string inventoryName)
        {
            byte[] audioData = File.ReadAllBytes(sourcePath);
            InventoryFolder AA = GetClient().Inventory.Store.RootFolder;
            List<InventoryBase> T = GetClient().Inventory.Store.GetContents(AA);
            AA = null;
            foreach (InventoryBase R in T)
            {
                if (R.Name == "Sounds")
                {
                    AA = (InventoryFolder)R;
                    break;
                }
            }
            AssetSound S = new()
            {
                AssetData = audioData
            };
            S.Encode();

            GetClient().Inventory.RequestCreateItemFromAsset(S.AssetData, inventoryName, "", AssetType.Sound, InventoryType.Sound, AA.UUID, null);
            return BasicReply("ok");
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
                return Failure("Invaild item UUID: "+ item, new [] { item });
            }
            InventoryItem itm = GetClient().Inventory.FetchItem(targetitem, GetClient().Self.AgentID, (5 * 1000));
            if (itm == null)
            {
                return Failure("Unable to find item: " + item, new [] { item });
            }
            UUID rezedobject = GetClient().Inventory.RequestRezFromInventory(GetClient().Network.CurrentSim, GetClient().Self.SimRotation, GetClient().Self.RelativePosition, itm);
            return BasicReply(rezedobject.ToString(), new [] { item });
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
                return Failure("invaild item uuid", new [] { item, newname });
            }
            if (newname.Length < 3)
            {
                return Failure("Item name is to short", new [] { item, newname });
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(GetClient(), target);
            if (realitem == null)
            {
                return Failure("Unable to find inventory item", new [] { item, newname });
            }
            realitem.Name = newname;
            GetClient().Inventory.RequestUpdateItem(realitem);
            return BasicReply("Ok", new [] { item, newname });
        }

        [About("Attempts to Remove the given inventory item")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")]
        [ArgHints("item", "UUID of item")]
        public object DeleteInventoryItem(string item)
        {
            if (UUID.TryParse(item, out UUID target) == false)
            {
                return Failure("invaild item uuid", new [] { item });
            }
            GetClient().Inventory.Remove(new List<UUID>() { target }, new List<UUID>());
            return BasicReply("Ok", new [] { item });
        }

        [About("Attempts to Remove the given inventory folder")]
        [ReturnHints("ok")]
        [ReturnHints("invaild folder uuid")]
        [ArgHints("folder", "UUID of folder")]
        public object DeleteInventoryFolder(string folder)
        {
            if (UUID.TryParse(folder, out UUID target) == false)
            {
                return Failure("invaild folder uuid", new [] { folder });
            }
            GetClient().Inventory.Remove(new List<UUID>(), new List<UUID>() { target });
            return BasicReply("Ok", new [] { folder });
        }

        [About("Attempts to attach the given inventory item")]
        [ReturnHints("ok")]
        [ReturnHints("invaild item uuid")] 
        [ArgHints("item", "UUID of item")]
        public object Attach(string item)
        {
            if (UUID.TryParse(item,out UUID itemuuid) == false)
            {
                return Failure("invaild item uuid", new [] { item });
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(GetClient(), itemuuid);
            GetClient().Appearance.AddAttachments(new List<InventoryItem>() { realitem }, false, false);
            return BasicReply("Ok", new [] { item });
        }

        [About("Attempts to Detach the given inventory item (or attachment point)")]
        [ReturnHints("ok")]
        [ReturnHints("okMulti")]
        [ReturnHints("invaild item uuid pr invaild attach point")]
        [ArgHints("item", "UUID of item, or the attach point, or * to remove everything")]
        public object Detach(string item)
        {
            if (UUID.TryParse(item, out UUID itemuuid) == false)
            {
                List<InventoryItem> toBeRemoved = new List<InventoryItem>();
                foreach (KeyValuePair<UUID, AttachmentPoint> pair in GetClient().Appearance.GetAttachmentsByItemId())
                {
                    if((pair.Value.ToString() != item) || (item == "*"))
                    {
                        continue;
                    }
                    toBeRemoved.Add(HelperInventory.getItemByInventoryUUID(GetClient(), pair.Key));
                }
                if(toBeRemoved.Count == 0)
                {
                    return Failure("invaild item uuid pr invaild attach point", new[] { item });
                }
                foreach(InventoryItem A in toBeRemoved)
                {
                    GetClient().Appearance.RemoveFromOutfit(A);
                }
                return BasicReply("ok", new[] { item });
            }
            
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(GetClient(), itemuuid);
            GetClient().Appearance.RemoveFromOutfit(realitem);
            return BasicReply("Ok", new [] { item });
        }


        [About("Replaces the current avatar outfit with the contents of the given folder")]
        [ReturnHints("ok")]
        [ReturnHints("Named folder value is empty")]
        [ReturnHints("Cant find target folder")]
        [ReturnHints("invaild folder uuid")]
        [ReturnHints("target folder is empty or so full I cant get it in 5 secs...")]
        [ArgHints("folder", "UUID of the folder")]
        public object SwapOutfit(string folder)
        {
            UUID folderUUID = UUID.Zero;
            if(UUID.TryParse(folder, out folderUUID) == false)
            {
                return Failure("invaild folder uuid", new[] { folder });
            }
            List<InventoryBase> contents = GetClient().Inventory.FolderContents(folderUUID, GetClient().Self.AgentID, true, true, InventorySortOrder.ByName, 5 * 1000);
            List<InventoryItem> wareables = new();
            if (contents == null)
            {
                return Failure("target folder is empty or so full I cant get it in 5 secs...", new[] { folder });
            }
            foreach (InventoryBase item in contents)
            {
                if ((item is InventoryWearable) || (item is InventoryObject))
                {
                    wareables.Add((InventoryItem)item);
                }
            }
            master.CurrentOutfitFolder.ReplaceOutfit(wareables);
            return BasicReply("ok", new[] { folder });
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
                return Failure("Named folder value is empty", new [] { name });
            }
            // uses the Clothing folder
            // must be a full outfit (shapes/eyes ect)
            InventoryFolder AA = GetClient().Inventory.Store.RootFolder;
            List<InventoryBase> T = GetClient().Inventory.Store.GetContents(AA);
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
                return Failure("Cant find Clothing folder", new [] { name });
            }
            T = GetClient().Inventory.Store.GetContents(AA);
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
                return Failure("Cant find target folder", new [] { name });
            }
            List<InventoryBase> contents = GetClient().Inventory.FolderContents(AA.UUID, GetClient().Self.AgentID, true, true, InventorySortOrder.ByName, 5 * 1000);
            List<InventoryItem> wareables = new();
            if (contents == null)
            {
                return Failure("target folder is empty or so full I cant get it in 5 secs...", new [] { name });
            }
            foreach (InventoryBase item in contents)
            {
                if ((item is InventoryWearable) || (item is InventoryObject))
                {
                    wareables.Add((InventoryItem)item);
                }
            }
            GetClient().Appearance.ReplaceOutfit(wareables, false);
            return BasicReply("ok", new [] { name });
        }



        [About("Searchs the notecards folder for notecards, any older than 31 days are deleted.<br/>Depending on the number of notecards this might require multiple calls!")]
        [ReturnHints("ok")]
        [ReturnHints("Unable to find notecard folder")]
        public object InventoryPurgeNotecards()
        {
            List<InventoryBase> T = GetClient().Inventory.Store.GetContents(GetClient().Inventory.Store.RootFolder);
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
            List<UUID> purge_notecards = new();
            List<InventoryBase> contents = GetClient().Inventory.FolderContents(NotecardFolder.UUID, GetClient().Self.AgentID, true, true, InventorySortOrder.ByDate, 40 * 1000);
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
            if (purge_notecards.Count > 0)
            {
                GetClient().Inventory.Remove(purge_notecards, new List<UUID>());
            }
            return BasicReply("Ok");
        }

        [About("converts a inventory uuid to a realworld uuid<br/>Needed for texture preview")]
        [ReturnHints("Asset UUID or UUID zero")]
        [ReturnHints("Invaild item uuid")]
        [ArgHints("item", "inventory level UUID of item")]
        public object GetRealUUID(string item)
        {
            if (UUID.TryParse(item, out UUID itemUUID) == false)
            {
                return Failure("Invaild item uuid", new [] { item });
            }
            UUID reply = HelperInventory.GetAssetUUID(GetClient(), itemUUID);
            return BasicReply(reply.ToString(), new [] { item });
        }

        [About("sends a item to an avatar via a path")]
        [ReturnHints("ok")]
        [ReturnHints("Failed")]
        [ReturnHints("Invaild avatar uuid")]
        [ReturnHints("Unable to find item")]
        [ArgHints("path", "item name and path example: Notecard/sendme or Objects/DemoItem")]
        [ArgHints("avatar", "a UUID or Firstname Lastname")]
        public object SendItemByPath(string path, string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", new[] { path, avatar });
            }
            UUID targetitem = GetClient().Inventory.FindObjectByPath(GetClient().Inventory.Store.RootFolder.UUID, GetClient().Self.AgentID, path, (3 * 1000));
            if(targetitem == UUID.Zero)
            {
                return Failure("Unable to find item via path", new[] { path, avatar });
            }
            InventoryItem itm = GetClient().Inventory.FetchItem(targetitem, GetClient().Self.AgentID, (3 * 1000));
            if (itm == null)
            {
                return Failure("Unable to find item with found uuid", new[] { path, avatar });
            }
            GetClient().Inventory.GiveItem(itm.UUID, itm.Name, itm.AssetType, avataruuid, false);
            return BasicReply("ok", new[] { path, avatar });
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
                return Failure("Invaild avatar uuid", new [] { item, avatar });
            }
            if (UUID.TryParse(item, out UUID targetitem) == false)
            {
                return Failure("Invaild item uuid", new [] { item, avatar });
            }
            InventoryItem itm = GetClient().Inventory.FetchItem(targetitem, GetClient().Self.AgentID, (3 * 1000));
            if (itm == null)
            {
                return Failure("Unable to find item", new [] { item, avatar });
            }
            GetClient().Inventory.GiveItem(itm.UUID, itm.Name, itm.AssetType, avataruuid, false);
            return BasicReply("ok", new [] { item, avatar });
        }
        [About("Sends a folder to an avatar")]
        [ReturnHints("ok")]
        [ReturnHints("Invaild avatar uuid")]
        [ReturnHints("Unable to find folder")]
        [ArgHints("path", "path to the folder from root")]
        [ArgHints("avatar", "a UUID or Firstname Lastname")]
        public object SendFolderByPath(string path, string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", new[] { path, avatar });
            }
            InventoryFolder FindFolderHelper = HelperInventory.FindFolderByPath(GetClient(), path);
            if (FindFolderHelper == null)
            {
                return Failure("Unable to find folder", new[] { path, avatar });
            }
            GetClient().Inventory.GiveFolder(FindFolderHelper.UUID, FindFolderHelper.Name, avataruuid, false);
            return BasicReply("ok", new[] { path, avatar });
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
                return Failure("Invaild avatar uuid", new [] { folder, avatar });
            }
            if (UUID.TryParse(folder, out UUID targetfolder) == false)
            {
                return Failure("Invaild folder uuid", new [] { folder, avatar });
            }
            InventoryBase FindFolderHelper = HelperInventory.FindFolder(GetClient(), GetClient().Inventory.Store.RootFolder, targetfolder);
            if (FindFolderHelper == null)
            {
                return Failure("Unable to find folder", new [] { folder, avatar });
            }
            GetClient().Inventory.GiveFolder(FindFolderHelper.UUID, FindFolderHelper.Name, avataruuid, false);
            return BasicReply("ok", new [] { folder, avatar });
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
                return Failure("Invaild item uuid", new [] { item, objectuuid, running });
            }
            if (UUID.TryParse(objectuuid, out UUID objectUUID) == false)
            {
                return Failure("Invaild object uuid", new [] { item, objectuuid, running });
            }
            if (bool.TryParse(running, out bool runscript) == false)
            {
                return Failure("Invaild running", new [] { item, objectuuid, running });
            }

            InventoryItem itm = GetClient().Inventory.FetchItem(itemuuid, GetClient().Self.AgentID, (3 * 1000));
            if (itm == null)
            {
                return Failure("Unable to find inventory", new [] { item, objectuuid, running });
            }
            Dictionary<uint, Primitive> objects_copy = GetClient().Network.CurrentSim.ObjectsPrimitives.Copy();
            KeyValuePair<uint, Primitive> RealObject = new(0, null);
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
                GetClient().Objects.RequestObjectMedia(objectUUID, GetClient().Network.CurrentSim, null);
                foreach (KeyValuePair<uint, Primitive> Obj in objects_copy)
                {
                    if (Obj.Value.ID == objectUUID)
                    {
                        RealObject = Obj;
                        break;
                    }
                }
                return Failure("Unable to find object", new [] { item, objectuuid, running });
            }
            bool scriptState = runscript;
            if (itm.AssetType != AssetType.LSLText)
            {
                scriptState = false;
            }
            if (itm.AssetType == AssetType.LSLText)
            {
                GetClient().Inventory.CopyScriptToTask(RealObject.Key, itm, scriptState);
                return BasicReply("Transfering script [state: "+scriptState.ToString()+"]", new [] { item, objectuuid, running });
            }
            GetClient().Inventory.UpdateTaskInventory(RealObject.Key, itm);
            return BasicReply("Transfering inventory", new [] { item, objectuuid, running });
        }

        [About("Requests the inventory folder layout as a json object InventoryMapFolder<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>subfolders: InventoryMapFolder[]</li></ul>")]
        [ReturnHints("array of InventoryMapFolder")]
        [ReturnHints("Error")]
        public object InventoryFolders()
        {
            string reply = HelperInventory.MapFolderJson(GetClient());
            if (reply != null) return BasicReply(reply);
            return Failure("Error");
        }

        [About("Requests folders limited to selected folder")]
        [ArgHints("targetfolder", "the UUID of the folder or root")]
        [ReturnHints("single InventoryMapFolder")]
        [ReturnHints("Error")]
        public object InventoryFoldersLimited(string targetfolder)
        {
            string reply = HelperInventory.MapFolderJson(GetClient(), targetfolder,false);
            if (reply != null) return BasicReply(reply);
            return Failure("Error");
        }

        [About("Requests the contents of a folder as an array of InventoryMapItem<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>typename: String</li></ul>")]
        [ReturnHints("array of InventoryMapItem")]
        [ReturnHints("Invaild folder UUID")]
        [ArgHints("folderUUID", "the folder to fetch (Found via: inventory/folders)")]
        public object InventoryContents(string folderUUID)
        {
            if (UUID.TryParse(folderUUID, out UUID folder) == false)
            {
                return Failure("Invaild folder UUID", new [] { folderUUID });
            }
            return BasicReply(HelperInventory.MapFolderInventoryJson(GetClient(), folder), new [] { folderUUID });
        }

        [About("creates a folder in a folder")]
        [ReturnHints("Ok")]
        [ReturnHintsFailure("invaild parent folder UUID")]
        [ReturnHintsFailure("new folder name is too short, must be longer than 3 characters.")]
        [ArgHints("parentFolder", "UUID of folder to create a subfolder in")]
        [ArgHints("folderName", "name of the new folder")]
        public object CreateInventoryFolder(string parentFolder, string folderName)
        {
            if (UUID.TryParse(parentFolder, out UUID parentFolderUUID) == false)
            {
                return Failure("invaild parent folder UUID", new[] { parentFolder });
            }
            if (folderName.Length < 3)
            {
                return Failure("new folder name is too short, must be longer than 3 characters.", new[] { folderName });
            }
            UUID folder = GetClient().Inventory.CreateFolder(parentFolderUUID, folderName);
            string uuid = folder.ToString();
            return BasicReply("Ok", new string[] { uuid });
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
                return Failure("invaild item uuid", new[] { item, folder });
            }
            if (UUID.TryParse(folder, out UUID folderUUID) == false)
            {
                return Failure("invaild folder uuid", new[] { item, folder });
            }
            GetClient().Inventory.MoveItem(itemUUID, folderUUID);
            return BasicReply("Ok", new string[] { item });
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
                return Failure("invaild source folder uuid", new[] { sourceFolder, destFolder });
            }
            if (UUID.TryParse(destFolder, out UUID destFolderUUID) == false)
            {
                return Failure("invaild dest folder uuid", new[] { sourceFolder, destFolder });
            }
            GetClient().Inventory.MoveFolder(sourceFolderUUID, destFolderUUID);
            return BasicReply("Ok", new string[] { sourceFolder, destFolder });
        }
    }
}
