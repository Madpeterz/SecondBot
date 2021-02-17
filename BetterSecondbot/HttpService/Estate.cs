using BetterSecondBotShared.Json;
using BetterSecondBot.bottypes;
using EmbedIO;
using EmbedIO.Routing;
using System;
using Newtonsoft.Json;
using OpenMetaverse;
using System.Collections.Generic;
using EmbedIO.WebApi;
using BetterSecondBotShared.Static;

namespace BetterSecondBot.HttpService
{



    public class HttpAPIEstate : WebApiControllerWithTokens
    {
        public HttpAPIEstate(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens)
        {

        }

        [About("Sends the message to the current sim")]
        [ReturnHints("Not an estate manager here")]
        [ReturnHints("restarting")]
        [ReturnHints("canceled")]
        [ArgHints("delay", "URLARG", "How long to delay the restart for (30 to 240 secs) - defaults to 240 if out of bounds \n" +
            "set to 0 if your canceling!")]
        [ArgHints("mode", "URLARG", "true to start a restart, false to cancel")]
        [Route(HttpVerbs.Get, "/SimRestart/{delay}/{mode}/{token}")]
        public object SimRestart(string delay, string mode, string token)
        {
            if (tokens.Allow(token, "estate", "SimMessage", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.GetClient.Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here");
            }
            bool.TryParse(mode, out bool modeflag);
            if (modeflag == false)
            {
                bot.GetClient.Estate.CancelRestart();
                return BasicReply("canceled");
            }
            int delay_restart = 60;
            int.TryParse(delay, out delay_restart);
            if((delay_restart < 30) || (delay_restart > 240))
            {
                delay_restart = 240;
            }
            bot.GetClient.Estate.RestartRegion(delay_restart);
            return BasicReply("restarting");
        }

        [About("Sends the message to the current sim")]
        [ReturnHints("Not an estate manager here")]
        [ReturnHints("Message empty")]
        [ReturnHints("ok")]
        [ArgHints("message", "Text", "What the message is")]
        [Route(HttpVerbs.Post, "/SimMessage/{token}")]
        public object SimMessage([FormField] string message, string token)
        {
            if (tokens.Allow(token, "estate", "SimMessage", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (helpers.notempty(message) == false)
            {
                return Failure("Message empty");
            }
            if (bot.GetClient.Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here");
            }
            bot.GetClient.Estate.SimulatorMessage(message);
            return BasicReply("ok");
        }

        [About("Fetchs the regions map tile")]
        [ReturnHints("Unable to find region")]
        [ReturnHints("Texture UUID")]
        [ArgHints("regionname", "URLARG", "the name of the region we are fetching")]
        [Route(HttpVerbs.Get, "/GetSimTexture/{regionname}/{token}")]
        public object GetSimTexture(string regionname, string token)
        {
            if (tokens.Allow(token, "estate", "GetSimTexture", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.GetClient.Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region) == false)
            {
                return Failure("Unable to find region");
            }
            return BasicReply(region.MapImageID.ToString());
        }

        [About("Reclaims ownership of the current parcel")]
        [ReturnHints("Not an estate manager here")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/EstateParcelReclaim/{token}")]
        public Object EstateParcelReclaim(string token)
        {
            if (tokens.Allow(token, "estate", "EstateParcelReclaim", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.GetClient.Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here");
            }
            int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
            bot.GetClient.Parcels.Reclaim(bot.GetClient.Network.CurrentSim, localid);
            return BasicReply("ok");
        }

        [About("Reclaims ownership of the current parcel")]
        [ReturnHints("Not an estate manager here")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/GetSimGlobalPos/{regionname}/{token}")]
        public Object GetSimGlobalPos(string regionname, string token)
        {
            if (tokens.Allow(token, "estate", "GetSimGlobalPos", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.GetClient.Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region) == false)
            {
                return Failure("Unable to find region");
            }
            Dictionary<string, string> reply = new Dictionary<string, string>();
            reply.Add("region", regionname);
            reply.Add("X", region.X.ToString());
            reply.Add("Y", region.Y.ToString());
            return reply;
        }

        [About("Requests the estate banlist")]
        [ReturnHints("")]
        [Route(HttpVerbs.Get, "/GetEstateBanList/{token}")]
        public Object GetEstateBanList(string token)
        {
            if (tokens.Allow(token, "estate", "banlist", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            return BasicReply(JsonConvert.SerializeObject(bot._lastUpdatedSimBlacklist));
        }

        [About("Attempts to add/remove the avatar to/from the Estate banlist")]
        [ReturnHints("Unban request accepted")]
        [ReturnHints("Ban request accepted")]
        [ReturnHints("Unable to find avatar UUID")]
        [ReturnHints("Unable to process global value please use true or false")]
        [ReturnHints("Not an estate manager on region {REGIONNAME}")]
        [ArgHints("avatar", "URLARG", "the uuid avatar you wish to ban")]
        [ArgHints("mode", "URLARG", "What action would you like to take<br/>Defaults to remove if not given \"add\"")]
        [ArgHints("global", "URLARG", "if true this the ban/unban will be applyed to all estates the bot has access to")]
        [Route(HttpVerbs.Get, "/UpdateEstateBanlist/{avatar}/{mode}/{global}/{token}")]

        public Object UpdateEstateBanlist(string avatar, string mode, string global, string token)
        {
            if (tokens.Allow(token, "estate", "banlist-add", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.GetClient.Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager on region " + bot.GetClient.Network.CurrentSim.Name);
            }
            UUID avataruuid = UUID.Zero;
            if (UUID.TryParse(avatar, out avataruuid) == false)
            {
                return Failure("Unable to find avatar UUID");
            }
            bool globalban = false;
            if (bool.TryParse(global, out globalban) == false)
            {
                return Failure("Unable to process global value please use true or false");
            }
            if (mode != "add")
            {
                bot.GetClient.Estate.UnbanUser(avataruuid, globalban);
                return BasicReply("Unban request accepted");
            }
            bot.GetClient.Estate.BanUser(avataruuid, globalban);
            return BasicReply("Ban request accepted");
        }
    }
}
