using System.Threading;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{
    public class ParcelDeedToGroup : ParcelCommand_RequirePerms_1arg_Group
    {
        public override string Helpfile { get { return "transfers the current parcel ownership to a group"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                targetparcel.GroupID = groupuuid;
                targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
                Thread.Sleep(1000);
                bot.GetClient.Parcels.DeedToGroup(bot.GetClient.Network.CurrentSim, targetparcel.LocalID, groupuuid);
                return true;
            }
            return false;
        }
    }
}
