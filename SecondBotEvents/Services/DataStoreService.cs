﻿using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using SecondBotEvents.Commands;
using OpenMetaverse.ImportExport.Collada14;
using System.Threading.Tasks;

namespace SecondBotEvents.Services
{
    public class DataStoreService : BotServices
    {
        protected DataStoreConfig myConfig;
        protected bool botConnected = false;
        public DataStoreService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new DataStoreConfig(master.fromEnv,master.fromFolder);
        }

        protected Dictionary<string, UUID> avatarsName2Key = new();
        protected Dictionary<UUID, long> avatarLastUsed =  new();

        protected Dictionary<UUID, string> groupsKey2Name = new();

        protected Dictionary<UUID, List<UUID>> groupMembers = new();
        protected Dictionary<UUID, Dictionary<UUID, GroupRole>> groupRoles = new();

        protected List<string> localChatHistory = new();

        protected Dictionary<UUID, List<string>> chatWindows = new(); // sess, list chat
        protected Dictionary<UUID, bool> chatWindowsUnread = new(); // sess, bool
        protected Dictionary<UUID, bool> chatWindowsIsGroup = new(); // sess, bool
        protected Dictionary<UUID, UUID> chatWindowsOwner = new(); // sess, owner id

        protected Dictionary<string, string> KeyValueStore = new();
        protected Dictionary<string, long> KeyValueStoreLastUsed = new();

        protected List<CommandHistory> commandHistories = new();

        protected long lastCleanupKeyValueStore = 0;
        protected long lastCleanupAvatarStore = 0;

        public bool GetIsGroup(UUID MaybeGroupUUID)
        {
            return groupsKey2Name.ContainsKey(MaybeGroupUUID);
        }

        public string GetGroupName(UUID group)
        {
            if (groupsKey2Name.ContainsKey(group) == true)
            {
                return groupsKey2Name[group];
            }
            string reply = "lookup";
            AutoResetEvent fetchEvent = new(false);
            void callback(object sender, CurrentGroupsEventArgs e)
            {
                foreach(KeyValuePair<UUID,Group> entry in e.Groups)
                {
                    if(entry.Value.ID == group)
                    {
                        reply = entry.Value.Name;
                        fetchEvent.Set();
                        break;
                    }
                }
            }
            GetClient().Groups.CurrentGroups += callback;
            GetClient().Groups.RequestCurrentGroups();

            fetchEvent.WaitOne(1000, false);
            GetClient().Groups.CurrentGroups -= callback;
            return reply;

        }
        public override string Status()
        {
            if (myConfig == null)
            {
                return "DataStore Service [Config broken]";
            }
            else if (myConfig.GetHideStatusOutput() == true)
            {
                return "hidden";
            }
            long unixtime = SecondbotHelpers.UnixTimeNow();
            if (myConfig.GetAutoCleanKeyValueStore() == true)
            {
                if (lastCleanupKeyValueStore == 0)
                {
                    lastCleanupKeyValueStore = SecondbotHelpers.UnixTimeNow();
                }
                long dif = unixtime - lastCleanupKeyValueStore;
                if (dif > 125)
                {
                    lastCleanupKeyValueStore = unixtime;
                    List<string> clearentrys = new();
                    foreach (KeyValuePair<string, long> entry in KeyValueStoreLastUsed)
                    {
                        dif = unixtime - entry.Value;
                        if (dif < (myConfig.GetCleanKeyValueStoreAfterMins()*60))
                        {
                            continue;
                        }
                        clearentrys.Add(entry.Key);
                    }
                    lock (KeyValueStore)
                    {
                        foreach (string A in clearentrys)
                        {
                            KeyValueStore.Remove(A);
                            KeyValueStoreLastUsed.Remove(A);
                        }
                    }
                }
            }
            if(myConfig.GetAutoCleanAvatars() == true)
            {
                if(lastCleanupAvatarStore == 0)
                {
                    lastCleanupAvatarStore = unixtime;
                }
                long dif = unixtime - lastCleanupAvatarStore;
                if (dif > 75)
                {
                    lastCleanupAvatarStore = unixtime;
                    Dictionary<UUID, string> clearLinks = new();
                    foreach (KeyValuePair<string, UUID> entry in avatarsName2Key)
                    {
                        dif = unixtime - avatarLastUsed[entry.Value];
                        if (dif < (myConfig.GetAvatarsCleanAfterMins() * 60))
                        {
                            continue;
                        }
                        clearLinks.Add(entry.Value, entry.Key);
                    }
                    lock (avatarLastUsed) lock(avatarsName2Key)
                    {
                        foreach (KeyValuePair<UUID, string> A in clearLinks)
                        {
                            avatarLastUsed.Remove(A.Key);
                            avatarsName2Key.Remove(A.Value);
                        }
                    }
                }
            }
            int sum = commandHistories.Count + KeyValueStoreLastUsed.Count + KeyValueStore.Count +
                chatWindowsOwner.Count + chatWindowsIsGroup.Count + chatWindowsUnread.Count +
                chatWindows.Count + localChatHistory.Count + groupRoles.Count +
                groupMembers.Count + groupsKey2Name.Count + avatarsName2Key.Count;
            return sum.ToString();
        }

