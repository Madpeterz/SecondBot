using BSB.bottypes;
using BSB.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Newtonsoft.Json;
using static OpenMetaverse.ParcelManager;

namespace BetterSecondBot.HttpService
{
    class HttpApiParcelEstate: WebApiControllerWithTokens
    {
        public HttpApiParcelEstate(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Fetchs the parcel ban list of the parcel the bot is currently on<br/>If the name returned is lookup the bot is currently requesting the avatar name")]
        [ReturnHints("array of UUID=Name")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [Route(HttpVerbs.Get, "/parcelbanlist/{token}")]
        public object parcelbanlist(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                if (bot.GetClient.Network.CurrentSim != null)
                {
                    int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                    Parcel targetparcel = null;
                    if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == true)
                    {
                        targetparcel = bot.GetClient.Network.CurrentSim.Parcels[localid];
                        Dictionary<string, string> reply = new Dictionary<string, string>();
                        foreach (ParcelAccessEntry e in targetparcel.AccessBlackList)
                        {
                            string name = bot.FindAvatarKey2Name(e.AgentID);
                            reply.Add(e.AgentID.ToString(), name);
                        }
                        return BasicReply(JsonConvert.SerializeObject(reply));
                    }
                    return BasicReply("Parcel data not ready");
                }
                return BasicReply("Error not in a sim");
            }
            return BasicReply("Token not accepted");
        }
    }
}
