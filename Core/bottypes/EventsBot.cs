using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;
using OpenMetaverse.Imaging;
using OpenMetaverse.Packets;

namespace BetterSecondBot.bottypes
{
    public abstract class VoidEventBot : AnimationsBot
    {
        protected long delay_appearence_update;

        public override string GetStatus()
        {
            if (Client != null)
            {
                if (Client.Network != null)
                {
                    if (Client.Network.Connected)
                    {
                        if (Client.Network.CurrentSim != null)
                        {
                            if (delay_appearence_update == 0)
                            {
                                delay_appearence_update = helpers.UnixTimeNow();
                            }
                            else if (delay_appearence_update > 0)
                            {
                                long dif = delay_appearence_update - helpers.UnixTimeNow();
                                if (dif <= 0)
                                {
                                    delay_appearence_update = -1;
                                    Client.Appearance.RequestSetAppearance();
                                }
                            }
                        }
                    }
                }
            }
            return base.GetStatus();
        }

        public override void AfterBotLoginHandler()
        {
            if (reconnect == false)
            {
                Client.Network.RegisterCallback(PacketType.SetFollowCamProperties, SetFollowCamPropertiesPacketHandler);
            }
            delay_appearence_update = 0; // reset Appearance helper
            base.AfterBotLoginHandler();
        }

        protected void SetFollowCamPropertiesPacketHandler(object sender, PacketReceivedEventArgs e)
        {
            // do nothing
        }


    }
    public class StatusMessageEvent : EventArgs
    {
        public bool connected  { get; }
        public string sim { get; }

        public StatusMessageEvent(bool connected,string sim)
        {
            this.connected = connected;
            this.sim = sim;
        }
    }
    public class GroupEventArgs : EventArgs
    {
        public bool ready { get; }

        public GroupEventArgs(bool ready)
        {
            this.ready = ready;
        }
    }

    public class ImSendArgs : EventArgs
    {
        public UUID avataruuid { get; }
        public string message { get;  }

        public ImSendArgs(UUID avataruuid, string message)
        {
            this.avataruuid = avataruuid;
            this.message = message;
        }
    }

    public class MessageEventArgs : EventArgs
    {
        public string message { get; }
        public string sender_name { get; }
        public UUID sender_uuid { get; }
        public bool avatar { get; }
        public bool group { get; }
        public UUID group_uuid { get; }
        public bool localchat { get; }
        public bool fromme { get; }

        public MessageEventArgs(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            this.message = message;
            this.sender_name = sender_name;
            this.sender_uuid = sender_uuid;
            this.avatar = avatar;
            this.group = group;
            this.group_uuid = group_uuid;
            this.localchat = localchat;
            this.fromme = fromme;
        }
    }

    public class EventsBot : VoidEventBot
    {
        private EventHandler<SimChangedEventArgs> _ChangeSim;
        private EventHandler<AlertMessageEventArgs> _AlertMessage;
        private EventHandler<LoginProgressEventArgs> _LoginProgess;
        private EventHandler<NextHomeRegionArgs> _NextHomeRegion;
        private EventHandler<MessageEventArgs> _MessageEvent;
        private EventHandler<StatusMessageEvent> _StatusMessageEvent;
        private EventHandler<GroupEventArgs> _GroupsEvent;
        private EventHandler<ImSendArgs> _SendImEvent;

        private readonly object _ChangeSimLock = new object();
        private readonly object _AlertMessageLock = new object();
        private readonly object _LoginProgressLock = new object();
        private readonly object _NextHomeRegionLock = new object();
        private readonly object _MessageEventLock = new object();
        private readonly object _StatusMessageEventlock = new object();
        private readonly object _GroupsEventLock = new object();
        private readonly object _SendImEventLock = new object();