        public void AddCommandToHistory(bool status, string command, string[] args)
        {
            lock (commandHistories)
            {
                CommandHistory A = new(status, command, args, SecondbotHelpers.UnixTimeNow());
                commandHistories.Add(A);
                int c = commandHistories.Count;
                while (c > myConfig.GetCommandHistoryLimit())
                {
                    commandHistories.RemoveAt(0);
                    c--;
                }
            }
        }

        public List<CommandHistory> GetCommandHistory()
        {
            return commandHistories;
        }

        public string GetKeyValue(string key)
        {
            lock (KeyValueStore)
            {
                if (KeyValueStore.ContainsKey(key) == false)
                {
                    return "";
                }
                KeyValueStoreLastUsed[key] = SecondbotHelpers.UnixTimeNow();
                return KeyValueStore[key];
            }
        }

        public void SetKeyValue(string key, string value)
        {
            lock (KeyValueStore)
            {
                if (KeyValueStore.ContainsKey(key) == false)
                {
                    KeyValueStore.Add(key, "");
                    KeyValueStoreLastUsed.Add(key, 0);
                }
                KeyValueStore[key] = value;
                KeyValueStoreLastUsed[key] = SecondbotHelpers.UnixTimeNow();
            }
        }

        public void ClearKeyValue(string key)
        {
            lock (KeyValueStore)
            {
                if (KeyValueStore.ContainsKey(key) == false)
                {
                    return;
                }
                KeyValueStore.Remove(key);
                KeyValueStoreLastUsed.Remove(key);
            }
        }

        public void AppendKeyValue(string key, string value)
        {
            if (KeyValueStore.ContainsKey(key) == false)
            {
                SetKeyValue(key, value);
                return;
            }
            KeyValueStoreLastUsed[key] = SecondbotHelpers.UnixTimeNow();
            KeyValueStore[key] = KeyValueStore[key] + value;
        }

        public Dictionary<UUID, string> GetGroups()
        {
            return groupsKey2Name;
        }

        public List<UUID> GetAllGroupsChatWithUnread()
        {
            List<UUID> reply = new();
            foreach(KeyValuePair<UUID, bool> windowState in chatWindowsUnread)
            {
                if(windowState.Value == false)
                {
                    continue;
                }    
                if(chatWindowsIsGroup[windowState.Key] == false)
                {
                    continue;
                }
                reply.Add(windowState.Key);
            }
            return reply;
        }

        public bool GetGroupChatHasUnread(UUID group)
        {
            if(chatWindowsUnread.ContainsKey(group) == false)
            {
                return false;
            }
            return chatWindowsUnread[group];
        }

