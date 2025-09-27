using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Services;
using System.Collections.Generic;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Control groups with ease")]
    public class GroupCommands(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Checks if the given UUID is in the given group<br/>Note: if group membership data is more than 60 secs old this will return Updating<br/>Please wait and retry later")]
        [ReturnHints("Membership reply with [membershipStatus,AvatarUUID,AvatarnameIfKnown,GroupUUID]")]
        [ReturnHintsFailure("Updating")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("avatar lookup")]
        [ArgHints("group", "What group to check","UUID")]
        [ArgHints("avatar", "The avatar in check","AVATAR")]
        [CmdTypeGet()]
        public object IsGroupMember(string group, string avatar)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group, avatar]);
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
        [ArgHints("group", "the group to get members list from","UUID")]
        [CmdTypeGet()]
        public object GetGroupMembers(string group)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group]);
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
        [ArgHints("group", "the group to apply the ban in","UUID")]
        [ArgHints("avatar", "the avatar to ban/unban","AVATAR")]
        [ArgHints("state", "true to ban false to unban","BOOL")]
        [CmdTypeDo()]
        public object GroupBan(string group, string avatar, string state)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group, avatar, state]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", [group, avatar, state]);
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Unknown avatar", [group, avatar, state]);
            }
            GroupBanAction Action = GroupBanAction.Ban;
            if (state == "false")
            {
                Action = GroupBanAction.Unban;
            }
            GetClient().Groups.EjectUser(groupuuid, avataruuid); //Eject first
            GetClient().Groups.RequestBanAction(groupuuid, Action, [avataruuid]);
            return BasicReply("Accepted "+ Action.ToString()+" for "+avataruuid);
        }



        [About("Eject selected avatar from group")]
        [ReturnHints("Accepted")]
        [ReturnHintsFailure("Updating")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Not in group")]
        [ReturnHintsFailure("avatar lookup")]
        [ReturnHintsFailure("Missing group Eject power")]
        [ArgHints("group", "the group to eject from", "UUID")]
        [ArgHints("avatar", "the avatar to remove from the group", "UUID")]
        [CmdTypeDo()]
        public object GroupEject(string group, string avatar)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group, avatar]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", [group, avatar]);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Unknown avatar", [group, avatar]);
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
        [ArgHints("group", "the group to give the avatar the new role for","UUID")]
        [ArgHints("avatar", "the avatar to give the role to", "AVATAR")]
        [ArgHints("role", "the role to give the avatar in the group", "UUID")]
        [CmdTypeSet()]
        public object GroupAddRole(string group, string avatar, string role)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group, avatar]);
            }
            if (UUID.TryParse(role, out UUID roleuuid) == false)
            {
                return Failure("Invaild role UUID", [group, avatar]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", [group, avatar, role]);
            }
            ProcessAvatar(avatar);
            if(master.DataStoreService.IsGroupMember(groupuuid, avataruuid) == false)
            {
                GetClient().Groups.Invite(groupuuid, [UUID.Zero, roleuuid], avataruuid);
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
        [ArgHints("group", "the group to invite to","UUID")]
        [ArgHints("avatar", "the avatar you wish to invite","AVATAR")]
        [ArgHints("role", "the role to invite them with or \"everyone\" to give the default role","UUID","everyone")]
        [CmdTypeDo()]
        public object GroupInvite(string group, string avatar, string role)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group, avatar, role]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", [group, avatar, role]);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", [group, avatar, role]);
            }
            List<UUID> roles =
            [
                UUID.Zero
            ];
            if (role != "everyone")
            {
                if (UUID.TryParse(role, out UUID roleuuid) == false)
                {
                    return Failure("Unable to process role UUID", [group, avatar, role]);
                }
                roles.Add(roleuuid);
            }
            GetClient().Groups.Invite(groupuuid, roles, avataruuid);
            return BasicReply("Invite sent", [group, avatar, role]);
        }

        [About("Sends a group notice (No attachments please use GroupnoticeWithAttachment to attach items!)")]
        [ReturnHints("Sending notice")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Title empty")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHintsFailure("Missing group Notice power")]
        [ArgHints("group", "the group to send the notice to", "UUID")]
        [ArgHints("title", "The title of the group notice", "Text", "New releases")]
        [ArgHints("message", "The body of the group notice", "Text", "We have 4 new items in store with 5% off for the first hour!")]
        [CmdTypeDo()]
        public object Groupnotice(string group, string title, string message)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group, title, message]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", [group, title, message]);
            }
            if (SecondbotHelpers.notempty(title) == false)
            {
                return Failure("Title empty", [group, title, message]);
            }
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("Message empty", [group, title, message]);
            }
            GroupNotice NewNotice = new()
            {
                Subject = title,
                Message = message,
                OwnerID = GetClient().Self.AgentID
            };
            GetClient().Groups.SendGroupNotice(groupuuid, NewNotice);
            return BasicReply("Sending notice", [group, title, message]);
        }

        [About("Activates the selected title")]
        [ReturnHints("Switching title")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Invaild role UUID")]
        [ArgHints("group", "the group to get the role from", "UUID")]
        [ArgHints("role", "the role to fetch the title from", "UUID")]
        [CmdTypeDo()]
        public object GroupActiveTitle(string group, string role)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group, role]);
            }
            if (UUID.TryParse(role, out UUID roleuuid) == false)
            {
                return Failure("Invaild role UUID", [group, role]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", [group, role]);
            }
            GetClient().Groups.ActivateTitle(groupuuid, roleuuid);
            return BasicReply("Switching title", [group, role]);
        }

        [About("Sets the selected group to the active group")]
        [ReturnHints("Switching active group")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ArgHints("group", "the group to set as active", "UUID")]
        [CmdTypeGet()]
        public object GroupActiveGroup(string group)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", [group]);
            }
            GetClient().Groups.ActivateGroup(groupuuid);
            return BasicReply("Switching active group", [group]);
        }

        [About("Sends a group notice with an attachment")]
        [ReturnHints("Sending notice with attachment")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Invaild inventory UUID")]
        [ReturnHintsFailure("Title empty")]
        [ReturnHintsFailure("Message empty")]
        [ReturnHintsFailure("Missing group Notice power")]
        [ArgHints("group", "the group to send the notice to", "UUID")]
        [ArgHints("title", "The title of the group notice","Text","Update to terms for rentals")]
        [ArgHints("message", "The body of the group notice","Text", "Hi everyone there has been a TOS update please check and review")]
        [ArgHints("attachment", "the inventory to send", "UUID")]
        [CmdTypeDo()]
        public object GroupnoticeWithAttachment(string group, string title, string message, string attachment)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group, title, message, attachment]);
            }
            if (UUID.TryParse(attachment, out UUID inventoryuuid) == false)
            {
                return Failure("Invaild inventory UUID", [group, title, message, attachment]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", [group, title, message, attachment]);
            }
            if (SecondbotHelpers.notempty(title) == false)
            {
                return Failure("Title empty", [group, title, message, attachment]);
            }
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("Message empty", [group, title, message, attachment]);
            }
            GroupNotice NewNotice = new()
            {
                Subject = title,
                Message = message,
                OwnerID = GetClient().Self.AgentID,
                AttachmentID = inventoryuuid
            };
            GetClient().Groups.SendGroupNotice(groupuuid, NewNotice);
            return BasicReply("Sending notice with attachment", [group, title, message, attachment]);
        }


        [About("fetchs a list of all groups known to the bot")]
        [ReturnHints("array UUID=name")]
        [CmdTypeGet()]
        public object GetGroupList()
        {
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetGroups()));
        }

        [About("Forces the bot to reload the groups list")]
        [ReturnHints("Working on it")]
        [CmdTypeDo()]
        public object ForceLoadGroups()
        {
            master.BotClient.client.Groups.RequestCurrentGroups();
            return BasicReply("Working on it");
        }

        [About("Requests the roles for the selected group")]
        [ReturnHints("GroupRoleDetails object")]
        [ReturnHintsFailure("Group is not currently known")]
        [ReturnHintsFailure("Invaild group UUID")]
        [ReturnHintsFailure("Updating")]
        [ArgHints("group", "the group to get roles from", "UUID")]
        [CmdTypeGet()]
        public object GetGroupRoles(string group)
        {
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", [group]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", [group]);
            }
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetGroupRoles(groupuuid)));
        }

        [About("fetchs a list of all groups with unread messages")]
        [ReturnHints("array UUID")]
        [CmdTypeGet()]
        public object GroupchatListAllUnreadGroups()
        {
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetAllGroupsChatWithUnread()));
        }

        [About("checks if a given group has any unread messages")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Unknown group")]
        [ReturnHintsFailure("group value is invaild")]
        [ArgHints("group", "the group to check if it has any unread messages", "UUID")]
        [CmdTypeGet()]
        public object GroupchatGroupHasUnread(string group)
        {
            if(UUID.TryParse(group,out UUID groupUUID) == false)
            {
                return Failure("group value is invaild", [group]);
            }
            if(GetClient().Groups.GroupName2KeyCache.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", [group]);
            }
            return BasicReply(master.DataStoreService.GetGroupChatHasUnread(groupUUID).ToString());
        }


        [About("checks if there are any groups with unread messages")]
        [ReturnHints("True|False")]
        [CmdTypeGet()]
        public object GroupchatAnyUnread()
        {
            return BasicReply(master.DataStoreService.GetGroupChatHasAnyUnread().ToString());
        }

        [About("Clears all group chat buffers at once")]
        [ReturnHints("ok")]
        [CmdTypeSet()]
        public object GroupchatClearAll()
        {
            master.DataStoreService.ClearGroupChat();
            return BasicReply("ok");
        }

        [About("fetchs the groupchat history")]
        [ReturnHintsFailure("Group UUID invaild")]
        [ReturnHints("Group Chat")]
        [ArgHints("group", "what group to get the chat history of","UUID")]
        [CmdTypeGet()]
        public object GroupchatHistory(string group)
        {
            if (UUID.TryParse(group, out UUID groupUUID) == false)
            {
                return Failure("Group UUID invaild", [group]);
            }
            return BasicReply(JsonConvert.SerializeObject(master.DataStoreService.GetGroupChatHistory(groupUUID)));
        }

        [About("sends a message to the groupchat")]
        [ArgHints("group", "what group to send a message to","UUID")]
        [ArgHints("message", "the message to send","Text","Hello everyone")]
        [ReturnHints("Sending")]
        [ReturnHintsFailure("Group UUID invaild")]
        [ReturnHintsFailure("Unknown group")]
        [CmdTypeDo()]
        public object Groupchat(string group, string message)
        {
            if (UUID.TryParse(group, out UUID groupUUID) == false)
            {
                return Failure("group value is invaild", [group, message]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", [group, message]);
            }
            if (GetClient().Groups.GroupName2KeyCache.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", [group]);
            }
            GetClient().Self.InstantMessageGroup(groupUUID, message);
            master.DataStoreService.BotRecordReplyIM(groupUUID, message);
            return BasicReply("Sending");
        }
    }
}
