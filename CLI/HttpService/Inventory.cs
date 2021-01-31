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

        [Route(HttpVerbs.Get, "/folders/{token}")]
        public string folders(string token)
        {
            if(tokens.Allow(token, getClientIP()) == true)
            {
                return HelperInventory.MapFolderJson(bot);
            }
            return "Token not accepted";
        }

        [Route(HttpVerbs.Get, "/contents/{folderUUID}/{token}")]
        public string Contents(string folderUUID, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                if (UUID.TryParse(folderUUID, out UUID folder) == true)
                {
                    return HelperInventory.MapFolderInventoryJson(bot, folder);
                }
                return "Invaild folder UUID";
            }
            return "Token not accepted";

        }
    }


}
