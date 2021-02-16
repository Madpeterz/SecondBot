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
        [ReturnHints("Error not in a sim")]
        [Route(HttpVerbs.Get, "/NearmeWithDetails/{token}")]
        public object NearmeWithDetails(string token)
        {
            if (tokens.Allow(token, "avatars", "nearmewithdetails", getClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return BasicReply("Error not in a sim");
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
            return BasicReply(JsonConvert.SerializeObject(BetterNearMe));
        }

        [About("returns a list of all known avatars nearby")]
        [ReturnHints("array UUID = Name")]
        [ReturnHints("Error not in a sim")]
        [Route(HttpVerbs.Get, "/Nearme/{token}")]
        public object Nearme(string token)
        {
            if (tokens.Allow(token, "avatars", "nearme", getClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            if (bot.GetClient.Network.CurrentSim == null)
            {
                return BasicReply("Error not in a sim");
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
            return BasicReply(JsonConvert.SerializeObject(NearMe));
        }


        [About("Makes the bot pay a avatar")]
        [ReturnHints("Accepted")]
        [ReturnHints("avatar lookup")]
        [ReturnHints("Invaild amount")]
        [ReturnHints("Transfer funds to avatars disabled")]
        [Route(HttpVerbs.Get, "/PayAvatar/{avatar}/{amount}/{token}")]
        public object PayAvatar(string avatar,string amount,string token)
        {
            if (tokens.Allow(token, "avatars", "payavatar", getClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            if (bot.GetAllowFunds == false)
            {
                return BasicReply("Transfer funds to avatars disabled");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return BasicReply("avatar lookup");
            }
            if (int.TryParse(amount, out int amountvalue) == false)
            {
                return BasicReply("Invaild amount");
            }
            if(amountvalue < 1)
            {
                return BasicReply("Invaild amount");
            }
            bot.GetClient.Self.GiveAvatarMoney(avataruuid, amountvalue);
            return BasicReply("Accepted");
        }
    }
}
