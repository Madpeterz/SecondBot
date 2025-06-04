using Newtonsoft.Json;
using OpenMetaverse;
using OpenMetaverse.Assets;
using SecondBotEvents.Services;


namespace SecondBotEvents.Commands
{
    [ClassInfo("Why this is not a built in script command I will never know, but its due to needing this that the bot was made")]
    public class Notecard(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Reads the notecards content, if it is able to read the notecard will give a json reply of notecarduuid name and contents")]
        [ReturnHintsFailure("Not a vaild UUID")]
        [ReturnHintsFailure("Unable to find inventory item")]
        [ReturnHintsFailure("Inventory item is not a notecard")]
        [ReturnHintsFailure("replytarget not set")]
        [ReturnHints("Making request to get data from notecard")]
        [ArgHints("notecardInventoryUUID", "What notecard to read","UUID")]
        [ArgHints("replytarget", "Were to send the reply","SMART")]
        public object NotecardRead(string notecardInventoryUUID, string replytarget)
        {
            if(SecondbotHelpers.isempty(replytarget) == true)
            {
                return Failure("replytarget not set");
            }
            NotecardReadReply reply = new()
            {
                uuid = notecardInventoryUUID,
                name = "?",
                status = false,
                content = "Not a vaild UUID"
            };
            if (UUID.TryParse(notecardInventoryUUID, out var uuid) == false)
            {
                master.CommandsService.SmartCommandReply(replytarget, JsonConvert.SerializeObject(reply), "NotecardRead");
                return Failure(reply.content, [notecardInventoryUUID]);
            }
            reply.content = "Unable to find inventory item";
            InventoryItem item = GetClient().Inventory.FetchItem(uuid, GetClient().Self.AgentID, new System.TimeSpan(0, 1, 30));
            if (item == null)
            {
                master.CommandsService.SmartCommandReply(replytarget, JsonConvert.SerializeObject(reply), "NotecardRead");
                return Failure(reply.content, [notecardInventoryUUID]);
            }
            reply.content = "Inventory item is not a notecard";
            if (item.InventoryType != InventoryType.Notecard)
            {
                master.CommandsService.SmartCommandReply(replytarget, JsonConvert.SerializeObject(reply), "NotecardRead");
                return Failure(reply.content, [notecardInventoryUUID]); ;
            }
            InventoryNotecard notecard = (InventoryNotecard)item;
            GetClient().Assets.RequestInventoryAsset(notecard, true, UUID.Random(), (AssetDownload transfer, Asset asset) =>
            {
                if (transfer.Success == false)
                {
                    reply.content = "!ERROR! - unable to read notecard";
                    master.CommandsService.SmartCommandReply(replytarget, JsonConvert.SerializeObject(reply), "NotecardRead");
                }
                else
                {
                    AssetNotecard note = (AssetNotecard)asset;
                    note.Decode();
                    string contents = "";
                    for (int index = 0; index < note.BodyText.Length; index++)
                    {
                        char c = note.BodyText[index];
                        if ((int)c == 0xdbc0)
                        {
                            contents = "[ATTACHMENT]";
                        }
                        else
                        {
                            contents += c;
                        }
                    }
                    reply.content = contents;
                    reply.name = notecard.Name;
                    reply.status = true;
                    master.CommandsService.SmartCommandReply(replytarget, JsonConvert.SerializeObject(reply), "NotecardRead");
                }
            }
            );
            return BasicReply("Making request to get data from notecard");
        }

        [About("Adds content to the Collection<br/> Also creates the Collection if it does not exist")]
        [ReturnHintsFailure("Collection value is empty")]
        [ReturnHintsFailure("Content value is empty")]
        [ReturnHints("ok")]
        [ArgHints("collection", "The name of the collection","Text","StorageMeme")]
        [ArgHints("content", "The text to add to the collection","Text","Add this line \n Please and thanks")]
        public object NotecardAdd(string collection, string content)
        {
            if (SecondbotHelpers.notempty(collection) == false)
            {
                return Failure("Collection value is empty", [collection, content]);
            }
            if (SecondbotHelpers.notempty(content) == false)
            {
                return Failure("Content value is empty", [collection, content]);
            }
            master.DataStoreService.AppendKeyValue("notecard_"+collection, content);
            return BasicReply("ok", [collection, master.DataStoreService.GetKeyValue("notecard_"+collection).Length.ToString()]);
        }

        [About("Clears the contents of a collection")]
        [ReturnHintsFailure("Collection value is empty")]
        [ReturnHints("ok")]
        [ArgHints("collection", "The name of the collection", "Text", "StorageMeme")]
        public object NotecardClear(string collection)
        {
            if (SecondbotHelpers.notempty(collection) == false)
            {
                return Failure("Collection value is empty", [collection]);
            }
            master.DataStoreService.ClearKeyValue("notecard_" + collection);
            return BasicReply("ok");
        }

        [About("Sends a notecard to a avatar using the text in the prebuilt collection [see NotecardAdd] and also clears the collection just before sending [see NotecardClear]")]
        [ReturnHintsFailure("Collection value is empty")]
        [ReturnHintsFailure("Notecardname value is empty")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHintsFailure("No content in notecard storage ?")]
        [ReturnHints("ok")]
        [ArgHints("avatar", "Who to send it to","AVATAR")]
        [ArgHints("collection", "The name of the collection", "Text", "StorageMeme")]
        [ArgHints("notecardname", "What to call the created notecard","Text", "My custom notecard")]
        public object NotecardSend(string avatar, string collection, string notecardname)
        {
            if (SecondbotHelpers.notempty(collection) == false)
            {
                return Failure("Collection value is empty", [avatar, collection, notecardname]);
            }
            if (SecondbotHelpers.notempty(notecardname) == false)
            {
                return Failure("Notecardname value is empty", [avatar, collection, notecardname]);
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [avatar, collection, notecardname]);
            }
            string content = master.DataStoreService.GetKeyValue("notecard_" + collection);
            if(content.Length <= 3)
            {
                return Failure("No content in notecard storage ?", [avatar, collection, notecardname]);
            }
            bool result = master.BotClient.SendNotecard(notecardname, content, avataruuid);
            if (result == false)
            {
                return Failure("Failed to create/send notecard", [avatar, content, notecardname]);
            }
            master.DataStoreService.ClearKeyValue("notecard_" + collection);
            return BasicReply("ok");
        }

        [About("Creates and sends a notecard in one command good if you are using HTTP otherwise see [NotecardSend]")]
        [ReturnHintsFailure("notecardname value is empty")]
        [ReturnHintsFailure("Content value is empty")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHints("ok")]
        [ArgHints("avatar", "Who to send the notecard to","AVATAR")]
        [ArgHints("content", "What to put in the notecard","Text","I am in a notecard")]
        [ArgHints("notecardname", "Whats the name of the notecard","Text","Custom notecard")]
        public object NotecardDirectSend(string avatar, string content, string notecardname)
        {
            if (SecondbotHelpers.notempty(notecardname) == false)
            {
                return Failure("notecardname value is empty", [avatar, content, notecardname]);
            }
            if (SecondbotHelpers.notempty(content) == false)
            {
                return Failure("content value is empty", [avatar, content, notecardname]);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [avatar, content, notecardname]);
            }
            bool result = master.BotClient.SendNotecard(notecardname, content, avataruuid);
            if(result == false)
            {
                return Failure("Failed to create/send notecard", [avatar, content, notecardname]);
            }
            return BasicReply("ok");
        }
    }

    public class NotecardReadReply
    {
        public string uuid;
        public string name;
        public string content;
        public bool status;
    }
}
