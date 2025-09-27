using BetterSecondBot.Static;
using Discord;
using LibreMetaverse;
using OpenMetaverse;
using OpenMetaverse.Assets;
using Org.BouncyCastle.Utilities.Collections;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using static Betalgo.Ranul.OpenAI.ObjectModels.RealtimeModels.RealtimeEventTypes;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Inventory controls for sending items and so on")]
    public class InventoryCommands(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Attachs an event for inventory changes")]
        [ReturnHints("cleared")]
        [ReturnHintsFailure("No action")]
        [ReturnHintsFailure("Events service is not running")]
        [ReturnHints("Event added")]
        [ArgHints("inventoryType", "What type to watch for", "Text", "Texture", new string[] { "Texture","Sound","Callcard","Landmark","Clothing",
        "Object","Notecard","Lsltext","Lslbyte","Animation","Gesture","Mesh" })]
        [ArgHints("outputTarget", "Where to send the updates to", "SMART")]
        [CmdTypeSet()]
        public object SetInventoryUpdate(string inventoryType, string outputTarget)
        {
            if (master.EventsService.isRunning() == true)
            {
                inventoryType = inventoryType.ToLower().FirstCharToUpper();
                master.EventsService.AddInventoryEvent(inventoryType, outputTarget);
                return BasicReply("Ok");
            }
            return Failure("Events service is not running");
        }

        [About("Uploads a new sound file to inventory")]
        [ReturnHints("ok")]
        [ArgHints("sourcePath", "accepts a file path to a wave PCM file @ 44100", "FILE")]
        [ArgHints("inventoryName", "the name in secondlife", "Text", "mycoolsound")]
        [CmdTypeDo()]
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

        protected InventoryNode SearchInventoryStore(UUID item)
        {
            Inventory store = GetClient().Inventory.GetStore;
            if (store.Items.ContainsKey(item))
            {
                return store.Items[item];
            }
            return null;
        }

        [About("rezs the item at the bots current location")]
        [ReturnHints("UUID of rezzed item")]
        [ReturnHintsFailure("Invaild item UUID")]
        [ReturnHintsFailure("Unable to find item")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ArgHints("item", "What to rez", "UUID")]
        [CmdTypeDo()]
        public object RezObject(string item)
        {
            if (UUID.TryParse(item, out UUID targetitem) == false)
            {
                return Failure("Invaild item UUID: " + item, [item]);
            }
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [item]);
            }
            InventoryNode find = SearchInventoryStore(targetitem);
            InventoryItem itm = null;
            if (find != null)
            {
                if (find.Data is InventoryItem itemfound)
                {
                    itm = itemfound;
                }
            }
            if (itm == null)
            {
                // cant get the item from store try and download it
                itm = GetClient().Inventory.FetchItem(targetitem, GetClient().Self.AgentID, TimeSpan.FromSeconds(15));
            }
            if (itm == null)
            {
                return Failure("Unable to find item: " + item, [item]);
            }
            Vector3 loc = GetClient().Self.RelativePosition;
            UUID rezedobject = GetClient().Inventory.RequestRezFromInventory(GetClient().Network.CurrentSim, GetClient().Self.SimRotation, loc, itm);
            return BasicReply(rezedobject.ToString(), [item]);
        }

        [About("rezs the item at the parcel center")]
        [ReturnHints("UUID of rezzed item")]
        [ReturnHintsFailure("Invaild item UUID")]
        [ReturnHintsFailure("Unable to find item")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Invaild Z offset")]
        [ReturnHintsFailure("Z offset must be between -2 and 2")]
        [ArgHints("item", "what to rez", "UUID")]
        [ArgHints("zoffset", "Z offset to apply (range -2 to 2)", "Number", "0.5")]
        [CmdTypeDo()]
        public object RezObjectParcelCenter(string item, string zoffset)
        {
            if (UUID.TryParse(item, out UUID targetitem) == false)
            {
                return Failure("Invaild item UUID: " + item, [item]);
            }
            if(float.TryParse(zoffset, out float zoff) == false)
            {
                return Failure("Invaild Z offset: " + zoffset, [item]);
            }
            if(zoff < -2 || zoff > 2)
            {
                return Failure("Z offset must be between -2 and 2", [item, zoffset]);
            }
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [item]);
            }
            InventoryNode find = SearchInventoryStore(targetitem);
            InventoryItem itm = null;
            if (find != null)
            {
                if (find.Data is InventoryItem itemfound)
                {
                    itm = itemfound;
                }
            }
            if (itm == null)
            {
                // cant get the item from store try and download it
                itm = GetClient().Inventory.FetchItem(targetitem, GetClient().Self.AgentID, TimeSpan.FromSeconds(15));
            }
            if (itm == null)
            {
                return Failure("Unable to find item: " + item, [item]);
            }
            float x = targetparcel.AABBMin.X + ((targetparcel.AABBMax.X - targetparcel.AABBMin.X) / 2);
            float y = targetparcel.AABBMin.Y + ((targetparcel.AABBMax.Y - targetparcel.AABBMin.Y) / 2);
            Vector3 resat = new Vector3(x, y, GetClient().Self.RelativePosition.Z+zoff);
            UUID rezedobject = GetClient().Inventory.RequestRezFromInventory(GetClient().Network.CurrentSim, GetClient().Self.SimRotation, resat, itm);
            return BasicReply(rezedobject.ToString(), [item]);
        }
        [About("rezs the item at the target location")]
        [ReturnHints("UUID of rezzed item")]
        [ReturnHintsFailure("Invaild item UUID")]
        [ReturnHintsFailure("Unable to find item")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Unable to unpack [X,y,z] cord")]
        [ArgHints("item", "what to rez", "UUID")]
        [ArgHints("x", "X cord to rez at", "Number", "123")]
        [ArgHints("y", "Y cord to rez at", "Number", "45")]
        [ArgHints("z", "Z cord to rez at", "Number", "26")]
        [CmdTypeDo()]
        public object RezObjectOnPos(string item, string x, string y, string z)
        {
            if (UUID.TryParse(item, out UUID targetitem) == false)
            {
                return Failure("Invaild item UUID: " + item, [item]);
            }
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [item]);
            }
            InventoryNode find = SearchInventoryStore(targetitem);
            InventoryItem itm = null;
            if (find != null)
            {
                if (find.Data is InventoryItem itemfound)
                {
                    itm = itemfound;
                }
            }
            if (itm == null)
            {
                // cant get the item from store try and download it
                itm = GetClient().Inventory.FetchItem(targetitem, GetClient().Self.AgentID, TimeSpan.FromSeconds(15));
            }
            if (itm == null)
            {
                return Failure("Unable to find item: " + item, [item]);
            }
            if (float.TryParse(x, out float x1) == false)
            {
                return Failure("Unable to unpack X cord", [item]);
            }
            if (float.TryParse(y, out float y1) == false)
            {
                return Failure("Unable to unpack X cord", [item]);
            }
            if (float.TryParse(z, out float z1) == false)
            {
                return Failure("Unable to unpack X cord", [item]);
            }
            Vector3 resat = new Vector3(x1, y1, z1);
            UUID rezedobject = GetClient().Inventory.RequestRezFromInventory(GetClient().Network.CurrentSim, GetClient().Self.SimRotation, resat, itm);
            return BasicReply(rezedobject.ToString(), [item]);
        }


        [About("renames a folder or inventory item")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("invaild item uuid")]
        [ReturnHintsFailure("Item name is to short")]
        [ReturnHintsFailure("Unable to find inventory item")]
        [ArgHints("item", "item/folder to name", "UUID")]
        [ArgHints("newname", "What we are changing it to", "Text", "Better name")]
        [CmdTypeSet()]
        public object RenameInventory(string item, string newname)
        {
            if (UUID.TryParse(item, out UUID target) == false)
            {
                return Failure("invaild item uuid", [item, newname]);
            }
            if (newname.Length < 3)
            {
                return Failure("Item name is to short", [item, newname]);
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(GetClient(), target);
            if (realitem == null)
            {
                return Failure("Unable to find inventory item", [item, newname]);
            }
            realitem.Name = newname;
            GetClient().Inventory.RequestUpdateItem(realitem);
            return BasicReply("Ok", [item, newname]);
        }

        [About("Attempts to Remove the given inventory item")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("invaild item uuid")]
        [ArgHints("item", "what item are we Deleting", "UUID")]
        [CmdTypeSet()]
        public object DeleteInventoryItem(string item)
        {
            if (UUID.TryParse(item, out UUID target) == false)
            {
                return Failure("invaild item uuid", [item]);
            }
            GetClient().Inventory.Remove([target], []);
            return BasicReply("Ok", [item]);
        }

        [About("Attempts to Remove the given inventory folder")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("invaild folder uuid")]
        [ArgHints("folder", "what folder are we removing", "UUID")]
        [CmdTypeSet()]
        public object DeleteInventoryFolder(string folder)
        {
            if (UUID.TryParse(folder, out UUID target) == false)
            {
                return Failure("invaild folder uuid", [folder]);
            }
            GetClient().Inventory.Remove([], [target]);
            return BasicReply("Ok", [folder]);
        }

        [About("Attempts to attach the given inventory item")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("invaild item uuid")]
        [ArgHints("item", "what item are we attaching", "UUID")]
        [CmdTypeDo()]
        public object Attach(string item)
        {
            if (UUID.TryParse(item, out UUID itemuuid) == false)
            {
                return Failure("invaild item uuid", [item]);
            }
            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(GetClient(), itemuuid);
            GetClient().Appearance.AddAttachments([realitem], false, false);
            return BasicReply("Ok", [item]);
        }

        [About("Attempts to Detach the given inventory item (or attachment point)")]
        [ReturnHints("ok")]
        [ReturnHints("okMulti")]
        [ReturnHintsFailure("invaild item uuid pr invaild attach point")]
        [ArgHints("item", "UUID of item, or the attach point, or * to remove everything", "Text", "Left Hand")]
        [CmdTypeDo()]
        public object Detach(string item)
        {
            if (UUID.TryParse(item, out UUID itemuuid) == false)
            {
                List<InventoryItem> toBeRemoved = [];
                foreach (KeyValuePair<UUID, AttachmentPoint> pair in GetClient().Appearance.GetAttachmentsByItemId())
                {
                    if ((pair.Value.ToString() != item) || (item == "*"))
                    {
                        continue;
                    }
                    toBeRemoved.Add(HelperInventory.getItemByInventoryUUID(GetClient(), pair.Key));
                }
                if (toBeRemoved.Count == 0)
                {
                    return Failure("invaild item uuid pr invaild attach point", [item]);
                }
                foreach (InventoryItem A in toBeRemoved)
                {
                    GetClient().Appearance.RemoveFromOutfit(A);
                }
                return BasicReply("ok", [item]);
            }

            InventoryItem realitem = HelperInventory.getItemByInventoryUUID(GetClient(), itemuuid);
            GetClient().Appearance.RemoveFromOutfit(realitem);
            return BasicReply("Ok", [item]);
        }


        [About("Replaces the current avatar outfit with the contents of the given folder")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Named folder value is empty")]
        [ReturnHintsFailure("Cant find target folder")]
        [ReturnHintsFailure("invaild folder uuid")]
        [ReturnHintsFailure("target folder is empty or so full I cant get it in 5 secs...")]
        [ArgHints("folder", "what folder are we swapping to", "UUID")]
        [CmdTypeDo()]
        public object SwapOutfit(string folder)
        {
            UUID folderUUID = UUID.Zero;
            if (UUID.TryParse(folder, out folderUUID) == false)
            {
                return Failure("invaild folder uuid", [folder]);
            }
            List<InventoryBase> contents = GetClient().Inventory.FolderContents(folderUUID, GetClient().Self.AgentID, true, true, InventorySortOrder.ByName, TimeSpan.FromSeconds(15));
            List<InventoryItem> wareables = [];
            if (contents == null)
            {
                return Failure("target folder is empty or so full I cant get it in 5 secs...", [folder]);
            }
            foreach (InventoryBase item in contents)
            {
                if ((item is InventoryWearable) || (item is InventoryObject))
                {
                    wareables.Add((InventoryItem)item);
                }
            }
            master.CurrentOutfitFolder.ReplaceOutfit(wareables);
            return BasicReply("ok", [folder]);
        }

        [About("Replaces the current avatar outfit with the contents of the given folder")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Named folder value is empty")]
        [ReturnHintsFailure("Cant find target folder")]
        [ReturnHintsFailure("invaild folder uuid")]
        [ReturnHintsFailure("target folder is empty or so full I cant get it in 5 secs...")]
        [ArgHints("folderpath", "what folder are we swapping to", "UUID")]
        [CmdTypeDo()]
        public object SwapOutfitByPath(string folderpath)
        {
            UUID folderload = GetFolderFromPath(folderpath);
            if(folderload == UUID.Zero)
            {
                return Failure("Named folder value is empty", [folderpath]);
            }
            return SwapOutfit(folderload.ToString());
        }

        [About("Replaces the current avatar outfit with the My Outfits/[NAME] folder")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Named folder value is empty")]
        [ReturnHintsFailure("Cant find target folder")]
        [ReturnHintsFailure("invaild folder uuid")]
        [ReturnHintsFailure("target folder is empty or so full I cant get it in 5 secs...")]
        [ArgHints("name", "Name of the folder", "Text", "My other outfit")]
        [CmdTypeDo()]
        public object Outfit(string name)
        {
            return SwapOutfitByPath("My Outfits/" + name);
        }



        [About("Searchs the notecards folder for notecards, any older than 31 days are deleted.<br/>Depending on the number of notecards this might require multiple calls!")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Unable to find notecard folder")]
        [CmdTypeDo()]
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
            List<UUID> purge_notecards = [];
            List<InventoryBase> contents = GetClient().Inventory.FolderContents(NotecardFolder.UUID, GetClient().Self.AgentID, true, true, InventorySortOrder.ByDate, TimeSpan.FromSeconds(15));
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
                GetClient().Inventory.Remove(purge_notecards, []);
            }
            return BasicReply("Ok");
        }

        [About("converts a inventory uuid to a realworld uuid<br/>Needed for texture preview")]
        [ReturnHints("Asset UUID or UUID zero")]
        [ReturnHintsFailure("Invaild item uuid")]
        [ArgHints("item", "inventory level item", "UUID")]
        [CmdTypeGet()]
        public object GetRealUUID(string item)
        {
            if (UUID.TryParse(item, out UUID itemUUID) == false)
            {
                return Failure("Invaild item uuid", [item]);
            }
            UUID reply = HelperInventory.GetAssetUUID(GetClient(), itemUUID);
            return BasicReply(reply.ToString(), [item]);
        }

        [About("sends a item to an avatar via a path")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Failed")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHintsFailure("Unable to find item")]
        [ArgHints("path", "item path and name", "Text", "Objects/DemoItem")]
        [ArgHints("avatar", "Who we are sending it to", "AVATAR")]
        [CmdTypeDo()]
        public object SendItemByPath(string path, string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [path, avatar]);
            }
            UUID targetitem = GetClient().Inventory.FindObjectByPath(GetClient().Inventory.Store.RootFolder.UUID, GetClient().Self.AgentID, path, TimeSpan.FromSeconds(15));
            if (targetitem == UUID.Zero)
            {
                return Failure("Unable to find item via path", [path, avatar]);
            }
            InventoryItem itm = GetClient().Inventory.FetchItem(targetitem, GetClient().Self.AgentID, TimeSpan.FromSeconds(25));
            if (itm == null)
            {
                return Failure("Unable to find item with found uuid", [path, avatar]);
            }
            GetClient().Inventory.GiveItem(itm.UUID, itm.Name, itm.AssetType, avataruuid, false);
            return BasicReply("ok", [path, avatar]);
        }

        [About("sends a item to an avatar")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Failed")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHintsFailure("Invaild item uuid")]
        [ReturnHintsFailure("Unable to find item")]
        [ArgHints("item", "what are we sending", "UUID")]
        [ArgHints("avatar", "Who are we sending it to", "AVATAR")]
        [CmdTypeDo()]
        public object SendItem(string item, string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [item, avatar]);
            }
            if (UUID.TryParse(item, out UUID targetitem) == false)
            {
                return Failure("Invaild item uuid", [item, avatar]);
            }
            InventoryItem itm = GetClient().Inventory.FetchItem(targetitem, GetClient().Self.AgentID, TimeSpan.FromSeconds(25));
            if (itm == null)
            {
                return Failure("Unable to find item", [item, avatar]);
            }
            GetClient().Inventory.GiveItem(itm.UUID, itm.Name, itm.AssetType, avataruuid, false);
            return BasicReply("ok", [item, avatar]);
        }
        [About("sends a item to an avatar with extra steps to bypass inventory searchs")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Failed")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHintsFailure("Invaild item uuid")]
        [ReturnHintsFailure("Invaild item name")]
        [ArgHints("itemuuid", "What are we sending", "UUID")]
        [ArgHints("itemname", "what name should it have", "Text", "My cool item", 
            new string[] { "texture", "sound", "callcard", "landmark", "lsltext", "clothing", "object", 
                "notecard", "animatn", "gesture", "mesh", "material" })]
        [ArgHints("avatar", "Who are we sending it to","AVATAR")]
        [CmdTypeDo()]
        public object SendItemDirect(string itemuuid, string itemname, string itemtype, string avatar)
        {
            if (UUID.TryParse(itemuuid, out UUID targetitem) == false)
            {
                return Failure("Invaild item uuid", [itemuuid, itemname, itemtype, avatar]);
            }
            if(SecondbotHelpers.isempty(itemname) == true)
            {
                return Failure("Invaild item name", [itemuuid, itemname, itemtype, avatar]);
            }
            if (SecondbotHelpers.isempty(itemtype) == true)
            {
                return Failure("Invaild item name", [itemuuid, itemname, itemtype, avatar]);
            }
            AssetType matchtype = AssetType.Unknown;
            itemtype = itemtype.ToLower();
            foreach (AssetType matchme in (AssetType[])Enum.GetValues(typeof(AssetType)))
            {
                string checktype = matchme.ToString().ToLower();
                if(checktype == itemtype)
                {
                    matchtype = matchme;
                    break;
                }
            }
            if(matchtype == AssetType.Unknown)
            {
                return Failure("Invaild item type", [itemuuid, itemname, itemtype, avatar]);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [itemuuid, itemname, itemtype, avataruuid.ToString()]);
            }
            GetClient().Inventory.GiveItem(targetitem, itemname, matchtype, avataruuid, false);
            return BasicReply("ok", [itemuuid, itemname, itemtype, avataruuid.ToString()]);
        }
        [About("Sends a folder to an avatar")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHintsFailure("Unable to find folder")]
        [ArgHints("path", "path to the folder from root","Text","Objects/MyFolderToSend")]
        [ArgHints("avatar", "Who are we sending it to","AVATAR")]
        [CmdTypeDo()]
        public object SendFolderByPath(string path, string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [path, avatar]);
            }
            InventoryFolder FindFolderHelper = HelperInventory.FindFolderByPath(GetClient(), path);
            if (FindFolderHelper == null)
            {
                return Failure("Unable to find folder", [path, avatar]);
            }
            GetClient().Inventory.GiveFolder(FindFolderHelper.UUID, FindFolderHelper.Name, avataruuid, false);
            return BasicReply("ok", [path, avatar]);
        }


        [About("Sends a folder to an avatar")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Failed")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHintsFailure("Invaild folter uuid")]
        [ReturnHintsFailure("Unable to find folder")]
        [ArgHints("item", "what folder are we sending","UUID")]
        [ArgHints("avatar", "Who are we sending it to", "AVATAR")]
        [CmdTypeDo()]
        public object SendFolder(string folder, string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [folder, avatar]);
            }
            if (UUID.TryParse(folder, out UUID targetfolder) == false)
            {
                return Failure("Invaild folder uuid", [folder, avatar]);
            }
            InventoryBase FindFolderHelper = HelperInventory.FindFolder(GetClient(), GetClient().Inventory.Store.RootFolder, targetfolder);
            if (FindFolderHelper == null)
            {
                return Failure("Unable to find folder", [folder, avatar]);
            }
            GetClient().Inventory.GiveFolder(FindFolderHelper.UUID, FindFolderHelper.Name, avataruuid, false);
            return BasicReply("ok", [folder, avatar]);
        }

        [About("Transfers a item [ARG 2] to a objects inventory [ARG 1] (And if set with the script running state [ARG 3])")]
        [ReturnHints("Transfering running script")]
        [ReturnHints("Transfering inventory")]
        [ReturnHintsFailure("Invaild item uuid")]
        [ReturnHintsFailure("Invaild object uuid")]
        [ReturnHintsFailure("Unable to find inventory")]
        [ReturnHintsFailure("Unable to find object")]
        [ReturnHintsFailure("Invaild running")]
        [ArgHints("item",  "what are we sending","UUID")]
        [ArgHints("object", "were are we sending it","UUID")]
        [ArgHints("running", "should it be running","BOOL")]
        [CmdTypeDo()]
        public object TransferInventoryToObject(string item, string objectuuid, string running)
        {
            if (UUID.TryParse(item, out UUID itemuuid) == false)
            {
                return Failure("Invaild item uuid", [item, objectuuid, running]);
            }
            if (UUID.TryParse(objectuuid, out UUID objectUUID) == false)
            {
                return Failure("Invaild object uuid", [item, objectuuid, running]);
            }
            if (bool.TryParse(running, out bool runscript) == false)
            {
                return Failure("Invaild running", [item, objectuuid, running]);
            }

            InventoryItem itm = GetClient().Inventory.FetchItem(itemuuid, GetClient().Self.AgentID, TimeSpan.FromSeconds(15));
            if (itm == null)
            {
                return Failure("Unable to find inventory", [item, objectuuid, running]);
            }
            Dictionary<uint, Primitive> objects_copy = GetClient().Network.CurrentSim.ObjectsPrimitives.ToDictionary(k => k.Key, v => v.Value);
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
                return Failure("Unable to find object", [item, objectuuid, running]);
            }
            bool scriptState = runscript;
            if (itm.AssetType != AssetType.LSLText)
            {
                scriptState = false;
            }
            if (itm.AssetType == AssetType.LSLText)
            {
                GetClient().Inventory.CopyScriptToTask(RealObject.Key, itm, scriptState);
                return BasicReply("Transfering script [state: "+scriptState.ToString()+"]", [item, objectuuid, running]);
            }
            GetClient().Inventory.UpdateTaskInventory(RealObject.Key, itm);
            return BasicReply("Transfering inventory", [item, objectuuid, running]);
        }

        [About("Requests the inventory folder layout as a json object InventoryMapFolder<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>subfolders: InventoryMapFolder[]</li></ul>")]
        [ReturnHints("array of InventoryMapFolder")]
        [ReturnHintsFailure("Error")]
        [CmdTypeGet()]
        public object InventoryFolders()
        {
            string reply = HelperInventory.MapFolderJson(GetClient());
            if (reply != null) return BasicReply(reply);
            return Failure("Error");
        }

        [About("Requests folders limited to selected folder")]
        [ArgHints("targetfolder", "the folder to look up","UUID")]
        [ReturnHints("single InventoryMapFolder")]
        [ReturnHintsFailure("Error")]
        [CmdTypeGet()]
        public object InventoryFoldersLimited(string targetfolder)
        {
            string reply = HelperInventory.MapFolderJson(GetClient(), targetfolder,false);
            if (reply != null) return BasicReply(reply);
            return Failure("Error");
        }

        [About("Requests the contents of a folder as an array of InventoryMapItem<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>typename: String</li></ul>")]
        [ReturnHints("array of InventoryMapItem")]
        [ReturnHintsFailure("Invaild folder UUID")]
        [ArgHints("folderUUID", "the folder to fetch (Found via: inventory/folders)","UUID")]
        [CmdTypeGet()]
        public object InventoryContents(string folderUUID)
        {
            if (UUID.TryParse(folderUUID, out UUID folder) == false)
            {
                return Failure("Invaild folder UUID", [folderUUID]);
            }
            return BasicReply(HelperInventory.MapFolderInventoryJson(GetClient(), folder), [folderUUID]);
        }

        [About("creates a folder in a folder")]
        [ReturnHints("Ok")]
        [ReturnHintsFailure("invaild parent folder UUID")]
        [ReturnHintsFailure("new folder name is too short, must be longer than 3 characters.")]
        [ArgHints("parentFolder", "folder to create a subfolder in","UUID")]
        [ArgHints("folderName", "name of the new folder","Text","MyNewFolder")]
        [CmdTypeSet()]
        public object CreateInventoryFolder(string parentFolder, string folderName)
        {
            if (UUID.TryParse(parentFolder, out UUID parentFolderUUID) == false)
            {
                return Failure("invaild parent folder UUID", [parentFolder]);
            }
            if (folderName.Length < 3)
            {
                return Failure("new folder name is too short, must be longer than 3 characters.", [folderName]);
            }
            UUID folder = GetClient().Inventory.CreateFolder(parentFolderUUID, folderName);
            string uuid = folder.ToString();
            return BasicReply("Ok", [uuid]);
        }


        [About("moves an item to another folder")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("invaild item uuid")]
        [ReturnHintsFailure("invaild folder uuid")]
        [ArgHints("item", " item to move", "UUID")]
        [ArgHints("folder", "destination folder", "UUID")]
        [CmdTypeDo()]
        public object MoveInventoryItem(string item, string folder)
        {
            if (UUID.TryParse(item, out UUID itemUUID) == false)
            {
                return Failure("invaild item uuid", [item, folder]);
            }
            if (UUID.TryParse(folder, out UUID folderUUID) == false)
            {
                return Failure("invaild folder uuid", [item, folder]);
            }
            GetClient().Inventory.MoveItem(itemUUID, folderUUID);
            return BasicReply("Ok", [item]);
        }

        [About("moves a folder to another folder")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("invaild source folder uuid")]
        [ReturnHintsFailure("invaild dest folder uuid")]
        [ArgHints("sourceFolder", "folder to move", "UUID")]
        [ArgHints("destFolder", "destination folder", "UUID")]
        [CmdTypeDo()]
        public object MoveInventoryFolder(string sourceFolder, string destFolder)
        {
            if (UUID.TryParse(sourceFolder, out UUID sourceFolderUUID) == false)
            {
                return Failure("invaild source folder uuid", [sourceFolder, destFolder]);
            }
            if (UUID.TryParse(destFolder, out UUID destFolderUUID) == false)
            {
                return Failure("invaild dest folder uuid", [sourceFolder, destFolder]);
            }
            GetClient().Inventory.MoveFolder(sourceFolderUUID, destFolderUUID);
            return BasicReply("Ok", [sourceFolder, destFolder]);
        }


        protected InventoryNode FindFolderInternal(InventoryNode currentNode, string currentPath, string desiredPath)
        {
            if (desiredPath == currentPath || desiredPath == (currentPath + "/"))
            {
                return currentNode;
            }
            return currentNode.Nodes.Values.Select(
                n => FindFolderInternal(n, (currentPath == "/"
                    ? currentPath
                    : currentPath + "/") + n.Data.Name.ToLower(), desiredPath))
                .FirstOrDefault(res => res != null);
        }
        protected InventoryNode RootFolder()
        {
            return GetClient().Inventory.GetStore.GetNodeFor(GetClient().Inventory.GetStore.RootFolder.UUID);
        }
        protected InventoryNode FindFolder(string path)
        {
            var root = RootFolder();
            if(root == null)
            {
                return null;
            }
            var pathfinder = Regex.Replace(path, @"^[/\s]*(.*)[/\s]*", @"$1").ToLower();
            var result = FindFolderInternal(root, "/", "/" + pathfinder);
            return root == null ? null : result;
        }
        protected UUID GetFolderFromPath(string path)
        {
            InventoryNode A = FindFolder(path);
            if (A != null)
            {
                return A.Data.UUID;
            }
            return UUID.Zero;
        }
    }
}
