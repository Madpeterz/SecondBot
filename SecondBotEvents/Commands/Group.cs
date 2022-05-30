using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Services;
using System.Collections.Generic;

namespace SecondBotEvents.Commands
{
    public class GroupCommands : CommandsAPI
    {
        public GroupCommands(EventsSecondBot setmaster) : base(setmaster)
        {
        }

        [About("Checks if the given UUID is in the given group<br/>Note: if group membership data is more than 60 secs old this will return Updating<br/>Please wait and retry later")]
        [ReturnHints("Membership reply with [membershipStatus,AvatarUUID,AvatarnameIfKnown,GroupUUID]")]
        [ReturnHintsFailure("Updating")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("avatar lookup")]
        [ArgHints("group", "the UUID of the group")]
        [ArgHints("avatar", "the UUID of the avatar you wish to check with")]
        public object IsGroupMember(string group, string avatar)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "IsGroupMember", new [] { group, avatar });
            }
            ProcessAvatar(avatar);
            return BasicReply(master.DataStoreService.IsGroupMember(groupuuid, avataruuid).ToString());
        }

        [About("Gets membership of a group")]
        [ReturnHints("list of UUIDS of group members")]
        [ReturnHintsFailure("Updating")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("avatar lookup")]
        [ArgHints("group", "the UUID of the group")]
        public object GetGroupMembers(string group)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GetGroupMembers", new [] { group });
            }
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetGroupMembers(groupuuid)));
        }

        [About("Attempts to ban/unban a given avatar from a group")]
        [ReturnHints("Accepted")]
        [ReturnHintsFailure("Updating")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("avatar lookup")]
        [ReturnHintsFailure("Missing group GroupBanAccess power")]
        [ArgHints("group", "the UUID of the group")]
        [ArgHints("avatar", "the UUID of the avatar or Firstname Lastname")]
        [ArgHints("state", "true to ban false to unban")]
        public object GroupBan(string group, string avatar, string state)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupBan", new[] { group, avatar, state });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupBan", new[] { group, avatar, state });
            }
            ProcessAvatar(avatar);
            if(avataruuid == null)
            {
                return Failure("Unknown avatar", "GroupBan", new[] { group, avatar, state });
            }
            GroupBanAction Action = GroupBanAction.Ban;
            if (state == "false")
            {
                Action = GroupBanAction.Unban;
            }
            List<UUID> avatarstoaction = new List<UUID>();
            avatarstoaction.Add(avataruuid);
            getClient().Groups.RequestBanAction(groupuuid, Action, avatarstoaction.ToArray());
            return BasicReply("Accepted");
        }



        [About("Eject selected avatar from group")]
        [ReturnHints("Accepted")]
        [ReturnHintsFailure("Updating")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Not in group")]
        [ReturnHintsFailure("avatar lookup")]
        [ReturnHintsFailure("Missing group Eject power")]
        [ArgHints("group", "the UUID of the group")]
        [ArgHints("avatar", "the UUID of the avatar you wish to check with")]
        public object GroupEject(string group, string avatar)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupEject", new [] { group, avatar });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupEject", new[] { group, avatar });
            }
            ProcessAvatar(avatar);
            if (avataruuid == null)
            {
                return Failure("Unknown avatar", "GroupEject", new[] { group, avatar});
            }
            getClient().Groups.EjectUser(groupuuid, avataruuid);
            return BasicReply("Accepted");
        }

        [About("Adds the avatar to the Group with the role \n if they are not in the group then it invites them at that role")]
        [ReturnHints("Invite sent")]
        [ReturnHints("Adding role")]
        [ReturnHintsFailure("Updating")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Invaild role UUID")]
        [ReturnHintsFailure("avatar lookup")]
        [ArgHints("group", "the UUID of the group")]
        [ArgHints("avatar", "the UUID of the avatar you wish to check with")]
        public object GroupAddRole(string group, string avatar, string role)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupAddRole", new [] { group, avatar });
            }
            if (UUID.TryParse(role, out UUID roleuuid) == false)
            {
                return Failure("Invaild role UUID", "GroupAddRole", new [] { group, avatar });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupAddRole", new[] { group, avatar, role });
            }
            ProcessAvatar(avatar);
            if(master.DataStoreService.IsGroupMember(groupuuid, avataruuid) == false)
            {
                getClient().Groups.Invite(groupuuid, new List<UUID>() { UUID.Zero, roleuuid }, avataruuid);
                return BasicReply("Invite sent");
            }
            getClient().Groups.AddToRole(groupuuid, roleuuid, avataruuid);
            return BasicReply("Adding role");
        }

        [About("Invites selected avatar to the group with the selected role")]
        [ReturnHints("Invite sent")]
        [ReturnHints("Already in group")]
        [ReturnHintsFailure("Updating")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("avatar lookup")]
        [ReturnHintsFailure("Missing group Invite power")]
        [ArgHints("group", "the UUID of the group")]
        [ArgHints("avatar", "the UUID of the avatar you wish to check with")]
        [ArgHints("role", "the UUID of the role to invite them at the word \"everyone\"")]
        public object GroupInvite(string group, string avatar, string role)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupInvite", new [] { group, avatar, role });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupInvite", new[] { group, avatar, role });
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", "GroupInvite", new [] { group, avatar, role });
            }
            List<UUID> roles = new List<UUID>();
            roles.Add(UUID.Zero);
            if(role != "everyone")
            {
                UUID roleuuid = UUID.Zero;
                if (UUID.TryParse(role, out roleuuid) == false)
                {
                    return Failure("Unable to process role UUID", "GroupInvite", new[] { group, avatar, role });
                }
                roles.Add(roleuuid);
            }
            getClient().Groups.Invite(groupuuid, roles, avataruuid);
            return BasicReply("Invite sent", "GroupInvite", new [] { group, avatar, role });
        }

        [About("Sends a group notice (No attachments please use GroupnoticeWithAttachment to attach items!)")]
        [ReturnHints("Sending notice")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Title empty")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHintsFailure("Missing group Notice power")]
        [ArgHints("group", "the UUID of the group")]
        [ArgHints("title", "The title of the group notice")]
        [ArgHints("message", "The body of the group notice")]
        public object Groupnotice(string group, string title, string message)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "Groupnotice", new [] { group, title, message });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "Groupnotice", new [] { group, title, message });
            }
            if (SecondbotHelpers.notempty(title) == false)
            {
                return Failure("Title empty", "Groupnotice", new [] { group, title, message });
            }
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("Message empty", "Groupnotice", new [] { group, title, message });
            }
            GroupNotice NewNotice = new GroupNotice();
            NewNotice.Subject = title;
            NewNotice.Message = message;
            NewNotice.OwnerID = getClient().Self.AgentID;
            getClient().Groups.SendGroupNotice(groupuuid, NewNotice);
            return BasicReply("Sending notice", "Groupnotice", new [] { group, title, message });
        }

        [About("Activates the selected title")]
        [ReturnHints("Switching title")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Invaild role UUID")]
        [ArgHints("group", "the UUID of the group")]
        [ArgHints("role", "the UUID of the role")]
        public object GroupActiveTitle(string group, string role)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupActiveTitle", new [] { group, role });
            }
            if (UUID.TryParse(role, out UUID roleuuid) == false)
            {
                return Failure("Invaild role UUID", "GroupActiveTitle", new [] { group, role });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupActiveTitle", new [] { group, role });
            }
            getClient().Groups.ActivateTitle(groupuuid, roleuuid);
            return BasicReply("Switching title", "GroupActiveTitle", new [] { group, role });
        }

        [About("Sets the selected group to the active group")]
        [ReturnHints("Switching active group")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ArgHints("group", "the UUID of the group")]
        public object GroupActiveGroup(string group)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupActiveGroup", new [] { group });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupActiveGroup", new[] { group });
            }
            getClient().Groups.ActivateGroup(groupuuid);
            return BasicReply("Switching active group", "GroupActiveGroup", new [] { group });
        }

        [About("Sends a group notice with an attachment")]
        [ReturnHints("Sending notice with attachment")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Invaild inventory UUID")]
        [ReturnHintsFailure("Title empty")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHintsFailure("Missing group Notice power")]
        [ArgHints("group", "the UUID of the group")]
        [ArgHints("title", "The title of the group notice")]
        [ArgHints("message", "The body of the group notice")]
        [ArgHints("attachment", "the UUID of inventory you wish to attach")]
        public object GroupnoticeWithAttachment(string group, string title, string message, string attachment)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            if (UUID.TryParse(attachment, out UUID inventoryuuid) == false)
            {
                return Failure("Invaild inventory UUID", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            if (SecondbotHelpers.notempty(title) == false)
            {
                return Failure("Title empty", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("Message empty", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            GroupNotice NewNotice = new GroupNotice();
            NewNotice.Subject = title;
            NewNotice.Message = message;
            NewNotice.OwnerID = getClient().Self.AgentID;
            NewNotice.AttachmentID = inventoryuuid;
            getClient().Groups.SendGroupNotice(groupuuid, NewNotice);
            return BasicReply("Sending notice with attachment", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
        }


        [About("fetchs a list of all groups known to the bot")]
        [ReturnHints("array UUID=name")]
        public object GetGroupList()
        {
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetGroups()));
        }

        [About("Requests the roles for the selected group")]
        [ReturnHints("GroupRoleDetails object")]
        [ReturnHintsFailure("Group is not currently known")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Updating")]
        [ArgHints("group", "the UUID of the group")]
        public object GetGroupRoles(string group)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GetGroupRoles", new [] { group });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GetGroupRoles", new [] { group });
            }
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetGroupRoles(groupuuid)));
        }

        [About("fetchs a list of all groups with unread messages")]
        [ReturnHints("array UUID")]
        public object GroupchatListAllUnreadGroups()
        {
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetAllGroupsChatWithUnread()));
        }

        [About("fetchs a list of all groups with unread messages")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("group value is invaild")]
        [ArgHints("group", "the UUID of the group")]
        public object GroupchatGroupHasUnread(string group)
        {
            UUID groupUUID = UUID.Zero;
            if(UUID.TryParse(group,out groupUUID) == false)
            {
                return Failure("group value is invaild", "GroupchatGroupHasUnread", new [] { group });
            }
            if(getClient().Groups.GroupName2KeyCache.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", "GroupchatGroupHasUnread", new [] { group });
            }
            return BasicReply(master.DataStoreService.GetGroupChatHasUnread(groupUUID).ToString());
        }


        [About("checks if there are any groups with unread messages")]
        [ReturnHints("True|False")]
        public object GroupchatAnyUnread()
        {
            return BasicReply(master.DataStoreService.GetGroupChatHasAnyUnread().ToString());
        }

        [About("Clears all group chat buffers at once")]
        [ReturnHints("ok")]
        public object GroupchatClearAll()
        {
            master.DataStoreService.clearGroupChat();
            return BasicReply("ok");
        }

        [About("fetchs the groupchat history")]
        [ReturnHintsFailure("Group UUID invaild")]
        [ReturnHints("Group Chat")]
        [ArgHints("group", "the UUID of the group")]
        public object GroupchatHistory(string group)
        {
            if (UUID.TryParse(group, out UUID groupUUID) == false)
            {
                return Failure("Group UUID invaild", "GroupchatHistory", new [] { group });
            }
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetGroupChatHistory(groupUUID)));
        }

        [About("sends a message to the groupchat")]
        [ArgHints("group", "UUID of the group")]
        [ArgHints("message", "the message to send")]
        [ReturnHints("Sending")]
        [ReturnHintsFailure("Group UUID invaild")]
        [ReturnHintsFailure("Unknown group")]
        public object Groupchat(string group, string message)
        {
            UUID groupUUID = UUID.Zero;
            if (UUID.TryParse(group, out groupUUID) == false)
            {
                return Failure("group value is invaild", "Groupchat", new [] { group, message });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", "Groupchat", new [] { group, message });
            }
            if (getClient().Groups.GroupName2KeyCache.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", "Groupchat", new[] { group });
            }
            getClient().Self.InstantMessageGroup(groupUUID, message);
            return BasicReply("Sending");
        }
    }
}
