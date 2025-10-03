using EmbedIO;
using System.Text.Json;
using OpenMetaverse;
using OpenMetaverse.Assets;
using OpenMetaverse.StructuredData;
using Org.BouncyCastle.Asn1.Ocsp;
using SecondBotEvents.Services;
using Swan;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using static Betalgo.Ranul.OpenAI.ObjectModels.RealtimeModels.RealtimeEventTypes;
using static Betalgo.Ranul.OpenAI.ObjectModels.StaticValues.AudioStatics;
using static OpenMetaverse.EstateTools;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Look after a sim as the estate manager")]
    public class Estate(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Sets the estate configuration for the current sim")]
        [ReturnHints("ok")]
        [ReturnHints("ok using fallback")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("Unable to process sun hour value please use a number between 0 and 23")]
        [ReturnHintsFailure("Sun hour value must be between 0 and 23")]
        [ReturnHintsFailure("Unable to process is sun fixed value please use true or false")]
        [ReturnHintsFailure("Unable to process is externally visible value please use true or false")]
        [ReturnHintsFailure("Unable to process allow direct teleport value please use true or false")]
        [ReturnHintsFailure("Unable to process deny anonymous value please use true or false")]
        [ReturnHintsFailure("Unable to process deny age unverified value please use true or false")]
        [ReturnHintsFailure("Unable to process block bots value please use true or false")]
        [ReturnHintsFailure("Unable to process allow voice chat value please use true or false")]
        [ReturnHintsFailure("Unable to process override public access value please use true or false")]
        [ReturnHintsFailure("Unable to process override environment value please use true or false")]
        [ReturnHintsFailure("Timed out waiting for EstateInfoReply")]
        [ReturnHintsFailure("No estate info reply received")]
        [ReturnHintsFailure("Unable to get URI for EstateChangeInfo")]
        [ReturnHintsFailure("HTTP error <http status> <response content>")]
        [ReturnHintsFailure("EstateChangeInfo error <error message>")]

        [ArgHints("sun_hour", "the hour of the day the sun should be at, 0 to 23", "Number", "12.0")]
        [ArgHints("is_sun_fixed", "true to fix the sun at the specified hour, false to have it move normally", "BOOL", "false")]
        [ArgHints("is_externally_visible", "true to make the region visible on the map, false to hide it", "BOOL", "true")]
        [ArgHints("allow_direct_teleport", "true to allow direct teleports to the region, false to block them", "BOOL", "true")]
        [ArgHints("deny_anonymous", "true to deny access to anonymous users, false to allow them", "BOOL", "false")]
        [ArgHints("deny_age_unverified", "true to deny access to age unverified users, false to allow them", "BOOL", "false")]
        [ArgHints("block_bots", "true to block known bots, false to allow them", "BOOL", "false")]
        [ArgHints("allow_voice_chat", "true to allow voice chat, false to block it", "BOOL", "true")]
        [ArgHints("override_public_access", "true to override public access settings, false to use parcel settings", "BOOL", "false")]
        [ArgHints("override_environment", "true to override environment settings, false to use parcel settings", "BOOL", "false")]
        [CmdTypeSet()]
        public object SetEstateConfig(string sun_hour, string is_sun_fixed, string is_externally_visible,
            string allow_direct_teleport, string deny_anonymous, string deny_age_unverified,
            string block_bots, string allow_voice_chat, string override_public_access, string override_environment)
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here");
            }
            if (float.TryParse(sun_hour, out float sunhourvalue) == false)
            {
                return Failure("Unable to process sun hour value please use a number between 0 and 23", [sun_hour]);
            }
            if(sunhourvalue < 0.0f || sunhourvalue > 23.0f)
            {
                return Failure("Sun hour value must be between 0 and 23", [sun_hour]);
            }
            if (bool.TryParse(is_sun_fixed, out bool issunfixed) == false)
            {
                return Failure("Unable to process is sun fixed value please use true or false", [sun_hour, is_sun_fixed]);
            }
            if(bool.TryParse(is_externally_visible, out bool isexternallyvisible) == false)
            {
                return Failure("Unable to process is externally visible value please use true or false", [sun_hour, is_sun_fixed, is_externally_visible]);
            }
            if (bool.TryParse(allow_direct_teleport, out bool allowdirectteleport) == false)
            {
                return Failure("Unable to process allow direct teleport value please use true or false", [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport]);
            }
            if (bool.TryParse(deny_anonymous, out bool denyanonymous) == false)
            {
                return Failure("Unable to process deny anonymous value please use true or false", [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport, deny_anonymous]);
            }
            if (bool.TryParse(deny_age_unverified, out bool denyageunverified) == false)
            {
                return Failure("Unable to process deny age unverified value please use true or false", [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport, deny_anonymous, deny_age_unverified]);
            }
            if (bool.TryParse(block_bots, out bool blockbots) == false)
            {
                return Failure("Unable to process block bots value please use true or false", [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport, deny_anonymous, deny_age_unverified, block_bots]);
            }
            if (bool.TryParse(allow_voice_chat, out bool allowvoicechat) == false)
            {
                return Failure("Unable to process allow voice chat value please use true or false", [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport, deny_anonymous, deny_age_unverified, block_bots, allow_voice_chat]);
            }
            if (bool.TryParse(override_public_access, out bool overridepublicaccess) == false)
            {
                return Failure("Unable to process override public access value please use true or false", [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport, deny_anonymous, deny_age_unverified, block_bots, allow_voice_chat, override_public_access]);
            }
            if (bool.TryParse(override_environment, out bool overrideenvironment) == false)
            {
                return Failure("Unable to process override environment value please use true or false", [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport, deny_anonymous, deny_age_unverified, block_bots, allow_voice_chat, override_public_access, override_environment]);
            }

            // Block inline and wait for the estate info reply to get the estate name
            string estatename = null;
            using (var waitHandle = new System.Threading.ManualResetEventSlim(false))
            {
                EventHandler<EstateUpdateInfoReplyEventArgs> handler = null;
                handler = (sender, e) =>
                {
                    GetClient().Estate.EstateUpdateInfoReply -= handler;
                    estatename = e.EstateName;
                    waitHandle.Set();
                };

                GetClient().Estate.EstateUpdateInfoReply += handler;
                GetClient().Estate.RequestInfo();

                // Wait up to 3 seconds for the reply
                if (!waitHandle.Wait(3000))
                {
                    GetClient().Estate.EstateUpdateInfoReply -= handler;
                    return Failure("Timed out waiting for EstateInfoReply");
                }
            }

            if (string.IsNullOrEmpty(estatename))
            {
                return Failure("No estate info reply received");
            }
            Uri uri = GetClient().Network.CurrentSim.Caps.CapabilityURI("EstateChangeInfo");
            if (uri == null)
            {
                // try data server fallback
                List<string> config = new List<string>();
                config.Add(estatename);
                Int64 bitmask = 0;
                if (issunfixed) bitmask |= 1 << 4;
                if (isexternallyvisible) bitmask |= 1 << 15;
                if (allowdirectteleport) bitmask |= 1 << 20;
                if (denyanonymous) bitmask |= 1 << 23;
                if (denyageunverified) bitmask |= 1 << 30;
                if (blockbots) bitmask |= 1 << 31;
                if (allowvoicechat) bitmask |= 1 << 28;
                if (overridepublicaccess) bitmask |= 1 << 5;
                if (overrideenvironment) bitmask |= 1 << 9;
                config.Add(bitmask.ToString());
                config.Add((sunhourvalue * 1024.0f).ToString());
                GetClient().Estate.EstateOwnerMessage("estatechangeinfo", config);

                return BasicReply("ok using fallback", [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport, deny_anonymous, deny_age_unverified, block_bots, allow_voice_chat, override_public_access, override_environment]);
            }

            OSDMap body = new OSDMap();
            body["estate_name"] = estatename;
            body["sun_hour"] = (sunhourvalue * 1024.0f);
            body["is_sun_fixed"] = issunfixed;
            body["is_externally_visible"] = isexternallyvisible;
            body["allow_direct_teleport"] = allowdirectteleport;
            body["deny_anonymous"] = denyanonymous;
            body["deny_age_unverified"] = denyageunverified;
            body["block_bots"] = blockbots;
            body["allow_voice_chat"] = allowvoicechat;
            body["override_public_access"] = overridepublicaccess;
            body["override_environment"] = overrideenvironment;
            body["invoice"] = UUID.Random();
            string requestBody = OSDParser.SerializeLLSDXmlString(body);
            try
            {
                var content = new StringContent(requestBody, Encoding.UTF8, "application/llsd+xml");
                var reply = GetClient().HttpCapsClient.PostAsync(uri.ToString(), content).Await();
                string responseContent = reply.Content.ReadAsStringAsync().Await();

                if (reply.IsSuccessStatusCode)
                {
                    return BasicReply("ok", [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport, deny_anonymous, deny_age_unverified, block_bots, allow_voice_chat, override_public_access, override_environment]);
                }
                return Failure( $"HTTP error {reply.StatusCode.ToString()} {responseContent}", [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport, deny_anonymous, deny_age_unverified, block_bots, allow_voice_chat, override_public_access, override_environment]);
            }
            catch (Exception e)
            {
                return Failure("EstateChangeInfo error "+ e.Message, [sun_hour, is_sun_fixed, is_externally_visible, allow_direct_teleport, deny_anonymous, deny_age_unverified, block_bots, allow_voice_chat, override_public_access, override_environment]);
            }
        }

        [About("Sets the terrain for the current sim from a url")]
        [ReturnHints("ok uploading with uuid <uuid>")]
        [ReturnHintsFailure("Not an estate manager here")]
        [ReturnHintsFailure("No url provided")]
        [ReturnHintsFailure("url must start with http:// or https://")]
        [ReturnHintsFailure("Unable to download terrain data from url error <error message>")]
        [ArgHints("urltofile", "the url to the terrain file you wish to upload", "Text")]
        [CmdTypeDo()]
        public object SetEstateTerrainRaw(string urltofile)
        {
            if (GetClient().Network.CurrentSim.IsEstateManager == false)
            {
                return Failure("Not an estate manager here", [urltofile]);
            }
            if (string.IsNullOrEmpty(urltofile))
            {
                return Failure("No url provided");
            }
            if (urltofile.StartsWith("http://") == false && urltofile.StartsWith("https://") == false)
            {
                return Failure("url must start with http:// or https://", [urltofile]);
            }
            byte[] terraindata = null;
            try
            {
                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    terraindata = httpClient.GetByteArrayAsync(urltofile).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                return Failure("Unable to download terrain data from url error "+ ex.Message, [urltofile]);
            }
            string filename = urltofile.Split('/')[^1];
            UUID request = GetClient().Estate.UploadTerrain(terraindata, filename);
            return BasicReply("ok uploading with uuid "+ request.ToString(), [urltofile]);
        }

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
            if (GetClient().Inventory.GetStore.Items.ContainsKey(targetitem))
            {
                find = GetClient().Inventory.GetStore.Items[targetitem];
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
            return BasicReply(JsonSerializer.Serialize(new
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
            RegionMaturity maturity = RegionMaturity.PG;
            if (mature)
            {
                maturity = RegionMaturity.Mature;
            }
            if (adult)
            {
                maturity = RegionMaturity.Adult;
            }
            GetClient().Estate.SetRegionInfo(blockTerraform, blockFly, blockFlyOver, allowDamage, allowLandResell, blockObjectPush, allowParcelChanges, agentLimitValue, primBonusValue, blockParcelSearch, maturity);
            return BasicReply("ok", [setBlockTerraform, setBlockFly, setBlockFlyOver, setAllowDamage, setAllowLandResell, setAgentLimit, setPrimBonus, setMature, setAdult, setBlockObjectPush, setAllowParcelChanges, setBlockParcelSearch]);
        }

        [About("Legacy command - will be phased out please use SetExtendedRegionInfo")]
        [CmdTypeSet()]
        public object SetRegionFlags(string setBlockTerraform, string setBlockFly, string setAllowDamage, string setAllowLandResell, string setBlockPushing, string setAllowParcelJoinDivide, string setAgentLimit, string setObjectBonus, string setMature, string setAdult)
        {
            return SetExtendedRegionInfo(setBlockTerraform, setBlockFly, "false", setAllowDamage, setAllowLandResell, setAgentLimit, setObjectBonus, setMature, setAdult, setBlockPushing, setAllowParcelJoinDivide, "false");
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
            if (SecondbotHelpers.NotEmpty(message) == false)
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
            return BasicReply(JsonSerializer.Serialize(reply));
        }

        [About("Requests the estate banlist")]
        [ReturnHints("ban list json")]
        [CmdTypeGet()]
        public object GetEstateBanList()
        {
            return BasicReply(JsonSerializer.Serialize(master.DataStoreService.GetEstateBans()));
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
