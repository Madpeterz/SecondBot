using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.CMD_Parcel
{
    class ParcelReturnTargeted : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]" }; } }
        public override int MinArgs { get { return 1; } }
        public override string Helpfile { get { return "Returns all objects from the current parcel for the selected avatar"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID avatarUUID) == true)
                {
                    int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                    if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == true)
                    {
                        Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
                        if (parcel_static.has_parcel_perm(p, bot) == true)
                        {
                            bot.GetClient.Parcels.ReturnObjects(bot.GetClient.Network.CurrentSim, localid, ObjectReturnType.None, new List<UUID>() { avatarUUID });
                            return true;
                        }
                        else
                        {
                            return Failed("Parcel perm check failed");
                        }
                    }
                    else
                    {
                        return Failed("Unable to find parcel in memory, please wait and try again");
                    }
                }
                else
                {
                    return Failed("Invaild avatar UUID");
                }
            }
            return false;
        }
    }
}
