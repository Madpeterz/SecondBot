using OpenMetaverse;
using System;
using System.Text;

namespace BSB.Commands.CMD_Parcel
{
    class ParcelPrimCountTotal : CoreCommand_SmartReply_1arg
    {
        public override string Helpfile { get { return "Returns the total number of prims on the current parcel as follows: DATA=SIMNAME,PARCELID,COUNT"; } }

        public override void Callback(string[] args, EventArgs e)
        {
            ParcelObjectOwnersReplyEventArgs ObjectOwners = (ParcelObjectOwnersReplyEventArgs)e;
            int total_count = 0;
            foreach (ParcelManager.ParcelPrimOwners owner in ObjectOwners.PrimOwners)
            {
                total_count += owner.Count;
            }
            int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
            collection.Add("sim", bot.GetClient.Network.CurrentSim.Name);
            collection.Add("parcelid", localid.ToString());
            collection.Add("count", total_count.ToString());
            base.Callback(args, e, bot.GetCommandsInterface.SmartCommandReply(true,args[0], "ok", CommandName,collection));
        }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == true)
                {
                    if (bot.CreateAwaitEventReply("parcelobjectowners", this, args) == true)
                    {
                        bot.GetClient.Parcels.RequestObjectOwners(bot.GetClient.Network.CurrentSim, localid);
                        return true;
                    }
                    else
                    {
                        return Failed("Unable to await reply");
                    }
                }
                else
                {
                    return Failed("Unable to find parcel in memory, please wait and try again");
                }
            }
            return false;
        }
    }
}
