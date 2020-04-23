using OpenMetaverse;
using System;
using System.Collections.Generic;

namespace BSB.bottypes
{
    public abstract class ChatRelay : RLVbot
    {
        #region GroupIMs
        protected int groupchatlimit = 10;
        protected Dictionary<UUID, List<string>> group_chat_history = new Dictionary<UUID, List<string>>();
        protected Dictionary<UUID, bool> group_chat_unread = new Dictionary<UUID, bool>();
        protected bool unread_groupchats;
        public override bool HasUnreadGroupchats()
        {
            return unread_groupchats;
        }
        protected void CheckUnreadGroupchat()
        {
            unread_groupchats = group_chat_unread.ContainsValue(true);
        }
        public override bool GroupHasUnread(UUID group)
        {
            if (group_chat_unread.ContainsKey(group) == true)
            {
                return group_chat_unread[group];
            }
            return false;
        }
        public override UUID[] UnreadGroupchatGroups()
        {
            List<UUID> reply = new List<UUID>();
            foreach(KeyValuePair<UUID,bool> test in group_chat_unread)
            {
                if (test.Value == true)
                {
                    reply.Add(test.Key);
                }
            }
            return reply.ToArray();
        }
        public override string[] GetGroupchat(UUID group)
        {
            if (group_chat_history.ContainsKey(group) == true)
            {
                group_chat_unread[group] = false;
                CheckUnreadGroupchat();
                return group_chat_history[group].ToArray();
            }
            return new string[] { };
        }
        protected override void AddToGroupchat(UUID group,string name,string message)
        {
            if (group_chat_history.ContainsKey(group) == false)
            {
                group_chat_history.Add(group, new List<string>());
                group_chat_unread.Add(group, false);
            }
            var date = DateTime.Now;
            group_chat_history[group].Add("[" + date.Hour.ToString() + ":" + date.Minute.ToString() + "] " + name + ":" + message + "");
            if (group_chat_history[group].Count > groupchatlimit)
            {
                group_chat_history[group].RemoveAt(0);
            }
            if (name != GetClient.Self.Name)
            {
                unread_groupchats = true;
                group_chat_unread[group] = true;
            }
        }
        public override void ClearGroupchat(UUID group)
        {
            if (group_chat_history.ContainsKey(group) == true)
            {
                group_chat_history.Remove(group);
                group_chat_unread.Remove(group);
                CheckUnreadGroupchat();
            }
        }
        public override void ClearAllGroupchat()
        {
            group_chat_history = new Dictionary<UUID, List<string>>();
            group_chat_unread = new Dictionary<UUID, bool>();
            unread_groupchats = false;
        }
        #endregion
    }
}
