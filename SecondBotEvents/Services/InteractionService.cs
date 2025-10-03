using System.Text.Json;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecondBotEvents.Services
{
    public class InteractionService : BotServices
    {
        protected new InteractionConfig myConfig;
        protected bool botConnected = false;
        public InteractionService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new InteractionConfig(master.fromEnv,master.fromFolder);
            if (myConfig.GetEnabled() == false)
            {
                LogFormater.Info(Status());
                return;
            }
        }

        public override string Status()
        {
            if (myConfig == null)
            {
                return "No Config";
            }
            else if (myConfig.GetHideStatusOutput() == true)
            {
                return "hidden";
            }
            else if (myConfig.GetEnabled() == false)
            {
                return "- Not requested -";
            }
            else if (botConnected == false)
            {
                return "Waiting for bot";
            }
            return "Active";
        }

        protected bool ProcessRequest(bool enabledByConfig, string ConfigAcceptLevel, string avatarName, string sourceName, UUID avataruuid, Dictionary<string,string> misc = null)
        {
            if (misc == null)
            {
                misc = [];
            }
            if (myConfig.GetEnabled() == false)
            {
                JsonOuput(false, sourceName, avatarName, "Interactions hooks disabled");
                return false;
            }
            if(enabledByConfig == false)
            {
                JsonOuput(false, sourceName, avatarName, "disabled by config");
                return false;
            }
            if (master.CommandsService.myConfig.GetEnableMasterControls() == false)
            {
                // master avatars list is not enabled. unable to continue
                JsonOuput(false, sourceName, avatarName, "masters AV list is disabled: Please set EnableMasterControls in Commands service to true");
                return false;
            }
            bool accepted = false;
            string ObjectOwnerName = master.DataStoreService.GetAvatarName(avataruuid);
            string whyAccepted = "rejected request";
            misc.Add("ObjectOwner", ObjectOwnerName);
            if (master.CommandsService.myConfig.GetMastersCSV().Contains(avatarName) == true)
            {
                whyAccepted = "on owners list";
                accepted = true;
            }
            else if (master.CommandsService.myConfig.GetMastersCSV().Contains(ObjectOwnerName) == true)
            {
                whyAccepted = "Object Owner is on master list";
                accepted = true;
            }
            else if (ConfigAcceptLevel == "Anyone")
            {
                whyAccepted = "Accepting from anyone";
                accepted = true;
            }
            else if((ConfigAcceptLevel == "Friends") && (sourceName != "FriendRequest"))
            {
                whyAccepted = "on friends list";
                accepted = GetClient().Friends.FriendList.ContainsKey(avataruuid);
            }
            if ((accepted == false) && (master.CommandsService.myConfig.GetCheckDotNames() == true))
            {
                string[] bits = avatarName.ToLower().Split('.');
                if (bits.Length == 2)
                {
                    accepted = master.CommandsService.myConfig.GetMastersCSV().Contains(bits[0].FirstCharToUpper() + " " + bits[1].FirstCharToUpper());
                    if (accepted == true)
                    {
                        whyAccepted = "on owners list [dot check]";
                    }
                }
                if (accepted == false)
                {
                    bits = ObjectOwnerName.ToLower().Split('.');
                    if (bits.Length == 2)
                    {
                        accepted = master.CommandsService.myConfig.GetMastersCSV().Contains(bits[0].FirstCharToUpper() + " " + bits[1].FirstCharToUpper());
                        if (accepted == true)
                        {
                            whyAccepted = "Object Owner is on master list [dot check]";
                        }
                    }
                }
            }
            if (accepted == false)
            {
                // check the one time accept list
                accepted = master.DataStoreService.GetNextAccept(sourceName, avataruuid);
                if(accepted == true)
                {
                    whyAccepted = "was on NextAccept list";
                }
            }
            JsonOuput(accepted, sourceName, avatarName, whyAccepted, misc);
            return accepted;
        }



        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isStart == false)
            {
                return;
            }
            botConnected = false;
            LogFormater.Info("Interaction Service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void JsonOuput(bool status, string eventype, string from, string why="", Dictionary<string, string> setMisc = null)
        {
            if(myConfig.GetEnableDebug() == true)
            {
                LogFormater.Warn("Interaction debug:" + JsonSerializer.Serialize(new InteractionEvent(from, eventype, status, why, setMisc)));
            }
            if (myConfig.GetEnableJsonOutputEvents() == false)
            {
                return;
            }
            master.CommandsService.SmartCommandReply(
                myConfig.GetJsonOutputEventsTarget(), 
                JsonSerializer.Serialize(new InteractionEvent(from, eventype, status, why, setMisc)), 
                "interactions"
            );
        }

        protected void JsonOuputCleaner(string misc, string source)
        {
            Dictionary<string, string> reply = new()
            {
                { "value", misc }
            };
            JsonOuputCleaner(reply, source);
        }

        protected void JsonOuputCleaner(Dictionary<string, string> misc, string source)
        {
            if (myConfig.GetEnableDebug() == true)
            {
                LogFormater.Warn("Interaction debug:" + JsonSerializer.Serialize(misc));
            }
            if (myConfig.GetEnableJsonOutputEvents() == false)
            {
                return;
            }
            misc.Add("eventsource", source);
            master.CommandsService.SmartCommandReply(
                myConfig.GetJsonOutputEventsTarget(),
                JsonSerializer.Serialize(misc),
                "interactions"
            );
        }



        protected void BotTeleportOffer(object o, InstantMessageEventArgs e)
        {
            if((e.IM.Dialog != InstantMessageDialog.RequestTeleport) && (e.IM.Dialog != InstantMessageDialog.RequestLure))
            {
                return;
            }
            string mode = "Teleport Bot to Av";
            bool markTeleport = true;
            if (e.IM.Dialog == InstantMessageDialog.RequestLure)
            {
                mode = "Teleport Av to Bot";
                markTeleport = false;
            }
            Dictionary<string,string> misc = new()
            {
                { "mode", mode }
            };
            if (ProcessRequest(myConfig.GetAcceptTeleports(),myConfig.GetTeleportRequestLevel(), e.IM.FromAgentName, "Teleport", e.IM.FromAgentID, misc) == false)
            {
                // rejected requests
                if (e.IM.Dialog != InstantMessageDialog.RequestLure)
                {
                    return;
                }
                GetClient().Self.TeleportLureRespond(e.IM.FromAgentID, e.IM.IMSessionID, false);
                return;
            }
            if (markTeleport == true)
            {
                master.HomeboundService.MarkTeleport();
            }
            JsonOuputCleaner(e.IM.FromAgentName, mode);
            if (e.IM.Dialog == InstantMessageDialog.RequestLure)
            {
                GetClient().Self.SendTeleportLure(e.IM.FromAgentID);
                return;
            }
            GetClient().Self.TeleportLureRespond(e.IM.FromAgentID, e.IM.IMSessionID, true);
            
        }

        protected void BotGroupInviteOffer(object o, GroupInvitationEventArgs e)
        {
            Dictionary<string, string> reply = new()
            {
                { "message", e.Message },
                { "fromuuid", e.AgentID.ToString() }
            };
            if (ProcessRequest(myConfig.GetAcceptGroupInvites(),myConfig.GetGroupInviteLevel(), e.FromName, "GroupInvite", e.AgentID, reply) == false)
            {
                return;
            }
            e.Accept = true;
            JsonOuputCleaner(reply, "GroupInvite");
        }

        protected void BotInventoryAdd(object o, InventoryObjectAddedEventArgs e)
        {
            InventoryItem F = (InventoryItem)e.Obj;
            Dictionary<string, string> details = new()
            {
                { "itemname", e.Obj.Name },
                { "itemuuid", e.Obj.UUID.ToString() },
                { "itemtype", F.AssetType.ToString()    },
            };
            if (master.EventsService.isRunning() == true)
            {
                master.EventsService.InventoryUpdateEvent(details);
            }
            JsonOuputCleaner(details, "InventoryAdd");
        }

        protected void BotInventoryOffer(object o, InventoryObjectOfferedEventArgs e)
        {
            Dictionary<string, string> details = new()
            {
                { "transactionid", e.Offer.IMSessionID.ToString() },
                { "itemtype", e.AssetType.ToString() },
                { "message", e.Offer.Message.ToString() },
                { "fromscript", e.FromTask.ToString() },
                { "targetfolder", e.FolderID.ToString() }
            };
            if (ProcessRequest(myConfig.GetAcceptInventory(),myConfig.GetInventoryTransferLevel(),e.Offer.FromAgentName, "InventoryOffer", e.Offer.FromAgentID, details) == false)
            {
                e.Accept = false;
                return;
            }
            e.Accept = true;
            if(e.FromTask == false)
            {
                master.DataStoreService.AddAvatar(e.Offer.FromAgentID, e.Offer.FromAgentName);
                details.Add("itemuuid", e.ObjectID.ToString());
                details.Add("itemname", "?");
                InventoryItem itm = GetClient().Inventory.FetchItem(e.ObjectID, GetClient().Self.AgentID, TimeSpan.FromSeconds(15));
                if(itm != null)
                {
                    details["itemname"] = itm.Name;
                }
            }
            JsonOuputCleaner(details, "InventoryOffer");
        }

        protected void BotFriendRequested(object o, FriendshipOfferedEventArgs e)
        {
            Dictionary<string, string> misc = new()
            {
                { "fromName", e.AgentName }
            };
            if (ProcessRequest(myConfig.GetAcceptFriendRequests(),myConfig.GetFriendRequestLevel(), e.AgentName, "FriendRequest", e.AgentID, misc) == false)
            {
                return;
            }
            master.DataStoreService.AddAvatar(e.AgentID, e.AgentName);
            GetClient().Friends.AcceptFriendship(e.AgentID, e.SessionID);
            JsonOuputCleaner(e.AgentName, "FriendRequest");
            
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            botConnected = false;
            GetClient().Network.SimConnected += BotLoggedIn;
            LogFormater.Info("Interaction Service [Standby]");
        }

        async Task awaitstable()
        {
            await Task.Delay(3000);
        }

        protected async void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            await awaitstable();
            GetClient().Network.SimConnected -= BotLoggedIn;
            GetClient().Friends.FriendshipOffered += BotFriendRequested;
            GetClient().Inventory.InventoryObjectOffered += BotInventoryOffer;
            GetClient().Groups.GroupInvitation += BotGroupInviteOffer;
            GetClient().Inventory.Store.InventoryObjectAdded += BotInventoryAdd;
            GetClient().Self.IM += BotTeleportOffer;
            botConnected = true;
            LogFormater.Info("Interaction Service [Active]");
        }

        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            if (updateEnabled)
            {
                myConfig.setEnabled(setEnabledTo);
            }
            if (myConfig.GetEnabled() == false)
            {
                LogFormater.Info("Interaction Service [- Not requested -]");
                return;
            }
            Stop();
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
            LogFormater.Info("Interaction Service [Starting]");
        }

        public override void Stop()
        {
            if(running == true)
            {
                LogFormater.Info("Interaction Service [Stopping]");
            }
            running = false;
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            master.BotClientNoticeEvent -= BotClientRestart;
            if (master.BotClient != null)
            {
                if (GetClient() != null)
                {
                    GetClient().Friends.FriendshipOffered -= BotFriendRequested;
                    GetClient().Inventory.InventoryObjectOffered -= BotInventoryOffer;
                    GetClient().Inventory.Store.InventoryObjectAdded -= BotInventoryAdd;
                    GetClient().Groups.GroupInvitation -= BotGroupInviteOffer;
                }
            }
        }
    }

    public class InteractionEvent
    {
        public string from = "?";
        public string eventType = "?";
        public bool accepted = false;
        public string info = "";
        public Dictionary<string, string> misc = [];
        public InteractionEvent(string setFrom, string setType, bool setAccepted, string setInfo, Dictionary<string, string> setMisc = null)
        {
            accepted = setAccepted;
            from = setFrom;
            eventType = setType;
            info = setInfo;
            if(setMisc != null)
            {
                misc = setMisc;
            }
        }
    }
}

