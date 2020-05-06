using OpenMetaverse;
using System;
using System.Text;

namespace BSB.Commands.CMD_Parcel
{
    class ParcelPrimCountTotal : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply target: Channel,Avatar,HTTP url" }; } }
        public override string Helpfile { get { return "Returns the total number of prims on the current parcel as follows: DATA=SIMNAME,PARCELID,COUNT"; } }

        public override void Callback(string[] args, EventArgs e)
        {
            ParcelObjectOwnersReplyEventArgs ObjectOwners = (ParcelObjectOwnersReplyEventArgs)e;
            int total_count = 0;
            foreach (ParcelManager.ParcelPrimOwners owner in ObjectOwners.PrimOwners)
            {
                total_count += owner.Count;
            }
            StringBuilder reply = new StringBuilder();
            int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
            reply.Append("DATA=");
            reply.Append(bot.GetClient.Network.CurrentSim.Name);
            reply.Append(",");
            reply.Append(localid.ToString());
            reply.Append(",");
            reply.Append(total_count.ToString());
            base.Callback(args, e, bot.GetCommandsInterface.SmartCommandReply(args[0], reply.ToString(), CommandName));
        }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.GetCommandsInterface.SmartCommandReply(args[0], "Vaildate", CommandName, true) == true)
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
                else
                {
                    return Failed("Arg 1 is not a vaild smart target");
                }
            }
            return false;
        }
    }
}
