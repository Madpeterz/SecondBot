using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{

    public class ParcelCleanAndSell : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Number" }; } }
        public override string[] ArgHints { get { return new[] { "L$ amount to mark the parcel for sale as" }; } }
        public override int MinArgs { get { return 1; } }

        public override string Helpfile { get { return "Sets the current parcel for sale for L$[ARG 1] (Also marks the parcel for sale)"; } }

        public override void Callback(string[] args, EventArgs e)
        {
            bool cleanup_ok = true;
            int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
            Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
            if (parcel_static.has_parcel_perm(p, bot) == true)
            {
                ParcelObjectOwnersReplyEventArgs ObjectOwners = (ParcelObjectOwnersReplyEventArgs)e;
                List<UUID> owner_uuids = new List<UUID>();
                foreach (ParcelManager.ParcelPrimOwners owner in ObjectOwners.PrimOwners)
                {
                    owner_uuids.Add(owner.OwnerID);
                }
                if (owner_uuids.Count > 0)
                {
                    if (parcel_static.has_parcel_perm(p, bot) == true)
                    {
                        bot.GetClient.Parcels.ReturnObjects(bot.GetClient.Network.CurrentSim, localid, ObjectReturnType.List, owner_uuids);
                    }
                    else
                    {
                        cleanup_ok = false;
                    }
                }
                if (cleanup_ok == true)
                {
                    int.TryParse(args[0], out int price);
                    bot.GetClient.Parcels.ReleaseParcel(bot.GetClient.Network.CurrentSim, localid);
                    p.SalePrice = price;
                    p.AuthBuyerID = UUID.Zero;
                    p.OwnerID = UUID.Zero;
                    parcel_static.ParcelSetFlag(ParcelFlags.ForSale, p, true);
                    parcel_static.ParcelSetFlag(ParcelFlags.ForSaleObjects, p, false);
                    p.Update(bot.GetClient.Network.CurrentSim, false);
                    base.Callback(args, e, true);
                }
                else
                {
                    base.Callback(args, e, false);
                }
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
                int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
                if (parcel_static.has_parcel_perm(p, bot) == true)
                {
                    if (int.TryParse(args[0], out int price) == true)
                    {
                        if (price >= 1)
                        {
                            if (price <= 99999)
                            {
                                bot.GetClient.Parcels.ReleaseParcel(bot.GetClient.Network.CurrentSim, localid);
                                p.AuthBuyerID = bot.GetClient.Self.AgentID;
                                p.SalePrice = 0;
                                parcel_static.ParcelSetFlag(ParcelFlags.ForSale, p, true);
                                parcel_static.ParcelSetFlag(ParcelFlags.ForSaleObjects, p, false);
                                p.Update(bot.GetClient.Network.CurrentSim, false);
                                Thread.Sleep(200);
                                bot.GetClient.Parcels.Buy(bot.GetClient.Network.CurrentSim, localid, false, UUID.Zero, false, p.Area, 0);
                                Thread.Sleep(1000);
                                if (bot.CreateAwaitEventReply("parcelobjectowners", this, args) == true)
                                {
                                    bot.GetClient.Parcels.RequestObjectOwners(bot.GetClient.Network.CurrentSim, localid);
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
                else
                {
                    return Failed("Incorrect perms to control parcel");
                }
            }
            return false;
        }
    }
}
