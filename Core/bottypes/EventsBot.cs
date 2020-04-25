using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.Static;
using BSB.Commands;
using OpenMetaverse;
using OpenMetaverse.Packets;

namespace BSB.bottypes
{
    public abstract class VoidEventBot : AnimationsBot
    {
        protected long delay_appearence_update = 0;

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
                                    if(Client.Appearance.RequestSetAppearance() == "retry")
                                    {
                                        delay_appearence_update = helpers.UnixTimeNow() + 1;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return base.GetStatus();
        }

        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            if (reconnect == false)
            {
                Client.Network.RegisterCallback(PacketType.SetFollowCamProperties, SetFollowCamPropertiesPacketHandler);
            }
            delay_appearence_update = 0; // reset Appearance helper
        }

        protected void SetFollowCamPropertiesPacketHandler(object sender, PacketReceivedEventArgs e)
        {
            // do nothing
        }


    }
    public class EventsBot : VoidEventBot
    {
        protected List<string> accept_animation_request_from_users = new List<string>();
        protected bool login_auto_logout = false;
        protected long delay_group_fetch = 0;

        Dictionary<UUID, KeyValuePair<long, List<UUID>>> group_members_storage = new Dictionary<UUID, KeyValuePair<long, List<UUID>>>();
        protected virtual void GroupMembersReplyHandler(object sender, GroupMembersReplyEventArgs e)
        {
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
                    if (dif < 60)
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

        protected void cleanup_group_member_storage()
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

        public void ToggleAutoAcceptAnimations(string userName)
        {
            if(accept_animation_request_from_users.Contains(userName) == true)
            {
                accept_animation_request_from_users.Remove(userName);
            }
            else
            {
                accept_animation_request_from_users.Add(userName);
            }
        }

        protected override void BotStartHandler()
        {
            Client.Network.LoginProgress += new EventHandler<LoginProgressEventArgs>(LoginHandler);
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
                            cleanup_await_events();
                            cleanup_group_member_storage();
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
            return base.GetStatus();
        }
        protected string login_status = "Waiting to login";
        protected override void LoginHandler(object o, LoginProgressEventArgs e)
        {
            if (e.Status == LoginStatus.Success)
            {
                login_status = "Ok";
                AfterBotLoginHandler();
            }
            else if(e.Status == LoginStatus.Failed)
            {
                login_status = "Logged out";
                if (e.FailReason == "presence")
                {
                    login_auto_logout = true;
                    login_status = "Logged out <Clear AV>";
                }
            }
            else if(e.Status == LoginStatus.Failed)
            {
                login_status = "Failed (Check username and password)";
            }
            else if(e.Status == LoginStatus.None)
            {
                login_status = "None :/";
            }
            else if(e.Status == LoginStatus.ConnectingToLogin)
            {
                login_status = "Starting login";
            }
            else if(e.Status == LoginStatus.ReadingResponse)
            {
                login_status = "Busy";
            }
            else if(e.Status == LoginStatus.ConnectingToSim)
            {
                login_status = "Joining sim";
            }
            else if(e.Status == LoginStatus.Redirecting)
            {
                login_status = "Redirecting";
            }

        }
        public virtual void GroupsHandler(object sender, CurrentGroupsEventArgs e)
        {
            mygroups = e.Groups;
            foreach(KeyValuePair<UUID,Group> entry in mygroups)
            {
                Client.Self.RequestJoinGroupChat(entry.Value.ID);
            }
        }

        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            if (reconnect == false)
            {
                Client.Self.ChatFromSimulator += ChatInputHandler;
                Client.Self.IM += MessageHandler;
                Client.Groups.CurrentGroups += GroupsHandler;
                Client.Self.ScriptQuestion += PermissionsHandler;
                delay_group_fetch = helpers.UnixTimeNow() + 10;
            }
        }

        protected override void PermissionsHandler(object sender, ScriptQuestionEventArgs e)
        {
            if(e.ObjectOwnerName == Client.Self.Name)
            {
                Client.Self.ScriptQuestionReply(Client.Network.CurrentSim, e.ItemID, e.TaskID, e.Questions);
            }
            else if(accept_animation_request_from_users.Contains(e.ObjectOwnerName) == true)
            {
                Client.Self.ScriptQuestionReply(Client.Network.CurrentSim, e.ItemID, e.TaskID, ScriptPermission.TriggerAnimation);
            }
            else if((e.ObjectOwnerName == myconfig.master) && (myconfig.master != ""))
            {
                Client.Self.ScriptQuestionReply(Client.Network.CurrentSim, e.ItemID, e.TaskID, e.Questions);
            }
            else if(Client.Self.SittingOn > 0)
            {
                if(Client.Network.CurrentSim.ObjectsPrimitives.ContainsKey(Client.Self.SittingOn))
                {
                    Primitive obj = GetClient.Network.CurrentSim.ObjectsPrimitives[Client.Self.SittingOn];
                    if(obj.ID == e.ItemID)
                    {
                        Client.Self.ScriptQuestionReply(Client.Network.CurrentSim, e.ItemID, e.TaskID, ScriptPermission.TriggerAnimation);
                    }     
                }
            }
            else
            {
                Client.Self.ScriptQuestionReply(Client.Network.CurrentSim, e.ItemID, e.TaskID, ScriptPermission.None);
            }
        }

        protected void cleanup_await_events()
        {
            long dif;
            List<string> purge_event_ids = new List<string>();
            foreach (KeyValuePair<string, long> CheckEvent in await_event_ages)
            {
                dif = last_cleanup - CheckEvent.Value;
                if (dif > 240)
                {
                    purge_event_ids.Add(CheckEvent.Key);
                }
            }
            foreach (string eventid in purge_event_ids)
            {
                string listener = await_event_idtolistener[eventid];
                await_event_idtolistener.Remove(eventid);
                await_event_ages.Remove(eventid);
                await_events[listener].Remove(eventid);
            }
        }

        protected long last_cleanup = 0;

        protected Dictionary<string, Dictionary<string, KeyValuePair<CoreCommand, string[]>>> await_events = new Dictionary<string, Dictionary<string, KeyValuePair<CoreCommand, string[]>>>();
        protected Dictionary<string, long> await_event_ages = new Dictionary<string, long>();
        protected Dictionary<string, string> await_event_idtolistener = new Dictionary<string, string>();
        protected int next_await_id = 1;
        public bool CreateAwaitEventReply(string event_listener, CoreCommand command, string[] args)
        {
            string eventid = "" + event_listener + "" + next_await_id.ToString() + "";
            next_await_id++;
            if (await_events.ContainsKey(event_listener) == false)
            {
                await_events.Add(event_listener, new Dictionary<string, KeyValuePair<CoreCommand, string[]>>());
            }
            if (await_events[event_listener].ContainsKey(eventid) == false)
            {
                if (await_event_ages.ContainsKey(eventid) == false)
                {
                    if (await_event_idtolistener.ContainsKey(eventid) == false)
                    {
                        await_events[event_listener].Add(eventid, new KeyValuePair<CoreCommand, string[]>(command, args));
                        await_event_ages.Add(eventid, helpers.UnixTimeNow());
                        await_event_idtolistener.Add(eventid, event_listener);
                        return true;
                    }
                    else return false;
                }
                return false;
            }
            else return false;
        }


    }


}
