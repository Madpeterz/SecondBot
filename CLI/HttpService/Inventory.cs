using BSB.bottypes;
using BSB.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BetterSecondBot.HttpService
{
    public class SecondbotInventory : WebApiControllerWithTokens
    {
        public SecondbotInventory(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }


        [Route(HttpVerbs.Get, "/rezobject/{item}/{token}")]
        public object rezobject(string item, string token, [FormField] string newname)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                bool status = bot.GetCommandsInterface.Call("RezObject", item);
                return BasicReply(status.ToString());
            }
            return BasicReply("Token not accepted");
        }


        [Route(HttpVerbs.Post, "/rename/{item}/{token}")]
        public object rename(string item, string token, [FormField] string newname)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                bool status = bot.GetCommandsInterface.Call("RenameInventory", "12~#~" + item + "~#~" + newname);
                return BasicReply(status.ToString());
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/realuuid/{item}/{token}")]
        public object realuuid(string item, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
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

        [Route(HttpVerbs.Get, "/send/{item}/{avatar}/{token}")]
        public object send(string item, string avatar, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
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

        [Route(HttpVerbs.Get, "/delete/{item}/{isfolder}/{token}")]
        public object delete(string item,string isfolder, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
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

        [Route(HttpVerbs.Get, "/folders/{token}")]
        public object folders(string token)
        {
            if(tokens.Allow(token, getClientIP()) == true)
            {
                string reply = HelperInventory.MapFolderJson(bot);
                if(reply != null) return BasicReply(reply);
                return BasicReply("error");
            }
            return BasicReply("Token not accepted");
        }

        [Route(HttpVerbs.Get, "/contents/{folderUUID}/{token}")]
        public object Contents(string folderUUID, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
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
