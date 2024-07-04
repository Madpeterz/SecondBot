using Newtonsoft.Json;
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
        protected InteractionConfig myConfig;
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

        protected bool ProcessRequest(bool enabledByConfig, string avatarName, string sourceName)
        {
            if(myConfig.GetEnabled() == false)
            {
                JsonOuput(false, sourceName, avatarName, "Interactions hooks disabled");
                return false;
            }
            if (master.CommandsService.myConfig.GetEnableMasterControls() == false)
            {
                // master avatars list is not enabled. unable to continue
                JsonOuput(false, sourceName, avatarName, "masters AV list is disabled: Please set EnableMasterControls in Commands service to true");
                return false;
            }
            if(enabledByConfig == true)
            {
                return true;
            }
            if (master.CommandsService.myConfig.GetMastersCSV().Contains(avatarName) == true)
            {
                return true;
            }
            JsonOuput(false, sourceName, avatarName, "disabled by config");
            return false;
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            botConnected = false;
            LogFormater.Info("Interaction Service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void JsonOuput(bool status, string eventype, string from, string why="", Dictionary<string, string> setMisc = null)
        {
            if (myConfig.GetEnableJsonOutputEvents() == false)
            {
                return;
            }
            master.CommandsService.SmartCommandReply(
                myConfig.GetJsonOutputEventsTarget(), 
                JsonConvert.SerializeObject(new InteractionEvent(from, eventype, status, why, setMisc)), 
                "interactions"
            );
        }

        protected void JsonOuputCleaner(string misc, string source)
        {
            Dictionary<string, string> reply = new Dictionary<string, string>();
            reply.Add("value", misc);
            JsonOuputCleaner(reply, source);
        }

        protected void JsonOuputCleaner(Dictionary<string, string> misc, string source)
        {
            if (myConfig.GetEnableJsonOutputEvents() == false)
            {
                return;
            }
            misc.Add("eventsource", source);
            master.CommandsService.SmartCommandReply(
                myConfig.GetJsonOutputEventsTarget(),
                JsonConvert.SerializeObject(misc),
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
            if (ProcessRequest(myConfig.GetAcceptTeleports(), e.IM.FromAgentName, mode) == false)
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
            if (ProcessRequest(myConfig.GetAcceptGroupInvites(), e.FromName, "GroupInvite") == false)
            {
                return;
            }
            e.Accept = true;
            Dictionary<string,string> reply = new Dictionary<string,string>();
            reply.Add("message", e.Message);
            reply.Add("fromuuid", e.AgentID.ToString());
            JsonOuputCleaner(reply, "GroupInvite");
        }

        protected void BotInventoryAdd(object o, InventoryObjectAddedEventArgs e)
        {
            InventoryItem F = (InventoryItem)e.Obj;
            Dictionary<string, string> details = new Dictionary<string, string>
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
            if(ProcessRequest(myConfig.GetAcceptInventory(),e.Offer.FromAgentName, "InventoryOffer") == false)
            {
                return;
            }
            e.Accept = true;
            Dictionary<string, string> details = new Dictionary<string, string>
            {
                { "transactionid", e.Offer.IMSessionID.ToString() },
                { "itemtype", e.AssetType.ToString() },
                { "message", e.Offer.Message.ToString() },
                { "fromscript", e.FromTask.ToString() },
                { "targetfolder", e.FolderID.ToString() }
            };
            if(e.FromTask == false)
            {
                master.DataStoreService.AddAvatar(e.Offer.FromAgentID, e.Offer.FromAgentName);
                details.Add("itemuuid", e.ObjectID.ToString());
                details.Add("itemname", "?");
                InventoryItem itm = GetClient().Inventory.FetchItem(e.ObjectID, GetClient().Self.AgentID, 2000);
                if(itm != null)
                {
                    details["itemname"] = itm.Name;
                }
            }
            JsonOuputCleaner(details, "InventoryOffer");
        }

        protected void BotFriendRequested(object o, FriendshipOfferedEventArgs e)
        {
            if (ProcessRequest(myConfig.GetAcceptGroupInvites(), e.AgentName, "FriendRequest") == false)
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

        public override void Start()
        {
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
        public Dictionary<string, string> misc = new Dictionary<string, string>();
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

