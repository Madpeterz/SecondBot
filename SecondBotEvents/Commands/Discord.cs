using Newtonsoft.Json;
using SecondBotEvents.Services;
using System.Collections.Generic;
using System.Linq;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Control discord from the grid")]
    public class DiscordCommands(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Adds a discord server role to the selected member")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "the server id to apply this action to","Number","1234555123")]
        [ArgHints("roleid", "the role id we are giving", "Number", "445442441")]
        [ArgHints("memberid", "who we are giving it to", "Number", "66522144")]
        [CmdTypeSet()]
        public object Discord_AddRole(string serverid,string roleid,string memberid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.AddRoleToMember(serverid, roleid, memberid).Result.ToString(), [serverid, roleid, memberid]);
        }

        [About("Adds the selected user to the ban list - Disallows rejoining untill they are removed from the list")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHintsFailure("Why empty")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [ArgHints("memberid", "who we are giving it to", "Number", "66522144")]
        [ArgHints("why", "why they are being banned", "Text", "Spammer")]
        [CmdTypeDo()]
        public object Discord_BanMember(string serverid, string memberid, string why)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            if (SecondbotHelpers.notempty(why) == false)
            {
                return Failure("Why empty", [serverid, memberid, why]);
            }
            return BasicReply(master.DiscordService.BanMember(serverid, memberid, why).Result.ToString(), [serverid, memberid, why]);
        }

        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHintsFailure("Get roles failure message")]
        [ReturnHints("json object of roles")]
        [About("Gets the roles assigned to the selected member")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [ArgHints("memberid", "who we are giving it to", "Number", "66522144")]
        [CmdTypeGet()]
        public object Discord_GetMemberRoles(string serverid, string memberid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            KeyValuePair<bool, List<string>> reply = master.DiscordService.DiscordMemerRoles(serverid, memberid);
            if(reply.Key == false)
            {
                return Failure(reply.Value.First());
            }
            return BasicReply(JsonConvert.SerializeObject(reply.Value));
        }

        [About("Clears messages on the server sent by the member in the last 13 days, 22hours 59mins")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [ArgHints("memberid", "who we are giving it to", "Number", "66522144")]
        [CmdTypeDo()]
        public object Discord_BulkClear_Messages(string serverid, string memberid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.ClearMessages(serverid, memberid).Result.ToString(), [serverid, memberid]);
        }

        [About("Sends a message directly to the user [They must be in the server]\n This command requires the SERVER MEMBERS INTENT found in discord app dev")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [ArgHints("memberid", "who we are giving it to", "Number", "66522144")]
        [ArgHints("message", "what we are sending", "Text", "Whats going on")]
        [CmdTypeDo()]
        public object Discord_Dm_Member(string serverid, string memberid, string message)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.MessageMember(serverid, memberid, message).Result.ToString(), [serverid, memberid, message]);
        }

        [About("Returns a list of members in a server \n collection is userid: username \n if the user has set a nickname: userid: nickname|username \n" +
            " This command requires Discord full client mode enabled and connected\n !!!! This command also requires: Privileged Gateway Intents / " +
            "SERVER MEMBERS INTENT set to true on the discord bot api area !!!")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHints("mixed array of userid: nickname|username  or   userid:username")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [CmdTypeGet()]
        public object Discord_MembersList(string serverid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.ListMembers(serverid).Result.ToString(), [serverid]);
        }

        [About("Sends a message to the selected channel - Optional TTS usage")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHintsFailure("message empty")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [ArgHints("channelid", "the channel id to apply this action to", "Number", "552121543")]
        [ArgHints("tts", "shoud tts be enabled","BOOL")]
        [ArgHints("message", "what we are sending", "Text", "Hi everyone in channel")]
        [CmdTypeDo()]
        public object Discord_MessageChannel(string serverid, string channelid, string tts, string message)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("message empty", [serverid, channelid, tts, message]);
            }
            return BasicReply(master.DiscordService.MessageChannel(serverid, channelid, message, tts).Result.ToString(), [serverid, channelid, tts, message]);
        }

        [About("Sends a message to the selected channel - Optional TTS usage")]
        [ReturnHints("mixed array of userid: nickname|username  or   userid:username")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [ArgHints("memberid", "who we are giving it to", "Number", "66522144")]
        [ArgHints("mode", "should we mute them", "BOOL")]
        [CmdTypeDo()]
        public object Discord_MuteMember(string serverid, string memberid, string mode)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.MuteMember(serverid, memberid, mode).Result.ToString(), [serverid, memberid, mode]);
        }

        [About("returns a collection of settings for the given role \n This command requires Discord full client mode enabled and connected")]
        [ReturnHints("KeyPair of status: KeyPair[] item = value")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id", "Number", "1234555123")]
        [ArgHints("roleid", "the role id", "Number", "66522144")]
        [CmdTypeGet()]
        public object Discord_Role_GetSettings(string serverid, string roleid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.RoleSettings(serverid, roleid).ToString(), [serverid, roleid]);
        }

        [About("Updates perm flags for the selected role \n example CSV format: Speak=True,SendMessages=False \n for a full list of perms see output of Discord_Role_GetSettings \n" +
            " This command requires Discord full client mode enabled and connected")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [ArgHints("roleid", "the role id", "Number", "66522144")]
        [ArgHints("flagscsv", "what we are setting", "Text", "Speak=True,SendMessages=False")]
        [CmdTypeSet()]
        public object Discord_Role_UpdatePerms(string serverid, string roleid, string flagscsv)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.UpdateRolePerms(serverid, roleid, flagscsv).Result.ToString(), [serverid, roleid, flagscsv]);
        }

        [About("Updates perm flags for the selected role \n example CSV format: Speak=True,SendMessages=False \n for a full list of perms see output of Discord_Role_GetSettings \n" +
    " This command requires Discord full client mode enabled and connected")]
        [ReturnHints("KeyPair of statusmessage=roleid or 0")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [ArgHints("role", "the name of the role we are creating", "Text", "Buyersclub")]
        [CmdTypeDo()]
        public object Discord_RoleCreate(string serverid, string role)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.CreateRole(serverid, role).Result.ToString(), [serverid, role]);
        }

        [About("Returns a list of roles and their ids in collection \n This command requires Discord full client mode enabled and connected")]
        [ReturnHints("KeyPair of status: KeyPair of roleid: rolename")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [CmdTypeGet()]
        public object Discord_RoleList(string serverid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.ListRoles(serverid).ToString(), [serverid]);
        }

        [About("Remove a role from a server \n This command requires Discord full client mode enabled and connected")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [CmdTypeDo()]
        public object Discord_RoleRemove(string serverid, string roleid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.RemoveRole(serverid, roleid).Result.ToString(), [serverid, roleid]);
        }

        [About("Returns a list of text channels in a server")]
        [ReturnHints("array of channelid: name")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to", "Number", "1234555123")]
        [CmdTypeGet()]
        public object Discord_TextChannels_List(string serverid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.ListTextChannels(serverid).ToString());
        }
    }
}
