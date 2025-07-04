﻿using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;
using Swan;
using RestSharp;

namespace SecondBotEvents.Services
{
    public class EventsService : BotServices
    {
        protected new EventsConfig myConfig;
        protected bool botConnected = false;
        protected string lastSimName = "";
        protected UUID TrackGroupUUID = UUID.Zero;
        protected List<UUID> GroupMembers = [];
        protected bool GroupLoaded = false;
        protected List<UUID> AvatarsOnSim = [];
        protected bool GuestListLoaded = false;
        protected string lastparcelname = "";
        protected UUID OutputAvatar = UUID.Zero;
        protected long lastGroupMembershipUpdate = 0;


        public void InventoryUpdateEvent(Dictionary<string, string> detail)
        {
            detail["itemtype"] = detail["itemtype"].ToLower().FirstCharToUpper();
            if (InventoryEvents.ContainsKey(detail["itemtype"]) == true)
            {
                foreach(string A in InventoryEvents[detail["itemtype"]])
                {
                    string output = JsonConvert.SerializeObject(detail);
                    if(UUID.TryParse(A, out UUID OutputAvatar) == true)
                    {
                        if (OutputAvatar != UUID.Zero)
                        {
                            GetClient().Self.InstantMessage(OutputAvatar, output);
                        }
                    }
                    else if (A.StartsWith("http") == true)
                    {
                        long unixtime = SecondbotHelpers.UnixTimeNow();
                        string token = SecondbotHelpers.GetSHA1(unixtime.ToString() + "EventService" + GetClient().Self.AgentID + output+master.CommandsService.myConfig.GetSharedSecret());
                        var client = new RestClient(A);
                        var request = new RestRequest("Inventory/Update", Method.Post);
                        request.AddParameter("token", token);
                        request.AddParameter("unixtime", unixtime.ToString());
                        request.AddParameter("method", "Inventory");
                        request.AddParameter("action", "Update");
                        request.AddParameter("botname", GetClient().Self.Name);
                        request.AddParameter("event", output);
                        request.AddHeader("content-type", "application/x-www-form-urlencoded");
                        client.ExecutePostAsync(request);
                    }
                    else if (int.TryParse(A, out int chann) == true)
                    {
                        if (chann >= 0)
                        {
                            GetClient().Self.Chat(output, chann, ChatType.Normal);
                        }
                    }

                }
            }
        }

        Dictionary<string, List<string>> InventoryEvents = [];
        public void AddInventoryEvent(string inventoryType, string outputTarget)
        {
            if(InventoryEvents.ContainsKey(inventoryType) == false)
            {
                InventoryEvents.Add(inventoryType, []);
            }
            if (InventoryEvents[inventoryType].Contains(outputTarget) == false)
            {
                InventoryEvents[inventoryType].Add(outputTarget);
            }
        }

        public EventsService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new EventsConfig(master.fromEnv, master.fromFolder);
            if(myConfig.GetEnabled() == false)
            {
                return;
            }
            if (UUID.TryParse(myConfig.GetOutputIMuuid(), out OutputAvatar) == false)
            {
                OutputAvatar = UUID.Zero;
            }
            if (UUID.TryParse(myConfig.GetGroupMemberEventsGroupUUID(), out TrackGroupUUID) == false)
            {
                TrackGroupUUID = UUID.Zero;
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
            TimedEvents();
            return "Active";
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isStart == false)
            {
                return;
            }
            LogFormater.Info("Events service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            GetClient().Network.SimConnected += BotLoggedIn;
            botConnected = false;
            LogFormater.Info("Events service [Bot commands disabled]");
            removeEvents();
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            botConnected = true;
            // attach events
            GetClient().Self.MoneyBalanceReply += BotMoneyEvent;
            GetClient().Network.SimChanged += BotChangedSim;
            GetClient().Groups.GroupMembersReply += BotGroupMembers;
            GetClient().Self.AlertMessage += BotAlertMessage;
        }

