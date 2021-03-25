using BetterSecondBot.bottypes;
using BetterSecondBot.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Newtonsoft.Json;
using BetterSecondBotShared.Static;

namespace BetterSecondBot.HttpService
{
    public class HTTP_Info : WebApiControllerWithTokens
    {
        public HTTP_Info(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Fetchs the current bot")]
        [ReturnHints("The build ID of the bot")]
        [Route(HttpVerbs.Get, "/Version/{token}")]
        public object Version(string token)
        {
            if (tokens.Allow(token, "info", "Version", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Version", new [] { });
            }
            return BasicReply(bot.MyVersion, "Version", new [] { });

        }

        [About("Fetchs the name of the bot")]
        [ReturnHints("Firstname Lastname")]
        [Route(HttpVerbs.Get, "/Name/{token}")]
        public object Name(string token)
        {
            if (tokens.Allow(token, "info", "Name", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Name", new [] { });
            }
            return BasicReply(bot.GetClient.Self.FirstName + " " + bot.GetClient.Self.LastName, "Name", new [] { });
        }

        [About("Fetchs the current parcels name")]
        [ReturnHints("Parcelname")]
        [ReturnHints("Error parcel not found")]
        [ReturnHints("Error not in a sim")]
        [Route(HttpVerbs.Get, "/ParcelName/{token}")]
        public object ParcelName(string token)
        {
            if (tokens.Allow(token, "info", "ParcelName", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "ParcelName", new [] { });
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return Failure("Error not in a sim", "ParcelName", new [] { });
            }
            int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
            if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == false)
            {
                return Failure("Error parcel not found", "ParcelName", new [] { });
            }
            return BasicReply(bot.GetClient.Network.CurrentSim.Parcels[localid].Name, "ParcelName", new [] { });
        }

        [About("Requests the current unixtime at the bot")]
        [ReturnHints("Unixtime")]
        [Route(HttpVerbs.Get, "/UnixTimeNow/{token}")]
        public object UnixTimeNow(string token)
        {
            if (tokens.Allow(token, "info", "UnixTimeNow", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "UnixTimeNow", new [] { });
            }
            return BasicReply(helpers.UnixTimeNow().ToString(), "UnixTimeNow", new [] { });
        }

        [About("Fetchs the current region name")]
        [ReturnHints("Regionname")]
        [ReturnHints("Error not in a sim")]
        [Route(HttpVerbs.Get, "/SimName/{token}")]
        public object SimName(string token)
        {
            if (tokens.Allow(token, "info", "SimName", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "SimName", new [] { });
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return Failure("Error not in a sim", "SimName", new [] { });
            }
            return BasicReply(bot.GetClient.Network.CurrentSim.Name, "SimName", new [] { });
        }

        [About("Fetchs the current location of the bot")]
        [ReturnHints("array of X,Y,Z values")]
        [ReturnHints("Error not in a sim")]
        [Route(HttpVerbs.Get, "/GetPosition/{token}")]
        public object GetPosition(string token)
        {
            if (tokens.Allow(token, "info", "GetPosition", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GetPosition", new [] { });
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return Failure("Error not in a sim", "GetPosition", new [] { });
            }
            Dictionary<string, int> pos = new Dictionary<string, int>();
            pos.Add("x", (int)Math.Round(bot.GetClient.Self.SimPosition.X));
            pos.Add("y", (int)Math.Round(bot.GetClient.Self.SimPosition.Y));
            pos.Add("z", (int)Math.Round(bot.GetClient.Self.SimPosition.Z));
            return BasicReply(JsonConvert.SerializeObject(pos), "GetPosition", new [] { });
        }
    }
}
