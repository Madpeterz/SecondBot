using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

namespace SecondBotEvents.Services
{
    public class DataStoreService : Services
    {
        protected DataStoreConfig myConfig;
        protected bool botConnected = false;
        public DataStoreService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new DataStoreConfig(master.fromEnv,master.fromFolder);
        }

        public override string Status()
        {
            if(myConfig == null)
            {
                return "DataStore Service [Config status broken]";
            }
            return "DataStore Service [Waiting for bot]";
        }

        protected Dictionary<string, UUID> avatarsName2Key = new Dictionary<string, UUID>();

        protected Dictionary<UUID, string> groupsKey2Name = new Dictionary<UUID, string>();

        protected Dictionary<UUID, List<UUID>> groupMembers = new Dictionary<UUID, List<UUID>>();
        protected Dictionary<UUID, Dictionary<UUID, GroupRole>> groupRoles = new Dictionary<UUID, Dictionary<UUID, GroupRole>>();

        protected List<string> localChatHistory = new List<string>();

        protected Dictionary<UUID, List<string>> chatWindows = new Dictionary<UUID, List<string>>(); // sess, list chat
        protected Dictionary<UUID, bool> chatWindowsUnread = new Dictionary<UUID, bool>(); // sess, bool
        protected Dictionary<UUID, bool> chatWindowsIsGroup = new Dictionary<UUID, bool>(); // sess, bool
        protected Dictionary<UUID, UUID> chatWindowsOwner = new Dictionary<UUID, UUID>(); // sess, owner id

        protected Dictionary<string, string> KeyValueStore = new Dictionary<string, string>();
        protected Dictionary<string, long> KeyValueStoreLastUsed = new Dictionary<string, long>();


        protected List<CommandHistory> commandHistories = new List<CommandHistory>();

        public void addCommandToHistory(UUID Source, string command, string[] args)
        {
            CommandHistory A = new CommandHistory(Source, command, args, SecondbotHelpers.UnixTimeNow());
            commandHistories.Add(A);
            int c = commandHistories.Count;
            while (c > myConfig.GetCommandHistoryLimit())
            {
                commandHistories.RemoveAt(0);
                c--;
            }
        }

        public string GetKeyValue(string key)
        {
            if(KeyValueStore.ContainsKey(key) == false)
            {
                return "";
            }
            KeyValueStoreLastUsed[key] = SecondbotHelpers.UnixTimeNow();
            return KeyValueStore[key];
        }

        public void SetKeyValue(string key, string value)
        {
            if(KeyValueStore.ContainsKey(key) == false)
            {
                KeyValueStore.Add(key, "");
                KeyValueStoreLastUsed.Add(key, 0);
            }
            KeyValueStore[key] = value;
            KeyValueStoreLastUsed[key] = SecondbotHelpers.UnixTimeNow();
        }

        public void ClearKeyValue(string key)
        {
            if (KeyValueStore.ContainsKey(key) == false)
            {
                return;
            }
            KeyValueStore.Remove(key);
            KeyValueStoreLastUsed.Remove(key);
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
            List<UUID> reply = new List<UUID>();
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
            return chatWindows[group];
        }

