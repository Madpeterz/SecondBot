using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{

    public class SetParcelSale : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Number","Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "L$ amount to mark the parcel for sale as (Range: 1 to 99999 unless [ARG 2] is set)", "Avatar who can buy the land (If set allows you to set the sell price to zero)" }; } }
        public override int MinArgs { get { return 1; } }

        public override string Helpfile { get { return "Sets the current parcel for sale for L$[ARG 1] (Also marks the parcel for sale)"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == true)
                {
                    Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
                    if (parcel_static.has_parcel_perm(p, bot) == true)
                    {
                        if (int.TryParse(args[0], out int price) == true)
                        {
                            UUID target_buyer = UUID.Zero;
                            int low_limit = 1;
                            if (args.Length == 2)
                            {
                                if (UUID.TryParse(args[1], out target_buyer) == true)
                                {
                                    low_limit = 0;
                                }
                            }
                            if (price >= low_limit)
                            {
                                if (price <= 99999)
                                {
                                    p.SalePrice = price;
                                    p.AuthBuyerID = target_buyer;
                                    parcel_static.ParcelSetFlag(ParcelFlags.ForSale, p, true);
                                    parcel_static.ParcelSetFlag(ParcelFlags.ForSaleObjects, p, false);
                                    p.Update(bot.GetClient.Network.CurrentSim, false);
                                    return true;
                                }
                                else
                                {
                                    return Failed("Price must be 99999 or less");
                                }
                            }
                            else
                            {
                                return Failed("Price must be " + low_limit.ToString() + " or more");
                            }
                        }
                        else
                        {
                            return Failed("Unable to process price");
                        }
                    }
                    else
                    {
                        return Failed("Incorrect perms to control parcel");
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
