using BetterSecondBot.bottypes;
using BetterSecondBot.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Newtonsoft.Json;
using static OpenMetaverse.ParcelManager;
using BetterSecondBotShared.Static;
using BetterSecondBotShared.logs;
using System.Threading;

namespace BetterSecondBot.HttpService
{
    public partial class HTTP_Parcel : WebApiControllerWithTokens
    {
        public HTTP_Parcel(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Sets the current parcel for sale Also marks the parcel for sale")]
        [ReturnHints("ok")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ReturnHints("Invaild amount")]
        [ArgHints("amount", "URLARG", "The amount to sell the parcel for from 1 to 9999999")]
        [ArgHints("avatar", "URLARG", "Avatar uuid or Firstname Lastname or \"none\" who we are locking the sale to")]
        [Route(HttpVerbs.Get, "/SetParcelSale/{amount}/{avatar}/{token}")]
        public object SetParcelSale(string amount,string avatar,string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "SetParcelSale");
            if(tests.Key == false)
            {
                return Failure(tests.Value);
            }
            if (int.TryParse(amount,out int amountvalue) == false)
            {
                return Failure("Invaild amount");
            }
            if((amountvalue < 1) || (amountvalue > 9999999))
            {
                return Failure("Invaild amount");
            }
            ProcessAvatar(avatar);
            targetparcel.SalePrice = amountvalue;
            targetparcel.AuthBuyerID = avataruuid;
            parcel_static.ParcelSetFlag(ParcelFlags.ForSale, targetparcel, true);
            parcel_static.ParcelSetFlag(ParcelFlags.ForSaleObjects, targetparcel, false);
            targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
            return Failure("ok");
        }

        [About("Updates the current parcels name")]
        [ReturnHints("ok")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ReturnHints("Parcel name is empty")]
        [ArgHints("name", "URLARG", "The new name of the parcel")]
        [Route(HttpVerbs.Get, "/SetParcelName/{name}/{token}")]
        public object SetParcelName(string name, string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "SetParcelName");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            if (helpers.notempty(name) == false)
            {
                return Failure("Parcel name is empty");
            }
            targetparcel.Name = name;
            targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
            return Failure("ok");
        }

