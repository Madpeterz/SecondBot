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
                return Failure("Invaild group UUID", new [] { group, avatar });
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
                return Failure("Invaild group UUID", new [] { group });
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
                return Failure("Invaild group UUID", new[] { group, avatar, state });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", new[] { group, avatar, state });
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Unknown avatar", new[] { group, avatar, state });
            }
            GroupBanAction Action = GroupBanAction.Ban;
            if (state == "false")
            {
                Action = GroupBanAction.Unban;
            }
            List<UUID> avatarstoaction = new()
            {
                avataruuid
            };
            GetClient().Groups.RequestBanAction(groupuuid, Action, avatarstoaction.ToArray());
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
                return Failure("Invaild group UUID", new [] { group, avatar });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", new[] { group, avatar });
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Unknown avatar", new[] { group, avatar});
            }
            GetClient().Groups.EjectUser(groupuuid, avataruuid);
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
                return Failure("Invaild group UUID", new [] { group, avatar });
            }
            if (UUID.TryParse(role, out UUID roleuuid) == false)
            {
                return Failure("Invaild role UUID", new [] { group, avatar });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", new[] { group, avatar, role });
            }
            ProcessAvatar(avatar);
            if(master.DataStoreService.IsGroupMember(groupuuid, avataruuid) == false)
            {
                GetClient().Groups.Invite(groupuuid, new List<UUID>() { UUID.Zero, roleuuid }, avataruuid);
                return BasicReply("Invite sent");
            }
            GetClient().Groups.AddToRole(groupuuid, roleuuid, avataruuid);
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
                return Failure("Invaild group UUID", new [] { group, avatar, role });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", new[] { group, avatar, role });
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", new [] { group, avatar, role });
            }
            List<UUID> roles = new()
            {
                UUID.Zero
            };
            if (role != "everyone")
            {
                if (UUID.TryParse(role, out UUID roleuuid) == false)
                {
                    return Failure("Unable to process role UUID", new[] { group, avatar, role });
                }
                roles.Add(roleuuid);
            }
            GetClient().Groups.Invite(groupuuid, roles, avataruuid);
            return BasicReply("Invite sent", new [] { group, avatar, role });
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
                return Failure("Invaild group UUID", new [] { group, title, message });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", new [] { group, title, message });
            }
            if (SecondbotHelpers.notempty(title) == false)
            {
                return Failure("Title empty", new [] { group, title, message });
            }
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("Message empty", new [] { group, title, message });
            }
            GroupNotice NewNotice = new()
            {
                Subject = title,
                Message = message,
                OwnerID = GetClient().Self.AgentID
            };
            GetClient().Groups.SendGroupNotice(groupuuid, NewNotice);
            return BasicReply("Sending notice", new [] { group, title, message });
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
                return Failure("Invaild group UUID", new [] { group, role });
            }
            if (UUID.TryParse(role, out UUID roleuuid) == false)
            {
                return Failure("Invaild role UUID", new [] { group, role });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", new [] { group, role });
            }
            GetClient().Groups.ActivateTitle(groupuuid, roleuuid);
            return BasicReply("Switching title", new [] { group, role });
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
                return Failure("Invaild group UUID", new [] { group });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", new[] { group });
            }
            GetClient().Groups.ActivateGroup(groupuuid);
            return BasicReply("Switching active group", new [] { group });
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
                return Failure("Invaild group UUID", new [] { group, title, message, attachment });
            }
            if (UUID.TryParse(attachment, out UUID inventoryuuid) == false)
            {
                return Failure("Invaild inventory UUID", new [] { group, title, message, attachment });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", new [] { group, title, message, attachment });
            }
            if (SecondbotHelpers.notempty(title) == false)
            {
                return Failure("Title empty", new [] { group, title, message, attachment });
            }
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("Message empty", new [] { group, title, message, attachment });
            }
            GroupNotice NewNotice = new()
            {
                Subject = title,
                Message = message,
                OwnerID = GetClient().Self.AgentID,
                AttachmentID = inventoryuuid
            };
            GetClient().Groups.SendGroupNotice(groupuuid, NewNotice);
            return BasicReply("Sending notice with attachment", new [] { group, title, message, attachment });
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
                return Failure("Invaild group UUID", new [] { group });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", new [] { group });
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
            if(UUID.TryParse(group,out UUID groupUUID) == false)
            {
                return Failure("group value is invaild", new [] { group });
            }
            if(GetClient().Groups.GroupName2KeyCache.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", new [] { group });
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
            master.DataStoreService.ClearGroupChat();
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
                return Failure("Group UUID invaild", new [] { group });
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
            if (UUID.TryParse(group, out UUID groupUUID) == false)
            {
                return Failure("group value is invaild", new [] { group, message });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", new [] { group, message });
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", new[] { group });
            }
            GetClient().Self.InstantMessageGroup(groupUUID, message);
            return BasicReply("Sending");
        }
    }
}
