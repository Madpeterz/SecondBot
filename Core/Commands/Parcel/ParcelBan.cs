using OpenMetaverse;
using static OpenMetaverse.ParcelManager;

namespace BSB.Commands.CMD_Parcel
{
    public class ParcelBan : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar", "True|False" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]", "Type" }; } }
        public override string Helpfile { get { return "Bans an avatar [ARG 1] from a parcel<br/>by setting Type to False you can unban"; } }
        
        protected bool ban_target(OpenMetaverse.Parcel p, UUID target)
        {
            bool alreadyBanned = false;
            foreach (ParcelAccessEntry E in p.AccessBlackList)
            {
                if (E.AgentID == target)
                {
                    alreadyBanned = true;
                    break;
                }
            }
            if (alreadyBanned == true)
            {
                return Failed("Target is in the blacklist");
            }
            ParcelAccessEntry entry = new ParcelAccessEntry();
            entry.AgentID = target;
            entry.Flags = AccessList.Ban;
            entry.Time = new System.DateTime(3030, 03, 03);
            p.AccessBlackList.Add(entry);
            p.Update(bot.GetClient.Network.CurrentSim, false);
            return true;
        }

        protected bool unban_target(OpenMetaverse.Parcel p, UUID target)
        {
            bool alreadyBanned = false;
            ParcelAccessEntry removeme = new ParcelAccessEntry();
            foreach (ParcelAccessEntry E in p.AccessBlackList)
            {
                if (E.AgentID == target)
                {
                    alreadyBanned = true;
                    removeme = E;
                    break;
                }
            }
            if (alreadyBanned == false)
            {
                return Failed("Target is NOT in the blacklist");
            }
            p.AccessBlackList.Remove(removeme);
            p.Update(bot.GetClient.Network.CurrentSim, false);
            return true;
        }



        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == false)
            {
                return false;
            }
            if (UUID.TryParse(args[0], out UUID target) == false)
            {
                return Failed("Arg 0 requires UUID");
            }
            bool banuser = true;
            if (args.Length == 2)
            {
                if (args[1] == "False")
                {
                    banuser = false;
                }
            }
            int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
            if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == false)
            {
                return Failed("Unable to get parcel details");
            }
            OpenMetaverse.Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];  
            if (banuser == true)
            {
                return ban_target(p, target);
            }
            return unban_target(p, target);
        }
    }
}
