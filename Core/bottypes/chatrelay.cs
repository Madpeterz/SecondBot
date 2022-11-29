using Newtonsoft.Json;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterSecondBot.bottypes
{
    public abstract class ChatRelay : AwaitReplyEventBot
    {
        #region GroupIMs
        protected int localchatlimit = 20;
        protected int groupchatlimit = 10;
        protected int imchatlimit = 20;
        protected Dictionary<UUID, List<string>> group_chat_history = new Dictionary<UUID, List<string>>();
        protected Dictionary<UUID, bool> group_chat_unread = new Dictionary<UUID, bool>();

        protected Dictionary<UUID, List<string>> im_chat_history = new Dictionary<UUID, List<string>>();
        protected Dictionary<UUID, bool> im_chat_unread = new Dictionary<UUID, bool>();
        protected Dictionary<UUID, string> im_chat_history_to_name = new Dictionary<UUID, string>();

        public override Dictionary<UUID, string> GetIMChatWindowKeyNames()
        {
            return im_chat_history_to_name;
        }

        public override List<UUID> IMChatWindows()
        {
            return im_chat_history.Keys.ToList();
        }
        public override bool ImChatWindowHasUnread(UUID chat_window)
        {
            if(im_chat_unread.ContainsKey(chat_window) == true)
            {
                return im_chat_unread[chat_window];
            }
            return false;
        }
        public override List<string> GetIMChatWindow(UUID chat_window)
        {
            if(im_chat_history.ContainsKey(chat_window) == true)
            {
                im_chat_unread[chat_window] = false;
                return im_chat_history[chat_window];
            }
            else
            {
                return new List<string>();
            }
        }
        public override void AddToIMchat(UUID avatar, string name, string message)
        {
            string sendername = name;
            if(sendername == GetClient.Self.Name)
            {
                name = FindAvatarKey2Name(avatar);
            }
            if (im_chat_history.ContainsKey(avatar) == false)
            {
                if(im_chat_history_to_name.ContainsKey(avatar) == false)
                {
                    im_chat_history_to_name.Add(avatar, name);
                }
                im_chat_history.Add(avatar, new List<string>());
                im_chat_unread.Add(avatar, false);
            }
            var date = DateTime.Now;
            im_chat_history[avatar].Add("[" + date.Hour.ToString() + ":" + date.Minute.ToString() + "] " + sendername + ": " + message + "");
            if (im_chat_history[avatar].Count > imchatlimit)
            {
                im_chat_history[avatar].RemoveAt(0);
            }
            if (name != GetClient.Self.Name)
            {
                im_chat_unread[avatar] = true;
            }
        }
        public override void AddToLocalChat(string name, string message)
        {
            var date = DateTime.Now;
            LocalChat_history.Add("[" + date.Hour.ToString() + ":" + date.Minute.ToString() + "] " + name + ": " + message + "");
            if (LocalChat_history.Count > localchatlimit)
            {
                LocalChat_history.RemoveAt(0);
            }
        }



        protected List<string> LocalChat_history = new List<string>();
        public override List<string> getLocalChatHistory()
        {
            return LocalChat_history;
        }

        protected override void BotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            if(sender_name == ""){
                sender_name = "(no name)";
            }

            base.BotChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, fromme);
            if(localchat == true)
            {
                if (avatar == true)
                {
                    if (fromme == false)
                    {
                        AddToLocalChat(sender_name, message);
                    }
                }
            }
            else
            {
                if (avatar == true)
                {
                    if(group == false)
                    {
                        if (fromme == false)
                        {
                            AddToIMchat(sender_uuid, sender_name, message);
                        }
                    }
                }
            }
        }

        public override bool HasUnreadGroupchats()
        {
            return group_chat_unread.ContainsValue(true);
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
        public override List<string> GetGroupchat(UUID group)
        {
            if (group_chat_history.ContainsKey(group) == true)
            {
                group_chat_unread[group] = false;
                return group_chat_history[group];
            }
            return new List<string>();
        }
        public override void AddToGroupchat(UUID group,string name,string message)
        {
            if (group_chat_history.ContainsKey(group) == false)
            {
                group_chat_history.Add(group, new List<string>());
                group_chat_unread.Add(group, false);
            }
            var date = DateTime.Now;
            group_chat_history[group].Add("[" + date.Hour.ToString() + ":" + date.Minute.ToString() + "] " + name + ": " + message + "");
            if (group_chat_history[group].Count > groupchatlimit)
            {
                group_chat_history[group].RemoveAt(0);
            }
            if (name != GetClient.Self.Name)
            {
                group_chat_unread[group] = true;
            }
        }
        public override void ClearGroupchat(UUID group)
        {
            if (group_chat_history.ContainsKey(group) == true)
            {
                group_chat_history.Remove(group);
                group_chat_unread.Remove(group);
            }
        }
        public override void ClearAllGroupchat()
        {
            group_chat_history = new Dictionary<UUID, List<string>>();
            group_chat_unread = new Dictionary<UUID, bool>();
        }
        #endregion
    }
}
