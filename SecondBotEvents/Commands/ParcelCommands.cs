using Newtonsoft.Json;
using OpenMetaverse;
using OpenMetaverse.ImportExport.Collada14;
using SecondBotEvents.Services;
using Swan;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static OpenMetaverse.DirectoryManager;
using static OpenMetaverse.ParcelManager;
using static OpenMetaverse.Stats.UtilizationStatistics;

namespace SecondBotEvents.Commands
{
    public class ParcelPacketReply
    {
        public List<string> groupuuids = [];
        public List<ParcelPacketData> parcels = [];
    }
    public class ParcelPacketData
    {
        public int area;
        public int groupindex;
        public string name;
        public int maxprims;
        public int currentprims;
        public string startbox;
        public string endbox;
    }

    [ClassInfo("Control the land under our feet")]
    public partial class ParcelCommands(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Requests the current parcel local id")]
        [ReturnHints("parcel local id (int)")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        public object GetCurrentParcelId()
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            return BasicReply(targetparcel.LocalID.ToString());
        }

        [About("Requests the bot update its list of parcels ready for other commands")]
        [ReturnHints("ok")]
        public object UpdateListOfParcels()
        {
            GetClient().Parcels.RequestAllSimParcels(GetClient().Network.CurrentSim);
            return BasicReply("ok");
        }
        [About("Requests a packet blob for the parcels snapshot data")]
        [ReturnHints("json encoded object")]
        public object GetParcelListSnapshots()
        {
            Dictionary<string, string> reply = [];
            List<Parcel> Parcels = [.. GetClient().Network.CurrentSim.Parcels.Copy().Values];
            foreach (Parcel A in Parcels)
            {
                reply.Add(A.Name, A.SnapshotID.ToString());
            }
            return BasicReply(JsonConvert.SerializeObject(reply));
        }

        [About("Requests the current parcel size")]
        [ReturnHints("width,height as a csv")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        public object GetCurrentParcelSize()
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            Vector3 start = targetparcel.AABBMin;
            Vector3 end = targetparcel.AABBMax;
            Vector3 dif = end - start;
            return BasicReply(dif.X.ToString() + "," + dif.Y.ToString());
        }


        [About("requests the parcel layout as a SVG\n call UpdateListOfParcels first to update list before calling")]
        [ReturnHints("json encoded svg")]
        public object GetSimParcelLayers()
        {
            Dictionary<int, Parcel> data = GetClient().Network.CurrentSim.Parcels.Copy();
            Dictionary<string, Vector4> parcelbox = [];
            uint SimWidth = GetClient().Network.CurrentSim.SizeX;
            uint SimHeight = GetClient().Network.CurrentSim.SizeY;
            Dictionary<int,string> maps = [];


            int lastparcelid = 0;
            uint lowX = 0;
            uint lowY = 0;
            Random rnd = new();
            uint startX = 4;
            string svg = "<svg width=\"" + SimWidth + "\" height=\"" + SimHeight + "\" xmlns=\"http://www.w3.org/2000/svg\">";
            uint boxheight = 0;
            var color = "";
            Parcel P = null;
            int checkparcel = GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, new Vector3(startX, 4, 20));
            while (startX < SimWidth)
            {
                uint startY = 4;
                string parcelname;
                while (startY < SimHeight)
                {
                    checkparcel = GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, new Vector3(startX, startY, 20));
                    if ((checkparcel != lastparcelid) || (startY >= SimHeight))
                    {
                        if (lastparcelid == 0)
                        {
                            lastparcelid = checkparcel;
                            lowX = startX - 4;
                            lowY = startY - 4;
                        }
                        else
                        {
                            boxheight = (startY - 4) - lowY;
                            color = "";
                            if (maps.ContainsKey(lastparcelid) == false)
                            {
                                color = "rgb(" + rnd.Next(0, 256) + "," + rnd.Next(0, 256) + ",125)";
                                maps.Add(lastparcelid, color);
                            }
                            else
                            {
                                color = maps[lastparcelid];
                            }
                            parcelname = "?";
                            if (data.ContainsKey(lastparcelid) == true)
                            {
                                P = GetClient().Network.CurrentSim.Parcels[lastparcelid];
                                parcelname = P.Name;
                            }
                            svg = svg + "<rect data-parcel=\"" + parcelname + "\" width=\"4\" height=\"" + boxheight + "\" x=\"" + lowX + "\" y=\"" + lowY + "\" fill=\"" + color + "\" />";
                            lastparcelid = checkparcel;
                            lowX = startX - 4;
                            lowY = startY - 4;
                        }
                    }
                    startY += 4;
                }
                boxheight = (startY - 8) - lowY;
                color = "";
                if (maps.ContainsKey(lastparcelid) == false)
                {
                    color = "rgb(" + rnd.Next(0, 256) + "," + rnd.Next(0, 256) + ",125)";
                    maps.Add(lastparcelid, color);
                }
                else
                {
                    color = maps[lastparcelid];
                }
                parcelname = "?";
                if (data.ContainsKey(lastparcelid) == true)
                {
                    P = GetClient().Network.CurrentSim.Parcels[lastparcelid];
                    parcelname = P.Name;
                }
                svg = svg + "<rect data-parcel=\"" + parcelname + "\" width=\"4\" height=\"" + boxheight + "\" x=\"" + lowX + "\" y=\"" + lowY + "\" fill=\"" + color + "\" />";
                lastparcelid = 0;
                startX += 4;
            }
            svg += "</svg>";
            return BasicReply(svg);
        }


        [About("Requests a packet blob for the parcels in the current sim\n call UpdateListOfParcels first to update list before calling")]
        [ReturnHints("json encoded object")]
        public object GetListOfParcels()
        {
            ParcelPacketReply reply = new();
            List<Parcel> Parcels = [.. GetClient().Network.CurrentSim.Parcels.Copy().Values];
            foreach (Parcel A in Parcels)
            {
                int index = reply.groupuuids.IndexOf(A.GroupID.ToString());
                if (index == -1)
                {
                    reply.groupuuids.Add(A.GroupID.ToString());
                    index = reply.groupuuids.IndexOf(A.GroupID.ToString());
                }
                ParcelPacketData R = new()
                {
                    groupindex = index,
                    area = A.Area,
                    startbox = A.AABBMin.ToString(),
                    endbox = A.AABBMax.ToString(),
                    currentprims = A.TotalPrims,
                    maxprims = A.MaxPrims,
                    name = A.Name
                };
                reply.parcels.Add(R);
            }
            return BasicReply(JsonConvert.SerializeObject(reply));
        }

        [About("Cancels the current parcel the bot is in sale")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        public object CancelParcelSale()
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            ParcelStatic.ParcelSetFlag(ParcelFlags.ForSale, targetparcel, false);
            ParcelStatic.ParcelSetFlag(ParcelFlags.ForSaleObjects, targetparcel, false);
            targetparcel.Update(GetClient());
            return BasicReply("ok");
        }

        [About("Sets the current parcel for sale Also marks the parcel for sale")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Invaild amount")]
        [ArgHints("amount", "The amount to sell the parcel for from 1 to 9999999, can be zero if avatar is assigned.")]
        [ArgHints("avatar", "Avatar uuid or Firstname Lastname or \"none\" who we are locking the sale to")]
        public object SetParcelSale(string amount,string avatar)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if(tests.Key == false)
            {
                return Failure(tests.Value, [amount, avatar]);
            }
            if (int.TryParse(amount,out int amountvalue) == false)
            {
                return Failure("Invaild amount", [amount, avatar]);
            }

            int minAmount = 1;
            avataruuid = UUID.Zero;
            if (avatar != "none")
            {
                ProcessAvatar(avatar);
                if(avataruuid != UUID.Zero)
                {
                    minAmount = 0;
                }
            }
            if ((amountvalue < minAmount) || (amountvalue > 9999999))
            {
                return Failure("Invaild amount", [amount, avatar]);
            }
            targetparcel.SalePrice = amountvalue;
            targetparcel.AuthBuyerID = avataruuid;
            ParcelStatic.ParcelSetFlag(ParcelFlags.ForSale, targetparcel, true);
            ParcelStatic.ParcelSetFlag(ParcelFlags.ForSaleObjects, targetparcel, false);
            targetparcel.Update(GetClient());
            return BasicReply("ok", [amount, avatar]);
        }



        [About("Gets the parcel Dwell (Traffic) value and returns it via the reply target")]
        [ReturnHints("traffic value")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        public object GetParcelTraffic()
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            return BasicReply(targetparcel.Dwell.ToString());
        }

        [About("Gets the current parcel sale amount and target for sale and returns it via the reply target")]
        [ReturnHints("json object with sale amount and target for sale")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        public object GetParcelSaleDetails()
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            Dictionary<string, string> collection = [];
            collection.Add("ForSale", targetparcel.Flags.HasFlag(ParcelFlags.ForSale).ToString());
            collection.Add("Cost", targetparcel.SalePrice.ToString());
            collection.Add("SaleTarget", targetparcel.AuthBuyerID.ToString());
            return BasicReply(JsonConvert.SerializeObject(collection));
        }

        [About("Make a request to the landsale directory")]
        [ReturnHints("json object with data of parcels for sale, max of 100 retults per page")]
        [ReturnHintsFailure("Invaild filterRegion")]
        [ReturnHintsFailure("Invaild filterPrice")]
        [ReturnHintsFailure("filterPrice must be >= 0")]
        [ReturnHintsFailure("Invaild filterArea")]
        [ReturnHintsFailure("filterArea must be >= 0")]
        [ReturnHintsFailure("Invaild pageNum")]
        [ReturnHintsFailure("pageNum must be >= 0")]
        [ReturnHintsFailure("Invaild timeout")]
        [ReturnHintsFailure("timeout must be >= 2000 and <= 7000")]
        [ReturnHintsFailure("Timed out while doing land search")]
        [ArgHints("filterRegion", "Any,Auction,Mainland,Estate")]
        [ArgHints("filterPrice", "the max price to get results for: a number greater than or equal to zero")]
        [ArgHints("filterArea", "the max area to get results for: a number greater than or equal to zero")]
        [ArgHints("pageNum", "the page to load from (starting at zero): a number greater than or equal to zero")]
        [ArgHints("timeout", "how long to wait for results: a number greater in the range 2000 to 7000")]

        public object QueryLandSaleData(string filterRegion,string filterPrice, string filterArea, string pageNum, string timeout)
        {
            if (Enum.TryParse<DirectoryManager.SearchTypeFlags>(filterRegion, out DirectoryManager.SearchTypeFlags filterRegionType) == false)
            {
                return Failure("Invaild filterRegion");
            }
            if(int.TryParse(filterPrice, out int filterMaxPrice) == false)
            {
                return Failure("Invaild filterPrice");
            }
            if(filterMaxPrice < 0)
            {
                return Failure("filterPrice must be >= 0");
            }
            if (int.TryParse(filterArea, out int filterMinArea) == false)
            {
                return Failure("Invaild filterArea");
            }
            if (filterMaxPrice < 0)
            {
                return Failure("filterArea must be >= 0");
            }
            if (int.TryParse(pageNum, out int GetpageNum) == false)
            {
                return Failure("Invaild pageNum");
            }
            if (GetpageNum < 0)
            {
                return Failure("GetpageNum must be >= 0");
            }
            if (int.TryParse(timeout, out int timeoutMS) == false)
            {
                return Failure("Invaild timeout");
            }
            if ((timeoutMS < 2000) || (timeoutMS > 7000))
            {
                return Failure("timeout must be >= 2000 and <= 7000");
            }
            AutoResetEvent landresultsTimeout = new(false);
            List<string> collection = [];
            void LandSearchResultsReply(object sender, DirLandReplyEventArgs e)
            {
                int counter = (100 * GetpageNum);
                foreach(DirectoryParcel parcel in e.DirParcels)
                {
                    collection.Add(JsonConvert.SerializeObject(parcel));
                    counter++;
                }
                landresultsTimeout.Set();
            }
            
            GetClient().Directory.DirLandReply += LandSearchResultsReply;
            GetClient().Directory.StartLandSearch(filterRegionType, filterMaxPrice, filterMinArea, GetpageNum);
            bool haveResults = landresultsTimeout.WaitOne(timeoutMS, false);
            if (haveResults == false)
            {
                GetClient().Directory.DirLandReply -= LandSearchResultsReply;
                return Failure("Timed out while doing land search");
            }
            GetClient().Directory.DirLandReply -= LandSearchResultsReply;
            string reply = JsonConvert.SerializeObject(collection);

            return BasicReply(reply, [filterRegion, filterPrice, filterArea, pageNum, timeout]);
        }


        [About("Changes the parcel landing mode to point and sets the landing point")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Invaild amount")]
        [ArgHints("x", "X point for landing")]
        [ArgHints("y", "Y point for landing")]
        [ArgHints("z", "Z point for landing")]
        public object SetParcelLandingZone(string x, string y, string z)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value,  [x, y, z]);
            }
            if(int.TryParse(x,out int X) == false)
            {
                return Failure("Unable to process landing point x value", [x, y, z]);
            }
            if (int.TryParse(y, out int Y) == false)
            {
                return Failure("Unable to process landing point y value", [x, y, z]);
            }
            if (int.TryParse(z, out int Z) == false)
            {
                return Failure("Unable to process landing point z value", [x, y, z]);
            }
            targetparcel.Landing = LandingType.LandingPoint;
            targetparcel.UserLocation = new Vector3(X, Y, Z);
            targetparcel.Update(GetClient());
            return BasicReply("ok", [x, y, z]);
        }


        [About("Updates the current parcels name")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Parcel name is empty")]
        [ArgHints("name", "The new name of the parcel")]
        public object SetParcelName(string name)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [name]);
            }
            if (SecondbotHelpers.notempty(name) == false)
            {
                return Failure("Parcel name is empty", [name]);
            }
            targetparcel.Name = name;
            targetparcel.Update(GetClient());
            return BasicReply("ok", [name]);
        }

        [About("Updates the current parcels description")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ArgHints("desc", "The new desc of the parcel")]
        public object SetParcelDesc(string desc)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [desc]);
            }
            targetparcel.Desc = desc;
            targetparcel.Update(GetClient());
            return BasicReply("ok", [desc]);
        }

        [About("Fetchs the current parcels desc")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        public object GetParcelDesc()
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            return BasicReply(targetparcel.Desc);
        }

        [About("gets the flags for the parcel")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        public object GetParcelFlags()
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            Dictionary<string, ParcelFlags> flags = ParcelStatic.GetFlagsList();
            Dictionary<string, string> collection = [];
            foreach (ParcelFlags cfg in flags.Values)
            {
                collection.Add(cfg.ToString(), targetparcel.Flags.HasFlag(cfg).ToString());
            }
            return BasicReply(JsonConvert.SerializeObject(collection));
        }

        [About("Ejects an avatar")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild avatar")]
        [ArgHints("avatar", "uuid of the avatar or Firstname Lastname")]
        public object ParcelEject(string avatar)
        {
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar", [avatar]);
            }
            GetClient().Parcels.EjectUser(avataruuid, false);
            return BasicReply("ok", [avatar]);
        }

        [About("Abandons the parcel the bot is currently on, returning it to Linden's or Estate owner")]
        [ReturnHints("ok")]
        public object AbandonLand()
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            GetClient().Parcels.ReleaseParcel(GetClient().Network.CurrentSim, targetparcel.LocalID);
            return BasicReply("ok");
        }


        [About("Bans an avatar from a parcel")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Invaild avatar")]
        [ReturnHintsFailure("Avatar is in the blacklist")]
        [ArgHints("avatar", "uuid of the avatar or Firstname Lastname")]
        public object ParcelBan(string avatar)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [avatar]);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar", [avatar]);
            }
            bool alreadyBanned = false;
            foreach (ParcelAccessEntry E in targetparcel.AccessBlackList)
            {
                if (E.AgentID == avataruuid)
                {
                    alreadyBanned = true;
                    break;
                }
            }
            if (alreadyBanned == true)
            {
                return BasicReply("Avatar is in the blacklist", [avatar]);
            }
            ParcelAccessEntry entry = new()
            {
                AgentID = avataruuid,
                Flags = AccessList.Ban,
                Time = new System.DateTime(3030, 03, 03)
            };
            targetparcel.AccessBlackList.Add(entry);
            targetparcel.Update(GetClient());
            return BasicReply("ok", [avatar]);
        }

        [About("Unbans an avatar from a parcel")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Invaild avatar")]
        [ReturnHintsFailure("Avatar is already unbanned")]
        [ArgHints("avatar", "uuid of the avatar or Firstname Lastname")]
        public object ParcelUnBan(string avatar)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [avatar]);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar", [avatar]);
            }
            bool alreadyBanned = false;
            ParcelAccessEntry removeentry = new();
            foreach (ParcelAccessEntry E in targetparcel.AccessBlackList)
            {
                if (E.AgentID == avataruuid)
                {
                    alreadyBanned = true;
                    removeentry = E;
                    break;
                }
            }
            if (alreadyBanned == false)
            {
                return BasicReply("Avatar is already unbanned", [avatar]);
            }
            targetparcel.AccessBlackList.Remove(removeentry);
            targetparcel.Update(GetClient());
            return BasicReply("ok", [avatar]);
        }


        [About("Returns all objects on a parcel owned by selected avatar")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Land is not group owned and im not the owner")]
        [ReturnHintsFailure("I do not have access to the group")]
        [ReturnHintsFailure("No objects found to return")]
        [ReturnHintsFailure("Timeout waiting for object owners reply")]
        [ReturnHintsFailure("Invaild avatar")]
        [ReturnHints("ok")]
        [ArgHints("avatar", "uuid of the avatar or Firstname Lastname")]
        public object ReturnAllObjectsOnParcelByOwner(string avatar)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar", [avatar]);
            }
            return ReturnAllObjectsOnParcelReal(avataruuid);
        }

        [About("Returns all objects on a parcel")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Land is not group owned and im not the owner")]
        [ReturnHintsFailure("I do not have access to the group")]
        [ReturnHintsFailure("No objects found to return")]
        [ReturnHintsFailure("Timeout waiting for object owners reply")]
        [ReturnHints("ok")]
        public object ReturnAllObjectsOnParcel()
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            return ReturnAllObjectsOnParcelReal(UUID.Zero);
        }

        protected object ReturnAllObjectsOnParcelReal(UUID filter,bool repeat = false)
        {
            if (targetparcel.OwnerID != GetClient().Self.AgentID)
            {
                // im not the owner of the parcel
                if (targetparcel.GroupID == UUID.Zero)
                {
                    // the parcel is not group owned
                    return Failure("Land is not group owned and im not the owner");
                }
                if (targetparcel.GroupID != GetClient().Self.ActiveGroup)
                {
                    if (repeat == true)
                    {
                        return Failure("I do not have access to the group");
                    }
                    GetClient().Groups.ActivateGroup(targetparcel.GroupID);
                    return ReturnAllObjectsOnParcelReal(filter,true);
                }
            }
            var ownersList = new List<UUID>();
            using (var waitHandle = new AutoResetEvent(false))
            {
                EventHandler<ParcelObjectOwnersReplyEventArgs> handler = null;
                handler = (sender, e) =>
                {
                    // Unsubscribe immediately to avoid leaks
                    GetClient().Parcels.ParcelObjectOwnersReply -= handler;

                    foreach (ParcelPrimOwners A in e.PrimOwners)
                    {
                        if (A.OwnerID == UUID.Zero)
                        {
                            // Skip if the owner is zero
                            continue;
                        }
                        if (ownersList.Contains(A.OwnerID))
                        {
                            // Skip if the owner is already in the list
                            continue;
                        }
                        if(filter != UUID.Zero)
                        {
                            if (filter == A.OwnerID)
                            {
                                // Skip if the owner is not the filter target
                                continue;
                            }
                        }
                        ownersList.Add(A.OwnerID);
                    }
                    waitHandle.Set();
                };

                GetClient().Parcels.ParcelObjectOwnersReply += handler;
                GetClient().Parcels.RequestObjectOwners(GetClient().Network.CurrentSim, targetparcel.LocalID);

                // Wait for the event to be signaled, with a timeout to avoid deadlock
                if (!waitHandle.WaitOne(13000)) // 13 seconds timeout
                {
                    GetClient().Parcels.ParcelObjectOwnersReply -= handler;
                    return Failure("Timeout waiting for object owners reply");
                }
            }
            if (ownersList.Count == 0)
            {
                return Failure("No objects found to return");
            }
            GetClient().Parcels.ReturnObjects(
                GetClient().Network.CurrentSim,
                targetparcel.LocalID,
                ObjectReturnType.List,
                ownersList
            );

            return BasicReply("ok");
        }


        [About("Updates the current parcels name")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ArgHints("musicurl", "The new name of the parcel")]
        public object SetParcelMusic(string musicurl)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [musicurl]);
            }
            bool status = ParcelStatic.SetParcelMusic(GetClient(), targetparcel, musicurl);
            return BasicReply(status.ToString(), [musicurl]);
        }

        [About("Updates the current parcels name")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Incorrect perms to control parcel")]
        [ReturnHintsFailure("No accepted flags")]
        [ReturnHintsFailure("Unable to set flag ...")]
        [ReturnHintsFailure("Flag: ? is unknown")]
        [ReturnHintsFailure("Flag: ? missing \"=\"")]
        [ArgHints("escapedflagdata", "repeatable flag data split by ::: formated Flag=True|False")]
        public object SetParcelFlag(string escapedflagdata)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [escapedflagdata]);
            }
            List<string> acceptablewords = [];
            Dictionary<string, ParcelFlags> flags = ParcelStatic.GetFlagsList();
            acceptablewords.AddRange(["True", "False"]);

            Dictionary<string, bool> setflags = [];
            string[] args = escapedflagdata.Split(":::");
            foreach (string a in args)
            {
                string[] parts = a.Split('=', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2)
                {
                    LogFormater.Warn("Flag: " + a + " missing \"=\"");
                    continue;
                }
                if (flags.ContainsKey(parts[0]) == false)
                {
                    LogFormater.Warn("Flag: " + parts[0] + " is unknown");
                    continue;
                }
                if (acceptablewords.Contains(parts[1]) == false)
                {
                    LogFormater.Crit("Unable to set flag " + parts[0] + " to : " + parts[1] + "");
                }
                setflags.Add(parts[0], Convert.ToBoolean(parts[1]));
            }
            if (setflags.Count == 0)
            {
                return Failure("No accepted flags", [escapedflagdata]);
            }
            if (ParcelStatic.HasParcelPerm(targetparcel, GetClient()) == false)
            {
                return Failure("Incorrect perms to control parcel", [escapedflagdata]);
            }
            foreach (KeyValuePair<string, bool> cfg in setflags)
            {
                if (flags.ContainsKey(cfg.Key) == true)
                {
                    ParcelStatic.ParcelSetFlag(flags[cfg.Key], targetparcel, cfg.Value);
                }
            }
            targetparcel.Update(GetClient());
            return BasicReply("Applying perms", [escapedflagdata]);
        }

        [About("transfers the current parcel ownership to the assigned group")]
        [ReturnHints("ok")]
        [ArgHints("group", "The group uuid to assign")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Invaild group uuid")]
        [ReturnHintsFailure("Not in group")]
        public object ParcelDeedToGroup(string group)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [group]);
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group uuid", [group]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Not in group", [group]);
            }
            ParcelStatic.ParcelSetFlag(ParcelFlags.AllowDeedToGroup, targetparcel, true);
            targetparcel.GroupID = groupuuid;
            targetparcel.Update(GetClient());
            Thread.Sleep(500);
            GetClient().Parcels.DeedToGroup(GetClient().Network.CurrentSim, targetparcel.LocalID, groupuuid);
            return BasicReply("ok", [group]);
        }

        [About("Attempts to buy the parcel the bot is standing on, the amount must match the sale price for the land!")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ReturnHintsFailure("Parcel not for sale")]
        [ReturnHintsFailure("Parcel not for sale")]
        [ReturnHintsFailure("Parcel sale locked to other avatars")]
        [ReturnHintsFailure("Parcel sale price and amount do not match")]
        [ReturnHintsFailure("Invaild amount")]
        [ArgHints("amount", "amount to pay for the parcel (min 1, unless the land is set to the bot as the locked buyer then its 0)")]
        public object ParcelBuy(string amount)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [amount]);
            }
            if ((targetparcel.AuthBuyerID != UUID.Zero) && (targetparcel.AuthBuyerID != GetClient().Self.AgentID))
            {
                return Failure("Parcel sale locked to other avatars", [amount]);
            }
            int minAmount = 1;
            if (targetparcel.AuthBuyerID == GetClient().Self.AgentID)
            {
                minAmount = 0;
            }
            if (int.TryParse(amount, out int amountvalue) == false)
            {
                return Failure("Invaild amount", [amount]);
            }
            if (amountvalue < minAmount)
            {
                return Failure("Invaild amount", [amount]);
            };
            if (targetparcel.Flags.HasFlag(ParcelFlags.ForSale) == false)
            {
                return Failure("Parcel not for sale", [amount]);
            }
            if (targetparcel.SalePrice != amountvalue)
            {
                return Failure("Parcel sale price and amount do not match", [amount]);
            }
            GetClient().Parcels.Buy(GetClient().Network.CurrentSim, targetparcel.LocalID, false, UUID.Zero, false, targetparcel.Area, amountvalue);
            return BasicReply("ok", [amount]);
        }

        [About("Freezes an avatar")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild avatar UUID")]
        [ReturnHintsFailure("Invaild state")]
        [ArgHints("avatar", "avatar uuid or Firstname Lastname")]
        [ArgHints("state", "setting state to false will unfreeze or true to freeze")]
        public object ParcelFreeze(string avatar, string state)
        {
            if (bool.TryParse(state, out bool freezestate) == false)
            {
                return Failure("Invaild state", [avatar, state]);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID", [avatar, state]);
            }
            GetClient().Parcels.FreezeUser(avataruuid, freezestate);
            return BasicReply("ok", [avatar, state]);
        }


        [About("Fetchs the parcel ban list of the parcel the bot is currently on<br/>If the name returned is lookup the bot is currently requesting the avatar name")]
        [ReturnHintsFailure("json object: GetParcelBanlistObject")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        public object GetParcelBanlist()
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            GetParcelBanlistObject reply = new();
            int delays = 0;
            bool haslookup = true;
            while ((haslookup == true) && (delays < 3))
            {
                haslookup = false;
                foreach (ParcelAccessEntry e in targetparcel.AccessBlackList)
                {
                    if (e.AgentID != UUID.Zero)
                    {
                        break;
                    }
                }
                if(haslookup == true)
                {
                    Thread.Sleep(1000);
                    delays++;
                }
            }
            foreach (ParcelAccessEntry e in targetparcel.AccessBlackList)
            {
                reply.entrys.Add(e.AgentID, master.DataStoreService.GetAvatarName(e.AgentID));
            }
            reply.reportedEntrys = targetparcel.AccessBlackList.Count;
            reply.delay = delays * 1000;
            reply.parcelName = targetparcel.Name;
            reply.regionName = GetClient().Network.CurrentSim.Name;
            return BasicReply(JsonConvert.SerializeObject(reply));
        }

        [About("Returns a rezzed object")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild object uuid")]
        [ReturnHintsFailure("Unable to find object")]
        [ArgHints("objectuuid", "object UUID to unrez")]
        public object UnRezObject(string objectuuid)
        {
            if (UUID.TryParse(objectuuid, out UUID targetobject) == false)
            {
                return Failure("Invaild object uuid", [objectuuid]);
            }
            bool found = false;
            Dictionary<uint, Primitive> objects_copy = GetClient().Network.CurrentSim.ObjectsPrimitives.Copy();
            foreach (KeyValuePair<uint, Primitive> Obj in objects_copy)
            {
                if (Obj.Value.ID == targetobject)
                {
                    GetClient().Inventory.RequestDeRezToInventory(Obj.Key);
                    found = true;
                    break;
                }
            }
            if(found == false)
            {
                return Failure("Unable to find object", [objectuuid]);
            }
            return BasicReply("ok", [objectuuid]);
        }

        [About("Updates the current parcels media settings \n" +
            "MediaAutoScale=Bool (True|False)\n" +
            "MediaLoop=Bool (True|False)\n" +
            "MediaID=UUID (Texture)\n" +
            "MediaURL=String\n" +
            "MediaDesc=String\n" +
            "MediaHeight=Int (256 to 1024)\n" +
            "MediaWidth=Int (256 to 1024)\n" +
            "MediaType=String [\"IMG-PNG\",\"IMG-JPG\",\"VID-MP4\",\"VID-AVI\" or \"Custom-MIME_TYPE_CODE\"]"
            )]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Error not in a sim")]
        [ReturnHintsFailure("Parcel data not ready")]
        [ArgHints("escapedflagdata", "repeatable flag data split by ::: formated Flag=True|False")]
        public object ParcelSetMedia(string escapedflagdata)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel();
            if (tests.Key == false)
            {
                return Failure(tests.Value, [escapedflagdata]);
            }
            string[] args = escapedflagdata.Split(":::");
            foreach (string A in args)
            {
                string[] bits = A.Split('=');
                if (bits.Length == 2)
                {
                    if (bits[0] == "MediaType")
                    {
                        UpdateParcelMediaType(targetparcel, bits[1]);
                    }
                    else if ((bits[0] == "MediaWidth") || (bits[0] == "MediaHeight"))
                    {
                        if (int.TryParse(bits[1], out int size) == true)
                        {
                            if ((size >= 256) || (size <= 1024))
                            {
                                if (bits[0] == "MediaHeight")
                                {
                                    targetparcel.Media.MediaHeight = size;
                                }
                                else if (bits[0] == "MediaWidth")
                                {
                                    targetparcel.Media.MediaWidth = size;
                                }
                            }
                        }
                    }
                    else if (bits[0] == "MediaID")
                    {
                        if (UUID.TryParse(bits[1], out UUID texture) == true)
                        {
                            targetparcel.Media.MediaID = texture;
                        }
                    }
                    else if (bits[0] == "MediaURL")
                    {
                        targetparcel.Media.MediaURL = bits[1];
                    }
                    else if (bits[0] == "MediaDesc")
                    {
                        targetparcel.Media.MediaDesc = bits[1];
                    }
                    else if (bits[0] == "MediaAutoScale")
                    {
                        if (bool.TryParse(bits[1], out bool output) == true)
                        {
                            targetparcel.Media.MediaAutoScale = output;
                        }
                    }
                    else if (bits[0] == "MediaLoop")
                    {
                        if (bool.TryParse(bits[1], out bool output) == true)
                        {
                            targetparcel.Media.MediaLoop = output;
                        }
                    }
                }
            }
            targetparcel.Update(GetClient());
            return BasicReply("ok", [escapedflagdata]);
        }

        protected static bool UpdateParcelMediaType(Parcel targetparcel, string value)
        {
            if (value == "IMG-PNG")
            {
                targetparcel.Media.MediaType = "image/png";
            }
            else if (value == "IMG-JPG")
            {
                targetparcel.Media.MediaType = "image/jpeg";
            }
            else if (value == "VID-MP4")
            {
                targetparcel.Media.MediaType = "video/mp4";
            }
            else if (value == "VID-AVI")
            {
                targetparcel.Media.MediaType = "video/x-msvideo";
            }
            else if (value.StartsWith("Custom-") == true)
            {
                string mime = value;
                mime = mime.Replace("Custom-", "");
                targetparcel.Media.MediaType = mime;
            }
            else
            {
                return false;
            }
            return true;
        }
    }

    public class GetParcelBanlistObject
    {
        public Dictionary<UUID, string> entrys = [];
        public string parcelName = "";
        public string regionName = "";
        public int delay = 0;
        public int reportedEntrys = 0;
    }
}
