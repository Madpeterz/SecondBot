using OpenMetaverse;
using System.Collections.Generic;

namespace BSB.Commands.CMD_Parcel
{
    class ParcelReturnTargeted : ParcelCommand_RequirePerms_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]" }; } }
        public override string Helpfile { get { return "Returns all objects from the current parcel for the selected avatar"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID avatarUUID) == true)
                {
                    bot.GetClient.Parcels.ReturnObjects(bot.GetClient.Network.CurrentSim, targetparcel.LocalID, ObjectReturnType.None, new List<UUID>() { avatarUUID });
                    return true;
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