        protected long lastAvUpdate = 0;
        protected string avatarhash = "";
        protected void TrackAvatarsOnParcel()
        {
            if ((myConfig.GetGuestTrackingSimname() != GetClient().Network.CurrentSim.Name) || (myConfig.GetGuestTrackingParcelname() != lastparcelname))
            {
                AvatarsOnSim = [];
                GuestListLoaded = false;
                return;
            }
            long dif = SecondbotHelpers.UnixTimeNow() - lastAvUpdate;
            if(dif < 30)
            {
                // only check for changes every 30 secs
                return;
            }
            lastAvUpdate = SecondbotHelpers.UnixTimeNow();

            string hashstring = "";
            List<UUID> avs = [];
            foreach(Avatar A in GetClient().Network.CurrentSim.ObjectsAvatars.ToDictionary(k => k.Key, v => v.Value).Values)
            {
                avs.Add(A.ID);
                hashstring = hashstring+A.ID.ToString();
            }
            string hash = SecondbotHelpers.GetSHA1(hashstring);
            if (hash == avatarhash)
            {
                return;
            }
            avatarhash = hash;

            if (GuestListLoaded == true)
            {
                List<UUID> joined = [];
                List<UUID> tracked = [.. AvatarsOnSim];
                foreach (UUID a in avs)
                {
                    if (tracked.Contains(a) == true)
                    {
                        tracked.Remove(a);
                        continue;
                    }
                    joined.Add(a);
                }
                if ((tracked.Count > 0) || (joined.Count > 0))
                {
                    if ((tracked.Count > 0) && (myConfig.GetGuestLeavesArea() == true))
                    {
                        foreach (UUID a in tracked)
                        {
                            GuestTrackingEvent(a, "exit");
                        }
                    }
                    if ((joined.Count > 0) && (myConfig.GetGuestEntersArea() == true))
                    {
                        foreach (UUID a in joined)
                        {
                            GuestTrackingEvent(a, "enter");
                        }
                    }
                }
            }
            AvatarsOnSim = avs;
            GuestListLoaded = true;
        }
        protected void TimedEvents()
        {
            int localid = GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, GetClient().Self.SimPosition);
            if (GetClient().Network.CurrentSim.Parcels.ContainsKey(localid) == false)
            {
                GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, GetClient().Self.SimPosition);
                return;
            }
            if(lastparcelname != GetClient().Network.CurrentSim.Parcels[localid].Name)
            {
                BotChangedParcel(GetClient().Network.CurrentSim.Parcels[localid].Name);
            }
            TrackAvatarsOnParcel();
            if(TrackGroupUUID != UUID.Zero)
            {
                long dif = SecondbotHelpers.UnixTimeNow() - lastGroupMembershipUpdate;
                if (dif > 120)
                {
                    lastGroupMembershipUpdate = SecondbotHelpers.UnixTimeNow();
                    GetClient().Groups.RequestGroupMembers(TrackGroupUUID);
                }
            }
        }

        protected void removeEvents()
        {
            botConnected = false;
            if (GetClient() != null)
            {
                GetClient().Self.MoneyBalanceReply -= BotMoneyEvent;
                GetClient().Network.SimChanged -= BotChangedSim;
                GetClient().Groups.GroupMembersReply -= BotGroupMembers;
                GetClient().Self.AlertMessage -= BotAlertMessage;
            }
        }

        protected void PushEvent(string eventname, Dictionary<string,string> args)
        {
            OnEvent e = new(
                GetClient().Self.AgentID.ToString(), 
                eventname, 
                args, 
                myConfig.GetOutputSecret()
            );
            string output = JsonConvert.SerializeObject(e);
            if(myConfig.GetOutputChannel() >= 0)
            {
                GetClient().Self.Chat(output, myConfig.GetOutputChannel(), ChatType.Normal);
            }
            if(OutputAvatar != UUID.Zero)
            {
                GetClient().Self.InstantMessage(OutputAvatar, output);
            }
            if(myConfig.GetOutputHttpURL() != null)
            {
                long unixtime = SecondbotHelpers.UnixTimeNow();
                string token = SecondbotHelpers.GetSHA1(unixtime.ToString() + "EventService" + GetClient().Self.AgentID + output + master.CommandsService.myConfig.GetSharedSecret());
                var client = new RestClient(myConfig.GetOutputHttpURL());
                var request = new RestRequest("Event/Service", Method.Post);
                request.AddParameter("token", token);
                request.AddParameter("unixtime", unixtime.ToString());
                request.AddParameter("method", "Dialog");
                request.AddParameter("action", "Relay");
                request.AddParameter("botname", GetClient().Self.Name);
                request.AddParameter("event", output);
                request.AddHeader("content-type", "application/x-www-form-urlencoded");
                client.ExecutePostAsync(request);
            }

        }

        protected void BotAlertMessage(object senderm, AlertMessageEventArgs e)
        {
            if(myConfig.GetSimAlertMessage() == false)
            {
                return;
            }
            Dictionary<string, string> args = new()
            {
                { "Message", e.Message },
                { "NotificationId", e.NotificationId }
            };
            PushEvent("AlertMessage", args);
        }

        protected void BotChangedParcel(string newparcelname)
        {
            if (myConfig.GetChangeParcel() == false)
            {
                return;
            }
            Dictionary<string, string> args = new()
            {
                { "oldparcel", lastparcelname },
                { "newparcel", newparcelname }
            };
            lastparcelname = newparcelname;
            PushEvent("ChangedParcel", args);
        }

        protected void BotStatusMessage(object sender, SystemStatusMessage e)
        {
            if((e.changed == false) || (myConfig.GetStatusMessage() == false))
            {
                return;
            }
            Dictionary<string, string> args = new()
            {
                { "Message", e.message }
            };
            PushEvent("StatusMessage", args);
        }

        protected void BotGroupMembers(object sender, GroupMembersReplyEventArgs e)
        {
            if(e.GroupID != TrackGroupUUID)
            {
                return;
            }
            if(GroupLoaded == true)
            {
                List<UUID> joined = [];
                List<UUID> tracked = [.. GroupMembers];
                master.DataStoreService.GetAvatarNames(tracked);
                foreach (UUID a in e.Members.Keys)
                {
                    if(tracked.Contains(a) == true)
                    {
                        tracked.Remove(a);
                        continue;
                    }
                    joined.Add(a);
                }
                if((tracked.Count > 0) && (myConfig.GetGroupMemberLeaves() == true))
                {
                    foreach(UUID a in tracked)
                    {
                        GroupMembershipEvent(a, "leave");
                    }
                }
                if ((joined.Count > 0) && (myConfig.GetGroupMemberJoins() == true))
                {
                    foreach (UUID a in joined)
                    {
                        GroupMembershipEvent(a, "join");
                    }
                }
            }

            GroupMembers = [.. e.Members.Keys];
            GroupLoaded = true;
        }

        protected void GroupMembershipEvent(UUID avatar, string action)
        {
            Dictionary<string, string> args = new()
            {
                { "uuid", avatar.ToString() },
                { "name", master.DataStoreService.GetAvatarName(avatar) },
                { "action", action }
            };
            PushEvent("GroupMembership", args);
        }

        protected void GuestTrackingEvent(UUID avatar, string action)
        {
            Dictionary<string, string> args = new()
            {
                { "uuid", avatar.ToString() },
                { "name", master.DataStoreService.GetAvatarName(avatar) },
                { "action", action }
            };
            PushEvent("GuestTracking", args);
        }

        protected void BotChangedSim(object sender, SimChangedEventArgs e)
        {
            if(myConfig.GetChangeSim() == false)
            {
                return;
            }
            Dictionary<string, string> args = new()
            {
                { "lastsim", lastSimName },
                { "newsim", GetClient().Network.CurrentSim.Name }
            };
            PushEvent("ChangedSim", args);
            lastSimName = e.PreviousSimulator.Name;
        }

        protected void BotMoneyEvent(object sender, MoneyBalanceReplyEventArgs e)
        {
            if(myConfig.GetMoneyEvent() == false)
            {
                return;
            }
            Dictionary<string, string> args = new()
            {
                { "fromuuid", e.TransactionInfo.SourceID.ToString() },
                { "touuid", e.TransactionInfo.DestID.ToString() },
                { "amount", e.TransactionInfo.Amount.ToString() },
                { "balance", e.Balance.ToString() },
                { "fromname", master.DataStoreService.GetAvatarName(e.TransactionInfo.SourceID) },
                { "toname", master.DataStoreService.GetAvatarName(e.TransactionInfo.DestID) },
                { "transactionid", e.TransactionID.ToString() }
            };
            PushEvent("MoneyEvent", args);
        }



        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            if (updateEnabled)
            {
                myConfig.setEnabled(setEnabledTo);
            }
            if (myConfig.GetEnabled() == false)
            {
                LogFormater.Info("Events service [Disabled]");
                return;
            }
            Stop();
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
            master.SystemStatusMessagesEvent += BotStatusMessage;
        }

        public override void Stop()
        {
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            running = false;
            master.BotClientNoticeEvent -= BotClientRestart;
            master.SystemStatusMessagesEvent -= BotStatusMessage;
            removeEvents();
        }
    }

    public class OnEvent
    {
        public string EventName = "";
        public Dictionary<string, string> Data = [];
        public string Hash = "";
        public long Unixtime = 0;
        public string BotUUID = "";

        public OnEvent(string eventBotUUID, string setEventName, Dictionary<string,string> eventData, string Secret)
        {
            BotUUID = eventBotUUID;
            EventName = setEventName;
            Data = eventData;
            Unixtime = SecondbotHelpers.UnixTimeNow();
            SignEvent(Secret);
        }

        public string getJson()
        {
            return "";
        }


        
        protected void SignEvent(string Secret)
        {
            string raw = BotUUID+ EventName;
            foreach(string A in Data.Values)
            {
                raw = raw + A;
            }
            raw = raw + Unixtime.ToString() + Secret;
            Hash = SecondbotHelpers.GetSHA1(raw);
        }
    }

    public class GuestUpdateEvent
    {
        public string parcelName = "";
        public string regionName = "";
        public string avatarName = "";
        public UUID avatarUUID = UUID.Zero;
        public bool entered = false;
    }
}
