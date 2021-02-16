using BetterSecondBotShared.Json;
using BetterSecondBot.bottypes;
using EmbedIO;
using EmbedIO.Routing;
using System;
using Newtonsoft.Json;
using OpenMetaverse;

namespace BetterSecondBot.HttpService
{



    public class HttpAPIEstate : WebApiControllerWithTokens
    {
        public HttpAPIEstate(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens)
        {

        }

        [About("Requests the estate banlist")]
        [ReturnHints("")]
        [Route(HttpVerbs.Get, "/banlist/{token}")]
        public Object Banlist(string token)
        {
            if (tokens.Allow(token, "estate", "banlist", getClientIP()) == true)
            {
                return BasicReply(JsonConvert.SerializeObject(bot._lastUpdatedSimBlacklist));
            }
            return BasicReply("Token not accepted");
        }

        [About("Attempts to add/remove the avatar to/from the Estate banlist")]
        [ReturnHints("Request accepted")]
        [ReturnHints("Unable to find avatar UUID")]
        [ReturnHints("Unable to process global value please use true or false")]
        [ReturnHints("Not an estate manager on region {REGIONNAME}")]
        [ArgHints("avatar", "uuid", "the avatar you wish to ban")]
        [ArgHints("mode", "add|remove", "What action would you like to take<br/>Defaults to remove if not given \"add\"")]
        [ArgHints("global", "true|false", "if true this the ban/unban will be applyed to all estates the bot has access to")]
        [Route(HttpVerbs.Get, "/update-banlist/{avatar}/{mode}/{global}/{token}")]

        public Object UpdateBanlist(string avatar, string mode, string global, string token)
        {
            if (tokens.Allow(token, "estate", "banlist-add", getClientIP()) == true)
            {
                if(bot.GetClient.Network.CurrentSim.IsEstateManager == false)
                {
                    return BasicReply("Not an estate manager on region "+ bot.GetClient.Network.CurrentSim.Name);
                }
                UUID avataruuid = UUID.Zero;
                if(UUID.TryParse(avatar,out avataruuid) == false)
                {
                    return BasicReply("Unable to find avatar UUID");
                }
                bool globalban = false;
                if(bool.TryParse(global,out globalban) == false)
                {
                    return BasicReply("Unable to process global value please use true or false");
                }
                if(mode != "add")
                {
                    bot.GetClient.Estate.UnbanUser(avataruuid, globalban);
                }
                else
                {
                    bot.GetClient.Estate.BanUser(avataruuid, globalban);
                }
                return BasicReply("Request accepted");
            }
            return BasicReply("Token not accepted");
        }
    }
}
