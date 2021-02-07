using BSB.bottypes;
using BSB.Static;
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
    public class HttpApiGroup : WebApiControllerWithTokens
    {
        public HttpApiGroup(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }


        [About("Checks if the given UUID is in the given group<br/>Note: if group membership data is more than 60 secs old this will return Updating<br/>Please wait and retry later")]
        [ReturnHints("True|False")]
        [ReturnHints("Updating")]
        [ReturnHints("Unknown group")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Unable to find avatar UUID")]
        [Route(HttpVerbs.Get, "/ismember/{group}/{avatar}/{token}")]
        public object ismember(string group,string avatar,string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                if (UUID.TryParse(group, out UUID groupuuid) == true)
                {
                    if (bot.MyGroups.ContainsKey(groupuuid) == true)
                    {
                        if (bot.NeedReloadGroupData(groupuuid) == false)
                        {
                            UUID avataruuid = UUID.Zero;
                            if(UUID.TryParse(avatar,out avataruuid) == false)
                            {
                                string lookup = bot.FindAvatarName2Key(avatar);
                                if (UUID.TryParse(lookup, out avataruuid) == false)
                                {
                                    return BasicReply("Unable to find avatar UUID");
                                }
                            }
                            bool status = bot.FastCheckInGroup(groupuuid, avataruuid);
                            return BasicReply(status.ToString());
                        }
                        bot.GetClient.Groups.RequestGroupMembers(groupuuid);
                        return BasicReply("Updating");
                    }
                    return BasicReply("Unknown group");
                }
                return BasicReply("Invaild group UUID");
            }
            return BasicReply("Token not accepted");
        }


        [About("fetchs a list of all groups known to the bot")]
        [ReturnHints("array UUID=name")]
        [Route(HttpVerbs.Get, "/listgroups/{token}")]
        public object listgroups(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                Dictionary<string, string> grouppackage = new Dictionary<string, string>();
                foreach(KeyValuePair<UUID,Group> entry in bot.MyGroups)
                {
                    grouppackage.Add(entry.Value.ID.ToString(), entry.Value.Name);
                }
                return BasicReply(JsonConvert.SerializeObject(grouppackage));
            }
            return BasicReply("Token not accepted");
        }

        [About("Requests the roles for the selected group<br/>Replys with GroupRoleDetails object formated as follows <ul><li>UpdateUnderway (Bool)</li><li>RoleDataAge (Int) [default -1]</li><li>Roles (KeyPair array of UUID=Name)</li></ul><br/>")]
        [ArgHints("group", "URLARG", "UUID of the group")]
        [ReturnHints("GroupRoleDetails object")]
        [ReturnHints("Group is not currently known")]
        [ReturnHints("Invaild group UUID")]
        [ReturnHints("Updating")]
        [Route(HttpVerbs.Get, "/listgrouproles/{group}/{token}")]
        public object listgrouproles(string group, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                if (UUID.TryParse(group, out UUID groupuuid) == true)
                {
                    if (bot.MyGroups.ContainsKey(groupuuid) == true)
                    {
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
                            foreach(GroupRole gr in bot.MyGroupRolesStorage[groupuuid].Value)
                            {
                                reply.Roles.Add(gr.ID.ToString(), gr.Name);
                            }
                        }
                        if(reply.UpdateUnderway == true)
                        {
                            bot.GetClient.Groups.RequestGroupRoles(groupuuid);
                            if (reply.Roles.Count == 0)
                            {
                                return BasicReply("Updating");
                            }
                        }
                        return BasicReply(JsonConvert.SerializeObject(reply));
                    }
                    return BasicReply("Group is not currently known");
                }
                return BasicReply("Invaild group UUID");
            }
            return BasicReply("Token not accepted");
        }

        [About("fetchs a list of all groups with unread messages")]
        [ReturnHints("array UUID")]
        [Route(HttpVerbs.Get, "/listgroupswithunread/{token}")]
        public object listgroupswithunread(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return BasicReply(JsonConvert.SerializeObject(bot.UnreadGroupchatGroups()));
            }
            return BasicReply("Token not accepted");
        }

        [About("checks if there are any groups with unread messages")]
        [ReturnHints("True|False")]
        [Route(HttpVerbs.Get, "/haveunreadgroupchat/{token}")]
        public object haveunreadgroupchat(string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                return BasicReply(bot.HasUnreadGroupchats().ToString());
            }
            return BasicReply("Token not accepted");
        }

        [About("fetchs the groupchat history")]
        [ArgHints("group", "URLARG", "UUID of the group")]
        [ReturnHints("Group UUID invaild")]
        [ReturnHints("Group Chat")]
        [Route(HttpVerbs.Get, "/getgroupchat/{group}/{token}")]
        public object getgroupchat(string group, string token)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                if (UUID.TryParse(group, out UUID groupUUID) == true)
                {
                    return BasicReply(JsonConvert.SerializeObject(bot.GetGroupchat(groupUUID)));
                }
                return BasicReply("Group UUID invaild");
            }
            return BasicReply("Token not accepted");
        }

        [About("sends a message to the groupchat")]
        [ArgHints("group", "URLARG", "UUID of the group")]
        [ArgHints("message", "Text", "the message to send")]
        [ReturnHints("Group UUID invaild")]
        [ReturnHints("Processing")]
        [Route(HttpVerbs.Post, "/sendgroupchat/{group}/{token}")]
        public object sendgroupchat(string group, string token, [FormField] string message)
        {
            if (tokens.Allow(token, getClientIP()) == true)
            {
                if (UUID.TryParse(group, out UUID groupUUID) == true)
                {
                    bool status = bot.GetCommandsInterface.Call("Groupchat", group + "~#~" + message, UUID.Zero, "~#~");
                    return BasicReply("Processing");
                }
                return BasicReply("Group UUID invaild");
            }
            return BasicReply("Token not accepted");
        }
    }

    public class GroupRoleDetails
    {
        public bool UpdateUnderway { get; set; }
        public Dictionary<string,string> Roles { get; set; }
        public long RoleDataAge = 0;
    }
}
