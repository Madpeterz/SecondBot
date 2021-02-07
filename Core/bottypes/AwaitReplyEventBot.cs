using BSB.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using BetterSecondBotShared.Static;

namespace BSB.bottypes
{
    public abstract class AwaitReplyEventBot : CommandsBot
    {
        public override List<UUID> GetActiveGroupchatSessions { get { return active_group_chat_sessions; } }

        protected override void AfterBotLoginHandler()
        {
            if (reconnect == false)
            {
                Client.Groups.GroupRoleDataReply += GroupRolesData;
                Client.Self.GroupChatJoined += GroupchatJoinHandler;
                Client.Groups.GroupMembersReply += GroupMembersReplyHandler;
                Client.Parcels.ParcelObjectOwnersReply += ObjectOwnersHandler;
            }
            base.AfterBotLoginHandler();
        }
        protected override void GroupMembersReplyHandler(object sender, GroupMembersReplyEventArgs e)
        {
            base.GroupMembersReplyHandler(sender, e);
            if (await_events.ContainsKey("groupmembersreply") == true)
            {
                List<string> PurgeAwaiters = new List<string>();
                foreach (KeyValuePair<string, KeyValuePair<CoreCommand, string[]>> await_reply in await_events["groupmembersreply"])
                {
                    // eventid
                    // KeyValuePair<CommandLink,Args>
                    // Args: Group, args, args
                    if (UUID.TryParse(await_reply.Value.Value[0], out UUID test_group) == true)
                    {
                        if (test_group == e.GroupID)
                        {
                            // oh shit its for me
                            PurgeAwaiters.Add(await_reply.Key);
                            await_reply.Value.Key.Callback(await_reply.Value.Value, e);
                        }
                    }
                }
                foreach (string eventid in PurgeAwaiters)
                {
                    await_event_ages.Remove(eventid);
                    await_event_idtolistener.Remove(eventid);
                    await_events["groupmembersreply"].Remove(eventid);
                }
            }
        }

        protected virtual void GroupchatJoinHandler(object sender, GroupChatJoinedEventArgs e)
        {
            if (active_group_chat_sessions.Contains(e.SessionID) == false)
            {
                if (e.Success == true)
                {
                    active_group_chat_sessions.Add(e.SessionID);
                }
            }
            if (await_events.ContainsKey("groupchatjoin") == true)
            {
                List<string> PurgeAwaiters = new List<string>();
                foreach (KeyValuePair<string, KeyValuePair<CoreCommand, string[]>> await_reply in await_events["groupchatjoin"])
                {
                    // eventid
                    // KeyValuePair<CommandLink,Args>
                    // Args: Group, Message
                    if (await_reply.Value.Value.Length == 2)
                    {
                        if (UUID.TryParse(await_reply.Value.Value[0], out UUID test_group) == true)
                        {
                            if (test_group == e.SessionID)
                            {
                                // oh shit its for me
                                PurgeAwaiters.Add(await_reply.Key);
                                await_reply.Value.Key.Callback(await_reply.Value.Value, e);
                            }
                        }
                    }
                }
                foreach (string eventid in PurgeAwaiters)
                {
                    await_event_ages.Remove(eventid);
                    await_event_idtolistener.Remove(eventid);
                    await_events["groupchatjoin"].Remove(eventid);
                }
            }
        }
        protected virtual void ObjectOwnersHandler(object sender, ParcelObjectOwnersReplyEventArgs e)
        {
            if (await_events.ContainsKey("parcelobjectowners") == true)
            {
                List<string> PurgeAwaiters = new List<string>();
                foreach (KeyValuePair<string, KeyValuePair<CoreCommand, string[]>> await_reply in await_events["parcelobjectowners"])
                {
                    PurgeAwaiters.Add(await_reply.Key);
                    await_reply.Value.Key.Callback(await_reply.Value.Value, e);
                }
                foreach (string eventid in PurgeAwaiters)
                {
                    await_event_ages.Remove(eventid);
                    await_event_idtolistener.Remove(eventid);
                    await_events["parcelobjectowners"].Remove(eventid);
                }
            }
        }
        protected void GroupRolesData(object sender,GroupRolesDataReplyEventArgs e)
        {
            if(await_events.ContainsKey("grouproles") == true)
            {
                // update group role storage
                List<GroupRole> entrys = new List<GroupRole>();
                foreach(GroupRole gr in e.Roles.Values)
                {
                    entrys.Add(gr);
                }

                KeyValuePair<long, List<GroupRole>> storage = new KeyValuePair<long, List<GroupRole>>(helpers.UnixTimeNow(), entrys);
                if (mygrouprolesstorage.ContainsKey(e.GroupID) == false)
                {
                    mygrouprolesstorage.Add(e.GroupID, storage);
                }
                else
                {
                    mygrouprolesstorage[e.GroupID] = storage;
                }


                List<string> PurgeAwaiters = new List<string>();
                foreach(KeyValuePair<string, KeyValuePair<CoreCommand, string[]>> await_reply in await_events["grouproles"])
                {
                    // eventid
                    // KeyValuePair<CommandLink,Args>
                    // Args: ReplyConfig, GroupUUID
                    if (await_reply.Value.Value.Length == 2)
                    {
                        if(UUID.TryParse(await_reply.Value.Value[1], out UUID test_group) == true)
                        {
                            if(test_group == e.GroupID)
                            {
                                // oh shit its for me
                                PurgeAwaiters.Add(await_reply.Key);
                                await_reply.Value.Key.Callback(await_reply.Value.Value,e);
                            }
                        }
                    }
                }
                foreach(string eventid in PurgeAwaiters)
                {
                    await_event_ages.Remove(eventid);
                    await_event_idtolistener.Remove(eventid);
                    await_events["grouproles"].Remove(eventid);
                }

            }
        }


    }
}