        public event EventHandler<SimChangedEventArgs> ChangeSimEvent
        {
            add { lock (_ChangeSimLock) { _ChangeSim += value; } }
            remove { lock (_ChangeSimLock) { _ChangeSim -= value; } }
        }
        public event EventHandler<AlertMessageEventArgs> AlertMessage
        {
            add { lock (_AlertMessageLock) { _AlertMessage += value; } }
            remove { lock (_AlertMessageLock) { _AlertMessage -= value; } }
        }
        public event EventHandler<LoginProgressEventArgs> LoginProgess
        {
            add { lock (_LoginProgressLock) { _LoginProgess += value; } }
            remove { lock (_LoginProgressLock) { _LoginProgess -= value; } }
        }
        public event EventHandler<NextHomeRegionArgs> NextHomeRegion
        {
            add { lock (_NextHomeRegionLock) { _NextHomeRegion += value; } }
            remove { lock (_NextHomeRegionLock) { _NextHomeRegion -= value; } }
        }

        public event EventHandler<MessageEventArgs> MessageEvent
        {
            add { lock (_MessageEventLock) { _MessageEvent += value; } }
            remove { lock (_MessageEventLock) { _MessageEvent -= value; } }
        }
        public event EventHandler<StatusMessageEvent> StatusMessageEvent
        {
            add { lock (_StatusMessageEventlock) { _StatusMessageEvent += value; } }
            remove { lock (_StatusMessageEventlock) { _StatusMessageEvent -= value; } }
        }
        public event EventHandler<GroupEventArgs> GroupsReadyEvent
        {
            add { lock (_GroupsEventLock) { _GroupsEvent += value; } }
            remove { lock (_GroupsEventLock) { _GroupsEvent -= value; } }
        }
        public event EventHandler<ImSendArgs> SendImEvent
        {
            add { lock (_SendImEventLock) { _SendImEvent += value; } }
            remove { lock (_SendImEventLock) { _SendImEvent -= value; } }
        }

        protected void On_SendImEvent(ImSendArgs e)
        {
            EventHandler<ImSendArgs> handler = _SendImEvent;
            handler?.Invoke(this, e);
        }
        protected void On_MessageEvent(MessageEventArgs e)
        {
            EventHandler<MessageEventArgs> handler = _MessageEvent;
            handler?.Invoke(this, e);
        }

        protected void On_StatusMessageEvent(StatusMessageEvent e)
        {
            EventHandler<StatusMessageEvent> handler = _StatusMessageEvent;
            handler?.Invoke(this, e);
        }

        protected void On_GroupsReadyEvent(GroupEventArgs e)
        {
            EventHandler<GroupEventArgs> handler = _GroupsEvent;
            handler?.Invoke(this, e);
        }

        public virtual void SendIM(UUID avatar, string message)
        {
            On_SendImEvent(new ImSendArgs(avatar, message));
        }

        protected virtual void BotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            if (_MessageEvent != null)
            {
                On_MessageEvent(new MessageEventArgs(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, fromme));
            }
        }

        protected override void LoginHandler(object o, LoginProgressEventArgs e)
        {
            base.LoginHandler(o, e);
            EventHandler<LoginProgressEventArgs> handler = _LoginProgess;
            handler?.Invoke(this, e);
        }

        protected void ChangeSim(object sender, SimChangedEventArgs e)
        {
            EventHandler<SimChangedEventArgs> handler = _ChangeSim;
            handler?.Invoke(this, e);
        }

        protected void AlertEvent(object sender, AlertMessageEventArgs e)
        {
            EventHandler<AlertMessageEventArgs> handler = _AlertMessage;
            handler?.Invoke(this, e);
        }

        public void GotoNextHomeRegion()
        {
            NextHomeRegionArgs e = new NextHomeRegionArgs();
            EventHandler<NextHomeRegionArgs> handler = _NextHomeRegion;
            handler?.Invoke(this, e);
        }

        protected bool login_auto_logout;
        protected long delay_group_fetch;
        protected Dictionary<UUID, KeyValuePair<long, List<UUID>>> group_members_storage = new Dictionary<UUID, KeyValuePair<long, List<UUID>>>();
        protected virtual void GroupMembersReplyHandler(object sender, GroupMembersReplyEventArgs e)
        {

        }

        public List<UUID> GetGroupMembers(UUID group)
        {
            if (group_members_storage.ContainsKey(group) == false)
            {
                return null;
            }
            return group_members_storage[group].Value;
        }