        public bool GetGroupChatHasAnyUnread()
        {
            bool reply = false;
            foreach(KeyValuePair<UUID,bool> check in chatWindowsIsGroup)
            {
                if(check.Value == false)
                {
                    continue;
                }
                if(chatWindowsUnread[check.Key] == true)
                {
                    reply = true;
                    break;
                }
            }

            return reply;
        }

        public List<string> GetGroupChatHistory(UUID group)
        {
            if(chatWindows.ContainsKey(group) == false)
            {
                return new List<string>();
            }
            chatWindowsUnread[group] = false;
            return chatWindows[group];
        }

        public void ClearGroupChat()
        {
            lock (chatWindows)
            {
                foreach (UUID group in groupsKey2Name.Keys)
                {
                    if (chatWindowsUnread.ContainsKey(group) == false)
                    {
                        continue;
                    }
                    chatWindowsUnread.Remove(group);
                    chatWindows.Remove(group);
                    chatWindowsIsGroup.Remove(group);
                    chatWindowsOwner.Remove(group);
                }
            }
        }

        public Dictionary<UUID, string> GetGroupRoles(UUID group)
        {
            Dictionary<UUID, string> reply = new();
            if (groupRoles.ContainsKey(group) == false)
            {
                
                AutoResetEvent fetchEvent = new(false);
                void callback(object sender, GroupRolesDataReplyEventArgs e)
                {
                    if (e.GroupID == group)
                    {
                        foreach (KeyValuePair<UUID, GroupRole> role in e.Roles)
                        {
                            reply[role.Value.ID] = role.Value.Name;
                        }
                        fetchEvent.Set();
                    }
                }
                GetClient().Groups.GroupRoleDataReply += callback;
                GetClient().Groups.RequestGroupRoles(group);

                fetchEvent.WaitOne(1000, false);
                GetClient().Groups.GroupRoleDataReply -= callback;
                return reply;
            }
            foreach (KeyValuePair<UUID, GroupRole> role in groupRoles[group])
            {
                reply[role.Value.ID] = role.Value.Name;
            }
            return reply;
        }

        public bool IsGroupMember(UUID group, UUID avatar)
        {
            bool reply = true;
            lock (groupMembers)
            {
                if (groupMembers.ContainsKey(group) == false)
                {
                    groupMembers.Add(group, new List<UUID>());
                }
                if (groupMembers[group].Contains(avatar) == false)
                {
                    reply = false;
                    AutoResetEvent fetchEvent = new(false);
                    void callback(object sender, GroupMembersReplyEventArgs e)
                    {
                        if (e.GroupID == group)
                        {
                            foreach (KeyValuePair<UUID, GroupMember> a in e.Members)
                            {
                                if (a.Value.ID == avatar)
                                {
                                    reply = true;
                                    fetchEvent.Set();
                                    break;
                                }
                            }

                        }
                    }
                    GetClient().Groups.GroupMembersReply += callback;
                    GetClient().Groups.RequestGroupMembers(group);

                    fetchEvent.WaitOne(1000, false);
                    GetClient().Groups.GroupMembersReply -= callback;
                }
            }
            return reply;
        }

        public List<UUID> GetGroupMembers(UUID Group)
        {
            lock (groupMembers)
            {
                if (groupMembers.ContainsKey(Group) == false)
                {
                    groupMembers.Add(Group, new List<UUID>());
                }
            }
            return groupMembers[Group];
        }

        public void BotRecordReplyIM(UUID sessionid,string message)
        {
            RecordChat(sessionid, "Bot", message);
        }
        public void BotRecordLocalchatReply(string message)
        {
            lock (localChatHistory)
            {
                localChatHistory.Add("{BOT} " + GetClient().Self.Name + ": " + message);
                if (localChatHistory.Count <= myConfig.GetLocalChatHistoryLimit())
                {
                    return;
                }
                localChatHistory.RemoveAt(0);
            }
        }

