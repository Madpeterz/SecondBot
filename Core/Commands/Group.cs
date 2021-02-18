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

namespace BetterSecondBot.HttpService
{
    public class HTTP_Group : WebApiControllerWithTokens
    {
        public HTTP_Group(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }


        [About("Checks if the given UUID is in the given group<br/>Note: if group membership data is more than 60 secs old this will return Updating<br/>Please wait and retry later")]
        [ReturnHints("True|False")]
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
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group");
            }
            if (bot.NeedReloadGroupData(groupuuid) == true)
            {
                bot.GetClient.Groups.RequestGroupMembers(groupuuid);
                return Failure("Updating");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup");
            }
            bool status = bot.FastCheckInGroup(groupuuid, avataruuid);
            return BasicReply(status.ToString());
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
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group");
            }
            if (bot.NeedReloadGroupData(groupuuid) == true)
            {
                bot.GetClient.Groups.RequestGroupMembers(groupuuid);
                return Failure("Updating");
            }
            return bot.GetGroupMembers(groupuuid);
        }

        [About("Eject selected avatar from group")]
        [ReturnHints("Requested")]
        [ReturnHints("Updating")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Not in group")]
        [ReturnHints("avatar lookup")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("avatar", "URLARG", "the UUID of the avatar you wish to check with")]
        [Route(HttpVerbs.Get, "/GroupEject/{group}/{avatar}/{token}")]
        public object GroupEject(string group, string avatar, string token)
        {
            if (tokens.Allow(token, "groups", "GroupEject", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group");
            }
            if (bot.NeedReloadGroupData(groupuuid) == true)
            {
                bot.GetClient.Groups.RequestGroupMembers(groupuuid);
                return Failure("Updating");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup");
            }
            bool status = bot.FastCheckInGroup(groupuuid, avataruuid);
            if(status == false)
            {
                return BasicReply("Not in group");
            }
            bot.GetClient.Groups.EjectUser(groupuuid, avataruuid);
            return BasicReply("Requested");
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
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID");
            }
            if (UUID.TryParse(role, out UUID roleuuid) == false)
            {
                return Failure("Invaild role UUID");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group");
            }
            if (bot.NeedReloadGroupData(groupuuid) == true)
            {
                bot.GetClient.Groups.RequestGroupMembers(groupuuid);
                return Failure("Updating");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup");
            }
            bool status = bot.FastCheckInGroup(groupuuid, avataruuid);
            if (status == false)
            {
                bot.GetClient.Groups.Invite(groupuuid, new List<UUID>() { roleuuid }, avataruuid);
                return BasicReply("Invite sent");
            }
            bot.GetClient.Groups.AddToRole(groupuuid, roleuuid, avataruuid);
            return BasicReply("Roles updated");
        }

        [About("Eject selected avatar from group")]
        [ReturnHints("Invite sent")]
        [ReturnHints("Updating")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Already in group")]
        [ReturnHints("avatar lookup")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("avatar", "URLARG", "the UUID of the avatar you wish to check with")]
        [ArgHints("role", "URLARG", "the UUID of the role to invite them at the word \"everyone\"")]
        [Route(HttpVerbs.Get, "/GroupInvite/{group}/{avatar}/{role}/{token}")]
        public object GroupInvite(string group, string avatar, string role, string token)
        {
            if (tokens.Allow(token, "groups", "GroupInvite", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group");
            }
            if (bot.NeedReloadGroupData(groupuuid) == true)
            {
                bot.GetClient.Groups.RequestGroupMembers(groupuuid);
                return Failure("Updating");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup");
            }
            bool status = bot.FastCheckInGroup(groupuuid, avataruuid);
            if (status == true)
            {
                return Failure("Already in group");
            }
            if(role == "everyone")
            {
                role = UUID.Zero.ToString();
            }
            if(UUID.TryParse(role,out UUID roleuuid) == false)
            {
                return Failure("Unable to process role UUID");
            }
            bot.GetClient.Groups.Invite(groupuuid, new List<UUID>() { roleuuid }, avataruuid);
            return BasicReply("Invite sent");
        }

        [About("Sends a group notice (No attachments please use GroupnoticeWithAttachment to attach items!)")]
        [ReturnHints("Sending notice")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Title empty")]
        [ReturnHints("Message empty")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("title", "Text", "The title of the group notice")]
        [ArgHints("message", "Text", "The body of the group notice")]
        [Route(HttpVerbs.Post, "/Groupnotice/{group}/{token}")]
        public object Groupnotice(string group, [FormField] string title, [FormField] string message, string token)
        {
            if (tokens.Allow(token, "groups", "Groupnotice", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group");
            }
            if (helpers.notempty(title) == false)
            {
                return Failure("Title empty");
            }
            if (helpers.notempty(message) == false)
            {
                return Failure("Message empty");
            }
            GroupNotice NewNotice = new GroupNotice();
            NewNotice.Subject = title;
            NewNotice.Message = message;
            NewNotice.OwnerID = bot.GetClient.Self.AgentID;
            bot.GetClient.Groups.SendGroupNotice(groupuuid, NewNotice);
            return BasicReply("Sending notice");
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
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID");
            }
            if (UUID.TryParse(role, out UUID roleuuid) == false)
            {
                return Failure("Invaild role UUID");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group");
            }
            bot.GetClient.Groups.ActivateTitle(groupuuid, roleuuid);
            return BasicReply("Switching title");
        }

        [About("Sets the selected group to the active group")]
        [ReturnHints("Switching active group")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [Route(HttpVerbs.Get, "/GroupActiveGroup/{group}/{token}")]
        public object GroupActiveGroup(string group, string role, string token)
        {
            if (tokens.Allow(token, "groups", "GroupActiveTitle", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group");
            }
            bot.GetClient.Groups.ActivateGroup(groupuuid);
            return BasicReply("Switching active group");
        }

        [About("Sends a group notice with an attachment")]
        [ReturnHints("Sending notice with attachment")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Invaild inventory UUID")]
        [ReturnHints("Title empty")]
        [ReturnHints("Message empty")]
        [ArgHints("group", "URLARG", "the UUID of the group")]
        [ArgHints("attachment", "URLARG", "the UUID of inventory you wish to attach")]
        [ArgHints("title", "Text", "The title of the group notice")]
        [ArgHints("message", "Text", "The body of the group notice")]
        [Route(HttpVerbs.Post, "/Groupnotice/{group}/{attachment}/{token}")]
        public object GroupnoticeWithAttachment(string group, [FormField] string title, [FormField] string message, string attachment, string token)
        {
            if (tokens.Allow(token, "groups", "GroupnoticeWithAttachment", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID");
            }
            if (UUID.TryParse(attachment, out UUID inventoryuuid) == false)
            {
                return Failure("Invaild inventory UUID");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group");
            }
            if (helpers.notempty(title) == false)
            {
                return Failure("Title empty");
            }
            if (helpers.notempty(message) == false)
            {
                return Failure("Message empty");
            }
            GroupNotice NewNotice = new GroupNotice();
            NewNotice.Subject = title;
            NewNotice.Message = message;
            NewNotice.OwnerID = bot.GetClient.Self.AgentID;
            NewNotice.AttachmentID = inventoryuuid;
            bot.GetClient.Groups.SendGroupNotice(groupuuid, NewNotice);
            return BasicReply("Sending notice with attachment");
        }


        [About("fetchs a list of all groups known to the bot")]
        [ReturnHints("array UUID=name")]
        [Route(HttpVerbs.Get, "/GetGroupList/{token}")]
        public object GetGroupList(string token)
        {
            if (tokens.Allow(token, "groups", "GetGroupList", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            Dictionary<string, string> grouppackage = new Dictionary<string, string>();
            foreach (KeyValuePair<UUID, Group> entry in bot.MyGroups)
            {
                grouppackage.Add(entry.Value.ID.ToString(), entry.Value.Name);
            }
            return BasicReply(JsonConvert.SerializeObject(grouppackage));
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
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupuuid) == false)
            {
                return Failure("Invaild group UUID");
            }
            if (bot.MyGroups.ContainsKey(groupuuid) == false)
            {
                return Failure("Unknown group");
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
                    return Failure("Updating");
                }
            }
            return BasicReply(JsonConvert.SerializeObject(reply));
        }

        [About("fetchs a list of all groups with unread messages")]
        [ReturnHints("array UUID")]
        [Route(HttpVerbs.Get, "/GroupchatListAllUnreadGroups/{token}")]
        public object GroupchatListAllUnreadGroups(string token)
        {
            if (tokens.Allow(token, "groups", "GroupchatListAllUnreadGroups", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            return BasicReply(JsonConvert.SerializeObject(bot.UnreadGroupchatGroups()));
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
            if (tokens.Allow(token, "groups", "GroupchatListAllUnreadGroups", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            UUID groupUUID = UUID.Zero;
            if(UUID.TryParse(group,out groupUUID) == false)
            {
                return Failure("group value is invaild");
            }
            if(bot.MyGroups.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group");
            }
            return BasicReply(bot.GroupHasUnread(groupUUID).ToString());
        }


        [About("checks if there are any groups with unread messages")]
        [ReturnHints("True|False")]
        [Route(HttpVerbs.Get, "/GroupchatAnyUnread/{token}")]
        public object GroupchatAnyUnread(string token)
        {
            if (tokens.Allow(token, "groups", "GroupchatAnyUnread", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            return BasicReply(bot.HasUnreadGroupchats().ToString());
        }

        [About("Clears all group chat buffers at once")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/GroupchatClearAll/{token}")]
        public object GroupchatClearAll(string token)
        {
            if (tokens.Allow(token, "groups", "GroupchatClearAll", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            bot.ClearAllGroupchat();
            return BasicReply("ok");
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
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(group, out UUID groupUUID) == false)
            {
                return Failure("Group UUID invaild");
            }
            return BasicReply(JsonConvert.SerializeObject(bot.GetGroupchat(groupUUID)));
        }

        [About("sends a message to the groupchat")]
        [ArgHints("group", "URLARG", "UUID of the group")]
        [ArgHints("message", "Text", "the message to send")]
        [ReturnHints("Group UUID invaild")]
        [ReturnHints("Processing")]
        [ReturnHints("Opening groupchat")]
        [ReturnHints("Sending")]
        [Route(HttpVerbs.Post, "/Groupchat/{group}/{token}")]
        public object Groupchat(string group, [FormField] string message, string token)
        {
            if (tokens.Allow(token, "groups", "Groupchat", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            UUID groupUUID = UUID.Zero;
            if (UUID.TryParse(group, out groupUUID) == false)
            {
                return Failure("group value is invaild");
            }
            if (bot.MyGroups.ContainsKey(groupUUID) == false)
            {
                return Failure("Unknown group");
            }
            if (bot.GetActiveGroupchatSessions.Contains(groupUUID) == false)
            {
                bot.GetClient.Self.RequestJoinGroupChat(groupUUID);
                return Failure("Opening groupchat");
            }
            bot.GetClient.Self.InstantMessageGroup(groupUUID, message);
            return BasicReply("Sending");
        }
    }

    public class GroupRoleDetails
    {
        public bool UpdateUnderway { get; set; }
        public Dictionary<string,string> Roles { get; set; }
        public long RoleDataAge = 0;
    }
}
