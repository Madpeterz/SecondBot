using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

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
        protected Dictionary<UUID, string> avatarsKey2Name = new Dictionary<UUID, string>();

        protected Dictionary<UUID, string> groupsKey2Name = new Dictionary<UUID, string>();
        protected Dictionary<string, UUID> groupsName2Key =  new Dictionary<string, UUID>();

        protected List<string> localChatHistory = new List<string>();


        protected Dictionary<UUID, List<string>> chatWindows = new Dictionary<UUID, List<string>>(); // sess, list chat
        protected Dictionary<UUID, bool> chatWindowsUnread = new Dictionary<UUID, bool>(); // sess, bool
        protected Dictionary<UUID, bool> chatWindowsIsGroup = new Dictionary<UUID, bool>(); // sess, bool
        protected Dictionary<UUID, UUID> chatWindowsOwner = new Dictionary<UUID, UUID>(); // sess, owner id


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
            return groupsName2Key.ContainsKey(name);
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
            int currentHash = master.botClient.client.Network.CurrentSim.ObjectsAvatars.GetHashCode();
            if (currentHash != ObjectsAvatarsHash)
            {
                ObjectsAvatarsHash = currentHash;
                List<Avatar> avs = master.botClient.client.Network.CurrentSim.ObjectsAvatars.Copy().Values.ToList();
                foreach (Avatar A in avs)
                {
                    addAvatar(A.ID, A.Name);
                }
            }
            if (avatarId != UUID.Zero)
            {
                if(avatarsKey2Name.ContainsKey(avatarId) == false)
                {
                    master.botClient.client.Avatars.RequestAvatarName(avatarId);
                    return false;
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
                master.botClient.client.Avatars.RequestAvatarNameSearch(name, UUID.Random());
                return false;
            }
            return true;
        }

        public string getAvatarName(UUID avatarId)
        {
            if(knownAvatar(null, avatarId) == false)
            {
                return "lookup";
            }
            return avatarsKey2Name[avatarId];
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
            avatarsKey2Name = new Dictionary<UUID, string>();

            groupsKey2Name = new Dictionary<UUID, string>();
            groupsName2Key = new Dictionary<string, UUID>();

            localChatHistory = new List<string>();

            notecardDataStore = new Dictionary<string, KeyValuePair<string, long>>();
        }


        protected void BotClientRestart(object o, BotClientNotice e)
        {
            botConnected = false;
            Console.WriteLine("DataStore Service [Attached to new client]");
            master.botClient.client.Network.LoggedOut += BotLoggedOut;
            master.botClient.client.Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            botConnected = false;
            master.botClient.client.Network.SimConnected += BotLoggedIn;
            Console.WriteLine("DataStore Service [Standby]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            master.botClient.client.Network.SimConnected -= BotLoggedIn;
            master.botClient.client.Groups.CurrentGroups += GroupCurrent;
            master.botClient.client.Groups.GroupJoinedReply += GroupJoin;
            master.botClient.client.Groups.GroupLeaveReply += GroupLeave;
            master.botClient.client.Avatars.UUIDNameReply += AvatarUUIDName;
            master.botClient.client.Friends.FriendNames += AvatarFriendNames;
            master.botClient.client.Self.IM += ImEvent;
            master.botClient.client.Self.ChatFromSimulator += ChatFromSim;
            master.botClient.client.Groups.RequestCurrentGroups();
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
                        cleanLocalChat();
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
                groupsName2Key.Add(G.Name, G.ID);
                master.botClient.client.Self.RequestJoinGroupChat(G.ID);
            }

            foreach(UUID G in missingGroups)
            {
                if (groupsKey2Name.ContainsKey(G) == false)
                {
                    continue;
                }
                string name = groupsKey2Name[G];
                groupsKey2Name.Remove(G);
                groupsName2Key.Remove(name);
                // @todo remove group chat here
            }
        }

        protected void GroupJoin(object o, GroupOperationEventArgs e)
        {
            master.botClient.client.Groups.RequestCurrentGroups();
        }

        protected void GroupLeave(object o, GroupOperationEventArgs e)
        {
            master.botClient.client.Groups.RequestCurrentGroups();
        }

        protected void addAvatar(UUID id, string name)
        {
            if (avatarsKey2Name.ContainsKey(id) == true)
            {
                return;
            }
            avatarsKey2Name.Add(id, name);
            avatarsName2Key.Add(name, id);
        }

        protected void AvatarUUIDName(object o, UUIDNameReplyEventArgs e)
        {
            foreach(KeyValuePair<UUID,string> pair in e.Names)
            {
                addAvatar(pair.Key, pair.Value);
            }
        }

        protected void AvatarFriendNames(object o, FriendNamesEventArgs e)
        {
            foreach (KeyValuePair<UUID, string> pair in e.Names)
            {
                addAvatar(pair.Key, pair.Value);
            }
        }

        protected void cleanGroupChat(UUID groupID)
        {
            if(groupsKey2Name.ContainsKey(groupID) == false)
            {
                return;
            }
            // @todo clean group chat here
        }

        protected void cleanAvatarChat(UUID avatarID)
        {
            if (avatarsKey2Name.ContainsKey(avatarID) == false)
            {
                return;
            }
            // @todo clean avatar chat here
        }

        protected void cleanLocalChat()
        {
            if (localChatHistory.Count <= myConfig.GetLocalChatHistoryLimit())
            {
                return;
            }
            localChatHistory.RemoveAt(0);
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
                if (master.botClient.client != null)
                {
                    master.botClient.client.Groups.CurrentGroups -= GroupCurrent;
                    master.botClient.client.Groups.GroupJoinedReply -= GroupJoin;
                    master.botClient.client.Groups.GroupLeaveReply -= GroupLeave;
                    master.botClient.client.Avatars.UUIDNameReply -= AvatarUUIDName;
                    master.botClient.client.Friends.FriendNames -= AvatarFriendNames;
                    master.botClient.client.Self.IM -= ImEvent;
                    master.botClient.client.Self.ChatFromSimulator -= ChatFromSim;
                }
            }
            Console.WriteLine("DataStore Service [Stopping]");
        }
    }
}