        public void clearGroupChat()
        {
            foreach(UUID group in groupsKey2Name.Keys)
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

        public Dictionary<UUID, string> GetGroupRoles(UUID group)
        {
            Dictionary<UUID, string> reply = new Dictionary<UUID, string>();
            if (groupRoles.ContainsKey(group) == false)
            {
                
                AutoResetEvent fetchEvent = new AutoResetEvent(false);
                EventHandler<GroupRolesDataReplyEventArgs> callback =
                    delegate (object sender, GroupRolesDataReplyEventArgs e)
                    {
                        if (e.GroupID == group)
                        {
                            foreach(KeyValuePair<UUID,GroupRole> role in e.Roles)
                            {
                                reply[role.Value.ID] = role.Value.Name;
                            }
                            fetchEvent.Set();
                        }
                    };
                getClient().Groups.GroupRoleDataReply += callback;
                getClient().Groups.RequestGroupRoles(group);

                fetchEvent.WaitOne(1000, false);
                getClient().Groups.GroupRoleDataReply -= callback;
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
            if(groupMembers.ContainsKey(group) == false)
            {
                groupMembers.Add(group, new List<UUID>());
            }
            if(groupMembers[group].Contains(avatar) == false)
            {
                bool found = false;
                AutoResetEvent fetchEvent = new AutoResetEvent(false);
                EventHandler<GroupMembersReplyEventArgs> callback =
                    delegate (object sender, GroupMembersReplyEventArgs e)
                    {
                        if (e.GroupID == group)
                        {
                            foreach (KeyValuePair<UUID, GroupMember> a in e.Members)
                            {
                                if (a.Value.ID == avatar)
                                {
                                    found = true;
                                    fetchEvent.Set();
                                    break;
                                }
                            }
                            
                        }
                    };
                getClient().Groups.GroupMembersReply += callback;
                getClient().Groups.RequestGroupMembers(group);

                fetchEvent.WaitOne(1000, false);
                getClient().Groups.GroupMembersReply -= callback;
                return found;
            }
            return true;
        }

        public List<UUID> GetGroupMembers(UUID Group)
        {
            if (groupMembers.ContainsKey(Group) == false)
            {
                groupMembers.Add(Group, new List<UUID>());
            }
            return groupMembers[Group];
        }

        protected void recordChat(UUID sessionID, string fromname, string message)
        {
            if(chatWindows.ContainsKey(sessionID) == false)
            {
                return; // chat window not open unable to record message
            }
            chatWindows[sessionID].Add(fromname + ": " + message);
            chatWindowsUnread[sessionID] = true;
            int purgeLen = myConfig.GetImChatHistoryLimit();
            if(chatWindowsIsGroup[sessionID] == true)
            {
                purgeLen = myConfig.GetGroupChatHistoryLimitPerGroup();
            }
            int counter = chatWindows[sessionID].Count();
            while (counter > purgeLen)
            {
                chatWindows[sessionID].RemoveAt(0);
                counter--;
            }
        }

        protected void closeChat(UUID owner)
        {
            UUID sessionID = UUID.Zero;
            if(chatWindowsOwner.ContainsValue(owner) == false)
            {
                return;
            }
            foreach(KeyValuePair<UUID,UUID> ownermap in chatWindowsOwner)
            {
                if(ownermap.Value != owner)
                {
                    continue;
                }
                sessionID = ownermap.Key;
            }
            if(sessionID == UUID.Zero)
            {
                return;
            }
            chatWindowsOwner.Remove(sessionID);
            chatWindowsIsGroup.Remove(sessionID);
            chatWindowsUnread.Remove(sessionID);
            chatWindows.Remove(sessionID);

        }

        protected void openChatWindow(bool group, UUID sessionID, UUID ownerID)
        {
            if (chatWindows.ContainsKey(sessionID) == true)
            {
                return; // window already open
            }
            if (chatWindowsOwner.ContainsValue(ownerID) == true)
            {
                // new session find the old one and update to this session
                Dictionary<UUID, UUID> oldOwners = chatWindowsOwner;
                foreach (KeyValuePair<UUID,UUID> kvp in oldOwners)
                {
                    if(kvp.Value == ownerID)
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


        protected Dictionary<string, KeyValuePair<string, long>> notecardDataStore = new Dictionary<string, KeyValuePair<string, long>>();

        public List<string> getLocalChat()
        {
            return localChatHistory;
        }

        public bool knownGroup(UUID groupId, string name)
        {
            if(groupId != UUID.Zero)
            {
                return groupsKey2Name.ContainsKey(groupId);
            }
            return groupsKey2Name.ContainsValue(name);
        }

        public List<string> getGroupChat(UUID groupId)
        {
            if(chatWindows.ContainsKey(groupId) == false)
            {
                return new List<string>();
            }
            return chatWindows[groupId];
        }

        /*
         *  checks if the avatar is known to the system
         *  if not the bot will attempt to
         *  lookup the avatar
         */

        public bool knownAvatar(string name)
        {
            return knownAvatar(name, UUID.Zero);
        }


        protected int ObjectsAvatarsHash = 0;
        /*
         *  checks if the avatar is known to the system
         *  if not the bot will attempt to
         *  lookup the avatar
         */

        public bool knownAvatar(string name, UUID avatarId)
        {
            int currentHash = getClient().Network.CurrentSim.ObjectsAvatars.GetHashCode();
            if (currentHash != ObjectsAvatarsHash)
            {
                ObjectsAvatarsHash = currentHash;
                List<Avatar> avs = getClient().Network.CurrentSim.ObjectsAvatars.Copy().Values.ToList();
                foreach (Avatar A in avs)
                {
                    addAvatar(A.ID, A.Name);
                }
            }
            if (avatarId != UUID.Zero)
            {
                if(avatarsName2Key.ContainsValue(avatarId) == false)
                {
                    bool hasReply = false;
                    AutoResetEvent fetchEvent = new AutoResetEvent(false);
                    EventHandler<UUIDNameReplyEventArgs> callback =
                        delegate (object sender, UUIDNameReplyEventArgs e)
                        {
                            foreach (KeyValuePair<UUID, string> av in e.Names)
                            {
                                if (av.Key == avatarId)
                                {
                                    hasReply = true;
                                    fetchEvent.Set();
                                }
                            }
                        };
                    getClient().Avatars.UUIDNameReply += callback;
                    getClient().Avatars.RequestAvatarName(avatarId);

                    fetchEvent.WaitOne(1000, false);
                    getClient().Avatars.UUIDNameReply -= callback;
                    return hasReply;
                }
                return true;
            }
            string[] bits = name.Split(' ');
            if (bits.Length == 1)
            {
                name = name + " Resident";
            }
            if (avatarsName2Key.ContainsKey(name) == false)
            {
                UUID wantedReplyID = UUID.SecureRandom();
                
                bool hasReply = false;
                AutoResetEvent fetchEvent = new AutoResetEvent(false);
                EventHandler<AvatarPickerReplyEventArgs> callback =
                    delegate (object sender, AvatarPickerReplyEventArgs e)
                    {
                        if (e.QueryID == wantedReplyID)
                        {
                            foreach(KeyValuePair<UUID,string> av in e.Avatars)
                            {
                                if(av.Value == name)
                                {
                                    hasReply = true;
                                    fetchEvent.Set();
                                }
                            }
                        }
                    };
                getClient().Avatars.AvatarPickerReply += callback;
                getClient().Avatars.RequestAvatarNameSearch(name, wantedReplyID);

                fetchEvent.WaitOne(1000, false);
                getClient().Avatars.AvatarPickerReply -= callback;
                return hasReply;
            }
            return true;
        }

        public string getAvatarName(UUID avatarId)
        {
            if(knownAvatar(null, avatarId) == false)
            {
                return "lookup";
            }
            string results = "lookup";
            foreach (KeyValuePair<string, UUID> av in avatarsName2Key)
            {
                if(av.Value == avatarId)
                {
                    results = av.Key;
                    break;
                }
            }
            return results;
        }

        public string getAvatarUUID(string name)
        {
            if (knownAvatar(name) == false)
            {
                return "lookup";
            }
            return avatarsName2Key[name].ToString();
        }

        public List<string> getAvatarIms(UUID avatarId)
        {
            if(chatWindowsOwner.ContainsValue(avatarId) == false)
            {
                return new List<string>();
            }
            UUID sessionID = UUID.Zero;
            foreach(KeyValuePair<UUID, UUID> links in chatWindowsOwner)
            {
                if(links.Value == avatarId)
                {
                    sessionID = links.Key;
                    break;
                }
            }
            if (sessionID == UUID.Zero)
            {
                return new List<string>();
            }
            return chatWindows[sessionID];
        }

        public Dictionary<UUID,string> getAvatarImWindows()
        {
            Dictionary<UUID, string> windows = new Dictionary<UUID, string>();
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
                windows[chatWindowsOwner[links.Key]] = getAvatarName(chatWindowsOwner[links.Key]);
            }
            return windows;
        }

        public List<string> getAvatarImWindow(UUID avatarId)
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
            return chatWindows[sessionID];

        }

        public bool getAvatarImWindowsUnreadAny()
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

        public List<UUID> getAvatarImWindowsUnread()
        {
            List<UUID> windows = new List<UUID>();
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



        protected void reset()
        {
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
            Console.WriteLine("DataStore Service [Attached to new client]");
            getClient().Network.LoggedOut += BotLoggedOut;
            getClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            botConnected = false;
            getClient().Network.SimConnected += BotLoggedIn;
            Console.WriteLine("DataStore Service [Standby]");
        }

        protected void GroupRoles(object o, GroupRolesDataReplyEventArgs e)
        {
            if(groupRoles.ContainsKey(e.GroupID) == false)
            {
                groupRoles.Add(e.GroupID, new Dictionary<UUID, GroupRole>());
            }
            groupRoles[e.GroupID] = e.Roles;
        }

        protected void GroupMembers(object o, GroupMembersReplyEventArgs e)
        {
            if(groupMembers.ContainsKey(e.GroupID) == true)
            {
                groupMembers.Remove(e.GroupID);
            }
            groupMembers.Add(e.GroupID, new List<UUID>());
            foreach (KeyValuePair<UUID,GroupMember> a in e.Members)
            {
                groupMembers[e.GroupID].Add(a.Value.ID);
            }
        }

        public List<UUID> GetEstateBans()
        {
            if (estateBanlist == null)
            {
                List<UUID> reply = new List<UUID>();
                AutoResetEvent fetchEvent = new AutoResetEvent(false);
                EventHandler<EstateBansReplyEventArgs> callback =
                    delegate (object sender, EstateBansReplyEventArgs e)
                    {
                        reply = e.Banned;
                        fetchEvent.Set();
                    };
                getClient().Estate.EstateBansReply += callback;
                getClient().Estate.RequestInfo();

                fetchEvent.WaitOne(1000, false);
                getClient().Estate.EstateBansReply -= callback;
                return reply;
            }
            return estateBanlist;
        }

        List<UUID> estateBanlist = null;

        protected void EstateBans(object o, EstateBansReplyEventArgs e)
        {
            estateBanlist = e.Banned;
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            getClient().Network.SimConnected -= BotLoggedIn;
            getClient().Groups.CurrentGroups += GroupCurrent;
            getClient().Groups.GroupJoinedReply += GroupJoin;
            getClient().Groups.GroupLeaveReply += GroupLeave;
            getClient().Avatars.UUIDNameReply += AvatarUUIDName;
            getClient().Friends.FriendNames += AvatarFriendNames;
            getClient().Self.IM += ImEvent;
            getClient().Self.ChatFromSimulator += ChatFromSim;
            getClient().Avatars.AvatarPickerReply += AvatarUUIDNamePicker;
            getClient().Groups.GroupMembersReply += GroupMembers;
            getClient().Groups.GroupRoleDataReply += GroupRoles;
            getClient().Estate.EstateBansReply += EstateBans;
            getClient().Groups.RequestCurrentGroups();
            botConnected = true;
            Console.WriteLine("DataStore Service [Active]");
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
                        if (e.SourceType != ChatSourceType.Agent)
                        {
                            break;
                        }
                        if (hard_blocked_agents.Contains(e.FromName.ToLowerInvariant()) == true)
                        {
                            break;
                        }
                        localChatHistory.Add(e.FromName + ": " + e.Message);
                        if (localChatHistory.Count <= myConfig.GetLocalChatHistoryLimit())
                        {
                            break;
                        }
                        localChatHistory.RemoveAt(0);
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
            List<UUID> missingGroups = groupsKey2Name.Keys.ToList();
            foreach(Group G in e.Groups.Values)
            {
                missingGroups.Remove(G.ID);
                if (groupsKey2Name.ContainsKey(G.ID) == true)
                {
                    continue;
                }
                groupsKey2Name.Add(G.ID, G.Name);
                getClient().Self.RequestJoinGroupChat(G.ID);
                if (myConfig.GetPrefetchGroupMembers() == true)
                {
                    getClient().Groups.RequestGroupMembers(G.ID);
                }
                if(myConfig.GetPrefetchGroupRoles() == true)
                {
                    getClient().Groups.RequestGroupRoles(G.ID);
                }
            }

            foreach(UUID G in missingGroups)
            {
                if (groupsKey2Name.ContainsKey(G) == false)
                {
                    continue;
                }
                string name = groupsKey2Name[G];
                groupsKey2Name.Remove(G);
                closeChat(G);
            }
        }

        protected void GroupJoin(object o, GroupOperationEventArgs e)
        {
            getClient().Groups.RequestCurrentGroups();
        }

        protected void GroupLeave(object o, GroupOperationEventArgs e)
        {
            getClient().Groups.RequestCurrentGroups();
        }

        protected void addAvatar(UUID id, string name)
        {
            if (avatarsName2Key.ContainsValue(id) == true)
            {
                return;
            }
            avatarsName2Key.Add(name, id);
        }

        protected void AvatarUUIDName(object o, UUIDNameReplyEventArgs e)
        {
            foreach(KeyValuePair<UUID,string> pair in e.Names)
            {
                addAvatar(pair.Key, pair.Value);
            }
        }

        protected void AvatarUUIDNamePicker(object o, AvatarPickerReplyEventArgs e)
        {
            foreach(KeyValuePair<UUID,string> Av in e.Avatars)
            {
                addAvatar(Av.Key, Av.Value);
            }
        }

        protected void AvatarFriendNames(object o, FriendNamesEventArgs e)
        {
            foreach (KeyValuePair<UUID, string> pair in e.Names)
            {
                addAvatar(pair.Key, pair.Value);
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
                        openChatWindow(true, e.IM.IMSessionID, e.IM.IMSessionID);
                        recordChat(e.IM.IMSessionID, "**Notice**", e.IM.Message);
                        break;
                    }
                case InstantMessageDialog.SessionSend:
                case InstantMessageDialog.MessageFromAgent:
                    {
                        if (groupsKey2Name.ContainsKey(e.IM.IMSessionID) == true)
                        {
                            // group IM
                            openChatWindow(true, e.IM.IMSessionID, e.IM.IMSessionID);
                            recordChat(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.Message);
                            break;
                        }
                        addAvatar(e.IM.FromAgentID, e.IM.FromAgentName);
                        openChatWindow(false, e.IM.IMSessionID, e.IM.FromAgentID);
                        recordChat(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.Message);
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
            master.BotClientNoticeEvent += BotClientRestart;
            Console.WriteLine("DataStore Service [Starting]");
        }

        public override void Stop()
        {
            reset();
            master.BotClientNoticeEvent -= BotClientRestart;
            if (master.botClient != null)
            {
                if (getClient() != null)
                {
                    getClient().Groups.CurrentGroups -= GroupCurrent;
                    getClient().Groups.GroupJoinedReply -= GroupJoin;
                    getClient().Groups.GroupLeaveReply -= GroupLeave;
                    getClient().Avatars.UUIDNameReply -= AvatarUUIDName;
                    getClient().Friends.FriendNames -= AvatarFriendNames;
                    getClient().Self.IM -= ImEvent;
                    getClient().Self.ChatFromSimulator -= ChatFromSim;
                    getClient().Avatars.AvatarPickerReply -= AvatarUUIDNamePicker;
                    getClient().Groups.GroupMembersReply -= GroupMembers;
                    getClient().Groups.GroupRoleDataReply -= GroupRoles;
                    getClient().Estate.EstateBansReply -= EstateBans;
                }
            }
            Console.WriteLine("DataStore Service [Stopping]");
        }
    }

    public class CommandHistory
    {
        public UUID Source = UUID.Zero;
        public string Command = "";
        public string[] Args = new string[] { };
        public long Unixtime = 0;

        public CommandHistory(UUID setSource, string setCommand, string[] setArgs, long setUnixtime)
        {
            Source = setSource;
            Command = setCommand;
            Args = setArgs;
            Unixtime = setUnixtime;
        }
    }
}



