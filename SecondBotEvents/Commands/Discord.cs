using SecondBotEvents.Services;

namespace SecondBotEvents.Commands
{
    public class DiscordCommands : CommandsAPI
    {
        public DiscordCommands(EventsSecondBot setmaster) : base(setmaster)
        {
        }
        [About("Adds a discord server role to the selected member")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "the server id to apply this action to")]
        [ArgHints("roleid", "the role id we are giving")]
        [ArgHints("memberid", "who we are giving it to")]
        public object Discord_AddRole(string serverid,string roleid,string memberid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.addRoleToMember(serverid, roleid, memberid).Result.ToString(), new [] { serverid, roleid, memberid });
        }

        [About("Adds the selected user to the ban list - Disallows rejoining untill they are removed from the list")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHintsFailure("Why empty")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "the server id to apply this action to")]
        [ArgHints("memberid", "who we are giving it to")]
        [ArgHints("why", "why they are being banned")]
        public object Discord_BanMember(string serverid, string memberid, string why)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            if (SecondbotHelpers.notempty(why) == false)
            {
                return Failure("Why empty", new [] { serverid, memberid, why });
            }
            return BasicReply(master.DiscordService.BanMember(serverid, memberid, why).Result.ToString(), new [] { serverid, memberid, why });
        }

        [About("Clears messages on the server sent by the member in the last 13 days, 22hours 59mins")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "the server id to apply this action to")]
        [ArgHints("memberid", "who we are giving it to")]
        public object Discord_BulkClear_Messages(string serverid, string memberid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.ClearMessages(serverid, memberid).Result.ToString(), new[] { serverid, memberid });
        }

        [About("Sends a message directly to the user [They must be in the server]\n This command requires the SERVER MEMBERS INTENT found in discord app dev")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "the server id to apply this action to")]
        [ArgHints("memberid", "who we are giving it to")]
        [ArgHints("message", "what we are sending")]
        public object Discord_Dm_Member(string serverid, string memberid, string message)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.MessageMember(serverid, memberid, message).Result.ToString(), new [] { serverid, memberid, message });
        }

        [About("Returns a list of members in a server \n collection is userid: username \n if the user has set a nickname: userid: nickname|username \n" +
            " This command requires Discord full client mode enabled and connected\n !!!! This command also requires: Privileged Gateway Intents / " +
            "SERVER MEMBERS INTENT set to true on the discord bot api area !!!")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHints("mixed array of userid: nickname|username  or   userid:username")]
        [ArgHints("serverid", "the server id to apply this action to")]
        public object Discord_MembersList(string serverid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.ListMembers(serverid).Result.ToString(), new[] { serverid });
        }

        [About("Sends a message to the selected channel - Optional TTS usage")]
        [ReturnHintsFailure("Discord client not ready")]
        [ReturnHintsFailure("message empty")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "the server id to apply this action to")]
        [ArgHints("channelid", "the channel id to apply this action to")]
        [ArgHints("tts", "shoud tts be enabled true or false")]
        [ArgHints("message", "what we are sending")]
        public object Discord_MessageChannel(string serverid, string channelid, string tts, string message)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            if (SecondbotHelpers.notempty(message) == false)
            {
                return Failure("message empty", new [] { serverid, channelid, tts, message });
            }
            return BasicReply(master.DiscordService.MessageChannel(serverid, channelid, message, tts).Result.ToString(), new [] { serverid, channelid, tts, message });
        }

        [About("Sends a message to the selected channel - Optional TTS usage")]
        [ReturnHints("mixed array of userid: nickname|username  or   userid:username")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to")]
        [ArgHints("memberid", "who we are giving it to")]
        [ArgHints("mode", "should we mute them \"true\" or unmute \"false\"")]
        public object Discord_MuteMember(string serverid, string memberid, string mode)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.MuteMember(serverid, memberid, mode).Result.ToString(), new [] { serverid, memberid, mode });
        }

        [About("returns a collection of settings for the given role \n This command requires Discord full client mode enabled and connected")]
        [ReturnHints("KeyPair of status: KeyPair[] item = value")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to")]
        [ArgHints("memberid", "who we are giving it to")]
        [ArgHints("mode", "should we mute them \"true\" or unmute \"false\"")]
        public object Discord_Role_GetSettings(string serverid, string roleid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.RoleSettings(serverid, roleid).ToString(), new [] { serverid, roleid });
        }

        [About("Updates perm flags for the selected role \n example CSV format: Speak=True,SendMessages=False \n for a full list of perms see output of Discord_Role_GetSettings \n" +
            " This command requires Discord full client mode enabled and connected")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to")]
        [ArgHints("roleid", "who we are giving it to")]
        [ArgHints("flagscsv", "what we are setting")]
        public object Discord_Role_UpdatePerms(string serverid, string roleid, string flagscsv)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.UpdateRolePerms(serverid, roleid, flagscsv).Result.ToString(), new [] { serverid, roleid, flagscsv });
        }

        [About("Updates perm flags for the selected role \n example CSV format: Speak=True,SendMessages=False \n for a full list of perms see output of Discord_Role_GetSettings \n" +
    " This command requires Discord full client mode enabled and connected")]
        [ReturnHints("KeyPair of statusmessage=roleid or 0")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to")]
        [ArgHints("role", "the name of the role we are creating")]
        public object Discord_RoleCreate(string serverid, string role)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.CreateRole(serverid, role).Result.ToString(), new[] { serverid, role });
        }

        [About("Returns a list of roles and their ids in collection \n This command requires Discord full client mode enabled and connected")]
        [ReturnHints("KeyPair of status: KeyPair of roleid: rolename")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to")]
        public object Discord_RoleList(string serverid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.ListRoles(serverid).ToString(), new [] { serverid });
        }

        [About("Remove a role from a server \n This command requires Discord full client mode enabled and connected")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to")]
        public object Discord_RoleRemove(string serverid, string roleid)
        {
            if (master.DiscordService.DiscordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(master.DiscordService.RemoveRole(serverid, roleid).Result.ToString(), new [] { serverid, roleid });
        }

        [About("Returns a list of text channels in a server")]
        [ReturnHints("array of channelid: name")]
        [ReturnHintsFailure("Discord client not ready")]
        [ArgHints("serverid", "the server id to apply this action to")]
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
