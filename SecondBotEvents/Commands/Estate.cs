using Newtonsoft.Json;
using OpenMetaverse;
using OpenMetaverse.Assets;
using SecondBotEvents.Services;
using Swan;
using System;
using System.Collections.Generic;
using static Betalgo.Ranul.OpenAI.ObjectModels.RealtimeModels.RealtimeEventTypes;
using static OpenMetaverse.EstateTools;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Look after a sim as the estate manager")]
    public class Estate(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Sets the estate covenant for the current sim")]
        [ReturnHints("changed Covenant to notecard: <notecard name>")]
        [ReturnHintsFailure("Invaild inventoryNotecardUUID")]
        [ReturnHintsFailure("Unable to find notecard")]
        [ReturnHintsFailure("Item is not a notecard")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ArgHints("inventoryNotecardUUID", "the UUID of the notecard you wish to set as the estate covenant", "UUID")]
        [CmdTypeSet()]
        public object SetEstateCovenant(string inventoryNotecardUUID)
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", [inventoryNotecardUUID]);
            }
            if (UUID.TryParse(inventoryNotecardUUID, out UUID targetitem) == false)
            {
                return Failure("Invaild inventoryNotecardUUID", [inventoryNotecardUUID]);
            }
            InventoryNode find = null;
            if (GetClient().Inventory._Store.Items.ContainsKey(targetitem))
            {
                find = GetClient().Inventory._Store.Items[targetitem];
            }
            InventoryItem itm = null;
            if (find != null)
            {
                if (find.Data is InventoryItem itemfound)
                {
                    itm = itemfound;
                }
            }
            if (itm == null)
            {
                // cant get the item from store try and download it
                itm = GetClient().Inventory.FetchItem(targetitem, GetClient().Self.AgentID, TimeSpan.FromSeconds(15));
            }
            if (itm == null)
            {
                return Failure("Unable to find notecard",[inventoryNotecardUUID]);
            }
            if (itm.AssetType != AssetType.Notecard)
            {
                return Failure("Item is not a notecard", [inventoryNotecardUUID]);
            }
            GetClient().Estate.EstateOwnerMessage("estatechangecovenantid", itm.AssetUUID.ToString());
            return BasicReply("changed Covenant to notecard: "+itm.Name, [inventoryNotecardUUID]);
        }

        protected KeyValuePair<bool,string> GetEstateNotecard(UUID notecarduuid)
        {

            KeyValuePair<bool, string> reply = new KeyValuePair<bool, string>(false, "!ERROR! - unable to read notecard"); ;
            using (var waitHandle = new System.Threading.ManualResetEventSlim(false))
            {
                AssetDownload transfer = new AssetDownload
                {
                    ID = UUID.Random(),
                    AssetID = notecarduuid,
                    AssetType = AssetType.Notecard,
                    Priority = 100.0f + (true ? 1.0f : 0.0f),
                    Channel = ChannelType.Asset,
                    Source = SourceType.SimEstate,
                    Simulator = GetClient().Network.CurrentSim,
                    Callback = (AssetDownload transfer, Asset asset) =>
                    {
                        if (transfer.Success == false)
                        {
                            reply = new KeyValuePair<bool, string>(false, "!ERROR! - unable to read notecard");
                        }
                        else
                        {
                            AssetNotecard note = (AssetNotecard)asset;
                            note.Decode();
                            string contents = "";
                            for (int index = 0; index < note.BodyText.Length; index++)
                            {
                                char c = note.BodyText[index];
                                if ((int)c == 0xdbc0)
                                {
                                    contents = "[ATTACHMENT]";
                                }
                                else
                                {
                                    contents += c;
                                }
                            }
                            reply = new KeyValuePair<bool, string>(true, contents);
                        }
                        waitHandle.Set();
                    }
                };
                GetClient().Assets.RequestEstateAsset(transfer, EstateAssetType.Covenant);
                // Wait up to 10 seconds for the reply
                if (!waitHandle.Wait(10000))
                {
                    reply = new KeyValuePair<bool, string>(false, "Timed out waiting for RequestEstateAsset");
                }
                return reply;
            }
        }


        [About("Gets the estate covenant for the current sim")]
        [ReturnHints("Estate covenant json object with Fetched, ID, Timestamp, EstateName, EstateOwnerID and CovenantText")]
        [ReturnHintsFailure("Timed out waiting for EstateCovenantReply")]
        [ReturnHintsFailure("No estate covenant reply received")]
        [ReturnHintsFailure("Failed to fetch covenant asset")]
        [ReturnHintsFailure("Failed to fetch covenant asset is null")]
        [CmdTypeGet()]
        public object GetEstateCovenant()
        {
            EstateCovenantReplyEventArgs result = null;
            using (var waitHandle = new System.Threading.ManualResetEventSlim(false))
            {
                EventHandler<EstateCovenantReplyEventArgs> handler = null;
                handler = (sender, e) =>
                {
                    GetClient().Estate.EstateCovenantReply -= handler;
                    result = e;
                    waitHandle.Set();
                };

                GetClient().Estate.EstateCovenantReply += handler;
                GetClient().Estate.RequestCovenant();

                // Wait up to 10 seconds for the reply
                if (!waitHandle.Wait(10000))
                {
                    GetClient().Estate.EstateCovenantReply -= handler;
                    return Failure("Timed out waiting for EstateCovenantReply");
                }
            }

            if (result == null)
            {
                return Failure("No estate covenant reply received");
            }

            KeyValuePair<bool, string> covenantAssetRead = new KeyValuePair<bool, string>(false, "There is no Covenant provided for this Estate");
            if(result.CovenantID != UUID.Zero)
            {
                covenantAssetRead = GetEstateNotecard(result.CovenantID);
            }
            string covenantText = covenantAssetRead.Value;
            // Return the result as a JSON object, including the text
            return BasicReply(JsonConvert.SerializeObject(new
            {
                Fetched = covenantAssetRead.Key,
                CovenantID = result.CovenantID.ToString(),
                Timestamp = result.Timestamp.ToString(),
                EstateName = result.EstateName.ToString(),
                EstateOwnerID = result.EstateOwnerID.ToString(),
                CovenantText = covenantAssetRead.Value
            }));
        }

        [About("Sets the extended region info for the sim you are on")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("Unable to process block terraform value please use true or false")]
        [ReturnHintsFailure("Unable to process block fly value please use true or false")]
        [ReturnHintsFailure("Unable to process block fly over value please use true or false")]
        [ReturnHintsFailure("Unable to process allow damage value please use true or false")]
        [ReturnHintsFailure("Unable to process allow land resell value please use true or false")]
        [ReturnHintsFailure("Unable to process agent limit value please use a number")]
        [ReturnHintsFailure("Unable to process prim bonus value please use a number")]
        [ReturnHintsFailure("Unable to process mature value please use true or false")]
        [ReturnHintsFailure("Unable to process adult value please use true or false")]
        [ReturnHintsFailure("Unable to process block object push value please use true or false")]
        [ReturnHintsFailure("Unable to process allow parcel changes value please use true or false")]
        [ReturnHintsFailure("Unable to process block parcel search value please use true or false")]
        [ArgHints("setBlockTerraform", "true to block terraform, false to allow it", "BOOL", "false")]
        [ArgHints("setBlockFly", "true to block flying, false to allow it", "BOOL", "false")]
        [ArgHints("setBlockFlyOver", "true to block flying over, false to allow it", "BOOL", "false")]
        [ArgHints("setAllowDamage", "true to allow damage, false to block it", "BOOL", "false")]
        [ArgHints("setAllowLandResell", "true to allow land reselling, false to block it", "BOOL", "false")]
        [ArgHints("setAgentLimit", "the maximum number of agents allowed in the sim", "Number", "100")]
        [ArgHints("setPrimBonus", "the prim bonus for the sim", "Number", "1.0")]
        [ArgHints("setMature", "true to set the sim as mature, false to set it as general", "BOOL", "false")]
        [ArgHints("setAdult", "true to set the sim as adult, false use the setMature setting", "BOOL", "false")]
        [ArgHints("setBlockObjectPush", "true to block object pushing, false to allow it", "BOOL", "false")]
        [ArgHints("setAllowParcelChanges", "true to allow parcel changes, false to block it", "BOOL", "false")]
        [ArgHints("setBlockParcelSearch", "true to block parcel search, false to allow it", "BOOL", "false")]
        [CmdTypeSet()]
        public object SetExtendedRegionInfo(string setBlockTerraform, string setBlockFly, string setBlockFlyOver,
            string setAllowDamage, string setAllowLandResell, string setAgentLimit, 
            string setPrimBonus, string setMature, string setAdult, string setBlockObjectPush, 
            string setAllowParcelChanges, string setBlockParcelSearch
            )
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here");
            }
            if (bool.TryParse(setBlockTerraform, out bool blockTerraform) == false)
            {
                return Failure("Unable to process block terraform value please use true or false", [setBlockTerraform]);
            }
            if (bool.TryParse(setBlockFly, out bool blockFly) == false)
            {
                return Failure("Unable to process block fly value please use true or false", [setBlockTerraform, setBlockFly]);
            }
            if (bool.TryParse(setBlockFlyOver, out bool blockFlyOver) == false)
            {
                return Failure("Unable to process block fly over value please use true or false", [setBlockTerraform, setBlockFly, setBlockFlyOver]);
            }
            if (bool.TryParse(setAllowDamage, out bool allowDamage) == false)
            {
                return Failure("Unable to process allow damage value please use true or false", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage]);
            }
            if (bool.TryParse(setAllowLandResell, out bool allowLandResell) == false)
            {
                return Failure("Unable to process allow land resell value please use true or false", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell]);
            }
            if (int.TryParse(setAgentLimit, out int agentLimitValue) == false)
            {
                return Failure("Unable to process agent limit value please use a number", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell, setAgentLimit]);
            }
            if (float.TryParse(setPrimBonus, out float primBonusValue) == false)
            {
                return Failure("Unable to process prim bonus value please use a number", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell, setAgentLimit, setPrimBonus]);
            }
            if (bool.TryParse(setMature, out bool mature) == false)
            {
                return Failure("Unable to process mature value please use true or false", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell, setAgentLimit, setPrimBonus, setMature]);
            }
            if (bool.TryParse(setAdult, out bool adult) == false)
            {
                return Failure("Unable to process adult value please use true or false", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell, setAgentLimit, setPrimBonus, setMature, setAdult]);
            }
            if (bool.TryParse(setBlockObjectPush, out bool blockObjectPush) == false)
            {
                return Failure("Unable to process block object push value please use true or false", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell, setAgentLimit, setPrimBonus, setMature, setAdult, setBlockObjectPush]);
            }
            if (bool.TryParse(setAllowParcelChanges, out bool allowParcelChanges) == false)
            {
                return Failure("Unable to process allow parcel changes value please use true or false", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell, setAgentLimit, setPrimBonus, setMature, setAdult, setBlockObjectPush, setAllowParcelChanges]);
            }
            if (bool.TryParse(setBlockParcelSearch, out bool blockParcelSearch) == false)
            {
                return Failure("Unable to process block parcel search value please use true or false", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell, setAgentLimit, setPrimBonus, setMature, setAdult, setBlockObjectPush, setAllowParcelChanges, setBlockParcelSearch]);
            }
            RegionMaturity regionMaturity = RegionMaturity.PG;
            if (mature)
            {
                regionMaturity = RegionMaturity.Mature;
            }
            if (adult)
            {
                regionMaturity = RegionMaturity.Adult;
            }
            KeyValuePair<bool, string> result = GetClient().Estate.extendedSetRegionInfo(blockTerraform, blockFly, blockFlyOver, allowDamage, allowLandResell,
                agentLimitValue, primBonusValue, regionMaturity, blockObjectPush, allowParcelChanges, blockParcelSearch).Await();
            if (result.Key == false)
            {
                return Failure(result.Value, [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell, setAgentLimit, setPrimBonus, setMature, setAdult, setBlockObjectPush, setAllowParcelChanges, setBlockParcelSearch]);
            }
            return BasicReply("ok", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell, setAgentLimit, setPrimBonus, setMature, setAdult, setBlockObjectPush, setAllowParcelChanges, setBlockParcelSearch]);
        }

        [About("Set the current region flags for the sim you are on")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("Unable to process block terraform value please use true or false")]
        [ReturnHintsFailure("Unable to process block fly value please use true or false")]
        [ReturnHintsFailure("Unable to process allow damage value please use true or false")]
        [ReturnHintsFailure("Unable to process allow land resell value please use true or false")]
        [ReturnHintsFailure("Unable to process block pushing value please use true or false")]
        [ReturnHintsFailure("Unable to process allow parcel join divide value please use true or false")]
        [ReturnHintsFailure("Unable to process agent limit value please use a number")]
        [ReturnHintsFailure("Unable to process object bonus value please use a number")]
        [ReturnHintsFailure("Unable to process mature value please use true or false")]
        [ReturnHintsFailure("Unable to process adult value please use true or false")]
        [ArgHints("setBlockTerraform", "true to block terraform, false to allow it", "BOOL", "false")]
        [ArgHints("setBlockFly", "true to block flying, false to allow it", "BOOL", "false")]
        [ArgHints("setAllowDamage", "true to allow damage, false to block it", "BOOL", "false")]
        [ArgHints("setAllowLandResell", "true to allow land reselling, false to block it", "BOOL", "false")]
        [ArgHints("setBlockPushing", "true to block pushing, false to allow it", "BOOL", "false")]
        [ArgHints("setAllowParcelJoinDivide", "true to allow parcel join and divide, false to block it", "BOOL", "false")]
        [ArgHints("setAgentLimit", "the maximum number of agents allowed in the sim", "Number", "100")]
        [ArgHints("setObjectBonus", "the object bonus for the sim", "Number", "1.0")]
        [ArgHints("setMature", "true to set the sim as mature, false to set it as general", "BOOL", "false")]
        [ArgHints("setAdult", "true to set the sim as adult, false use the setMature setting", "BOOL", "false")]
        [CmdTypeSet()]
        public object SetRegionFlags(string setBlockTerraform, string setBlockFly, string setAllowDamage, string setAllowLandResell, string setBlockPushing, string setAllowParcelJoinDivide, string setAgentLimit, string setObjectBonus, string setMature, string setAdult)
        {
            if(GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here");
            }
            if(bool.TryParse(setBlockTerraform, out bool blockTerraform) == false)
            {
                return Failure("Unable to process block terraform value please use true or false", [setBlockTerraform]);
            }
            if (bool.TryParse(setBlockFly, out bool blockFly) == false)
            {
                return Failure("Unable to process block fly value please use true or false", [setBlockTerraform, setBlockFly]);
            }
            if (bool.TryParse(setAllowDamage, out bool allowDamage) == false)
            {
                return Failure("Unable to process allow damage value please use true or false", [setBlockTerraform, setBlockFly, setAllowDamage]);
            }
            if (bool.TryParse(setAllowLandResell, out bool allowLandResell) == false)
            {
                return Failure("Unable to process allow land resell value please use true or false", [setBlockTerraform, setBlockFly, setAllowDamage, setAllowLandResell]);
            }
            if (bool.TryParse(setBlockPushing, out bool blockPushing) == false)
            {
                return Failure("Unable to process block pushing value please use true or false", [setBlockTerraform, setBlockFly, setAllowDamage, setAllowLandResell, setBlockPushing]);
            }
            if (bool.TryParse(setAllowParcelJoinDivide, out bool allowParcelJoinDivide) == false)
            {
                return Failure("Unable to process allow parcel join divide value please use true or false", [setBlockTerraform, setBlockFly, setAllowDamage, setAllowLandResell, setBlockPushing, setAllowParcelJoinDivide]);
            }
            if (float.TryParse(setAgentLimit, out float agentLimit) == false)
            {
                return Failure("Unable to process agent limit value please use a number", [setBlockTerraform, setBlockFly, setAllowDamage, setAllowLandResell, setBlockPushing, setAllowParcelJoinDivide, setAgentLimit]);
            }
            if (float.TryParse(setObjectBonus, out float objectBonus) == false)
            {
                return Failure("Unable to process object bonus value please use a number", [setBlockTerraform, setBlockFly, setAllowDamage, setAllowLandResell, setBlockPushing, setAllowParcelJoinDivide, setAgentLimit, setObjectBonus]);
            }
            if (bool.TryParse(setMature, out bool mature) == false)
            {
                return Failure("Unable to process mature value please use true or false", [setBlockTerraform, setBlockFly, setAllowDamage, setAllowLandResell, setBlockPushing, setAllowParcelJoinDivide, setAgentLimit, setObjectBonus, setMature]);
            }
            if (bool.TryParse(setAdult, out bool adult) == false)
            {
                return Failure("Unable to process adult value please use true or false", [setBlockTerraform, setBlockFly, setAllowDamage, setAllowLandResell, setBlockPushing, setAllowParcelJoinDivide, setAgentLimit, setObjectBonus, setMature, setAdult]);
            }
            RegionMaturity rating = mature ? RegionMaturity.Mature : RegionMaturity.PG;
            if (adult)
            {
                rating = RegionMaturity.Adult;
            }
            GetClient().Estate.SetRegionInfo(blockTerraform, blockFly, allowDamage, allowLandResell, blockPushing, allowParcelJoinDivide, agentLimit, objectBonus, rating);
            return BasicReply("ok", [setBlockTerraform, setBlockFly, setAllowDamage, setAllowLandResell, setBlockPushing, setAllowParcelJoinDivide, setAgentLimit, setObjectBonus, setMature, setAdult]);
        }

        [About("Adds an estate manager to the current sim or all estates you control")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("Unable to find avatar UUID")]
        [ReturnHintsFailure("Unable to process all estates value please use true or false")]
        [ArgHints("avatar", "the avatar you wish to add as an estate manager", "AVATAR")]
        [ArgHints("allEstates", "true to add to all estates you control, false to add to just this sim", "BOOL", "false")]
        [CmdTypeSet()]
        public object AddEstateManager(string avatar, string allEstates)
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", [avatar]);
            }
            if(bool.TryParse(allEstates, out bool allEstatesFlag) == false)
            {
                return Failure("Unable to process all estates value please use true or false", [avatar, allEstates]);
            }
            UUID avataruuid = UUID.Zero;
            if (UUID.TryParse(avatar, out avataruuid) == false)
            {
                return Failure("Unable to find avatar UUID", [avatar]);
            }
            GetClient().Estate.AddEstateManager(avataruuid, allEstatesFlag);
            return BasicReply("ok", [avatar]);
        }

        [About("Removes an estate manager from the current sim or all estates you control")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("Unable to find avatar UUID")]
        [ReturnHintsFailure("Unable to process all estates value please use true or false")]
        [ArgHints("avatar", "the avatar you wish to remove as an estate manager", "AVATAR")]
        [ArgHints("allEstates", "true to remove from all estates you control, false to remove from just this sim", "BOOL", "false")]
        [CmdTypeSet()]
        public object RemoveEstateManager(string avatar, string allEstates)
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", [avatar]);
            }
            if (bool.TryParse(allEstates, out bool allEstatesFlag) == false)
            {
                return Failure("Unable to process all estates value please use true or false", [avatar, allEstates]);
            }
            UUID avataruuid = UUID.Zero;
            if (UUID.TryParse(avatar, out avataruuid) == false)
            {
                return Failure("Unable to find avatar UUID", [avatar]);
            }
            GetClient().Estate.RemoveEstateManager(avataruuid, allEstatesFlag);
            return BasicReply("ok", [avatar]);
        }

        [About("Sends the message to the current sim")]
        [ReturnHints("restarting")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHints("canceled")]
        [ArgHints("delay", "How long to delay the restart for (30 to 240 secs) - defaults to 240 if out of bounds \n" +
            "set to 0 if your canceling!", "Number", "60")]
        [ArgHints("mode", "true to start a restart, false to cancel", "BOOL")]
        [CmdTypeDo()]
        public object SimRestart(string delay, string mode)
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", [delay, mode]);
            }
            bool.TryParse(mode, out bool modeflag);
            if (modeflag == false)
            {
                GetClient().Estate.CancelRestart();
                return BasicReply("canceled", [delay, mode]);
            }
            int.TryParse(delay, out int delay_restart);
            if ((delay_restart < 30) || (delay_restart > 240))
            {
                delay_restart = 240;
            }
            GetClient().Estate.RestartRegion(delay_restart);
            return BasicReply("restarting", [delay, mode]);
        }

        [About("Sends the message to the current sim")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHints("ok")]
        [ArgHints("message", "What the message is", "Text", "Hi everyone I need to restart this sim")]
        [CmdTypeDo()]
        public object SimMessage(string message)
        {
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("Message empty", [message]);
            }
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", [message]);
            }
            GetClient().Estate.SimulatorMessage(message);
            return BasicReply("ok", [message]);
        }

        [About("Fetchs the regions map tile")]
        [ReturnHintsFailure("Unable to find region")]
        [ReturnHints("Texture UUID")]
        [ArgHints("regionname", "the name of the region we are fetching", "Text", "Lostworld")]
        [CmdTypeGet()]
        public object GetSimTexture(string regionname)
        {
            if (GetClient().Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region) == false)
            {
                return Failure("Unable to find region", [regionname]);
            }
            return BasicReply(region.MapImageID.ToString(), [regionname]);
        }

        [About("Reclaims ownership of the current parcel")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHints("ok")]
        [CmdTypeDo()]
        public object EstateParcelReclaim()
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here");
            }
            int localid = GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, GetClient().Self.SimPosition);
            GetClient().Parcels.Reclaim(GetClient().Network.CurrentSim, localid);
            return BasicReply("ok");
        }

        [About("Gets the global location of a sim [region]")]
        [ArgHints("regionname", "the region we want", "Text", "Lostworld")]
        [ReturnHintsFailure("Unable to find region")]
        [ReturnHints("a json object with the x,y and region name")]
        [CmdTypeGet()]
        public object GetSimGlobalPos(string regionname)
        {
            if (GetClient().Grid.GetGridRegion(regionname, GridLayerType.Objects, out GridRegion region) == false)
            {
                return Failure("Unable to find region", [regionname]);
            }
            Dictionary<string, string> reply = new()
            {
                { "region", regionname },
                { "X", region.X.ToString() },
                { "Y", region.Y.ToString() }
            };
            return BasicReply(JsonConvert.SerializeObject(reply));
        }

        [About("Requests the estate banlist")]
        [ReturnHints("ban list json")]
        [CmdTypeGet()]
        public object GetEstateBanList()
        {
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetEstateBans()));
        }

        [About("Attempts to add/remove the avatar to/from the Estate banlist")]
        [ReturnHints("Unban request accepted")]
        [ReturnHints("Ban request accepted")]
        [ReturnHintsFailure("Unable to find avatar UUID")]
        [ReturnHintsFailure("Unable to process global value please use true or false")]
        [ReturnHintsFailure("Not an estate manager on region {REGIONNAME}")]
        [ArgHints("avatar", "avatar you wish to ban", "AVATAR")]
        [ArgHints("mode", "What action would you like to take<br/>Defaults to remove if not given \"add\"", "Text", "add", new string[] {"add","remove"})]
        [ArgHints("global", "if true this the ban/unban will be applyed to all estates the bot has access to", "BOOL")]
        [CmdTypeSet()]
        public object UpdateEstateBanlist(string avatar, string mode, string global)
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager on region " + GetClient().Network.CurrentSim.Name, [avatar, mode, global]);
            }
            UUID avataruuid = UUID.Zero;
            if (UUID.TryParse(avatar, out avataruuid) == false)
            {
                return Failure("Unable to find avatar UUID", [avatar, mode, global]);
            }
            if (bool.TryParse(global, out bool globalban) == false)
            {
                return Failure("Unable to process global value please use true or false", [avatar, mode, global]);
            }
            if (mode != "add")
            {
                GetClient().Estate.UnbanUser(avataruuid, globalban);
                return BasicReply("Unban request accepted", [avatar, mode, global]);
            }
            GetClient().Estate.BanUser(avataruuid, globalban);
            return BasicReply("Ban request accepted", [avatar, mode, global]);
        }
    }
}
