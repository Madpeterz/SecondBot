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

        [About("Lists objects that are sculpty type in the current sim that the bot can see")]
        [ReturnHints("A json object of [UUID=Pos]")]
        [Route(HttpVerbs.Get, "/ListSculptys/{token}")]
        public object ListSculptys(string token)
        {
            Dictionary<string, string> Sculpts = new Dictionary<string, string>();
            Dictionary<uint, Primitive> objects_copy = bot.GetClient.Network.CurrentSim.ObjectsPrimitives.Copy();
            foreach (KeyValuePair<uint, Primitive> Obj in objects_copy)
            {
                if (Obj.Value.Type == PrimType.Sculpt)
                {
                    Sculpts.Add(Obj.Value.ID.ToString(), Obj.Value.Position.ToString());
                }
            }
            return BasicReply(JsonConvert.SerializeObject(Sculpts), "ListSculptys");
        }

        [About("Fetchs the current bot")]
        [ReturnHints("The build ID of the bot")]
        [Route(HttpVerbs.Get, "/Version/{token}")]
        public object Version(string token)
        {
            if (tokens.Allow(token, "info", "Version", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Version");
            }
            return BasicReply(bot.MyVersion, "Version");

        }

        [About("Fetchs the name of the bot")]
        [ReturnHints("Firstname Lastname")]
        [Route(HttpVerbs.Get, "/Name/{token}")]
        public object Name(string token)
        {
            if (tokens.Allow(token, "info", "Name", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Name");
            }
            return BasicReply(bot.GetClient.Self.FirstName + " " + bot.GetClient.Self.LastName, "Name");
        }

        [About("Fetchs the current parcels name")]
        [ReturnHints("Parcelname")]
        [ReturnHintsFailure("Error parcel not found")]
        [ReturnHintsFailure("Error not in a sim")]
        [Route(HttpVerbs.Get, "/ParcelName/{token}")]
        public object ParcelName(string token)
        {
            if (tokens.Allow(token, "info", "ParcelName", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "ParcelName");
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return Failure("Error not in a sim", "ParcelName");
            }
            int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
            if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == false)
            {
                return Failure("Error parcel not found", "ParcelName");
            }
            return BasicReply(bot.GetClient.Network.CurrentSim.Parcels[localid].Name, "ParcelName");
        }

        [About("Requests the current unixtime at the bot")]
        [ReturnHints("Unixtime")]
        [Route(HttpVerbs.Get, "/UnixTimeNow/{token}")]
        public object UnixTimeNow(string token)
        {
            if (tokens.Allow(token, "info", "UnixTimeNow", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "UnixTimeNow");
            }
            return BasicReply(helpers.UnixTimeNow().ToString(), "UnixTimeNow");
        }

        [About("Fetchs the current region name")]
        [ReturnHints("Regionname")]
        [ReturnHintsFailure("Error not in a sim")]
        [Route(HttpVerbs.Get, "/SimName/{token}")]
        public object SimName(string token)
        {
            if (tokens.Allow(token, "info", "SimName", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "SimName");
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return Failure("Error not in a sim", "SimName");
            }
            return BasicReply(bot.GetClient.Network.CurrentSim.Name, "SimName");
        }

        [About("Fetchs the current location of the bot")]
        [ReturnHints("array of X,Y,Z values")]
        [ReturnHintsFailure("Error not in a sim")]
        [Route(HttpVerbs.Get, "/GetPosition/{token}")]
        public object GetPosition(string token)
        {
            if (tokens.Allow(token, "info", "GetPosition", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GetPosition");
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return Failure("Error not in a sim", "GetPosition");
            }
            Dictionary<string, int> pos = new Dictionary<string, int>();
            pos.Add("x", (int)Math.Round(bot.GetClient.Self.SimPosition.X));
            pos.Add("y", (int)Math.Round(bot.GetClient.Self.SimPosition.Y));
            pos.Add("z", (int)Math.Round(bot.GetClient.Self.SimPosition.Z));
            return BasicReply(JsonConvert.SerializeObject(pos), "GetPosition");
        }
    }
}
