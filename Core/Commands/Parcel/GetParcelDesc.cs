using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{

    public class GetParcelDesc : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Smart"}; } }
        public override string[] ArgHints { get { return new[] { "Smart reply [Channel|Avatar|http url]" }; } }
        public override int MinArgs { get { return 1; } }
        public override string Helpfile { get { return "Fetchs the current parcels desc and sends it to the smart reply target on [ARG 1]"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == true)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(args[0], bot.GetClient.Network.CurrentSim.Parcels[localid].Desc, CommandName);
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
