using BetterSecondBotShared.Static;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.CMD_Parcel
{
    class ParcelSetMusicURL : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "String" }; } }
        public override string[] ArgHints { get { return new[] { "A vaild url or \"clear\"" }; } }
        public override int MinArgs { get { return 1; } }
        public override string Helpfile { get { return "Updates the current parcels music url"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
                if (parcel_static.has_parcel_perm(p, bot) == true)
                {
                    if(args[0] == "clear")
                    {
                        p.MusicURL = "";
                    }
                    else if(args[0].StartsWith("http") == true)
                    {
                        p.MusicURL = args[0];
                    }
                    p.Update(bot.GetClient.Network.CurrentSim, false);
                }
                else
                {
                    return Failed("Do not have expected perms for this parcel!");
                }
            }
            return false;
        }
    }
}
