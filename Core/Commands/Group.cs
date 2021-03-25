using BetterSecondBot.bottypes;
using BetterSecondBot.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Newtonsoft.Json;
using BetterSecondBotShared.Static;
using System.Threading;

namespace BetterSecondBot.HttpService
{
    public class HTTP_Group : WebApiControllerWithTokens
    {
        public HTTP_Group(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }


        [About("Checks if the given UUID is in the given group<br/>Note: if group membership data is more than 60 secs old this will return Updating<br/>Please wait and retry later")]
        [ReturnHints("Membership reply with [membershipStatus,AvatarUUID,AvatarnameIfKnown,GroupUUID]")]
        [ReturnHints("Updating")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("avatar lookup")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("avatar", "URLARG", "the UUID of the avatar you wish to check with")]
        [Route(HttpVerbs.Get, "/IsGroupMember/{group}/{avatar}/{token}")]
        public object IsGroupMember(string group, string avatar, string token)
        {
            if (tokens.Allow(token, "groups", "IsGroupMember", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "IsGroupMember", new [] { group, avatar });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "IsGroupMember", new [] { group, avatar });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "IsGroupMember", new [] { group, avatar });
            }
            if (bot.NeedReloadGroupData(groupuuid) == true)
            {
                bot.GetClient.Groups.RequestGroupMembers(groupuuid);
            }
            ProcessAvatar(avatar);
            KeyValuePair<bool, string> reply = waitForReady(true, true, groupuuid);
            if (reply.Key == false)
            {
                return Failure(reply.Value, "IsGroupMember", new [] { group, avatar });
            }
            IsGroupMemberReply replyobject = new IsGroupMemberReply();
            replyobject.membershipStatus = bot.FastCheckInGroup(groupuuid, avataruuid);
            replyobject.AvatarUUID = avataruuid.ToString();
            replyobject.AvatarnameIfKnown = bot.FindAvatarKey2Name(avataruuid);
            replyobject.GroupUUID = groupuuid.ToString();
            return BasicReply(JsonConvert.SerializeObject(replyobject), "IsGroupMember", new [] { group, avatar });
        }

        [About("Gets membership of a group")]
        [ReturnHints("list of UUIDS of group members")]
        [ReturnHints("Updating")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("avatar lookup")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [Route(HttpVerbs.Get, "/GetGroupMembers/{group}/{token}")]
        public object GetGroupMembers(string group, string token)
        {
            if (tokens.Allow(token, "groups", "GetGroupMembers", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GetGroupMembers", new [] { group });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GetGroupMembers", new [] { group });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GetGroupMembers", new [] { group });
            }
            if (bot.NeedReloadGroupData(groupuuid) == true)
            {
                bot.GetClient.Groups.RequestGroupMembers(groupuuid);
            }
            KeyValuePair<bool, string> reply = waitForReady(false, true, groupuuid);
            if (reply.Key == false)
            {
                return Failure(reply.Value, "GetGroupMembers", new [] { group });
            }
            SuccessNoReturn("GetGroupMembers", new [] { group });
            return bot.GetGroupMembers(groupuuid);
        }

        [About("Attempts to ban/unban a given avatar from a group")]
        [ReturnHints("? request accepted")]
        [ReturnHints("Updating")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("avatar lookup")]
        [ReturnHints("Missing group GroupBanAccess power")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("avatar", "URLARG", "the UUID of the avatar or Firstname Lastname")]
        [ArgHints("state", "URLARG", "true to ban false to unban")]
        [Route(HttpVerbs.Get, "/GroupBan/{group}/{avatar}/{state}/{token}")]
        public object GroupBan(string group, string avatar, string state, string token)
        {
            if (tokens.Allow(token, "groups", "GroupBan", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupBan", new [] { group,avatar,state });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupBan", new [] { group, avatar, state });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupBan", new [] { group, avatar, state });
            }
            Group G = bot.MyGroups[groupuuid];
            if (G.Powers.HasFlag(GroupPowers.GroupBanAccess) == false)
            {
                return Failure("Missing group GroupBanAccess power", "GroupBan", new [] { group, avatar, state });
            }
            if (bool.TryParse(state, out bool banstate) == false)
            {
                return Failure("Invaild ban state", "GroupBan", new [] { group, avatar, state });
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", "GroupBan", new [] { group, avatar, state });
            }
            GroupBanAction action = GroupBanAction.Unban;
            string statename = "Unban";
            if (banstate == true)
            {
                action = GroupBanAction.Ban;
                statename = "Ban";
            }
            bot.GetClient.Groups.RequestBanAction(groupuuid, action, new UUID[] { avataruuid });
            return BasicReply(statename+" request accepted", "GroupBan", new [] { group, avatar, state });
        }



        [About("Eject selected avatar from group")]
        [ReturnHints("Requested")]
        [ReturnHints("Updating")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Not in group")]
        [ReturnHints("avatar lookup")]
        [ReturnHints("Missing group Eject power")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("avatar", "URLARG", "the UUID of the avatar you wish to check with")]
        [Route(HttpVerbs.Get, "/GroupEject/{group}/{avatar}/{token}")]
        public object GroupEject(string group, string avatar, string token)
        {
            if (tokens.Allow(token, "groups", "GroupEject", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupEject", new [] { group, avatar });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupEject", new [] { group, avatar });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupEject", new [] { group, avatar });
            }
            Group G = bot.MyGroups[groupuuid];
            if (G.Powers.HasFlag(GroupPowers.Eject) == false)
            {
                return Failure("Missing group Eject power", "GroupEject", new [] { group, avatar });
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", "GroupEject", new [] { group, avatar });
            }
            bot.GetClient.Groups.EjectUser(groupuuid, avataruuid);
            return BasicReply("Requested", "GroupEject", new [] { group, avatar });
        }

        [About("Adds the avatar to the Group with the role \n if they are not in the group then it invites them aswell")]
        [ReturnHints("Roles updated")]
        [ReturnHints("Invite sent")]
        [ReturnHints("Updating")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Invaild role UUID")]
        [ReturnHints("Not in group")]
        [ReturnHints("avatar lookup")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("avatar", "URLARG", "the UUID of the avatar you wish to check with")]
        [Route(HttpVerbs.Get, "/GroupAddRole/{group}/{avatar}/{role}/{token}")]
        public object GroupAddRole(string group, string avatar, string role, string token)
        {
            if (tokens.Allow(token, "groups", "GroupAddRole", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupAddRole", new [] { group, avatar });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupAddRole", new [] { group, avatar });
            }
            if (UUID.TryParse(role, out UUID roleuuid) == false)
            {
                return Failure("Invaild role UUID", "GroupAddRole", new [] { group, avatar });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupAddRole", new [] { group, avatar });
            }
            if (bot.NeedReloadGroupData(groupuuid) == true)
            {
                bot.GetClient.Groups.RequestGroupMembers(groupuuid);
            }
            ProcessAvatar(avatar);
            KeyValuePair<bool, string> reply = waitForReady(true, true, groupuuid);
            if (reply.Key == false)
            {
                return Failure(reply.Value, "GroupAddRole", new [] { group, avatar });
            }
            bool status = bot.FastCheckInGroup(groupuuid, avataruuid);
            if (status == false)
            {
                bot.GetClient.Groups.Invite(groupuuid, new List<UUID>() { roleuuid }, avataruuid);
                return BasicReply("Invite sent", "GroupAddRole", new [] { group, avatar });
            }
            bot.GetClient.Groups.AddToRole(groupuuid, roleuuid, avataruuid);
            return BasicReply("Roles updated", "GroupAddRole", new [] { group, avatar });
        }

        [About("Eject selected avatar from group")]
        [ReturnHints("Invite sent")]
        [ReturnHints("Updating")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Already in group")]
        [ReturnHints("avatar lookup")]
        [ReturnHints("Missing group Invite power")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("avatar", "URLARG", "the UUID of the avatar you wish to check with")]
        [ArgHints("role", "URLARG", "the UUID of the role to invite them at the word \"everyone\"")]
        [Route(HttpVerbs.Get, "/GroupInvite/{group}/{avatar}/{role}/{token}")]
        public object GroupInvite(string group, string avatar, string role, string token)
        {
            if (tokens.Allow(token, "groups", "GroupInvite", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupInvite", new [] { group, avatar, role });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupInvite", new [] { group, avatar, role });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupInvite", new [] { group, avatar, role });
            }
            if (bot.NeedReloadGroupData(groupuuid) == true)
            {
                bot.GetClient.Groups.RequestGroupMembers(groupuuid);
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", "GroupInvite", new [] { group, avatar, role });
            }
            bool status = bot.FastCheckInGroup(groupuuid, avataruuid);
            if (status == true)
            {
                return Failure("Already in group", "GroupInvite", new [] { group, avatar, role });
            }
            if(role == "everyone")
            {
                role = UUID.Zero.ToString();
            }
            if(UUID.TryParse(role,out UUID roleuuid) == false)
            {
                return Failure("Unable to process role UUID", "GroupInvite", new [] { group, avatar, role });
            }
            Group G = bot.MyGroups[groupuuid];
            if(G.Powers.HasFlag(GroupPowers.Invite) == false)
            {
                return Failure("Missing group invite power", "GroupInvite", new [] { group, avatar, role });
            }
            bot.GetClient.Groups.Invite(groupuuid, new List<UUID>() { roleuuid }, avataruuid);
            return BasicReply("Invite sent", "GroupInvite", new [] { group, avatar, role });
        }

        [About("Sends a group notice (No attachments please use GroupnoticeWithAttachment to attach items!)")]
        [ReturnHints("Sending notice")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Title empty")]
        [ReturnHints("Message empty")]
        [ReturnHints("Missing group Notice power")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("title", "Text", "The title of the group notice")]
        [ArgHints("message", "Text", "The body of the group notice")]
        [Route(HttpVerbs.Post, "/Groupnotice/{group}/{token}")]
        public object Groupnotice(string group, [FormField] string title, [FormField] string message, string token)
        {
            if (tokens.Allow(token, "groups", "Groupnotice", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Groupnotice", new [] { group, title, message });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "Groupnotice", new [] { group, title, message });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "Groupnotice", new [] { group, title, message });
            }
            if (helpers.notempty(title) == false)
            {
                return Failure("Title empty", "Groupnotice", new [] { group, title, message });
            }
            if (helpers.notempty(message) == false)
            {
                return Failure("Message empty", "Groupnotice", new [] { group, title, message });
            }
            Group G = bot.MyGroups[groupuuid];
            if (G.Powers.HasFlag(GroupPowers.SendNotices) == false)
            {
                return Failure("Missing group notice power", "Groupnotice", new [] { group, title, message });
            }
            GroupNotice NewNotice = new GroupNotice();
            NewNotice.Subject = title;
            NewNotice.Message = message;
            NewNotice.OwnerID = bot.GetClient.Self.AgentID;
            bot.GetClient.Groups.SendGroupNotice(groupuuid, NewNotice);
            return BasicReply("Sending notice", "Groupnotice", new [] { group, title, message });
        }

        [About("Activates the selected title")]
        [ReturnHints("Switching title")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Invaild role UUID")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("role", "URLARG", "tje UUID of the role")]
        [Route(HttpVerbs.Get, "/GroupActiveTitle/{group}/{token}")]
        public object GroupActiveTitle(string group, string role, string token)
        {
            if (tokens.Allow(token, "groups", "GroupActiveTitle", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupActiveTitle", new [] { group, role });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupActiveTitle", new [] { group, role });
            }
            if (UUID.TryParse(role, out UUID roleuuid) == false)
            {
                return Failure("Invaild role UUID", "GroupActiveTitle", new [] { group, role });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupActiveTitle", new [] { group, role });
            }
            bot.GetClient.Groups.ActivateTitle(groupuuid, roleuuid);
            return BasicReply("Switching title", "GroupActiveTitle", new [] { group, role });
        }

        [About("Sets the selected group to the active group")]
        [ReturnHints("Switching active group")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [Route(HttpVerbs.Get, "/GroupActiveGroup/{group}/{token}")]
        public object GroupActiveGroup(string group, string token)
        {
            if (tokens.Allow(token, "groups", "GroupActiveGroup", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupActiveGroup", new [] { group });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupActiveGroup", new [] { group });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupActiveGroup", new [] { group });
            }
            bot.GetClient.Groups.ActivateGroup(groupuuid);
            return BasicReply("Switching active group", "GroupActiveGroup", new [] { group });
        }

        [About("Sends a group notice with an attachment")]
        [ReturnHints("Sending notice with attachment")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Invaild inventory UUID")]
        [ReturnHints("Title empty")]
        [ReturnHints("Message empty")]
        [ReturnHints("Missing group Notice power")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("title", "Text", "The title of the group notice")]
        [ArgHints("message", "Text", "The body of the group notice")]
        [ArgHints("attachment", "URLARG", "the UUID of inventory you wish to attach")]
        [Route(HttpVerbs.Post, "/Groupnotice/{group}/{attachment}/{token}")]
        public object GroupnoticeWithAttachment(string group, [FormField] string title, [FormField] string message, string attachment, string token)
        {
            if (tokens.Allow(token, "groups", "GroupnoticeWithAttachment", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            if (UUID.TryParse(attachment, out UUID inventoryuuid) == false)
            {
                return Failure("Invaild inventory UUID", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            if (helpers.notempty(title) == false)
            {
                return Failure("Title empty", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            if (helpers.notempty(message) == false)
            {
                return Failure("Message empty", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            Group G = bot.MyGroups[groupuuid];
            if (G.Powers.HasFlag(GroupPowers.SendNotices) == false)
            {
                return Failure("Missing group notice power", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
            }
            GroupNotice NewNotice = new GroupNotice();
            NewNotice.Subject = title;
            NewNotice.Message = message;
            NewNotice.OwnerID = bot.GetClient.Self.AgentID;
            NewNotice.AttachmentID = inventoryuuid;
            bot.GetClient.Groups.SendGroupNotice(groupuuid, NewNotice);
            return BasicReply("Sending notice with attachment", "GroupnoticeWithAttachment", new [] { group, title, message, attachment });
        }


        [About("fetchs a list of all groups known to the bot")]
        [ReturnHints("array UUID=name")]
        [Route(HttpVerbs.Get, "/GetGroupList/{token}")]
        public object GetGroupList(string token)
        {
            if (tokens.Allow(token, "groups", "GetGroupList", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GetGroupList");
            }
            Dictionary<string, string> grouppackage = new Dictionary<string, string>();
            foreach (KeyValuePair<UUID, Group> entry in bot.MyGroups)
            {
                grouppackage.Add(entry.Value.ID.ToString(), entry.Value.Name);
            }
            return BasicReply(JsonConvert.SerializeObject(grouppackage), "GetGroupList");
        }

        [About("Requests the roles for the selected group<br/>Replys with GroupRoleDetails object formated as follows <ul><li>UpdateUnderway (Bool)</li><li>RoleDataAge (Int) [default -1]</li><li>Roles (KeyPair array of UUID=Name)</li></ul><br/>")]
        [ReturnHints("GroupRoleDetails object")]
        [ReturnHints("Group is not currently known")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Updating")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [Route(HttpVerbs.Get, "/GetGroupRoles/{group}/{token}")]
        public object GetGroupRoles(string group, string token)
        {
            if (tokens.Allow(token, "groups", "GetGroupRoles", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GetGroupRoles", new [] { group });
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID", "GetGroupRoles", new [] { group });
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group", "GetGroupRoles", new [] { group });
            }
            GroupRoleDetails reply = new GroupRoleDetails();
            reply.UpdateUnderway = false;
            reply.RoleDataAge = -1;
            reply.Roles = new Dictionary<string, string>();
            if (bot.MyGroupRolesStorage.ContainsKey(groupuuid) == false)
            {
                reply.UpdateUnderway = true;
            }
            else
            {
                long dif = helpers.UnixTimeNow() - bot.MyGroupRolesStorage[groupuuid].Key;
                reply.RoleDataAge = dif;
                if (dif >= 120)
                {
                    reply.UpdateUnderway = true;
                }
                foreach (GroupRole gr in bot.MyGroupRolesStorage[groupuuid].Value)
                {
                    reply.Roles.Add(gr.ID.ToString(), gr.Name);
                }
            }
            if (reply.UpdateUnderway == true)
            {
                bot.GetClient.Groups.RequestGroupRoles(groupuuid);
                if (reply.Roles.Count == 0)
                {
                    return Failure("Updating", "GetGroupRoles", new [] { group });
                }
            }
            return BasicReply(JsonConvert.SerializeObject(reply), "GetGroupRoles", new [] { group });
        }

        [About("fetchs a list of all groups with unread messages")]
        [ReturnHints("array UUID")]
        [Route(HttpVerbs.Get, "/GroupchatListAllUnreadGroups/{token}")]
        public object GroupchatListAllUnreadGroups(string token)
        {
            if (tokens.Allow(token, "groups", "GroupchatListAllUnreadGroups", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupchatListAllUnreadGroups");
            }
            return BasicReply(JsonConvert.SerializeObject(bot.UnreadGroupchatGroups()), "GroupchatListAllUnreadGroups");
        }

        [About("fetchs a list of all groups with unread messages")]
        [ReturnHints("true")]
        [ReturnHints("false")]
        [ReturnHints("Unknown group")]
        [ReturnHints("group value is invaild")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [Route(HttpVerbs.Get, "/GroupchatGroupHasUnread/{group}/{token}")]
        public object GroupchatGroupHasUnread(string group,string token)
        {
            if (tokens.Allow(token, "groups", "GroupchatGroupHasUnread", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupchatGroupHasUnread", new [] { group });
            }
            UUID groupUUID = UUID.Zero;
            if(UUID.TryParse(group,out groupUUID) == false)
            {
                return Failure("group value is invaild", "GroupchatGroupHasUnread", new [] { group });
            }
            if(bot.MyGroups.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", "GroupchatGroupHasUnread", new [] { group });
            }
            return BasicReply(bot.GroupHasUnread(groupUUID).ToString(), "GroupchatGroupHasUnread", new [] { group });
        }


        [About("checks if there are any groups with unread messages")]
        [ReturnHints("True|False")]
        [Route(HttpVerbs.Get, "/GroupchatAnyUnread/{token}")]
        public object GroupchatAnyUnread(string token)
        {
            if (tokens.Allow(token, "groups", "GroupchatAnyUnread", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupchatAnyUnread");
            }
            return BasicReply(bot.HasUnreadGroupchats().ToString(), "GroupchatAnyUnread");
        }

        [About("Clears all group chat buffers at once")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/GroupchatClearAll/{token}")]
        public object GroupchatClearAll(string token)
        {
            if (tokens.Allow(token, "groups", "GroupchatClearAll", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupchatClearAll");
            }
            bot.ClearAllGroupchat();
            return BasicReply("ok", "GroupchatClearAll");
        }

        [About("fetchs the groupchat history")]
        [ReturnHints("Group UUID invaild")]
        [ReturnHints("Group Chat")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [Route(HttpVerbs.Get, "/GroupchatHistory/{group}/{token}")]
        public object GroupchatHistory(string group, string token)
        {
            if (tokens.Allow(token, "groups", "GroupchatHistory", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GroupchatHistory", new [] { group });
            }
            if (UUID.TryParse(group, out UUID groupUUID) == false)
            {
                return Failure("Group UUID invaild", "GroupchatHistory", new [] { group });
            }
            return BasicReply(JsonConvert.SerializeObject(bot.GetGroupchat(groupUUID)), "GroupchatHistory", new [] { group });
        }

        [About("sends a message to the groupchat")]
        [ArgHints("group", "URLARG", "UUID of the group")]
        [ArgHints("message", "Text", "the message to send")]
        [ReturnHints("Group UUID invaild")]
        [ReturnHints("Processing")]
        [ReturnHints("Opening groupchat")]
        [ReturnHints("Sending")]
        [ReturnHints("Missing group JoinChat power")]
        [Route(HttpVerbs.Post, "/Groupchat/{group}/{token}")]
        public object Groupchat(string group, [FormField] string message, string token)
        {
            if (tokens.Allow(token, "groups", "Groupchat", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Groupchat", new [] { group, message });
            }
            UUID groupUUID = UUID.Zero;
            if (UUID.TryParse(group, out groupUUID) == false)
            {
                return Failure("group value is invaild", "Groupchat", new [] { group, message });
            }
            if (bot.MyGroups.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group", "Groupchat", new [] { group, message });
            }
            Group G = bot.MyGroups[groupUUID];
            if (G.Powers.HasFlag(GroupPowers.JoinChat) == false)
            {
                return Failure("Missing group JoinChat power", "Groupchat", new [] { group, message });
            }
            if (bot.GetActiveGroupchatSessions.Contains(groupUUID) == false)
            {
                bot.GetClient.Self.RequestJoinGroupChat(groupUUID);
                return Failure("Opening groupchat", "Groupchat", new [] { group, message });
            }
            bot.GetClient.Self.InstantMessageGroup(groupUUID, message);
            return BasicReply("Sending", "Groupchat", new [] { group, message });
        }

        protected KeyValuePair<bool, string> waitForReady(bool avatar, bool group, UUID groupuuid)
        {
            bool exit = false;
            int sleeps = 0;
            string failed_on = "";
            bool allok = true;
            while (exit == false)
            {
                allok = true;
                failed_on = "";
                if (avatar == true)
                {
                    if (avataruuid == UUID.Zero)
                    {
                        allok = false;
                        failed_on = "avatar lookup";
                    }
                }
                if ((group == true) && (allok == true))
                {
                    allok = !bot.NeedReloadGroupData(groupuuid);
                    failed_on = "Updating";
                }
                if (allok == false)
                {
                    if (sleeps > 3)
                    {
                        exit = true;
                    }
                }
                if (allok == true)
                {
                    exit = true;
                }
                else
                {
                    if (exit == false)
                    {
                        Thread.Sleep(1000);
                        sleeps++;
                    }
                }
            }
            return new KeyValuePair<bool, string>(allok, failed_on);
        }
    }

    public class GroupRoleDetails
    {
        public bool UpdateUnderway { get; set; }
        public Dictionary<string,string> Roles { get; set; }
        public long RoleDataAge = 0;
    }

    public class IsGroupMemberReply
    {
        public bool membershipStatus { get; set; }
        public string AvatarUUID { get; set; }
        public string GroupUUID { get; set; }
        public string AvatarnameIfKnown { get; set; }
    }
}
