using OpenMetaverse;

namespace BetterSecondBot.Commands.CMD_Parcel
{
    class ParcelBuy : ParcelCommand_CheckParcel
    {
        public override string[] ArgTypes { get { return new[] { "Number" }; } }
        public override string[] ArgHints { get { return new[] { "expected price to pay" }; } }
        public override string Helpfile { get { return "Attempts to buy the parcel the bot is standing on, note: if no price expected price is set it will pay the listed price"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                int expected_price = -1;
                if (args.Length == 1)
                {
                    int.TryParse(args[0], out expected_price);
                }
                if ((targetparcel.SalePrice == expected_price) || (expected_price == -1))
                {
                    bot.GetClient.Parcels.Buy(bot.GetClient.Network.CurrentSim, targetparcel.LocalID, false, UUID.Zero, false, targetparcel.Area, targetparcel.SalePrice);
                    return true;
                }
                else
                {
                    return Failed("Expected price is not -1 or matchs: " + expected_price.ToString() + "");
                }
            }
            return false;
        }
    }
}