        protected void RecordChat(UUID sessionID, string fromname, string message)
        {
            lock (chatWindows)
            {
                if (chatWindows.ContainsKey(sessionID) == false)
                {
                    return; // chat window not open unable to record message
                }
                chatWindows[sessionID].Add(fromname + ": " + message);
                chatWindowsUnread[sessionID] = true;
                int purgeLen = myConfig.GetImChatHistoryLimit();
                if (chatWindowsIsGroup[sessionID] == true)
                {
                    purgeLen = myConfig.GetGroupChatHistoryLimitPerGroup();
                }
                int counter = chatWindows[sessionID].Count;
                while (counter > purgeLen)
                {
                    chatWindows[sessionID].RemoveAt(0);
                    counter--;
                }
            }
        }

        public void CloseChat(UUID owner)
        {
            lock (chatWindows)
            {
                UUID sessionID = UUID.Zero;
                if (chatWindowsOwner.ContainsValue(owner) == false)
                {
                    return;
                }
                foreach (KeyValuePair<UUID, UUID> ownermap in chatWindowsOwner)
                {
                    if (ownermap.Value != owner)
                    {
                        continue;
                    }
                    sessionID = ownermap.Key;
                }
                if (sessionID == UUID.Zero)
                {
                    return;
                }
                chatWindowsOwner.Remove(sessionID);
                chatWindowsIsGroup.Remove(sessionID);
                chatWindowsUnread.Remove(sessionID);
                chatWindows.Remove(sessionID);
            }

        }

        public void OpenChatWindow(bool group, UUID sessionID, UUID ownerID)
        {
            lock (chatWindows)
            {
                if (chatWindows.ContainsKey(sessionID) == true)
                {
                    return; // window already open
                }
                if (chatWindowsOwner.ContainsValue(ownerID) == true)
                {
                    // new session find the old one and update to this session
                    Dictionary<UUID, UUID> oldOwners = chatWindowsOwner;
                    foreach (KeyValuePair<UUID, UUID> kvp in oldOwners)
                    {
                        if (kvp.Value == ownerID)
                        {
                            chatWindows.Add(sessionID, chatWindows[kvp.Key]);
                            chatWindowsUnread.Add(sessionID, chatWindowsUnread[kvp.Key]);
                            chatWindowsIsGroup.Add(sessionID, chatWindowsIsGroup[kvp.Key]);
                            chatWindowsOwner.Add(sessionID, ownerID);
                            chatWindows.Remove(kvp.Key);
                            chatWindowsUnread.Remove(kvp.Key);
                            chatWindowsIsGroup.Remove(kvp.Key);
                            chatWindowsOwner.Remove(kvp.Key);
                            break;
                        }
                    }
                    return; // chat recovered
                }
                chatWindows.Add(sessionID, new List<string>());
                chatWindowsUnread.Add(sessionID, false);
                chatWindowsIsGroup.Add(sessionID, group);
                chatWindowsOwner.Add(sessionID, ownerID);
                // new window now open
            }
        }


        protected Dictionary<string, KeyValuePair<string, long>> notecardDataStore = new();

        public List<string> GetLocalChat()
        {
            return localChatHistory;
        }

        public bool KnownGroup(UUID groupId, string name)
        {
            if(groupId != UUID.Zero)
            {
                return groupsKey2Name.ContainsKey(groupId);
            }
            return groupsKey2Name.ContainsValue(name);
        }

        /*
         *  checks if the avatar is known to the system
         *  if not the bot will attempt to
         *  lookup the avatar
         */
        public bool KnownAvatar(string name)
        {
            return KnownAvatar(name, UUID.Zero);
        }


        protected int ObjectsAvatarsHash = 0;
        /*
         *  checks if the avatar is known to the system
         *  if not the bot will attempt to
         *  lookup the avatar
         */

