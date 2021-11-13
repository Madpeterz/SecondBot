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
using System.Reflection;

namespace BetterSecondBot.HttpService
{
    public class HTTP_Avatars : WebApiControllerWithTokens
    {
        public HTTP_Avatars(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("an improved version of near me with extra details<br/>NearMeDetails is a object formated as follows<br/><ul><li>id</li><li>name</li><li>x</li><li>y</li><li>z</li><li>range</li></ul>")]
        [ReturnHints("array NearMeDetails")]
        [ReturnHintsFailure("Error not in a sim")]
        [Route(HttpVerbs.Get, "/NearmeWithDetails/{token}")]
        public object NearmeWithDetails(string token)
        {
            if (tokens.Allow(token, "avatars", "nearmewithdetails", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted", "nearmewithdetails");
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return BasicReply("Error not in a sim", "nearmewithdetails");
            }
            List<NearMeDetails> BetterNearMe = new List<NearMeDetails>();
            Dictionary<uint, Avatar> avcopy = bot.GetClient.Network.CurrentSim.ObjectsAvatars.Copy();
            foreach (Avatar av in avcopy.Values)
            {
                if (av.ID != bot.GetClient.Self.AgentID)
                {
                    NearMeDetails details = new NearMeDetails();
                    details.id = av.ID.ToString();
                    details.name = av.Name;
                    details.x = (int)Math.Round(av.Position.X);
                    details.y = (int)Math.Round(av.Position.Y);
                    details.z = (int)Math.Round(av.Position.Z);
                    details.range = (int)Math.Round(Vector3.Distance(av.Position, bot.GetClient.Self.SimPosition));
                    BetterNearMe.Add(details);
                }
            }
            return BasicReply(JsonConvert.SerializeObject(BetterNearMe), "nearmewithdetails");
        }

        [About("returns a list of all known avatars nearby")]
        [ReturnHints("array UUID = Name")]
        [ReturnHintsFailure("Error not in a sim")]
        [Route(HttpVerbs.Get, "/Nearme/{token}")]
        public object Nearme(string token)
        {
            if (tokens.Allow(token, "avatars", "nearme", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted", "nearme");
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return BasicReply("Error not in a sim", "nearme");
            }
            Dictionary<UUID, string> NearMe = new Dictionary<UUID, string>();
            Dictionary<uint, Avatar> avcopy = bot.GetClient.Network.CurrentSim.ObjectsAvatars.Copy();
            foreach (Avatar av in avcopy.Values)
            {
                if (av.ID != bot.GetClient.Self.AgentID)
                {
                    NearMe.Add(av.ID, av.Name);
                }
            }
            return BasicReply(JsonConvert.SerializeObject(NearMe), "nearme");
        }
    }
}
