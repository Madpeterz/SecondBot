using BetterSecondBot;
using BetterSecondBot.bottypes;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using Newtonsoft.Json;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace BetterSecondbot.OnEvents
{
    public class onevent
    {
        protected Cli controler = null;
        protected StatusMessageEvent LastStatus = null;
        protected int lastMinTick = 0;
        protected int lastSecTick = 0;

        protected Dictionary<UUID, KeyValuePair<long, int>> group_membership_update_q = new Dictionary<UUID, KeyValuePair<long, int>>();
        protected List<OnEvent> Events = new List<OnEvent>();
        protected Dictionary<UUID, List<UUID>> GroupMembership = new Dictionary<UUID, List<UUID>>();
        protected Dictionary<UUID, long> Lockout = new Dictionary<UUID, long>();

        protected void CleanupLockout()
        {
            List<UUID> purgeEntrys = new List<UUID>();
            lock(Lockout)
            {
                long now = helpers.UnixTimeNow();
                foreach(KeyValuePair<UUID,long> entry in Lockout)
                {
                    if(entry.Value < now)
                    {
                        purgeEntrys.Add(entry.Key);
                    }
                }
                foreach(UUID a in purgeEntrys)
                {
                    Lockout.Remove(a);
                }
            }
        }
        protected void TriggerEvent(string trigger, Dictionary<string, string> args, bool enableCronArgs = false)
        {
            if (controler.BotReady() == true)
            {
                if(controler.getBot().GetClient.Self.SittingOn == 0)
                {
                    args.Add("BotSitting", "false");
                }
                else
                {
                    args.Add("BotSitting", "true");
                }
                args.Add("BotSim", controler.getBot().GetClient.Network.CurrentSim.Name);
                int localp = controler.getBot().GetClient.Parcels.GetParcelLocalID(controler.getBot().GetClient.Network.CurrentSim, controler.getBot().GetClient.Self.SimPosition);
                if (controler.getBot().GetClient.Network.CurrentSim.Parcels.ContainsKey(localp) == true)
                {
                    Parcel P = controler.getBot().GetClient.Network.CurrentSim.Parcels[localp];
                    args.Add("BotParcel", P.Name);
                }
                else
                {
                    args.Add("BotParcel", "Unknown");
                }
                foreach (OnEvent E in Events)
                {
                    if (E.Enabled != true)
                    {
                        continue;
                    }
                    if (E.On != trigger)
                    {
                        continue;
                    }
                    if (whereChecks(E, args, enableCronArgs) == false)
                    {
                        continue;
                    }
                    actionEvents(E, args);
                }
            }
        }

        protected void actionEvents(OnEvent E, Dictionary<string, string> args)
        {
            foreach (string a in E.Actions)
            {
                string wip = UpdateBits(a, args);
                if (wip.StartsWith("lockout=") == true)
                {
                    processLockout(wip);
                    continue;
                }
                string[] bits = wip.Split("|||", StringSplitOptions.None);
                if (bits.Length == 2)
                {
                    string[] subbits = bits[1].Split("#|#");
                    if(subbits.Length == 2)
                    {
                        controler.getBot().CallAPI(bits[0], subbits[0].Split("~#~"), subbits[1]);
                    }
                    else
                    {
                        controler.getBot().CallAPI(bits[0], bits[1].Split("~#~"));
                    }
                }

            }
        }

        protected void processLockout(string lockoutString)
        {
            lock(Lockout)
            {
                string[]bits = lockoutString.Split("=", StringSplitOptions.None);
                if(bits.Length == 2)
                {
                    string[]subbits= bits[1].Split("+", StringSplitOptions.None);
                    if(subbits.Length == 2)
                    {
                        if(int.TryParse(subbits[1],out int addsecs) == true)
                        {
                            if(addsecs < 60)
                            {
                                addsecs = 60;
                            }
                            if (addsecs > 9999)
                            {
                                addsecs = 9999;
                            }
                            if (UUID.TryParse(subbits[0],out UUID addUUID) == true)
                            {
                                if(Lockout.ContainsKey(addUUID) == true)
                                {
                                    Lockout.Remove(addUUID);
                                }
                                Lockout.Add(addUUID, helpers.UnixTimeNow() + addsecs);
                            }
                        }
                    }
                }
            }
        }

        protected int dayOfWeek(DayOfWeek e)
        {
            if (e == DayOfWeek.Monday) return 1;
            else if (e == DayOfWeek.Tuesday) return 2;
            else if(e == DayOfWeek.Wednesday) return 3;
            else if(e == DayOfWeek.Thursday) return 4;
            else if(e == DayOfWeek.Friday) return 5;
            else if(e == DayOfWeek.Saturday) return 6;
            return 7;
        }

        public string WhyCronFailed = "";
        protected bool cronMagic(string checkString,string requestedResult)
        {
            string[] cronbits = checkString.Split(" ");
            if (cronbits.Length != 5)
            {
                WhyCronFailed = checkString + " is not formated correctly";
                return false;
            }
            DateTime now = DateTime.Now;
            int[] checkingCron = new int[] { now.Minute, now.Hour, now.Day, now.Month, dayOfWeek(now.DayOfWeek) };
            int[] lastCronValue = new int[] { 59, 23, DateTime.DaysInMonth(now.Year, now.Month), 12, 7 };
            int loop = 0;
            bool cronStatus = true;
            while (loop < checkingCron.Length)
            {
                int index = loop;
                loop++;
                if (cronbits[index] == "*")
                {
                    continue;
                }
                else if (cronbits[index] == "L")
                {
                    if (checkingCron[index] != lastCronValue[index])
                    {
                        WhyCronFailed = checkString + " failed on index: " + index.ToString() + " ruleset: L check: =";
                        cronStatus = false;
                        break;
                    }
                }
                else if (cronbits[index] == "E")
                {
                    if ((checkingCron[index] % 2) != 0)
                    {
                        WhyCronFailed = checkString + " failed on index: " + index.ToString() + " ruleset: E check: mod2=0";
                        cronStatus = false;
                        break;
                    }
                }
                else if (cronbits[index] == "O")
                {
                    if ((checkingCron[index] % 2) == 0)
                    {
                        WhyCronFailed = checkString + " failed on index: " + index.ToString() + " ruleset: O check: mod2!=0";
                        cronStatus = false;
                        break;
                    }
                }
                else if (cronbits[index].StartsWith("/") == true)
                {
                    if (int.TryParse(cronbits[index].Replace("/", ""), out int B) == false)
                    {
                        WhyCronFailed = checkString + " failed on index: " + index.ToString() + " convert /X to int";
                        cronStatus = false;
                        break;
                    }
                    int returnValue = (checkingCron[index] % B);
                    if (returnValue != 0)
                    {
                        WhyCronFailed = checkString + " failed on index: " + index.ToString() + " ruleset: / check: mod"+ B.ToString() +"= 0";
                        cronStatus = false;
                        break;
                    }
                }
                else
                {
                    if (int.TryParse(cronbits[index], out int B) == false)
                    {
                        WhyCronFailed = checkString + " failed on index: " + index.ToString() + " ruleset: = Failure on convert";
                        cronStatus = false;
                        break;
                    }
                    if (B != checkingCron[index])
                    {
                        WhyCronFailed = checkString + " failed on index: " + index.ToString() + " ruleset: = check: "+ B.ToString() + "="+ checkingCron[index].ToString();
                        cronStatus = false;
                        break;
                    }
                }
            }
            if (bool.TryParse(requestedResult, out bool expected) == false)
            {
                WhyCronFailed = checkString + " Unable to convert expected value "+ requestedResult;
                return false;
            }
            if (cronStatus != expected)
            {
                WhyCronFailed = checkString + " is not vaild for this time" + requestedResult;
                return false;
            }
            WhyCronFailed = checkString + " has passed ok";
            return true;
        }

        protected bool DebugWhere = true;
        protected bool whereChecks(OnEvent E, Dictionary<string, string> args, bool enableCronArgs = false)
        {
            bool WherePassed = true;

            Dictionary<string, string> updateArgs = new Dictionary<string, string>();
            string[] keys = args.Keys.ToArray();
            foreach(string K in keys)
            {
                args[K] = args[K].Trim();
            }
            List<string> WhereFilters = new List<string>()
            {
                "CRON", "IN_GROUP", "NOT_IN_GROUP",
                "IS", "NOT", "IS_EMPTY", "HAS", "MISSING",
                "IS_UUID", "CRON", "LOCKOUT"
            };
            foreach (string A in E.Where)
            {
                if((A.Contains(" {CRON} ") == true) && (enableCronArgs == false))
                {
                    WherePassed = false;
                    break;
                }
                List<string> bits = new List<string>();
                string filter = "";
                foreach(string F in WhereFilters)
                {
                    if (A.Contains(" {"+F+"} ") == true)
                    {
                        filter = F;
                        bits = A.Split(" {" + F + "} ", StringSplitOptions.None).ToList();
                        break;
                    }
                }
                if(filter == "")
                {
                    WherePassed = false;
                    break;
                }
                if(bits.Count != 2)
                {
                    WherePassed = false;
                    break;
                }
                bits[0] = bits[0].Trim();
                bits[1] = bits[1].Trim();
                bits[0] = UpdateBits(bits[0], args);
                bits[1] = UpdateBits(bits[1], args);
                if (filter == "CRON")
                {
                    if(cronMagic(bits[0], bits[1]) == false)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn(WhyCronFailed, true);
                        }
                        WherePassed = false;
                        break;
                    }
                }
                else if(filter == "LOCKOUT")
                {
                    if (bool.TryParse(bits[1], out bool status) == false)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{LOCKOUT} Unable to unpack settings for right flag", true);
                        }
                        WherePassed = false;
                        break;
                    }
                    if(UUID.TryParse(bits[0], out UUID leftUUID) == false)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{LOCKOUT} Unable to unpack settings for left UUID", true);
                        }
                        WherePassed = false;
                        break;
                    }
                    bool check = Lockout.ContainsKey(leftUUID);
                    if (check != status)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{LOCKOUT} Expected right flag does not match check", true);
                        }
                        WherePassed = false;
                        break;
                    }
                }
                else if (filter == "IS")
                {
                    if(bits[0] != bits[1])
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{IS} "+ bits[0]+" != "+bits[1]+"", true);
                        }
                        WherePassed = false;
                        break;
                    }
                }
                else if (filter == "NOT")
                {
                    if (bits[0] == bits[1])
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{NOT} " + bits[0] + " == " + bits[1] + "", true);
                        }
                        WherePassed = false;
                        break;
                    }
                }
                else if (filter == "IS_EMPTY")
                {
                    if(bool.TryParse(bits[1],out bool status) == false)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{IS_EMPTY} unable to unpack right flag", true);
                        }
                        WherePassed = false;
                        break;
                    }
                    bool check = false;
                    if(bits[0].Length == 0)
                    {
                        check = true;
                    }
                    if(check != status)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{IS_EMPTY} "+bits[0]+" "+ bits[1]+"", true);
                        }
                        WherePassed = false;
                        break;
                    }
                }
                else if (filter == "HAS")
                {
                    if (bits[0].Contains(bits[1]) == false)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{HAS} "+ bits[0]+" does not contain "+bits[1]+"", true);
                        }
                        WherePassed = false;
                        break;
                    }
                }
                else if (filter == "MISSING")
                {
                    if (bits[0].Contains(bits[1]) == true)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{MISSING} " + bits[0] + " does contain " + bits[1] + "", true);
                        }
                        WherePassed = false;
                        break;
                    }
                }
                else if (filter == "IS_UUID")
                {
                    if (bool.TryParse(bits[1], out bool resultCheck) == false)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{IS_UUID} " + bits[1] + " unable to unpack", true);
                        }
                        WherePassed = false;
                        break;
                    }
                    bool isUUID = UUID.TryParse(bits[0], out UUID _);
                    if(resultCheck != isUUID)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{IS_UUID} " + bits[0] + " failed UUID check result "+bits[1], true);
                        }
                        WherePassed = false;
                        break;
                    }
                }
                else if((filter == "IN_GROUP") || (filter == "NOT_IN_GROUP"))
                {
                    bool expectedStatus = true;
                    if (filter == "NOT_IN_GROUP")
                    {
                        expectedStatus = false;
                    }
                    if (UUID.TryParse(bits[0], out UUID avatar) == false)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{"+ filter+"} " + bits[0] + " unable to unpack avatar UUID", true);
                        }
                        WherePassed = false;
                        break;
                    }
                    if (UUID.TryParse(bits[1],out UUID group) == false)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{" + filter + "} " + bits[1] + " unable to unpack group UUID", true);
                        }
                        WherePassed = false;
                        break;
                    }
                    if(GroupMembership.ContainsKey(group) == false)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{" + filter + "} GroupMembership does not have group: "+bits[1], true);
                        }
                        WherePassed = false;
                        break;
                    }
                    bool status = GroupMembership[group].Contains(avatar);
                    if(status != expectedStatus)
                    {
                        if (DebugWhere == true)
                        {
                            LogFormater.Warn("{" + filter + "} Failed checks, true);
                        }
                        WherePassed = false;
                        break;
                    }
                }
            }

            return WherePassed;
        }

        public string UpdateBits(string bit,Dictionary<string,string> args)
        {
            foreach(KeyValuePair<string,string> A in args)
            {
                bit = bit.Replace("[" + A.Key + "]", A.Value);
            }
            return bit;
        }


        public onevent(Cli master, bool LoadFromDocker)
        {
            controler = master;
            if (LoadFromDocker == true)
            {
                loadFromDockerEnv();
            }
            else
            {
                loadFromDisk();
            }
            SetupMonitor();
            attachEvents();
#if DEBUG
            testCron();
#endif

        }

        protected void testCron()
        {
            string[] crontests = new string[]
            {
                "* * * * *",
                "* * * * "+dayOfWeek(DateTime.Now.DayOfWeek).ToString(),
                "* * * "+DateTime.Now.Month.ToString()+" *",
                "* * "+DateTime.Now.Day.ToString()+" * *",
                "* "+DateTime.Now.Hour.ToString()+" * * *",
                ""+DateTime.Now.Minute.ToString()+" * * * *",
                "* * * * /"+dayOfWeek(DateTime.Now.DayOfWeek).ToString(),
                "* * * /"+DateTime.Now.Month.ToString()+" *",
                "* * /"+DateTime.Now.Day.ToString()+" * *",
                "* /"+DateTime.Now.Hour.ToString()+" * * *",
                "/"+DateTime.Now.Minute.ToString()+" * * * *",
                "61 25 77 44 8",
                "E O E O E",
            };
            bool[] expected = new bool[]
            {
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                false,
                false,
            };
            int loop = 1;
            foreach(string a in crontests)
            {
                bool result = cronMagic(a, "true");
                if (result != expected[loop - 1])
                {
                    LogFormater.Info("failed test \"" + a + "\" " + loop.ToString() + " " + result.ToString(), true);
                }
                loop++;
            }

            
        }

        protected void SetupMonitor()
        {
            string[] skipped = new string[] { "None", "" };
            foreach (OnEvent E in Events)
            {
                if (E.Enabled == false)
                {
                    continue;
                }
                if ((E.Monitor == "None") || (E.Monitor == ""))
                {
                    continue;
                }
                UUID checking = UUID.Zero;
                if (UUID.TryParse(E.Monitor, out checking) == false)
                {
                    continue;
                }
                if (checking == UUID.Zero)
                {
                    continue;
                }
                if (group_membership_update_q.ContainsKey(checking) == true)
                {
                    continue;
                }
                //LogFormater.Info("OnEvent - attaching to group membership: " + checking.ToString(), true);
                group_membership_update_q.Add(checking, new KeyValuePair<long, int>(helpers.UnixTimeNow(), 45));
            }
        }

        protected string getEnv(string name)
        {
            return Environment.GetEnvironmentVariable(name);
        }

        protected void loadFromDockerEnv()
        {
            LogFormater.Info("Loading onEvents from ENV", true);
            int loop = 1;
            bool found = true;
            while (found == true)
            {
                string title = getEnv("event_" + loop.ToString() + "_title");
                if (helpers.notempty(title) == true)
                {
                    OnEvent config = new OnEvent();
                    bool.TryParse(getEnv("event_" + loop.ToString() + "_enabled"), out bool enabled);
                    config.Enabled = enabled;
                    if (enabled == true)
                    {
                        config.title = title;
                        config.Monitor = getEnv("event_" + loop.ToString() + "_monitor");
                        config.On = getEnv("event_" + loop.ToString() + "_on");

                        int loop2 = 1;
                        bool found2 = true;
                        List<string> wherefields = new List<string>();
                        while (found2 == true)
                        {
                            found2 = false;
                            string wherefield = getEnv("event_" + loop.ToString() + "_where_" + loop2.ToString());
                            if (helpers.notempty(wherefield) == true)
                            {
                                found2 = true;
                                wherefields.Add(wherefield);
                            }
                            loop2++;
                        }
                        config.Where = wherefields.ToArray();
                        loop2 = 1;
                        found2 = true;
                        List<string> actionfields = new List<string>();
                        while (found2 == true)
                        {
                            found2 = false;
                            string actionfield = getEnv("event_" + loop.ToString() + "_action_" + loop2.ToString());
                            if (helpers.notempty(actionfield) == true)
                            {
                                found2 = true;
                                actionfields.Add(actionfield);
                            }
                            loop2++;
                        }
                        config.Actions = actionfields.ToArray();
                        Events.Add(config);
                    }
                }
                else
                {
                    found = false;
                }
                loop++;
            }
        }


        protected void loadFromDisk()
        {
            LogFormater.Info("Loading onEvents from Disk", true);
            OnEventBlob DemoloadedEvents = new OnEventBlob();
            OnEvent demoEvent = new OnEvent();
            demoEvent.Enabled = false;
            demoEvent.Actions = new string[] { "Say|||0~#~hi mom im on TV!"};
            demoEvent.title = "demo";
            demoEvent.On = "Clock";
            demoEvent.Where = new string[] { "59 23 * * 7 {CRON} true"};
            demoEvent.Monitor = "None";
            DemoloadedEvents.listEvents = new OnEvent[] { demoEvent };

            string targetfile = "events.json";
            SimpleIO io = new SimpleIO();
            io.ChangeRoot(controler.getFolderUsed());
            if (io.Exists(targetfile) == false)
            {
                LogFormater.Status("Creating new events file", true);
                io.WriteJsonEvents(DemoloadedEvents, targetfile);
                return;
            }
            string json = io.ReadFile(targetfile);
            if (json.Length > 0)
            {
                try
                {
                    OnEventBlob loadedEvents = JsonConvert.DeserializeObject<OnEventBlob>(json);
                    foreach (OnEvent loaded in loadedEvents.listEvents)
                    {
                        Events.Add(loaded);
                    }
                }
                catch
                {
                    io.MarkOld(targetfile);
                    io.WriteJsonEvents(DemoloadedEvents, targetfile);
                }
                return;
            }
        }

        protected void attachEvents()
        {
            controler.getBot().ChangeSimEvent += changeSim;
            controler.getBot().TrackerEvent += trackerEvent;
            controler.getBot().AlertMessage += alertMessage;
            controler.getBot().StatusMessageEvent += statusMessage;
            controler.getBot().GetClient.Groups.GroupMembersReply += groupMembershipUpdate;
            controler.getBot().GetClient.Groups.GroupMemberEjected += groupMembershipUpdateEject;
        }

        protected bool requestedGroupdetails = false;

        protected void groupMembershipChecks()
        {
            long now = helpers.UnixTimeNow();
            List<UUID> repop = new List<UUID>();
            foreach (KeyValuePair<UUID, KeyValuePair<long, int>> entry in group_membership_update_q)
            {
                long dif = now - entry.Value.Key;
                if (dif >= entry.Value.Value)
                {
                    repop.Add(entry.Key);
                }
            }
            foreach (UUID entry in repop)
            {
                //LogFormater.Info("OnEvent - requesting group membership: " + entry.ToString(), true);
                group_membership_update_q[entry] = new KeyValuePair<long, int>(now + 120, 45);
                controler.getBot().GetClient.Groups.RequestGroupMembers(entry);
            }
        }
        protected void markFastCheck(UUID group, bool fastPoll)
        {
            if (group_membership_update_q.ContainsKey(group) == false)
            {
                return;
            }
            group_membership_update_q[group] = new KeyValuePair<long, int>(helpers.UnixTimeNow(), 45);
        }
        protected void updateGroupPolling(UUID group)
        {
            if(group_membership_update_q.ContainsKey(group) == false)
            {
                return;
            }
            group_membership_update_q[group] = new KeyValuePair<long, int>(helpers.UnixTimeNow(), 45);
        }

        protected void groupMembershipUpdateEject(object o, GroupOperationEventArgs e)
        {
            if (e.Success == true)
            {
                markFastCheck(e.GroupID,true);
            }
        }

        protected void groupMembershipUpdate(object o, GroupMembersReplyEventArgs e)
        {
            if (group_membership_update_q.ContainsKey(e.GroupID) == true)
            {
                //LogFormater.Info("OnEvent - updating group membership: " + e.GroupID.ToString(), true);
                updateGroupPolling(e.GroupID);
                bool enableChanges = GroupMembership.ContainsKey(e.GroupID);
                List<UUID> members = new List<UUID>();
                foreach (KeyValuePair<UUID, GroupMember> entry in e.Members)
                {
                    members.Add(entry.Key);
                }
                if (enableChanges == false)
                {
                    bool skip = false;
                    if(members.Count == 1)
                    {
                        skip = members.Contains(UUID.Zero);
                    }
                    if (skip == false)
                    {
                        GroupMembership.Add(e.GroupID, members);
                    }
                }
                else
                {
                    List<UUID> newEntrys = new List<UUID>();
                    List<UUID> missingEntrys = GroupMembership[e.GroupID];
                    foreach (UUID updated in members)
                    {
                        if (missingEntrys.Contains(updated) == false)
                        {
                            newEntrys.Add(updated);
                            
                        }
                        else
                        {
                            missingEntrys.Remove(updated);
                        }
                    }
                    GroupMembership[e.GroupID] = members;
                    int lookups = 0;
                    foreach (UUID newMember in newEntrys)
                    {
                        string name = controler.getBot().FindAvatarKey2Name(newMember);
                        if (name == "lookup")
                        {
                            lookups++;
                        }
                        //LogFormater.Info("OnEvent - new group member: " + name, true);
                    }
                    foreach (UUID missingMember in missingEntrys)
                    {
                        string name = controler.getBot().FindAvatarKey2Name(missingMember);
                        if (name == "lookup")
                        {
                            lookups++;
                        }
                        //LogFormater.Info("OnEvent - leaving group member: " + name, true);
                    }
                    if (lookups > 0)
                    {
                        Thread.Sleep(500);
                    }
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args.Add("avataruuid", "notset");
                    args.Add("avatarname", "notset");
                    args.Add("groupuuid", e.GroupID.ToString());

                    foreach (UUID newMember in newEntrys)
                    {
                        string name = controler.getBot().FindAvatarKey2Name(newMember);
                        args["avataruuid"] = newMember.ToString();
                        args["avatarname"] = name;
                        TriggerEvent("GroupMemberJoins", args, false);
                    }

                    foreach (UUID missingMember in missingEntrys)
                    {
                        string name = controler.getBot().FindAvatarKey2Name(missingMember);
                        args["avataruuid"] = missingMember.ToString();
                        args["avatarname"] = name;
                        TriggerEvent("GroupMemberLeaves", args, false);
                    }
                }
            }
        }

        protected void ClockEvent()
        {
            TriggerEvent("Clock", new Dictionary<string, string>(), true);
        }

        protected void statusMessage(object o, StatusMessageEvent e)
        {
            if (controler.BotReady() == true)
            {
                bool send = false;
                DateTime Dt = DateTime.Now;
                if (Dt.Minute != lastMinTick)
                {
                    lastMinTick = Dt.Minute;
                    ClockEvent();
                    CleanupLockout();
                    if(requestedGroupdetails == false)
                    {
                        requestedGroupdetails = true;
                        foreach(UUID groupuuid in group_membership_update_q.Keys)
                        {
                            controler.getBot().GetClient.Groups.RequestGroupMembers(groupuuid);
                        }
                    }
                }
                if (Dt.Second != lastSecTick)
                {
                    lastSecTick = Dt.Second;
                     groupMembershipChecks();
                }
                if (LastStatus == null)
                {
                    send = true;
                }
                else if (e.connected != LastStatus.connected)
                {
                    send = true;
                }
                else if (e.sim != LastStatus.sim)
                {
                    send = true;
                }
                if (send == true)
                {
                    LastStatus = e;
                    Dictionary<string, string> args = new Dictionary<string, string>();
                    args.Add("connected", e.connected.ToString());
                    args.Add("sim", e.sim);
                    TriggerEvent("StatusMessage", args);
                }
            }
        }

        protected void alertMessage(object o, AlertMessageEventArgs e)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("message ", e.Message);
            TriggerEvent("SimAlertMessage", args);
        }

        protected void changeSim(object o, SimChangedEventArgs e)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("oldSim", e.PreviousSimulator.Name);
            TriggerEvent("ChangeSim", args);
        }

        protected void trackerEvent(object o, TrackerEventArgs e)
        {
            Dictionary<string, string> args = new Dictionary<string, string>();
            args.Add("avataruuid", e.avatarUUID.ToString());
            args.Add("avatarname", e.avatarName);
            if(e.Leaving == true)
            {
                TriggerEvent("GuestLeavesArea", args);
            }
            else
            {
                args.Add("AvatarSim", e.simName);
                args.Add("AvatarParcel", e.parcelName);
                TriggerEvent("GuestEntersArea", args);
            }
        }

    }


}
