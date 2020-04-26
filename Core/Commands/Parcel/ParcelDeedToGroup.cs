using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{

    public class ParcelDeedToGroup : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "Group UUID" }; } }
        public override int MinArgs { get { return 1; } }

        public override string Helpfile { get { return "transfers the current parcel ownership to a group"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID groupuuid) == true)
                {
                    int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                    Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
                    if (parcel_static.has_parcel_perm(p, bot) == true)
                    {
                        bot.GetClient.Parcels.DeedToGroup(bot.GetClient.Network.CurrentSim, localid, groupuuid);
                        return true;
                    }
                    else
                    {
                        return Failed("Incorrect perms to control parcel");
                    }
                }
                else
                {
                    return Failed("Unable to process group uuid");
                }
            }
            return false;
        }
    }
}