        public bool FastCheckInGroup(UUID group,UUID avatar)
        {
            if(NeedReloadGroupData(group) == false)
            {
                return group_members_storage[group].Value.Contains(avatar);
            }
            return false;
        }
        public bool NeedReloadGroupData(UUID group)
        {
            bool result = true;
            lock (group_members_storage)
            {
                if (group_members_storage.ContainsKey(group) == true)
                {
                    long dif = last_cleanup - group_members_storage[group].Key;
                    if (dif < 240)
                    {
                        result= false;
                    }
                    else
                    {
                        group_members_storage.Remove(group);
                    }
                }
            }
            return result;
        }

        long last_cleanup = 0;

        protected void Cleanup_group_member_storage()
        {
            lock (group_members_storage)
            {
                List<UUID> groups_to_purge = new List<UUID>();
                foreach (UUID groupid in group_members_storage.Keys)
                {
                    long dif = last_cleanup - group_members_storage[groupid].Key;
                    if (dif > 60)
                    {
                        groups_to_purge.Add(groupid);
                    }
                }
                foreach (UUID group in groups_to_purge)
                {
                    group_members_storage.Remove(group);
                }
            }
        }

        protected override void BotStartHandler()
        {
            Client.Network.LoginProgress += LoginHandler;
        }

        public override string GetStatus()
        {
            if (Client != null)
            {
                if (Client.Network != null)
                {
                    if (Client.Network.Connected)
                    {
                        long dif = helpers.UnixTimeNow() - last_cleanup;
                        if (dif > 30)
                        {
                            last_cleanup = helpers.UnixTimeNow();
                            Cleanup_group_member_storage();
                        }
                        if (delay_group_fetch > 0)
                        {
                            dif = helpers.UnixTimeNow() - delay_group_fetch;
                            if (dif > 0)
                            {
                                delay_group_fetch = 0;
                                Client.Groups.RequestCurrentGroups();
                            }
                        }
                    }
                }
            }
            if (Client.Network.CurrentSim != null)
            {
                On_StatusMessageEvent(new StatusMessageEvent(true, Client.Network.CurrentSim.Name));
            }
            else
            {
                On_StatusMessageEvent(new StatusMessageEvent(true, "None"));
            }
            return base.GetStatus();
        }
        public override void KillMePlease()
        {
            On_StatusMessageEvent(new StatusMessageEvent(false, "None"));
            base.KillMePlease();
        }

        public virtual void GroupsHandler(object sender, CurrentGroupsEventArgs e)
        {
            mygroups = e.Groups;
            foreach(KeyValuePair<UUID,Group> entry in mygroups)
            {
                Client.Self.RequestJoinGroupChat(entry.Value.ID);
            }
            On_GroupsReadyEvent(new GroupEventArgs(true));

        }

        public override void AfterBotLoginHandler()
        {
            if (reconnect == false)
            {
                Client.Self.ChatFromSimulator += ChatInputHandler;
                Client.Self.IM += MessageHandler;
                Client.Groups.CurrentGroups += GroupsHandler;
                Client.Self.ScriptQuestion += PermissionsHandler;
                Client.Friends.FriendshipResponse += FriendshipResponse;
                Client.Friends.FriendNames += AvatarFriendNames;
                delay_group_fetch = helpers.UnixTimeNow() + 10;
            }
            base.AfterBotLoginHandler();
        }


        protected virtual void AvatarFriendNames(object o, FriendNamesEventArgs E)
        {

        }

        protected virtual void FriendshipResponse(object o, FriendshipResponseEventArgs E)
        {

        }

        protected override void PermissionsHandler(object sender, ScriptQuestionEventArgs e)
        {
            if (e.Questions == ScriptPermission.TriggerAnimation)
            {
                TriggerAnimation(e);
            }
            else
            {
                if (e.ObjectOwnerName == Client.Self.Name)
                {
                    Client.Self.ScriptQuestionReply(Client.Network.CurrentSim, e.ItemID, e.TaskID, e.Questions);
                }
                else
                {
                    Client.Self.ScriptQuestionReply(Client.Network.CurrentSim, e.ItemID, e.TaskID, ScriptPermission.None);
                }
            }
        }

