using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{

    public class ParcelGetParcelName : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Smart"}; } }
        public override string[] ArgHints { get { return new[] { "Smart reply [Channel|Avatar|http url]" }; } }
        public override int MinArgs { get { return 1; } }
        public override string Helpfile { get { return "Updates the current parcels name"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                return bot.GetCommandsInterface.SmartCommandReply(args[0], bot.GetClient.Network.CurrentSim.Parcels[localid].Name, CommandName);
            }
            return false;
        }
    }
}