        public bool KnownAvatar(string name, UUID avatarId)
        {
            int currentHash = GetClient().Network.CurrentSim.ObjectsAvatars.GetHashCode();
            if (currentHash != ObjectsAvatarsHash)
            {
                ObjectsAvatarsHash = currentHash;
                List<Avatar> avs = GetClient().Network.CurrentSim.ObjectsAvatars.Copy().Values.ToList();
                foreach (Avatar A in avs)
                {
                    AddAvatar(A.ID, A.Name);
                }
            }
            if (avatarId != UUID.Zero)
            {
                if(avatarsName2Key.ContainsValue(avatarId) == false)
                {
                    bool hasReply = false;
                    AutoResetEvent fetchEvent = new(false);
                    void callback(object sender, UUIDNameReplyEventArgs e)
                    {
                        foreach (KeyValuePair<UUID, string> av in e.Names)
                        {
                            if (av.Key == avatarId)
                            {
                                hasReply = true;
                                fetchEvent.Set();
                            }
                        }
                    }
                    GetClient().Avatars.UUIDNameReply += callback;
                    GetClient().Avatars.RequestAvatarName(avatarId);

                    fetchEvent.WaitOne(1000, false);
                    GetClient().Avatars.UUIDNameReply -= callback;
                    return hasReply;
                }
                return true;
            }
            string[] bits = name.Split(' ');
            if (bits.Length == 1)
            {
                name += " Resident";
            }
            if (avatarsName2Key.ContainsKey(name) == false)
            {
                UUID wantedReplyID = UUID.SecureRandom();
                
                bool hasReply = false;
                AutoResetEvent fetchEvent = new(false);
                void callback(object sender, AvatarPickerReplyEventArgs e)
                {
                    if (e.QueryID == wantedReplyID)
                    {
                        foreach (KeyValuePair<UUID, string> av in e.Avatars)
                        {
                            if (av.Value == name)
                            {
                                hasReply = true;
                                fetchEvent.Set();
                            }
                        }
                    }
                }
                GetClient().Avatars.AvatarPickerReply += callback;
                GetClient().Avatars.RequestAvatarNameSearch(name, wantedReplyID);

                fetchEvent.WaitOne(1000, false);
                GetClient().Avatars.AvatarPickerReply -= callback;
                return hasReply;
            }
            return true;
        }

        public void GetAvatarNames(List<UUID> avatars)
        {
            List<UUID> lookup = new();
            foreach (UUID id in avatars)
            {
                if (avatarsName2Key.ContainsValue(id) == false)
                {
                    lookup.Add(id);
                }
            }
            if(lookup.Count == 0)
            {
                return;
            }
            GetClient().Avatars.RequestAvatarNames(lookup);
        }

        public string GetAvatarName(UUID avatarId)
        {
            string reply = "lookup";
            lock (avatarLastUsed)
            {
                if (KnownAvatar(null, avatarId) == false)
                {
                    return reply;
                }
                foreach (KeyValuePair<string, UUID> av in avatarsName2Key)
                {
                    if (av.Value == avatarId)
                    {
                        avatarLastUsed[av.Value] = SecondbotHelpers.UnixTimeNow();
                        reply = av.Key;
                        break;
                    }
                }
            }
            return reply;
        }

        public string GetAvatarUUID(string name)
        {

            if (KnownAvatar(name) == false)
            {
                return "lookup";
            }
            lock (avatarLastUsed)
            {
                avatarLastUsed[avatarsName2Key[name]] = SecondbotHelpers.UnixTimeNow();
            }
            return avatarsName2Key[name].ToString();
        }

        public Dictionary<UUID,string> GetAvatarImWindows()
        {
            Dictionary<UUID, string> windows = new();
            foreach (KeyValuePair<UUID,bool> links in chatWindowsIsGroup)
            {
                if(links.Value == true)
                {
                    continue;
                }
                if (chatWindowsIsGroup[links.Key] == true)
                {
                    continue;
                }
                windows[chatWindowsOwner[links.Key]] = GetAvatarName(chatWindowsOwner[links.Key]);
            }
            return windows;
        }

        public List<string> GetAvatarImWindow(UUID avatarId)
        {
            if (chatWindowsOwner.ContainsValue(avatarId) == false)
            {
                return new List<string>();
            }
            UUID sessionID = UUID.Zero;
            foreach (KeyValuePair<UUID, UUID> links in chatWindowsOwner)
            {
                if (links.Value == avatarId)
                {
                    sessionID = links.Key;
                    break;
                }
            }
            if(chatWindowsIsGroup[sessionID] == true)
            {
                return new List<string>();
            }
            chatWindowsUnread[sessionID] = false;
            return chatWindows[sessionID];

        }

