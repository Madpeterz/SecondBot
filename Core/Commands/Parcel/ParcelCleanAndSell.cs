using System;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;

namespace BetterSecondBot.Commands.CMD_Parcel
{
    public class ParcelCleanAndSell : ParcelCommand_RequirePerms_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Number" }; } }
        public override string[] ArgHints { get { return new[] { "L$ amount to mark the parcel for sale as" }; } }

        public override string Helpfile { get { return "Sets the current parcel for sale for L$[ARG 1] (Also marks the parcel for sale)"; } }

        public override void Callback(string[] args, EventArgs e)
        {
            if (base.CallFunction(args) == true)
            {
                ParcelObjectOwnersReplyEventArgs ObjectOwners = (ParcelObjectOwnersReplyEventArgs)e;
                List<UUID> owner_uuids = new List<UUID>();
                foreach (ParcelManager.ParcelPrimOwners owner in ObjectOwners.PrimOwners)
                {
                    owner_uuids.Add(owner.OwnerID);
                }
                if (owner_uuids.Count > 0)
                {
                    bot.GetClient.Parcels.ReturnObjects(bot.GetClient.Network.CurrentSim, targetparcel.LocalID, ObjectReturnType.List, owner_uuids);
                }
                int.TryParse(args[0], out int price);
                bot.GetClient.Parcels.ReleaseParcel(bot.GetClient.Network.CurrentSim, targetparcel.LocalID);
                targetparcel.SalePrice = price;
                targetparcel.AuthBuyerID = UUID.Zero;
                targetparcel.OwnerID = UUID.Zero;
                parcel_static.ParcelSetFlag(ParcelFlags.ForSale, targetparcel, true);
                parcel_static.ParcelSetFlag(ParcelFlags.ForSaleObjects, targetparcel, false);
                targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
                base.Callback(args, e, true);
            }
            else
            {
                base.Callback(args, e, false);
            }
        }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (int.TryParse(args[0], out int price) == true)
                {
                    if (price >= 1)
                    {
                        if (price <= 99999)
                        {
                            bot.GetClient.Parcels.ReleaseParcel(bot.GetClient.Network.CurrentSim, targetparcel.LocalID);
                            targetparcel.AuthBuyerID = bot.GetClient.Self.AgentID;
                            targetparcel.SalePrice = 0;
                            parcel_static.ParcelSetFlag(ParcelFlags.ForSale, targetparcel, true);
                            parcel_static.ParcelSetFlag(ParcelFlags.ForSaleObjects, targetparcel, false);
                            targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
                            Thread.Sleep(200);
                            bot.GetClient.Parcels.Buy(bot.GetClient.Network.CurrentSim, targetparcel.LocalID, false, UUID.Zero, false, targetparcel.Area, 0);
                            Thread.Sleep(1000);
                            if (bot.CreateAwaitEventReply("parcelobjectowners", this, args) == true)
                            {
                                bot.GetClient.Parcels.RequestObjectOwners(bot.GetClient.Network.CurrentSim, targetparcel.LocalID);
                                return true;
                            }
                            else
                            {
                                return Failed("Unable to send request for parcel object owners");
                            }
                        }
                        else
                        {
                            return Failed("Price must be 99999 or less");
                        }
                    }
                    else
                    {
                        return Failed("Price must be 1 or more");
                    }
                }
                else
                {
                    return Failed("Unable to process price");
                }
            }
            return false;
        }
    }
}
