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
        public override string[] ArgHints { get { return new[] { "A vaild url" }; } }
        public override string Helpfile { get { return "Updates the current parcels music url"; } }

        public override bool CallFunction(string[] args)
        {
            int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
            Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
            if (parcel_static.has_parcel_perm(p, bot) == true)
            {
                if (args.Length == 0)
                {
                    if (p.MusicURL != "")
                    {
                        p.MusicURL = "";
                        p.Update(bot.GetClient.Network.CurrentSim, false);
                        return true;
                    }
                    else
                    {
                        return Failed("No change made");
                    }
                }
                else
                {
                    if (args[0].StartsWith("http") == true)
                    {
                        if (p.MusicURL != args[0])
                        {
                            p.MusicURL = args[0];
                            p.Update(bot.GetClient.Network.CurrentSim, false);
                            return true;
                        }
                        else
                        {
                            return Failed("No change made");
                        }
                    }
                    else
                    {
                        return Failed("Invaild url");
                    }
                }
            }
            else
            {
                return Failed("Required parcel perms missing!");
            }
        }
    }
}