        public bool GetAvatarImWindowsUnreadAny()
        {
            bool replyState = false;
            foreach (KeyValuePair<UUID, bool> links in chatWindowsIsGroup)
            {
                if (links.Value == true)
                {
                    continue;
                }
                if (chatWindowsIsGroup[links.Key] == true)
                {
                    continue;
                }
                if (chatWindowsUnread[links.Key] == false)
                {
                    continue;
                }
                replyState = true;
                break;
            }
            return replyState;
        }

        public List<UUID> GetAvatarImWindowsUnread()
        {
            List<UUID> windows = new();
            foreach (KeyValuePair<UUID, bool> links in chatWindowsIsGroup)
            {
                if (links.Value == true)
                {
                    continue;
                }
                if (chatWindowsIsGroup[links.Key] == true)
                {
                    continue;
                }
                if (chatWindowsUnread[links.Key] == false)
                {
                    continue;
                }
                windows.Add(chatWindowsOwner[links.Key]);
            }
            return windows;
        }



        protected void Reset()
        {
            if (estateBanlist != null)
            {
                lock (avatarsName2Key) lock (groupMembers) lock (localChatHistory) lock (chatWindows) lock (estateBanlist)
                {
                    ClearMem();
                }
                return;
            }
            ClearMem();
        }

        protected void ClearMem()
        {
            lastCleanupKeyValueStore = 0;

            avatarsName2Key = new Dictionary<string, UUID>();

            groupsKey2Name = new Dictionary<UUID, string>();

            groupMembers = new Dictionary<UUID, List<UUID>>();
            groupRoles = new Dictionary<UUID, Dictionary<UUID, GroupRole>>();

            localChatHistory = new List<string>();

            notecardDataStore = new Dictionary<string, KeyValuePair<string, long>>();

            chatWindows = new Dictionary<UUID, List<string>>(); // sess, list chat
            chatWindowsUnread = new Dictionary<UUID, bool>(); // sess, bool
            chatWindowsIsGroup = new Dictionary<UUID, bool>(); // sess, bool
            chatWindowsOwner = new Dictionary<UUID, UUID>(); // sess, owner id

            estateBanlist = new List<UUID>();
        }


