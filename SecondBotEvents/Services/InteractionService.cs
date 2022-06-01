using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace SecondBotEvents.Services
{
    public class InteractionService : Services
    {
        protected InteractionConfig myConfig;
        protected bool botConnected = false;
        public InteractionService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new InteractionConfig(master.fromEnv,master.fromFolder);
            if (myConfig.GetEnabled() == false)
            {
                Console.WriteLine(Status());
                return;
            }
        }

        public override string Status()
        {
            if(myConfig == null)
            {
                return "Interaction Service [Config status broken]";
            }
            if (myConfig.GetEnabled() == false)
            {
                return "Interaction Service [- Not requested -]";
            }
            return "Interaction Service [Waiting for bot]";
        }

        protected bool processRequest(bool enabledByConfig, string avatarName, string sourceName)
        {
            if(myConfig.GetEnabled() == false)
            {
                jsonOuput(false, sourceName, avatarName, "Interactions hooks disabled");
                return false;
            }
            if (enabledByConfig == false)
            {
                jsonOuput(false, sourceName, avatarName, "disabled by config");
                return false;
            }
            if (master.CommandsService.myConfig.GetEnableMasterControls() == false)
            {
                // master avatars list is not enabled. unable to continue
                jsonOuput(false, sourceName, avatarName, "masters AV list is disabled");
                return false;
            }
            if (master.CommandsService.myConfig.GetMastersCSV().Contains(avatarName) == false)
            {
                jsonOuput(false, sourceName, avatarName, "Not from accepted master");
                return false;
            }
            return true;
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            botConnected = false;
            Console.WriteLine("Interaction Service [Attached to new client]");
            getClient().Network.LoggedOut += BotLoggedOut;
            getClient().Network.SimConnected += BotLoggedIn;
        }

        protected void jsonOuput(bool status, string eventype, string from, string why="", Dictionary<string, string> setMisc = null)
        {
            if (myConfig.GetEnableJsonOutputEvents() == false)
            {
                return;
            }
            master.CommandsService.SmartCommandReply(
                status, 
                myConfig.GetJsonOutputEventsTarget(), 
                JsonConvert.SerializeObject(new interactionEvent(from, eventype, status, why, setMisc)), 
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
            if (e.IM.Dialog == InstantMessageDialog.RequestLure)
            {
                mode = "Teleport Av to Bot";
            }
            if (processRequest(myConfig.GetAcceptTeleports(), e.IM.FromAgentName, mode) == false)
            {
                if (e.IM.Dialog != InstantMessageDialog.RequestLure)
                {
                    return;
                }
                getClient().Self.TeleportLureRespond(e.IM.FromAgentID, e.IM.IMSessionID, false);
                return;
            }
            jsonOuput(true, mode, e.IM.FromAgentName);
            if (e.IM.Dialog == InstantMessageDialog.RequestLure)
            {
                getClient().Self.SendTeleportLure(e.IM.FromAgentID);
                return;
            }
            getClient().Self.TeleportLureRespond(e.IM.FromAgentID, e.IM.IMSessionID, true);
            
        }

        protected void BotGroupInviteOffer(object o, GroupInvitationEventArgs e)
        {
            if (processRequest(myConfig.GetAcceptGroupInvites(), e.FromName, "GroupInvite") == false)
            {
                return;
            }
            e.Accept = true;
            jsonOuput(true, "GroupInvite", e.FromName);
        }

        protected void BotInventoryOffer(object o, InventoryObjectOfferedEventArgs e)
        {
            if(processRequest(myConfig.GetAcceptInventory(),e.Offer.FromAgentName, "InventoryUpdate") == false)
            {
                return;
            }
            e.Accept = true;
            Dictionary<string, string> details = new Dictionary<string, string>();
            details.Add("itemuuid", e.ObjectID.ToString());
            details.Add("itemtype", e.AssetType.ToString());
            InventoryItem itm = getClient().Inventory.FetchItem(e.ObjectID, getClient().Self.AgentID, 2000);
            if(itm != null)
            {
                details.Add("itemname", itm.Name);
            }
            jsonOuput(true, "InventoryUpdate", e.Offer.FromAgentName, "see misc", details);
        }

        protected void BotFriendRequested(object o, FriendshipOfferedEventArgs e)
        {
            if (processRequest(myConfig.GetAcceptGroupInvites(), e.AgentName, "FriendRequest") == false)
            {
                return;
            }
            getClient().Friends.AcceptFriendship(e.AgentID, e.SessionID);
            jsonOuput(true, "FriendRequest", e.AgentName);
            
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            botConnected = false;
            getClient().Network.SimConnected += BotLoggedIn;
            Console.WriteLine("Interaction Service [Standby]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            getClient().Network.SimConnected -= BotLoggedIn;
            getClient().Friends.FriendshipOffered += BotFriendRequested;
            getClient().Inventory.InventoryObjectOffered += BotInventoryOffer;
            getClient().Groups.GroupInvitation += BotGroupInviteOffer;
            getClient().Self.IM += BotTeleportOffer;
            botConnected = true;
            Console.WriteLine("Interaction Service [Active]");
        }

        public override void Start()
        {
            if (myConfig.GetEnabled() == false)
            {
                Console.WriteLine("Interaction Service [- Not requested -]");
                return;
            }
            Stop();
            master.BotClientNoticeEvent += BotClientRestart;
            Console.WriteLine("Interaction Service [Starting]");
        }

        public override void Stop()
        {
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            master.BotClientNoticeEvent -= BotClientRestart;
            if (master.botClient != null)
            {
                if (getClient() != null)
                {
                    getClient().Friends.FriendshipOffered -= BotFriendRequested;
                    getClient().Inventory.InventoryObjectOffered -= BotInventoryOffer;
                    getClient().Groups.GroupInvitation -= BotGroupInviteOffer;
                }
            }
            Console.WriteLine("Interaction Service [Stopping]");
        }
    }

    public class interactionEvent
    {
        public string from = "?";
        public string eventType = "?";
        public bool accepted = false;
        public string info = "";
        public Dictionary<string, string> misc = new Dictionary<string, string>();
        public interactionEvent(string setFrom, string setType, bool setAccepted, string setInfo, Dictionary<string, string> setMisc = null)
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

