using BetterSecondBotShared.Static;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.CMD_Parcel
{
    class ParcelBuy : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Number" }; } }
        public override string[] ArgHints { get { return new[] { "expected price to pay" }; } }
        public override string Helpfile { get { return "Attempts to buy the parcel the bot is standing on, note: if no price expected price is set it will pay the listed price"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == true)
                {
                    Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
                    int expected_price = -1;
                    if (args.Length == 1)
                    {
                        int.TryParse(args[0], out expected_price);
                    }
                    if ((p.SalePrice == expected_price) || (expected_price == -1))
                    {
                        bot.GetClient.Parcels.Buy(bot.GetClient.Network.CurrentSim, localid, false, UUID.Zero, false, p.Area, p.SalePrice);
                        return true;
                    }
                    else
                    {
                        return Failed("Expected price is not -1 or matchs: " + expected_price.ToString() + "");
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
