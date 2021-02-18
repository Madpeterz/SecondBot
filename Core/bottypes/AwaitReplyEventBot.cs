using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using BetterSecondBotShared.Static;

namespace BetterSecondBot.bottypes
{
    public abstract class AwaitReplyEventBot : CommandsBot
    {
        public override List<UUID> GetActiveGroupchatSessions { get { return active_group_chat_sessions; } }

        public override void AfterBotLoginHandler()
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
        }
        protected virtual void ObjectOwnersHandler(object sender, ParcelObjectOwnersReplyEventArgs e)
        {

        }
        protected void GroupRolesData(object sender,GroupRolesDataReplyEventArgs e)
        {
            // update group role storage
            List<GroupRole> entrys = new List<GroupRole>();
            foreach (GroupRole gr in e.Roles.Values)
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
        }
    }
}