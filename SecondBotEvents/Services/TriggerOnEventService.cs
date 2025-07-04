﻿using Microsoft.Extensions.Logging;
using OpenMetaverse;
using OpenMetaverse.Rendering;
using SecondBotEvents.Config;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace SecondBotEvents.Services
{
    public class TriggerOnEventService : BotServices
    {
        protected new OnEventConfig myConfig;
        protected bool botConnected = false;
        Dictionary<string,List<CustomOnEvent>> MyCustomEvents = [];
        Dictionary<string, long> lockouts = [];
        List<UUID> trackEventGroups = [];
        public TriggerOnEventService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new OnEventConfig(master.fromEnv, master.fromFolder);
            int loop = 1;
            string[] delimiterChars = ["{IS}", "{NOT}", "{IS_UUD}", "{MISSING}", "{CONTAINS}", "{IN_GROUP}", "{NOT_IN_GROUP}", "{LESSTHAN}", "{MORETHAN}", "{LOCKOUT}"];
            if(myConfig.GetCount() == 0)
            {
                myConfig.setEnabled(false);
                LogFormater.Info("OnEvent Service - No events enabled stopping");
            }
            while (loop <= myConfig.GetCount())
            {
                if(myConfig.GetEventEnabled(loop) == false)
                {
                    LogFormater.Info("OnEvent Service - Event "+loop.ToString()+" is not enabled");
                    loop++;
                    continue;
                }
                CustomOnEvent Event = new()
                {
                    Source = myConfig.GetSource(loop),
                    MonitorFlags = [.. myConfig.GetSourceMonitor(loop).Split("=#=")]
                };
                bool eventWhereOk = true;
                int loop2 = 1;
                while(loop2 <= myConfig.GetWhereCount(loop))
                {
                    string check = myConfig.GetWhereCheck(loop, loop2);
                    foreach (string delimiterChar in delimiterChars)
                    {
                        if(check.Contains(delimiterChar) == false)
                        {
                            continue;
                        }
                        string[] bits = check.Split(delimiterChar, StringSplitOptions.RemoveEmptyEntries);
                        if (bits.Length == 2)
                        {
                            string delimiterCharU = delimiterChar.Replace("{", "");
                            delimiterCharU = delimiterCharU.Replace("}", "");
                            delimiterCharU = delimiterCharU.Trim();
                            Event.WhereChecksLeft.Add(bits[0].Trim());
                            Event.WhereChecksCenter.Add(delimiterCharU);
                            Event.WhereChecksRight.Add(bits[1].Trim());
                        }
                        else
                        {
                            eventWhereOk = false;
                        }
                        break;
                    }
                    loop2++;
                }
                if (eventWhereOk == false)
                {
                    LogFormater.Info("OnEvent Service - Event " + loop.ToString() + " failed where config checks");
                    loop++;
                    continue;
                }
                Event.Source = myConfig.GetSource(loop);
                Event.MonitorFlags = [.. myConfig.GetSourceMonitor(loop).Split("=#=")];
                if ((Event.Source == "GroupMemberJoin") || (Event.Source == "GroupMemberLeave"))
                {
                    UUID groupUUID = UUID.Zero;
                    if (Event.MonitorFlags.Count != 1)
                    {
                        LogFormater.Info("OnEvent Service - Event " + loop.ToString() + " GroupMember monitor flags not vaild");
                        loop++;
                        continue;
                    }
                    if (UUID.TryParse(Event.MonitorFlags[0], out groupUUID) == false)
                    {
                        LogFormater.Info("OnEvent Service - Event " + loop.ToString() + " GroupMember monitor flags not vaild");
                        loop++;
                        continue;
                    }
                    if (trackEventGroups.Contains(groupUUID) == false)
                    {
                        trackEventGroups.Add(groupUUID);
                    }
                }
                else if ((Event.Source == "GuestJoins") || (Event.Source == "GuestLeaves"))
                {
                    if(Event.MonitorFlags.Count != 2)
                    {
                        LogFormater.Info("OnEvent Service - Event " + loop.ToString() + " Guest monitor flags not vaild");
                        loop++;
                        continue;
                    }
                }
                else if (Event.Source == "AvatarIm")
                {
                    if (Event.MonitorFlags.Count != 1)
                    {
                        LogFormater.Info("OnEvent Service - Event " + loop.ToString() + " AvatarIm monitor flags not vaild");
                        loop++;
                        continue;
                    }
                    UUID testUUID = UUID.Zero;
                    UUID.TryParse(Event.MonitorFlags[0],out testUUID);
                    if ((Event.MonitorFlags[0] != "Any") && (testUUID != UUID.Zero))
                    {
                        LogFormater.Info("OnEvent Service - Event " + loop.ToString() + " AvatarIm monitor flags not vaild");
                        loop++;
                        continue;
                    }
                }
                loop2 = 1;
                while (loop2 <= myConfig.GetActionCount(loop))
                {
                    Event.Actions.Add(myConfig.GetActionStep(loop, loop2));
                    loop2++;
                }
                if(Event.Actions.Count == 0)
                {
                    LogFormater.Info("OnEvent Service - Event " + loop.ToString() + " Actions are not setup");
                    loop++;
                    continue;
                }
                if(MyCustomEvents.ContainsKey(Event.Source) == false)
                {
                    MyCustomEvents.Add(Event.Source, []);
                }
                MyCustomEvents[Event.Source].Add(Event);
                LogFormater.Info("OnEvent Service - Event " + loop.ToString() + " is being tracked");
                loop++;
            }
            myConfig.unloadEvents(); // remove unneeded copy of events from the config as we wont be using it
            if (MyCustomEvents.Count == 0)
            {
                myConfig.setEnabled(false);
                LogFormater.Info("OnEvent Service - failed to load any events stopping");
                return;
            }
            LogFormater.Info("OnEvent Service - loaded "+MyCustomEvents.Count.ToString()+" events");
        }


        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isStart == false)
            {
                return;
            }
            botConnected = false;
            LogFormater.Info("OnEvent Service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            botConnected = false;
            GetClient().Network.SimConnected += BotLoggedIn;
            LogFormater.Info("OnEvent Service [Standby]");
        }

        protected void BotImMessage(object o, InstantMessageEventArgs e)
        {
            switch (e.IM.Dialog)
            {
                case InstantMessageDialog.MessageFromObject:
                    {
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        if (e.IM.GroupIM == true)
                        {
                            break;
                        }
                        master.DataStoreService.AddAvatar(e.IM.FromAgentID, e.IM.FromAgentName);
                        // trigger avatar IM
                        TriggerEvent("AvatarIm", new Dictionary<string, string>() {
                            { "message", e.IM.Message} , 
                            { "avatarname", e.IM.FromAgentName}, 
                            { "avataruuid", e.IM.FromAgentID.ToString() },
                        });
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        protected void RemoveEvents()
        {
            if (myConfig.GetDebugMe() == true)
            {
                LogFormater.Info("OnEvent Service (DebugMe) RemoveEvents");
            }
            if (master.BotClient != null)
            {
                if (GetClient() != null)
                {
                    GetClient().Self.IM -= BotImMessage;
                    GetClient().Groups.GroupMembersReply -= BotGroupMembers;
                    GetClient().Groups.CurrentGroups -= BotGroupsCurrent;
                    GetClient().Network.SimChanged -= BotChangedSim;
                    GetClient().Self.MoneyBalance -= BotFundsEvent;
                    GetClient().Self.AlertMessage -= BotAlertMessage;
                }
            }
        }

        protected void AttachEvents()
        {
            if(myConfig.GetDebugMe() == true)
            {
                LogFormater.Info("OnEvent Service (DebugMe) AttachEvents");
            }
            GetClient().Self.IM += BotImMessage;
            GetClient().Groups.GroupMembersReply += BotGroupMembers;
            GetClient().Groups.CurrentGroups += BotGroupsCurrent;
            GetClient().Network.SimChanged += BotChangedSim;
            GetClient().Self.MoneyBalance += BotFundsEvent;
            GetClient().Self.AlertMessage += BotAlertMessage;
            GetClient().Groups.RequestCurrentGroups();
            GetClient().Self.RequestBalance();
        }

        protected void BotAlertMessage(object o, AlertMessageEventArgs e)
        {
            TriggerEvent("SimAlert", new Dictionary<string, string>() {
                        { "alertmessage", e.Message},
                    });
        }

        protected void BotFundsEvent(object o, BalanceEventArgs e)
        {
            TriggerEvent("BalanceUpdate", new Dictionary<string, string>() {
                        { "funds", e.Balance.ToString()},
                    });
        }

        protected void BotGroupsCurrent(object o, CurrentGroupsEventArgs e)
        {
            lock (GroupMembership) lock (GroupMembershipUpdated) lock(GroupNames)
            {
                foreach(Group G in e.Groups.Values)
                {
                    if (trackEventGroups.Contains(G.ID) == false)
                    {
                        return;
                    }
                    if (GroupNames.ContainsKey(G.ID) == true)
                    {
                        continue;
                    }
                    GroupNames.Add(G.ID, G.Name);
                    GroupMembership.Add(G.ID, []);
                    GroupMembershipUpdated.Add(G.ID, 0);
                }
            }
        }

        protected Dictionary<UUID, List<UUID>> GroupMembership = [];
        protected Dictionary<UUID, string> GroupNames = [];
        protected Dictionary<UUID, long> GroupMembershipUpdated = [];
        protected long lastGroupMembershipUpdate = 0; 
        // request group membership updates max of once per 60 secs
        // request membership update for group once every 15 mins
        protected void GroupMembershipUpkeep()
        {
            lock (GroupMembershipUpdated)
            {
                long dif = SecondbotHelpers.UnixTimeNow() - lastGroupMembershipUpdate;
                if (dif < 60)
                {
                    return;
                }
                UUID updateGroup = UUID.Zero;
                foreach (KeyValuePair<UUID, long> entry in GroupMembershipUpdated)
                {
                    dif = SecondbotHelpers.UnixTimeNow() - entry.Value;
                    if (dif > (15 * 60))
                    {
                        updateGroup = entry.Key;
                        break;
                    }
                }
                if (updateGroup != UUID.Zero)
                {
                    lastGroupMembershipUpdate = SecondbotHelpers.UnixTimeNow();
                    GroupMembershipUpdated[updateGroup] = SecondbotHelpers.UnixTimeNow();
                    GetClient().Groups.RequestGroupMembers(updateGroup);
                }
            }
        }

        protected void BotGroupMembers(object sender, GroupMembersReplyEventArgs e)
        {
            if(trackEventGroups.Contains(e.GroupID) == false)
            {
                return;
            }
            lock (GroupMembership) lock(GroupMembershipUpdated)
            {
                bool GroupLoaded = false;
                if (GroupMembership.ContainsKey(e.GroupID) == true)
                {
                    if (GroupMembership[e.GroupID].Count > 0)
                    {
                        GroupLoaded = true;
                    }
                }
                if (GroupLoaded == false)
                {
                    foreach (GroupMember a in e.Members.Values)
                    {
                        GroupMembership[e.GroupID].Add(a.ID);
                    }
                    return;
                }
                List<UUID> joined = [];
                List<UUID> tracked = [.. GroupMembership[e.GroupID]];
                master.DataStoreService.GetAvatarNames(tracked);
                foreach (UUID a in e.Members.Keys)
                {
                    if (tracked.Contains(a) == true)
                    {
                        tracked.Remove(a);
                        continue;
                    }
                    joined.Add(a);

                }
                string groupname = "none";
                if(GroupNames.ContainsKey(e.GroupID) == true)
                {
                    groupname = GroupNames[e.GroupID];
                }
                foreach (UUID a in tracked)
                {
                    // leave
                    GroupMembership[e.GroupID].Remove(a);
                    TriggerEvent("GroupMemberLeave", new Dictionary<string, string>() { 
                        { "groupuuid", e.GroupID.ToString()},
                        { "groupname", groupname },
                        { "avataruuid", a.ToString() } 
                    });
                }
                foreach (UUID a in joined)
                {
                    // join
                    GroupMembership[e.GroupID].Add(a);
                    TriggerEvent("GroupMemberJoin", new Dictionary<string, string>() {
                        { "groupuuid", e.GroupID.ToString()},
                        { "groupname", groupname },
                        { "avataruuid", a.ToString() }
                    });
                    }
            }
        }

        protected string GetAvName(string avataruuid)
        {
            string avatarname = "lookup";
            if (avataruuid != null)
            {
                if (UUID.TryParse(avataruuid, out UUID avUUID) == true)
                {
                    avatarname = master.DataStoreService.GetAvatarName(avUUID);
                }
            }
            return avatarname;
        }

        protected bool IsLockedout(string uuid)
        {
            lock (lockouts)
            {
                if (lockouts.ContainsKey(uuid) == false)
                {
                    return false;
                }
                long now = SecondbotHelpers.UnixTimeNow();
                if(now >= lockouts[uuid])
                {
                    lockouts.Remove(uuid);
                    return false;
                }
                return true;
            }
        }

        protected void AddToLockout(string uuid, int mins)
        {
            lock (lockouts)
            {
                long newvalue = SecondbotHelpers.UnixTimeNow() + (mins * 60);
                if (lockouts.ContainsKey(uuid) == true)
                {
                    if (lockouts[uuid] < newvalue)
                    {
                        lockouts[uuid] = newvalue;
                    }
                    return;
                }
                lockouts.Add(uuid, newvalue);
            }
        }

        long lastCleanedLockouts = 0;
        protected void CleanLockouts()
        {
            long dif = SecondbotHelpers.UnixTimeNow() - lastCleanedLockouts;
            if(dif < 60)
            {
                return;
            }
            lastCleanedLockouts = SecondbotHelpers.UnixTimeNow();
            long now = SecondbotHelpers.UnixTimeNow();
            lock (lockouts)
            {
                List<string> toberemoved = [];
                foreach(KeyValuePair<string, long> pair in lockouts)
                {
                    if(pair.Value > now)
                    {
                        continue;
                    }
                    toberemoved.Add(pair.Key);
                }
                foreach(string A in toberemoved)
                {
                    lockouts.Remove(A);
                }
            }
        }
        protected void BotChangedSim(object sender, SimChangedEventArgs e)
        {
            TriggerEvent("ChangeSim", new Dictionary<string, string>()
            {
                { "oldsimname", e.PreviousSimulator.Name },
                { "newsimname", GetClient().Network.CurrentSim.Name },
            });
        }

        protected string inArgsOrDefault(string arg, Dictionary<string, string> args, string defaultvalue)
        {
            if (args.ContainsKey(arg) == true)
            {
                return args[arg];
            }
            return defaultvalue;
        }
        protected void TriggerEvent(string eventName, Dictionary<string,string> args)
        {
            if (waitingForTimer == true)
            {
                return;
            }
            if (myConfig.GetDebugMe() == true)
            {
                StringBuilder debugoutme = new();
                string debugoutmeaddon = "";
                foreach(KeyValuePair<string,string> Ar in args)
                {
                    debugoutme.Append(Ar.Key);
                    debugoutme.Append("=");
                    debugoutme.Append(Ar.Value);
                    debugoutme.Append(debugoutmeaddon);
                    debugoutmeaddon = ",";
                }
                LogFormater.Info("OnEvent Service (DebugMe) TriggerEvent "+eventName+" args: "+ debugoutme.ToString());
            }
            if (MyCustomEvents.ContainsKey(eventName) == false)
            {
                if (myConfig.GetDebugMe() == true)
                {
                    LogFormater.Info("OnEvent Service (DebugMe) Not a tracked event");
                }
                return;
            }
            string avataruuid = inArgsOrDefault("avataruuid", args, null);

            List<int> AvPos = GetAvPos(avataruuid);
            Vector3 A = GetClient().Self.SimPosition;
            List<int> BotPos = [(int)Math.Round(A.X), (int)Math.Round(A.Y), (int)Math.Round(A.Z)];
            Dictionary<string, string> values = new()
            {
                { "alertmessage", inArgsOrDefault("alertmessage", args, "none") },
                { "message",  inArgsOrDefault("message", args, "none") },
                { "funds", inArgsOrDefault("funds", args, "-1") },
                { "eventype", eventName },
                { "groupuuid", inArgsOrDefault("groupuuid", args, "none") },
                { "groupname", inArgsOrDefault("groupname", args, "none") },
                { "oldsimname", inArgsOrDefault("oldsimname", args, "none") },
                { "newsimname", inArgsOrDefault("newsimname", args, "none") },
                { "avataruuid", avataruuid },
                { "avatarname", GetAvName(avataruuid) },
                { "avatarparcel", GetAvParcel(avataruuid) },
                { "botsim", GetClient().Network.CurrentSim.Name },
                { "botparcel", GetBotParcel() },
                { "avatarx", AvPos[0].ToString() },
                { "avatary", AvPos[1].ToString() },
                { "avatarz", AvPos[2].ToString() },
                { "avatardistance", AvPos[3].ToString() },
                { "botx", AvPos[0].ToString() },
                { "boty", AvPos[1].ToString() },
                { "botz", AvPos[2].ToString() },
                { "clockhour", DateTime.Now.ToString("HH") },
                { "clockmin", DateTime.Now.ToString("mm") },
                { "dayofweek", ((int)DateTime.Now.DayOfWeek).ToString() }
            };
            if (myConfig.GetDebugMe() == true)
            {
                LogFormater.Info("OnEvent Service (DebugMe) There are "+ MyCustomEvents[eventName].Count.ToString()+" events to trigger");
            }
            int eventid = -1;
            foreach (CustomOnEvent E in MyCustomEvents[eventName])
            {
                eventid++;
                bool canFireEvent = true;
                if ((E.Source == "GroupMemberJoin") || (E.Source == "GroupMemberLeave"))
                {
                    if (E.MonitorFlags[0] != values["groupuuid"])
                    {
                        if (myConfig.GetDebugMe() == true)
                        {
                            LogFormater.Info("OnEvent Service (DebugMe) Failed checks on " + eventName + " id " + (eventid + 1).ToString() + " failed Group checks");
                        }
                        continue;
                    }
                }
                else if ((E.Source == "GuestJoins") || (E.Source == "GuestLeaves"))
                {
                    if (
                        (E.MonitorFlags[0] != values["botsim"]) || 
                        (E.MonitorFlags[1] != values["botparcel"]) || 
                        (E.MonitorFlags[1] != values["avatarparcel"])
                    )
                    {
                        if (myConfig.GetDebugMe() == true)
                        {
                            LogFormater.Info("OnEvent Service (DebugMe) Failed checks on " + eventName + " id " + (eventid + 1).ToString() + " failed Guest checks");
                        }
                        continue;
                    }
                }
                int loop = 0;
                while(loop < E.WhereChecksLeft.Count)
                {
                    string left = swapvalues(E.WhereChecksLeft[loop],values);
                    string center = E.WhereChecksCenter[loop];
                    string right = swapvalues(E.WhereChecksRight[loop], values);
                    int leftAsInt = -1;
                    int rightAsInt = -1;
                    int.TryParse(left, out leftAsInt);
                    int.TryParse(right, out rightAsInt);
                    bool leftAsBool = false;
                    if(left == "true") leftAsBool = true;
                    bool rightAsBool = false;
                    if (right == "true") rightAsBool = true;
                    bool leftContactinsRight = left.Contains(right);
                    UUID leftAsUUID = UUID.Zero;
                    bool leftIsUUID = UUID.TryParse(left, out leftAsUUID);
                    UUID rightAsUUID = UUID.Zero;
                    bool rightIsUUID = UUID.TryParse(right, out rightAsUUID);
                    bool leftIsInGroup = false;
                    bool leftIsLockedout = IsLockedout(left);
                    bool leftIsEven = true;
                    bool leftIsDivs = true;
                    if (leftAsInt != 0)
                    {
                        leftIsEven = ((leftAsInt % 2) == 0);
                        if(rightAsInt != 0)
                        {
                            leftIsDivs = ((leftAsInt % rightAsInt) == 0);
                        }
                    }
                    if ((leftIsUUID == true) && (rightIsUUID == true))
                    {
                        leftIsInGroup = master.DataStoreService.IsGroupMember(rightAsUUID, leftAsUUID);
                    }
                    if ((center == "IS") && (left != right)) { canFireEvent = false; }
                    else if ((center == "NOT") && (left == right)) { canFireEvent = false; }
                    else if ((center == "IS_UUID") && (leftIsUUID != rightAsBool)) { canFireEvent = false; }
                    else if ((center == "MISSING") && (leftContactinsRight == true)) { canFireEvent = false; }
                    else if ((center == "CONTAINS") && (leftContactinsRight == false)) { canFireEvent = false; }
                    else if ((center == "IN_GROUP") && (leftIsInGroup == false)) { canFireEvent = false; }
                    else if ((center == "NOT_IN_GROUP") && (leftIsUUID == true) && (rightIsUUID == true) && (leftIsInGroup == true)) { canFireEvent = false; }
                    else if ((center == "LESSTHAN") && (leftAsInt >= rightAsInt)) { canFireEvent = false; }
                    else if ((center == "MORETHAN") && (leftAsInt <= rightAsInt)) { canFireEvent = false; }
                    else if ((center == "LOCKOUT") && (leftIsLockedout != rightAsBool)) { canFireEvent = false; }
                    else if ((center == "IS_EVEN") && (leftIsEven != rightAsBool)) { canFireEvent = false; }
                    else if ((center == "DIVISIBLE") && (leftIsDivs == false)) { canFireEvent = false; }
                    if (canFireEvent == false)
                    {
                        if (myConfig.GetDebugMe() == true)
                        {
                            LogFormater.Info("OnEvent Service (DebugMe) Failed checks on " + eventName + " id " + (eventid + 1).ToString()+" rule id "+(loop+1).ToString()+" #"+center);
                        }
                        break;
                    }
                    loop++;
                }
                if (canFireEvent == false)
                {
                    continue;
                }
                if (myConfig.GetDebugMe() == true)
                {
                    LogFormater.Info("OnEvent Service (DebugMe) Triggering "+E.Actions.Count().ToString()+ " actions for event id "+(eventid+1).ToString());
                }
                int loop2 = 1;
                foreach (string action in E.Actions)
                {
                    string useaction = swapvalues(action, values);
                    string[] bits = useaction.Split('=');
                    if(bits.Length == 2)
                    {
                        if (bits[0] == "lockout")
                        {
                            bits = bits[1].Split("+");
                            if(bits.Length == 2)
                            {
                                if (int.TryParse(bits[1],out int mins) == true)
                                {
                                    AddToLockout(bits[0], mins);
                                }
                            }
                            continue;
                        }
                    }
                    if (myConfig.GetDebugMe() == true)
                    {
                        LogFormater.Info("OnEvent Service (DebugMe) Triggering " + useaction + " actions for event id " + (loop2).ToString());
                    }
                    loop2++;
                    master.CommandsService.CommandInterfaceCaller(useaction, false, false, "OnEvents");
                }
            }
        }

        protected string swapvalues(string input, Dictionary<string, string> values)
        {
            foreach(KeyValuePair<string, string> pair in values)
            {
                input = input.Replace("["+pair.Key+"]", pair.Value);
            }
            return input;
        }

        protected List<int> GetAvPos(string avataruuid)
        {
            if (UUID.TryParse(avataruuid, out UUID avUUID) == false)
            {
                return [-1, -1, -1, -1];
            }
            foreach (Avatar A in GetClient().Network.CurrentSim.ObjectsAvatars.ToDictionary(k => k.Key, v => v.Value).Values)
            {
                if (A.ID != avUUID)
                {
                    continue;
                }
                Vector3 avpos = A.Position;
                int dist = (int)Math.Round(Vector3.Distance(avpos, GetClient().Self.SimPosition));
                return [(int)Math.Round(A.Position.X), (int)Math.Round(A.Position.Y), (int)Math.Round(A.Position.Z), dist];
            }
            return [-1, -1, -1, -1];
        }

        protected string GetAvParcel(string avataruuid)
        {
            if (UUID.TryParse(avataruuid, out UUID avUUID) == false)
            {
                return "?";
            }
            foreach (Avatar A in GetClient().Network.CurrentSim.ObjectsAvatars.ToDictionary(k => k.Key, v => v.Value).Values)
            {
                if(A.ID != avUUID)
                {
                    continue;
                }
                int localid = GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, A.Position);
                if (GetClient().Network.CurrentSim.Parcels.ContainsKey(localid) == false)
                {
                    return "?";
                }
                return GetClient().Network.CurrentSim.Parcels[localid].Name;
            }
            return "?";
        }

        protected string GetBotParcel()
        {
            int localid = GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, GetClient().Self.SimPosition);
            if (GetClient().Network.CurrentSim.Parcels.ContainsKey(localid) == false)
            {
                return "-";
            }
            return GetClient().Network.CurrentSim.Parcels[localid].Name;
        }

        string avatarhash = "";
        bool GuestListLoaded = false;
        protected List<UUID> AvatarsOnSim = [];
        protected void TrackAvatarsOnParcel()
        {
            long dif = SecondbotHelpers.UnixTimeNow() - lastAvUpdate;
            if (dif < 30)
            {
                // only check for changes every 30 secs
                return;
            }
            lastAvUpdate = SecondbotHelpers.UnixTimeNow();
            string hashstring = "";
            List<UUID> avs = [];
            foreach (Avatar A in GetClient().Network.CurrentSim.ObjectsAvatars.ToDictionary(k => k.Key, v => v.Value).Values)
            {
                avs.Add(A.ID);
                master.DataStoreService.AddAvatar(A.ID, A.Name);
                hashstring = hashstring + A.ID.ToString();
            }
            string hash = SecondbotHelpers.GetSHA1(hashstring);
            if (hash == avatarhash)
            {
                // no changes
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
                    List<UUID> seen = [];
                    if (tracked.Count > 0)
                    {
                        
                        foreach (UUID a in tracked)
                        {
                            if(seen.Contains(a) == true)
                            {
                                continue;
                            }
                            seen.Add(a);
                            TriggerEvent("GuestLeaves", new Dictionary<string, string>() { { "avataruuid", a.ToString() } });
                        }
                    }
                    if (joined.Count > 0)
                    {
                        foreach (UUID a in joined)
                        {
                            if (seen.Contains(a) == true)
                            {
                                continue;
                            }
                            seen.Add(a);
                            TriggerEvent("GuestJoins", new Dictionary<string, string>() { { "avataruuid", a.ToString() } });
                        }
                    }
                }
            }
            AvatarsOnSim = avs;
            GuestListLoaded = true;
        }

        long lastClockEvent = 0;
        protected void ClockEvent()
        {
            long dif = SecondbotHelpers.UnixTimeNow() - lastClockEvent;
            if (dif < 60)
            { 
                return;
            }
            lastClockEvent = SecondbotHelpers.UnixTimeNow();
            TriggerEvent("Clock", []);
        }

        long lastAvUpdate = 0;
        protected void TimedEvents()
        {
            if(waitingForTimer == true)
            {
                return;
            }
            TrackAvatarsOnParcel();
            CleanLockouts();
            ClockEvent();
            GroupMembershipUpkeep();
        }

        public override string Status()
        {
            if (myConfig == null)
            {
                return "No Config";
            }
            else if (myConfig.GetEnabled() == false)
            {
                return "- Not requested -";
            }
            if (botConnected == true)
            {
                TimedEvents();
            }
            if (myConfig.GetHideStatusOutput() == true)
            {
                return "hidden";
            }
            else if (botConnected == false)
            {
                return "Waiting for bot";
            }
            else if(waitingForTimer == true)
            {
                if(SecondbotHelpers.UnixTimeNow() >= waitUntil)
                {
                    waitingForTimer = false;
                    return "Ready for on events";
                }
                return "Waiting for timer";
            }
            return "Active";
        }
        protected bool waitingForTimer = false;
        protected long waitUntil = 0;

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            botConnected = true;
            LogFormater.Info("OnEvent Service [Active]");
            waitingForTimer = true;
            waitUntil = SecondbotHelpers.UnixTimeNow() + myConfig.GetWaitSecsToStart();
            AttachEvents();
        }

        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            if (updateEnabled)
            {
                myConfig.setEnabled(setEnabledTo);
            }
            if (myConfig.GetEnabled() == false)
            {
                LogFormater.Info("OnEvent Service [- Not requested -]");
                return;
            }
            Stop();
            running = true;
            waitingForTimer = true;
            waitUntil = SecondbotHelpers.UnixTimeNow() + 9999;
            master.BotClientNoticeEvent += BotClientRestart;
            LogFormater.Info("OnEvent Service [Starting]");
        }

        public override void Stop()
        {
            if (running == true)
            {
                LogFormater.Info("OnEvent Service [Stopping]");
            }
            running = false;
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            master.BotClientNoticeEvent -= BotClientRestart;
            RemoveEvents();
            waitingForTimer = true;
            waitUntil = SecondbotHelpers.UnixTimeNow() + 9999;
        }
    }

    public class CustomOnEvent
    {
        public string Source = "";
        public List<string> MonitorFlags = [];
        public List<string> WhereChecksLeft = [];
        public List<string> WhereChecksCenter = [];
        public List<string> WhereChecksRight = [];
        public List<string> Actions = [];
    }
}