        public bool IsSittingOnUUID(UUID id)
        {
            if (Client.Self.SittingOn > 0)
            {
                if (Client.Network.CurrentSim.ObjectsPrimitives.ContainsKey(Client.Self.SittingOn))
                {
                    Primitive obj = GetClient.Network.CurrentSim.ObjectsPrimitives[Client.Self.SittingOn];
                    if (obj.ID == id)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual void TriggerAnimation(ScriptQuestionEventArgs e)
        {
            bool accept_invite;
            if (Is_avatar_master(e.ObjectOwnerName) == true)
            {
                accept_invite = true;
            }
            else if (e.ObjectOwnerName == Client.Self.Name)
            {
                accept_invite = true;
            }
            else if(IsSittingOnUUID(e.ItemID) == true)
            {
                accept_invite = true;
            }
            else
            {
                accept_invite = Accept_action_from("animation", e.ItemID);
                if (accept_invite == true)
                {
                    Remove_action_from("animation", e.ItemID);
                }
            }
            if (accept_invite == true)
            {
                Client.Self.ScriptQuestionReply(Client.Network.CurrentSim, e.ItemID, e.TaskID, ScriptPermission.TriggerAnimation);
            }
        }

        protected override void FriendshipOffer(UUID IMSessionID, string FromAgentName, UUID FromAgentID)
        {
            bool accept_invite;
            if (Is_avatar_master(FromAgentName) == true)
            {
                accept_invite = true;
            }
            else
            {
                accept_invite = Accept_action_from("friend", FromAgentID);
                if (accept_invite == true)
                {
                    Remove_action_from("friend", FromAgentID);
                }
            }
            if (accept_invite == true)
            {
                Client.Friends.AcceptFriendship(FromAgentID, IMSessionID);
            }
        }

        protected override void RequestTeleport(UUID IMSessionID, string FromAgentName, UUID FromAgentID)
        {
            bool accept_invite;
            if (Is_avatar_master(FromAgentName) == true)
            {
                accept_invite = true;
            }
            else
            {
                accept_invite = Accept_action_from("teleport", FromAgentID);
                if (accept_invite == true)
                {
                    Remove_action_from("teleport", FromAgentID);
                }
            }
            if (accept_invite == true)
            {
                ResetAnimations();
                SetTeleported();
                Client.Self.TeleportLureRespond(FromAgentID, IMSessionID, true);
            }
        }

        protected override void GroupInvite(InstantMessageEventArgs e)
        {
            LogFormater.Info("Group invite event from: " + e.IM.FromAgentName);
            string[] stage1 = e.IM.FromAgentName.ToLowerInvariant().Split('.');
            if(stage1.Length == 1)
            {
                stage1 = e.IM.FromAgentName.ToLowerInvariant().Split(" ");
            }
            string name = "" + stage1[0].FirstCharToUpper() + "";
            if (stage1.Length == 1)
            {
                name = " Resident";
            }
            else
            {
                name = "" + name + " " + stage1[1].FirstCharToUpper() + "";
            }
            bool accept_invite;
            string whyAccept = "";
            if (Is_avatar_master(name) == true)
            {
                accept_invite = true;
                whyAccept = "Master";
            }
            else
            {
                accept_invite = Accept_action_from("group", e.IM.FromAgentID);
                if(accept_invite == true)
                {
                    whyAccept = "Action auth";
                    Remove_action_from("group", e.IM.FromAgentID);
                }
            }
            if (accept_invite == true)
            {
                LogFormater.Info("Group invite event from: " + e.IM.FromAgentName+" Accepted - "+ whyAccept);
                GroupInvitationEventArgs G = new GroupInvitationEventArgs(e.Simulator, e.IM.FromAgentID, e.IM.FromAgentName, e.IM.Message);
                Client.Self.GroupInviteRespond(G.AgentID, e.IM.IMSessionID, true);
                Client.Groups.RequestCurrentGroups();
            }
            else
            {
                LogFormater.Info("Group invite event from: " + e.IM.FromAgentName + " Rejected");
            }
        }
    }
}
