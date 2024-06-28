using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecondBotEvents.Services
{
    public class TriggerOnEventService : BotServices
    {
        protected OnEventConfig myConfig;
        protected bool botConnected = false;
        Dictionary<string,List<CustomOnEvent>> MyCustomEvents = new Dictionary<string, List<CustomOnEvent>>();
        Dictionary<string, long> lockouts = new Dictionary<string, long>();
        public TriggerOnEventService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new OnEventConfig(master.fromEnv, master.fromFolder);
            int loop = 1;
            string[] delimiterChars = new string[] { "{IS}", "{NOT}", "{IS_UUD}", "{MISSING}", "{CONTAINS}", "{IN_GROUP}", "{NOT_IN_GROUP}", "{LESSTHAN}", "{MORETHAN}", "{LOCKOUT}" };
            while (loop < myConfig.GetCount())
            {
                if(myConfig.GetEventEnabled(loop) == false)
                {
                    loop++;
                    continue;
                }
                CustomOnEvent Event = new CustomOnEvent();
                Event.Source = myConfig.GetSource(loop);
                Event.MonitorFlags = myConfig.GetSourceMonitor(loop).Split(',').ToList();
                bool eventWhereOk = true;
                int loop2 = 1;
                while(loop2 < myConfig.GetWhereCount(loop))
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
                if(eventWhereOk == false)
                {
                    continue;
                }
                loop2 = 1;
                while (loop2 < myConfig.GetActionCount(loop))
                {
                    Event.Actions.Add(myConfig.GetActionStep(loop, loop2));
                    loop2++;
                }
                if(MyCustomEvents.ContainsKey(Event.Source) == false)
                {
                    MyCustomEvents.Add(Event.Source, new List<CustomOnEvent>());
                }
                MyCustomEvents[Event.Source].Add(Event);
                loop++;
            }
            myConfig.unloadEvents(); // remove unneeded copy of events from the config as we wont be using it
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
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
            if (master.BotClient != null)
            {
                if (GetClient() != null)
                {
                    GetClient().Self.IM -= BotImMessage;
                }
            }
        }

        protected void AttachEvents()
        {
            GetClient().Self.IM += BotImMessage;
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
                List<string> toberemoved = new List<string>();
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

        protected void TriggerEvent(string eventName, Dictionary<string,string> args)
        {
            if (MyCustomEvents.ContainsKey(eventName) == false)
            {
                return;
            }
            string avataruuid = null;
            if (args.ContainsKey("avataruuid") == true)
            {
                avataruuid = args["avataruuid"];
            }
            Dictionary<string, string> values = new Dictionary<string, string>();
            values.Add("eventype", eventName);
            values.Add("avatarname", GetAvName(avataruuid));
            values.Add("avatarparcel", GetAvParcel(avataruuid));
            values.Add("botsim", GetClient().Network.CurrentSim.Name);
            values.Add("botparcel", GetBotParcel());
            List<int> AvPos = GetAvPos(avataruuid);
            values.Add("avatarx", AvPos[0].ToString());
            values.Add("avatary", AvPos[1].ToString());
            values.Add("avatarz", AvPos[2].ToString());
            values.Add("avatardistance", AvPos[3].ToString());
            Vector3 A = GetClient().Self.SimPosition;
            List<int> BotPos = new List<int>() { (int)Math.Round(A.X), (int)Math.Round(A.Y), (int)Math.Round(A.Z) };
            values.Add("botx", AvPos[0].ToString());
            values.Add("boty", AvPos[1].ToString());
            values.Add("botz", AvPos[2].ToString());
            values.Add("clockhour", DateTime.Now.ToString("HH"));
            values.Add("clockmin", DateTime.Now.ToString("mm"));
            values.Add("dayofweek", ((int)DateTime.Now.DayOfWeek).ToString());
            

            if (avataruuid == null)
            {
                avataruuid = "none";
            }
            if(args.ContainsKey("message") == false)
            {
                args["message"] = "none";
            }
            values.Add("avataruuid", avataruuid);
            foreach (CustomOnEvent E in MyCustomEvents[eventName])
            {
                bool canFireEvent = true;
                int loop = 0;
                while(loop < E.WhereChecksLeft.Count())
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
                    bool leftIsEven = (leftAsInt % 2 == 0);
                    bool leftIsDivs = (leftAsInt % rightAsInt == 0);
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
                        break;
                    }
                    loop++;
                }
                if (canFireEvent == false)
                {
                    continue;
                }
                foreach(string action in E.Actions)
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
                return new List<int>() { -1, -1, -1, -1 };
            }
            foreach (Avatar A in GetClient().Network.CurrentSim.ObjectsAvatars.Copy().Values)
            {
                if (A.ID != avUUID)
                {
                    continue;
                }
                Vector3 avpos = A.Position;
                int dist = (int)Math.Round(Vector3.Distance(avpos, GetClient().Self.SimPosition));
                return new List<int>() { (int)Math.Round(A.Position.X), (int)Math.Round(A.Position.Y), (int)Math.Round(A.Position.Z), dist };
            }
            return new List<int>() { -1, -1, -1, -1 };
        }

        protected string GetAvParcel(string avataruuid)
        {
            if (UUID.TryParse(avataruuid, out UUID avUUID) == false)
            {
                return "?";
            }
            foreach (Avatar A in GetClient().Network.CurrentSim.ObjectsAvatars.Copy().Values)
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
        protected List<UUID> AvatarsOnSim = new List<UUID>();
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
            List<UUID> avs = new List<UUID>();
            foreach (Avatar A in GetClient().Network.CurrentSim.ObjectsAvatars.Copy().Values)
            {
                avs.Add(A.ID);
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
                List<UUID> joined = new List<UUID>();
                List<UUID> tracked = new List<UUID>();
                foreach (UUID a in AvatarsOnSim)
                {
                    tracked.Add(a);
                }
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
                    if (tracked.Count > 0)
                    {
                        foreach (UUID a in tracked)
                        {
                            TriggerEvent("GuestLeaves", new Dictionary<string, string>() { { "avataruuid", a.ToString() } });
                        }
                    }
                    if (joined.Count > 0)
                    {
                        foreach (UUID a in joined)
                        {
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
            TriggerEvent("Clock", new Dictionary<string, string>());
        }

        long lastAvUpdate = 0;
        protected void TimedEvents()
        {
            TrackAvatarsOnParcel();
            CleanLockouts();
            ClockEvent();
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
            return "Active";
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            botConnected = true;
            LogFormater.Info("OnEvent Service [Active]");
            AttachEvents();
        }

        public override void Start()
        {
            if (myConfig.GetEnabled() == false)
            {
                LogFormater.Info("OnEvent Service [- Not requested -]");
                return;
            }
            Stop();
            running = true;
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
        }
    }

    public class CustomOnEvent
    {
        public string Source = "";
        public List<string> MonitorFlags = new List<string>();
        public List<string> WhereChecksLeft = new List<string>();
        public List<string> WhereChecksCenter = new List<string>();
        public List<string> WhereChecksRight = new List<string>();
        public List<string> Actions = new List<string>();
    }
}
