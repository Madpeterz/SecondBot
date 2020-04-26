using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{

    public class AbandonLand : CoreCommand
    {
        public override string Helpfile { get { return "Abandons the parcel the bot is currently on, returning it to Linden's or Estate owner"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
                if (parcel_static.has_parcel_perm(p, bot) == true)
                {
                    bot.GetClient.Parcels.ReleaseParcel(bot.GetClient.Network.CurrentSim, localid);
                    return true;
                }
                else
                {
                    return Failed("Incorrect perms to control parcel");
                }
            }
            return false;
        }
    }
}
