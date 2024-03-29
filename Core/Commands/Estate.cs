﻿using BetterSecondBotShared.Json;
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



    public class HTTP_Estate : WebApiControllerWithTokens
    {
        public HTTP_Estate(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Sends the message to the current sim")]
        [ReturnHints("restarting")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("canceled")]
        [ArgHints("delay", "URLARG", "How long to delay the restart for (30 to 240 secs) - defaults to 240 if out of bounds \n" +
            "set to 0 if your canceling!")]
        [ArgHints("mode", "URLARG", "true to start a restart, false to cancel")]
        [Route(HttpVerbs.Get, "/SimRestart/{delay}/{mode}/{token}")]
        public object SimRestart(string delay, string mode, string token)
        {
            if (tokens.Allow(token, "estate", "SimRestart", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "SimRestart", new [] { delay, mode });
            }
            if (bot.GetClient.Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", "SimRestart", new [] { delay, mode });
            }
            bool.TryParse(mode, out bool modeflag);
            if (modeflag == false)
            {
                bot.GetClient.Estate.CancelRestart();
                return BasicReply("canceled", "SimRestart", new [] { delay, mode });
            }
            int delay_restart = 60;
            int.TryParse(delay, out delay_restart);
            if((delay_restart < 30) || (delay_restart > 240))
            {
                delay_restart = 240;
            }
            bot.GetClient.Estate.RestartRegion(delay_restart);
            return BasicReply("restarting", "SimRestart", new [] { delay, mode });
        }

        [About("Sends the message to the current sim")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHints("ok")]
        [ArgHints("message", "Text", "What the message is")]
        [Route(HttpVerbs.Post, "/SimMessage/{token}")]
        public object SimMessage([FormField] string message, string token)
        {
            if (tokens.Allow(token, "estate", "SimMessage", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "SimMessage", new [] { message });
            }
            if (helpers.notempty(message) == false)
            {
                return Failure("Message empty", "SimMessage", new [] { message });
            }
            if (bot.GetClient.Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", "SimMessage", new [] { message });
            }
            bot.GetClient.Estate.SimulatorMessage(message);
            return BasicReply("ok", "SimMessage", new [] { message });
        }

        [About("Fetchs the regions map tile")]
        [ReturnHintsFailure("Unable to find region")]
        [ReturnHints("Texture UUID")]
        [ArgHints("regionname", "URLARG", "the name of the region we are fetching")]
        [Route(HttpVerbs.Get, "/GetSimTexture/{regionname}/{token}")]
        public object GetSimTexture(string regionname, string token)
        {
            if (tokens.Allow(token, "estate", "GetSimTexture", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GetSimTexture", new [] { regionname });
            }
            if (bot.GetClient.Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region) == false)
            {
                return Failure("Unable to find region", "GetSimTexture", new [] { regionname });
            }
            return BasicReply(region.MapImageID.ToString(), "GetSimTexture", new [] { regionname });
        }

        [About("Reclaims ownership of the current parcel")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/EstateParcelReclaim/{token}")]
        public Object EstateParcelReclaim(string token)
        {
            if (tokens.Allow(token, "estate", "EstateParcelReclaim", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "EstateParcelReclaim");
            }
            if (bot.GetClient.Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", "EstateParcelReclaim");
            }
            int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
            bot.GetClient.Parcels.Reclaim(bot.GetClient.Network.CurrentSim, localid);
            return BasicReply("ok", "EstateParcelReclaim");
        }

        [About("Reclaims ownership of the current parcel")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/GetSimGlobalPos/{regionname}/{token}")]
        public Object GetSimGlobalPos(string regionname, string token)
        {
            if (tokens.Allow(token, "estate", "GetSimGlobalPos", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GetSimGlobalPos", new [] { regionname });
            }
            if (bot.GetClient.Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region) == false)
            {
                return Failure("Unable to find region", "GetSimGlobalPos", new [] { regionname });
            }
            Dictionary<string, string> reply = new Dictionary<string, string>();
            reply.Add("region", regionname);
            reply.Add("X", region.X.ToString());
            reply.Add("Y", region.Y.ToString());
            SuccessNoReturn("GetSimGlobalPos", new [] { regionname });
            return reply;
        }

        [About("Requests the estate banlist")]
        [ReturnHints("ban list json")]
        [Route(HttpVerbs.Get, "/GetEstateBanList/{token}")]
        public Object GetEstateBanList(string token)
        {
            if (tokens.Allow(token, "estate", "banlist", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "banlist");
            }
            return BasicReply(JsonConvert.SerializeObject(bot._lastUpdatedSimBlacklist), "banlist");
        }

        [About("Attempts to add/remove the avatar to/from the Estate banlist")]
        [ReturnHints("Unban request accepted")]
        [ReturnHints("Ban request accepted")]
        [ReturnHintsFailure("Unable to find avatar UUID")]
        [ReturnHintsFailure("Unable to process global value please use true or false")]
        [ReturnHintsFailure("Not an estate manager on region {REGIONNAME}")]
        [ArgHints("avatar", "URLARG", "the uuid avatar you wish to ban")]
        [ArgHints("mode", "URLARG", "What action would you like to take<br/>Defaults to remove if not given \"add\"")]
        [ArgHints("global", "URLARG", "if true this the ban/unban will be applyed to all estates the bot has access to")]
        [Route(HttpVerbs.Get, "/UpdateEstateBanlist/{avatar}/{mode}/{global}/{token}")]

        public Object UpdateEstateBanlist(string avatar, string mode, string global, string token)
        {
            if (tokens.Allow(token, "estate", "UpdateEstateBanlist", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "UpdateEstateBanlist", new [] { avatar, mode, global });
            }
            if (bot.GetClient.Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager on region " + bot.GetClient.Network.CurrentSim.Name, "UpdateEstateBanlist", new [] { avatar, mode, global });
            }
            UUID avataruuid = UUID.Zero;
            if (UUID.TryParse(avatar, out avataruuid) == false)
            {
                return Failure("Unable to find avatar UUID", "UpdateEstateBanlist", new [] { avatar, mode, global });
            }
            bool globalban = false;
            if (bool.TryParse(global, out globalban) == false)
            {
                return Failure("Unable to process global value please use true or false", "UpdateEstateBanlist", new [] { avatar, mode, global });
            }
            if (mode != "add")
            {
                bot.GetClient.Estate.UnbanUser(avataruuid, globalban);
                return BasicReply("Unban request accepted", "UpdateEstateBanlist", new [] { avatar, mode, global });
            }
            bot.GetClient.Estate.BanUser(avataruuid, globalban);
            return BasicReply("Ban request accepted", "UpdateEstateBanlist", new [] { avatar, mode, global });
        }
    }
}
