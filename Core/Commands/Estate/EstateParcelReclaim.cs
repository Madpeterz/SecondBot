using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.Estate
{
    public class EstateParcelReclaim : CoreCommand
    {
        public override string Helpfile { get { return "Reclaims ownership of the current parcel"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.GetClient.Network.CurrentSim.IsEstateManager == true)
                {
                    int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                    bot.GetClient.Parcels.Reclaim(bot.GetClient.Network.CurrentSim, localid);
                    return true;
                }
                else
                {
                    return Failed("Not an estate manager here");
                }
            }
            return false;
        }
    }
}
