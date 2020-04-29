using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{

    public class GetParcelDesc : ParcelCommand_CheckParcel_1arg_smart
    {
        public override string Helpfile { get { return "Fetchs the current parcels desc and sends it to the smart reply target on [ARG 1]"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(args[0], targetparcel.Desc, CommandName);
            }
            return false;
        }
    }
}
