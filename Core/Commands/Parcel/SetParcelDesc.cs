using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{

    public class SetParcelDesc : ParcelCommand_RequirePerms_1arg
    {
        public override string[] ArgTypes { get { return new[] { "String" }; } }
        public override string[] ArgHints { get { return new[] { "The new desc" }; } }
        public override string Helpfile { get { return "Updates the current parcels desc"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                targetparcel.Desc = args[0];
                targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
                return true;
            }
            return false;
        }
    }
}