        [About("Updates the current parcels description")]
        [ReturnHints("ok")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ArgHints("desc", "Text", "The new desc of the parcel")]
        [Route(HttpVerbs.Post, "/SetParcelDesc/{token}")]
        public object SetParcelDesc([FormField] string desc, string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "SetParcelDesc");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            targetparcel.Desc = desc;
            targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
            return Failure("ok");
        }

        [About("Fetchs the current parcels desc")]
        [ReturnHints("ok")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [Route(HttpVerbs.Get, "/GetParcelDesc/{token}")]
        public object GetParcelDesc(string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "GetParcelDesc");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            return Failure(targetparcel.Desc);
        }

        [About("gets the flags for the parcel")]
        [ReturnHints("ok")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [Route(HttpVerbs.Get, "/GetParcelFlags/{token}")]
        public object GetParcelFlags(string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "GetParcelFlags");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            Dictionary<string, ParcelFlags> flags = parcel_static.get_flags_list();
            Dictionary<string, string> collection = new Dictionary<string, string>();
            foreach (ParcelFlags cfg in flags.Values)
            {
                collection.Add(cfg.ToString(), targetparcel.Flags.HasFlag(cfg).ToString());
            }
            return collection;
        }

        [About("Ejects an avatar")]
        [ReturnHints("ok")]
        [ReturnHints("Invaild avatar")]
        [ArgHints("avatar", "URLARG", "uuid of the avatar or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/ParcelEject/{avatar}/{token}")]
        public object ParcelEject(string avatar,string token)
        {
            if (tokens.Allow(token, "parcel", "ParcelEject", handleGetClientIP()) == false)
            {
                return BasicReply("Token not accepted");
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar");
            }
            bot.GetClient.Parcels.EjectUser(avataruuid, false);
            return BasicReply("ok");
        }

        [About("Abandons the parcel the bot is currently on, returning it to Linden's or Estate owner")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/AbandonLand/{token}")]
        public object AbandonLand(string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "AbandonLand");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            bot.GetClient.Parcels.ReleaseParcel(bot.GetClient.Network.CurrentSim, targetparcel.LocalID);
            return BasicReply("ok");
        }


        [About("Bans an avatar from a parcel")]
        [ReturnHints("ok")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ReturnHints("Invaild avatar")]
        [ReturnHints("Avatar is in the blacklist")]
        [ArgHints("avatar", "URLARG", "uuid of the avatar or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/ParcelBan/{avatar}/{token}")]
        public object ParcelBan(string avatar, string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "ParcelBan");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar");
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
                return BasicReply("Avatar is in the blacklist");
            }
            ParcelAccessEntry entry = new ParcelAccessEntry();
            entry.AgentID = avataruuid;
            entry.Flags = AccessList.Ban;
            entry.Time = new System.DateTime(3030, 03, 03);
            targetparcel.AccessBlackList.Add(entry);
            targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
            return BasicReply("ok");
        }

        [About("Unbans an avatar from a parcel")]
        [ReturnHints("ok")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ReturnHints("Invaild avatar")]
        [ReturnHints("Avatar is already unbanned")]
        [ArgHints("avatar", "URLARG", "uuid of the avatar or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/ParcelUnBan/{avatar}/{token}")]
        public object ParcelUnBan(string avatar, string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "ParcelUnBan");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar");
            }
            bool alreadyBanned = false;
            ParcelAccessEntry removeentry = new ParcelAccessEntry();
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
                return BasicReply("Avatar is already unbanned");
            }
            targetparcel.AccessBlackList.Remove(removeentry);
            targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
            return BasicReply("ok");
        }


        [About("Updates the current parcels name")]
        [ReturnHints("true|false")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ArgHints("musicurl", "URLARG", "The new name of the parcel")]
        [Route(HttpVerbs.Get, "/SetParcelMusic/{musicurl}/{token}")]
        public object SetParcelMusic(string musicurl, string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "SetParcelMusic");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            bool status = parcel_static.set_parcel_music(bot, targetparcel, musicurl);
            return Failure(status.ToString());
        }

        [About("Updates the current parcels name")]
        [ReturnHints("true|false")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ReturnHints("Incorrect perms to control parcel")]
        [ReturnHints("No accepted flags")]
        [ReturnHints("Unable to set flag ...")]
        [ReturnHints("Flag: ? is unknown")]
        [ReturnHints("Flag: ? missing \"=\"")]
        [ArgHints("escapedflagdata", "Text", "repeatable flag data split by ::: formated Flag=True|False")]
        [Route(HttpVerbs.Post, "/SetParcelFlag/{escapedflagdata}/{token}")]
        public object SetParcelFlag([FormField] string escapedflagdata, string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "SetParcelFlag");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            List<string> acceptablewords = new List<string>();
            Dictionary<string, ParcelFlags> flags = parcel_static.get_flags_list();
            acceptablewords.AddRange(new[] { "True", "False" });

            Dictionary<string, bool> setflags = new Dictionary<string, bool>();
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
                return Failure("No accepted flags");
            }
            if (parcel_static.has_parcel_perm(targetparcel, bot) == false)
            {
                return Failure("Incorrect perms to control parcel");
            }
            foreach (KeyValuePair<string, bool> cfg in setflags)
            {
                if (flags.ContainsKey(cfg.Key) == true)
                {
                    parcel_static.ParcelSetFlag(flags[cfg.Key], targetparcel, cfg.Value);
                }
            }
            targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
            return BasicReply("Applying perms");
        }

        [About("Returns all objects from the current parcel for the selected avatar")]
        [ReturnHints("ok")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ReturnHints("Invaild avatar UUID")]
        [ArgHints("avatar", "URLARG", "avatar uuid or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/ParcelReturnTargeted/{avatar}/{token}")]
        public object ParcelReturnTargeted(string avatar, string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "ParcelReturnTargeted");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID");
            }
            bot.GetClient.Parcels.ReturnObjects(bot.GetClient.Network.CurrentSim, targetparcel.LocalID, ObjectReturnType.None, new List<UUID>() { avataruuid });
            return BasicReply("ok");
        }


        [About("transfers the current parcel ownership to the assigned group")]
        [ReturnHints("ok")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ReturnHints("Invaild group uuid")]
        [ReturnHints("Not in group")]
        [Route(HttpVerbs.Get, "/ParcelDeedToGroup/{group}/{token}")]
        public object ParcelDeedToGroup(string group, string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "ParcelReturnTargeted");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group uuid");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Not in group");
            }
            targetparcel.GroupID = groupuuid;
            targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
            Thread.Sleep(500);
            bot.GetClient.Parcels.DeedToGroup(bot.GetClient.Network.CurrentSim, targetparcel.LocalID, groupuuid);
            return BasicReply("ok");
        }

        [About("Attempts to buy the parcel the bot is standing on, the amount must match the sale price for the land!")]
        [ReturnHints("ok")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ReturnHints("Parcel not for sale")]
        [ReturnHints("Parcel not for sale")]
        [ReturnHints("Parcel sale locked to other avatars")]
        [ReturnHints("Parcel sale price and amount do not match")]
        [ReturnHints("Invaild amount")]
        [ArgHints("amount", "URLARG", "amount to pay for the parcel (min 1)")]
        [Route(HttpVerbs.Get, "/ParcelBuy/{amount}/{token}")]
        public object ParcelBuy(string amount, string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "ParcelBuy");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            if (int.TryParse(amount, out int amountvalue) == false)
            {
                return Failure("Invaild amount");
            }
            if (amountvalue < 1)
            {
                return Failure("Invaild amount");
            };
            if (targetparcel.Flags.HasFlag(ParcelFlags.ForSale) == false)
            {
                return Failure("Parcel not for sale");
            }
            if ((targetparcel.AuthBuyerID != UUID.Zero) && (targetparcel.AuthBuyerID != bot.GetClient.Self.AgentID))
            {
                return Failure("Parcel sale locked to other avatars");
            }
            if (targetparcel.SalePrice != amountvalue)
            {
                return Failure("Parcel sale price and amount do not match");
            }
            bot.GetClient.Parcels.Buy(bot.GetClient.Network.CurrentSim, targetparcel.LocalID, false, UUID.Zero, false, targetparcel.Area, amountvalue);
            return BasicReply("ok");
        }

        [About("Freezes an avatar")]
        [ReturnHints("ok")]
        [ReturnHints("Invaild avatar UUID")]
        [ReturnHints("Invaild state")]
        [ArgHints("avatar", "URLARG", "avatar uuid or Firstname Lastname")]
        [ArgHints("state", "URLARG", "setting state to false will unfreeze or true to freeze")]
        [Route(HttpVerbs.Get, "/ParcelFreeze/{avatar}/{state}/{token}")]
        public object ParcelFreeze(string avatar, string state, string token)
        {
            if (tokens.Allow(token, "parcel", "ParcelFreeze", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if(bool.TryParse(state, out bool freezestate) == false)
            {
                return Failure("Invaild state");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID");
            }
            bot.GetClient.Parcels.FreezeUser(avataruuid, freezestate);
            return BasicReply("ok");
        }


        [About("Fetchs the parcel ban list of the parcel the bot is currently on<br/>If the name returned is lookup the bot is currently requesting the avatar name")]
        [ReturnHints("array of UUID=Name")]
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [Route(HttpVerbs.Get, "/GetParcelBanlist/{token}")]
        public object GetParcelBanlist(string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "GetParcelBanlist");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            Dictionary<string, string> reply = new Dictionary<string, string>();
            foreach (ParcelAccessEntry e in targetparcel.AccessBlackList)
            {
                string name = bot.FindAvatarKey2Name(e.AgentID);
                reply.Add(e.AgentID.ToString(), name);
            }
            return BasicReply(JsonConvert.SerializeObject(reply));
        }

        [About("Returns a rezzed object")]
        [ReturnHints("ok")]
        [ReturnHints("Invaild object uuid")]
        [ReturnHints("Unable to find object")]
        [ArgHints("objectuuid", "URLARG", "object UUID to unrez")]
        [Route(HttpVerbs.Get, "/UnRezObject/{objectuuid}/{token}")]
        public object UnRezObject(string objectuuid,string token)
        {
            if (tokens.Allow(token, "parcel", "UnRezObject", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(objectuuid, out UUID targetobject) == false)
            {
                return Failure("Invaild object uuid");
            }
            bool found = false;
            Dictionary<uint, Primitive> objects_copy = bot.GetClient.Network.CurrentSim.ObjectsPrimitives.Copy();
            foreach (KeyValuePair<uint, Primitive> Obj in objects_copy)
            {
                if (Obj.Value.ID == targetobject)
                {
                    bot.GetClient.Inventory.RequestDeRezToInventory(Obj.Key);
                    found = true;
                    break;
                }
            }
            if(found == false)
            {
                return Failure("Unable to find object");
            }
            return BasicReply("ok");
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
        [ReturnHints("Error not in a sim")]
        [ReturnHints("Parcel data not ready")]
        [ArgHints("escapedflagdata", "Text", "repeatable flag data split by ::: formated Flag=True|False")]
        [Route(HttpVerbs.Get, "/ParcelSetMedia/{token}")]
        public object ParcelSetMedia([FormField] string escapedflagdata, string token)
        {
            KeyValuePair<bool, string> tests = SetupCurrentParcel(token, "parcel", "ParcelSetMedia");
            if (tests.Key == false)
            {
                return Failure(tests.Value);
            }
            string[] args = escapedflagdata.Split(":::");
            foreach (string A in args)
            {
                string[] bits = A.Split('=');
                if (bits.Length == 2)
                {
                    if (bits[0] == "MediaType")
                    {
                        UpdateParcel_MediaType(targetparcel, bits[1]);
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
            targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
            return BasicReply("ok");
        }

        protected bool UpdateParcel_MediaType(Parcel targetparcel, string value)
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
}
