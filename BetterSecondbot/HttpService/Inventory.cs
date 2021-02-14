using BetterSecondBot.bottypes;
using BetterSecondBot.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BetterSecondBot.HttpService
{
    public class HttpApiInventory : WebApiControllerWithTokens
    {
        public HttpApiInventory(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("rezs the item at the bots current location")]
        [ReturnHints("True|False")]
        [ArgHints("item", "URLARG", "UUID of item to rez")]
        [Route(HttpVerbs.Get, "/rezobject/{item}/{token}")]
        public object rezobject(string item, string token, [FormField] string newname)
        {
            if (tokens.Allow(token, "inventory", "rezobject", getClientIP()) == true)
            {
                bool status = bot.GetCommandsInterface.Call("RezObject", item);
                return BasicReply(status.ToString());
            }
            return BasicReply("Token not accepted");
        }

        [About("renames a folder or inventory item")]
        [ReturnHints("True|False")]
        [ArgHints("item", "URLARG", "UUID of item/folder to name")]
        [ArgHints("newname", "Text", "What we are changing it to")]
        [Route(HttpVerbs.Post, "/rename/{item}/{token}")]
        public object rename(string item, string token, [FormField] string newname)
        {
            if (tokens.Allow(token, "inventory", "rename", getClientIP()) == true)
            {
                bool status = bot.GetCommandsInterface.Call("RenameInventory", "12~#~" + item + "~#~" + newname);
                return BasicReply(status.ToString());
            }
            return BasicReply("Token not accepted");
        }

        [About("converts a inventory uuid to a realworld uuid<br/>Needed for texture preview")]
        [ReturnHints("Asset UUID")]
        [ReturnHints("Failed")]
        [ArgHints("item", "URLARG", "inventory level UUID of item")]
        [Route(HttpVerbs.Get, "/realuuid/{item}/{token}")]
        public object realuuid(string item, string token)
        {
            if (tokens.Allow(token, "inventory", "realuuid", getClientIP()) == true)
            {
                if (UUID.TryParse(item, out UUID itemUUID) == true)
                {
                    UUID reply = HelperInventory.GetAssetUUID(bot, itemUUID);
                    if (reply != UUID.Zero)
                    {
                        return BasicReply(reply.ToString());
                    }
                }
                return BasicReply("Failed");
            }
            return BasicReply("Token not accepted");
        }

        [About("sends a item to an avatar")]
        [ReturnHints("True|False")]
        [ReturnHints("Failed")]
        [ArgHints("item", "URLARG", "UUID of item")]
        [ArgHints("avatar", "URLARG", "a UUID or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/send/{item}/{avatar}/{token}")]
        public object send(string item, string avatar, string token)
        {
            if (tokens.Allow(token, "inventory", "send", getClientIP()) == true)
            {
                if (UUID.TryParse(item, out UUID itemUUID) == true)
                {
                    bool status = bot.GetCommandsInterface.Call("SendItem", avatar+"~#~"+item);
                    return BasicReply(status.ToString());
                }
                return BasicReply("Failed - item must be a vaild UUID");
            }
            return BasicReply("Token not accepted");
        }

        [About("Removes a item/folder from inventory (Make sure you set the isfolder flag correctly!)")]
        [ReturnHints("True|False")]
        [ReturnHints("Failed - isfolder must be True or False")]
        [ReturnHints("Failed - item must be a vaild UUID")]
        [ArgHints("item", "URLARG", "UUID of item")]
        [ArgHints("isfolder", "URLARG", "True or False if this is a folder")]
        [Route(HttpVerbs.Get, "/delete/{item}/{isfolder}/{token}")]
        public object delete(string item,string isfolder, string token)
        {
            if (tokens.Allow(token, "inventory", "delete", getClientIP()) == true)
            {
                if (UUID.TryParse(item, out UUID itemUUID) == true)
                {
                    if (bool.TryParse(isfolder, out bool asfolder) == true)
                    {
                        bool status = false;
                        if (asfolder == true) status = bot.GetCommandsInterface.Call("DeleteInventoryFolder", item);
                        else status = bot.GetCommandsInterface.Call("DeleteInventoryItem", item);
                        return BasicReply(status.ToString());
                    }
                    return BasicReply("Failed - isfolder must be True or False");
                }
                return BasicReply("Failed - item must be a vaild UUID");
            }
            return BasicReply("Token not accepted");
        }

        [About("Requests the inventory folder layout as a json object InventoryMapFolder<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>subfolders: InventoryMapFolder[]</li></ul>")]
        [ReturnHints("array of InventoryMapFolder")]
        [ReturnHints("Error")]
        [Route(HttpVerbs.Get, "/folders/{token}")]
        public object folders(string token)
        {
            if(tokens.Allow(token, "inventory", "folders", getClientIP()) == true)
            {
                string reply = HelperInventory.MapFolderJson(bot);
                if(reply != null) return BasicReply(reply);
                return BasicReply("Error");
            }
            return BasicReply("Token not accepted");
        }

        [About("Requests the contents of a folder as an array of InventoryMapItem<br/>Formated as follows<br/>InventoryMapItem<br/><ul><li>id: UUID</li><li>name: String</li><li>typename: String</li></ul>")]
        [ReturnHints("array of InventoryMapItem")]
        [ReturnHints("Invaild folder UUID")]
        [ArgHints("folderUUID", "URLARG", "the folder to fetch (Found via: inventory/folders)")]
        [Route(HttpVerbs.Get, "/contents/{folderUUID}/{token}")]
        public object Contents(string folderUUID, string token)
        {
            if (tokens.Allow(token, "inventory", "contents", getClientIP()) == true)
            {
                if (UUID.TryParse(folderUUID, out UUID folder) == true)
                {
                    return BasicReply(HelperInventory.MapFolderInventoryJson(bot, folder));
                }
                return BasicReply("Invaild folder UUID");
            }
            return BasicReply("Token not accepted");

        }

    }

}