        protected void BotClientRestart(object o, BotClientNotice e)
        {
            botConnected = false;
            LogFormater.Info("DataStore Service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            botConnected = false;
            GetClient().Network.SimConnected += BotLoggedIn;
            LogFormater.Info("DataStore Service [Standby]");
        }

        protected void GroupRoles(object o, GroupRolesDataReplyEventArgs e)
        {
            lock (groupRoles)
            {
                if (groupRoles.ContainsKey(e.GroupID) == false)
                {
                    groupRoles.Add(e.GroupID, new Dictionary<UUID, GroupRole>());
                }
                groupRoles[e.GroupID] = e.Roles;
            }
        }

        protected void GroupMembers(object o, GroupMembersReplyEventArgs e)
        {
            lock (groupMembers)
            {
                if (groupMembers.ContainsKey(e.GroupID) == true)
                {
                    groupMembers.Remove(e.GroupID);
                }
                groupMembers.Add(e.GroupID, new List<UUID>());
                foreach (KeyValuePair<UUID, GroupMember> a in e.Members)
                {
                    groupMembers[e.GroupID].Add(a.Value.ID);
                }
            }
        }

        public List<UUID> GetEstateBans()
        {
            if (estateBanlist == null)
            {
                List<UUID> reply = new();
                AutoResetEvent fetchEvent = new(false);
                void callback(object sender, EstateBansReplyEventArgs e)
                {
                    reply = e.Banned;
                    fetchEvent.Set();
                }
                GetClient().Estate.EstateBansReply += callback;
                GetClient().Estate.RequestInfo();

                fetchEvent.WaitOne(1000, false);
                GetClient().Estate.EstateBansReply -= callback;
                return reply;
            }
            return estateBanlist;
        }

        List<UUID> estateBanlist = null;

        protected void EstateBans(object o, EstateBansReplyEventArgs e)
        {
            lock (estateBanlist)
            {
                estateBanlist = e.Banned;
            }
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            botConnected = true;
            LogFormater.Info("DataStore Service [Attaching]");
            GetAvatarName(GetClient().Self.AgentID);
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                attachEventsAfterDelay();
            });
            
        }

        protected void attachEventsAfterDelay()
        {
            GetClient().Groups.CurrentGroups += GroupCurrent;
            GetClient().Groups.GroupJoinedReply += GroupJoin;
            GetClient().Groups.GroupLeaveReply += GroupLeave;
            GetClient().Avatars.UUIDNameReply += AvatarUUIDName;
            GetClient().Friends.FriendNames += AvatarFriendNames;
            GetClient().Self.IM += ImEvent;
            GetClient().Self.ChatFromSimulator += ChatFromSim;
            GetClient().Avatars.AvatarPickerReply += AvatarUUIDNamePicker;
            GetClient().Groups.GroupMembersReply += GroupMembers;
            GetClient().Groups.GroupRoleDataReply += GroupRoles;
            GetClient().Estate.EstateBansReply += EstateBans;
            GetClient().Groups.RequestCurrentGroups();
        }
        readonly string[] hard_blocked_agents = new[] { "secondlife", "second life" };
        protected void ChatFromSim(object o, ChatEventArgs e)
        {
            switch(e.Type)
            {
                case ChatType.Whisper:
                case ChatType.Normal:
                case ChatType.Shout:
                    {
                        if (hard_blocked_agents.Contains(e.FromName.ToLowerInvariant()) == true)
                        {
                            break;
                        }
                        if (e.SourceID == GetClient().Self.AgentID)
                        {
                            break;
                        }
                            string source = "{Av}";
                        if (e.SourceType == ChatSourceType.Object)
                        {
                            source = "{Obj}";
                        }
                        else if (e.SourceType == ChatSourceType.System)
                        {
                            source = "{Sys}";
                        }
                        lock (localChatHistory)
                        {
                            localChatHistory.Add(source+ " "+e.FromName + ": " + e.Message);
                            if (localChatHistory.Count <= myConfig.GetLocalChatHistoryLimit())
                            {
                                break;
                            }
                            localChatHistory.RemoveAt(0);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        protected void GroupCurrent(object o, CurrentGroupsEventArgs e)
        {
            lock (groupsKey2Name)
            {
                List<UUID> missingGroups = groupsKey2Name.Keys.ToList();
                foreach (Group G in e.Groups.Values)
                {
                    missingGroups.Remove(G.ID);
                    if (groupsKey2Name.ContainsKey(G.ID) == true)
                    {
                        continue;
                    }
                    groupsKey2Name.Add(G.ID, G.Name);
                    GetClient().Self.RequestJoinGroupChat(G.ID);
                    if (myConfig.GetPrefetchGroupMembers() == true)
                    {
                        GetClient().Groups.RequestGroupMembers(G.ID);
                    }
                    if (myConfig.GetPrefetchGroupRoles() == true)
                    {
                        GetClient().Groups.RequestGroupRoles(G.ID);
                    }
                }

                foreach (UUID G in missingGroups)
                {
                    if (groupsKey2Name.ContainsKey(G) == false)
                    {
                        continue;
                    }
                    groupsKey2Name.Remove(G);
                    CloseChat(G);
                }
            }
        }

        protected void GroupJoin(object o, GroupOperationEventArgs e)
        {
            GetClient().Groups.RequestCurrentGroups();
        }

        protected void GroupLeave(object o, GroupOperationEventArgs e)
        {
            GetClient().Groups.RequestCurrentGroups();
        }

        protected void AddAvatar(UUID id, string name)
        {
            lock (avatarsName2Key) lock (avatarLastUsed)
            {
                if (avatarsName2Key.ContainsValue(id) == true)
                {
                    avatarLastUsed[id] = SecondbotHelpers.UnixTimeNow();
                    return;
                }
                avatarsName2Key.Add(name, id);
                avatarLastUsed.Add(id, SecondbotHelpers.UnixTimeNow());
            }
        }

        protected void AvatarUUIDName(object o, UUIDNameReplyEventArgs e)
        {
            foreach(KeyValuePair<UUID,string> pair in e.Names)
            {
                AddAvatar(pair.Key, pair.Value);
            }
        }

        protected void AvatarUUIDNamePicker(object o, AvatarPickerReplyEventArgs e)
        {
            foreach(KeyValuePair<UUID,string> Av in e.Avatars)
            {
                AddAvatar(Av.Key, Av.Value);
            }
        }

        protected void AvatarFriendNames(object o, FriendNamesEventArgs e)
        {
            foreach (KeyValuePair<UUID, string> pair in e.Names)
            {
                AddAvatar(pair.Key, pair.Value);
            }
        }

        protected void ImEvent(object o, InstantMessageEventArgs e)
        {
            switch(e.IM.Dialog)
            {
                case InstantMessageDialog.GroupNotice:
                    {
                        if(groupsKey2Name.ContainsKey(e.IM.IMSessionID) == false)
                        {
                            break;
                        }
                        OpenChatWindow(true, e.IM.IMSessionID, e.IM.IMSessionID);
                        RecordChat(e.IM.IMSessionID, "**Notice**", e.IM.Message);
                        break;
                    }
                case InstantMessageDialog.SessionSend:
                case InstantMessageDialog.MessageFromAgent:
                    {
                        if (groupsKey2Name.ContainsKey(e.IM.IMSessionID) == true)
                        {
                            // group IM
                            OpenChatWindow(true, e.IM.IMSessionID, e.IM.IMSessionID);
                            RecordChat(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.Message);
                            break;
                        }
                        AddAvatar(e.IM.FromAgentID, e.IM.FromAgentName);
                        OpenChatWindow(false, e.IM.IMSessionID, e.IM.FromAgentID);
                        if (e.IM.FromAgentID != GetClient().Self.AgentID)
                        {
                            RecordChat(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.Message);
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        public override void Start()
        {
            Stop();
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
            LogFormater.Info("DataStore Service [Starting]");
        }

        public override void Stop()
        {
            if(running == true)
            {
                LogFormater.Info("DataStore Service [Stopping]");
            }
            running = false;
            Reset();
            master.BotClientNoticeEvent -= BotClientRestart;
            if (master.BotClient != null)
            {
                if (GetClient() != null)
                {
                    GetClient().Groups.CurrentGroups -= GroupCurrent;
                    GetClient().Groups.GroupJoinedReply -= GroupJoin;
                    GetClient().Groups.GroupLeaveReply -= GroupLeave;
                    GetClient().Avatars.UUIDNameReply -= AvatarUUIDName;
                    GetClient().Friends.FriendNames -= AvatarFriendNames;
                    GetClient().Self.IM -= ImEvent;
                    GetClient().Self.ChatFromSimulator -= ChatFromSim;
                    GetClient().Avatars.AvatarPickerReply -= AvatarUUIDNamePicker;
                    GetClient().Groups.GroupMembersReply -= GroupMembers;
                    GetClient().Groups.GroupRoleDataReply -= GroupRoles;
                    GetClient().Estate.EstateBansReply -= EstateBans;
                }
            }
            
        }
    }

    public class CommandHistory
    {
        public string Command = "";
        public string[] Args = Array.Empty<string>();
        public long Unixtime = 0;
        public bool status = false;

        public CommandHistory(bool setStatus, string setCommand, string[] setArgs, long setUnixtime)
        {
            status = setStatus;
            Command = setCommand;
            Args = setArgs;
            Unixtime = setUnixtime;
        }
    }
}



