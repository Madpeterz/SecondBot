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
using System.Reflection;
using Discord.WebSocket;
using System.Threading.Tasks;
using Discord;
using BetterSecondBotShared.Static;
using Discord.Rest;

namespace BetterSecondBot.HttpService
{
    public class HTTP_Discord : WebApiControllerWithTokens
    {
        public HTTP_Discord(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Adds a discord server role to the selected member")]
        [ReturnHints("Discord client not ready")]
        [ReturnHints("true|false")]
        [ArgHints("serverid","URLARG","the server id to apply this action to")]
        [ArgHints("roleid", "URLARG", "the role id we are giving")]
        [ArgHints("memberid", "URLARG", "who we are giving it to")]
        [Route(HttpVerbs.Get, "/Discord_AddRole/{serverid}/{roleid}/{memberid}/{token}")]
        public object Discord_AddRole(string serverid,string roleid,string memberid,string token)
        {
            if (tokens.Allow(token, "discord", "Discord_AddRole", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(addRoleToMember(serverid, roleid, memberid).Result.ToString());
        }

        [About("Adds the selected user to the ban list - Disallows rejoining untill they are removed from the list")]
        [ReturnHints("Discord client not ready")]
        [ReturnHints("Why empty")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [ArgHints("memberid", "URLARG", "who we are giving it to")]
        [ArgHints("why", "string", "why they are being banned")]
        [Route(HttpVerbs.Post, "/Discord_BanMember/{serverid}/{memberid}/{token}")]
        public object Discord_BanMember(string serverid, string memberid, [FormField] string why, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_BanMember", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            if (helpers.notempty(why) == false)
            {
                return Failure("Why empty");
            }
            return BasicReply(BanMember(serverid, memberid, why).Result.ToString());
        }

        [About("Clears messages on the server sent by the member in the last 13 days, 22hours 59mins")]
        [ReturnHints("Discord client not ready")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [ArgHints("memberid", "URLARG", "who we are giving it to")]
        [Route(HttpVerbs.Get, "/Discord_BulkClear_Messages/{serverid}/{memberid}/{token}")]
        public object Discord_BulkClear_Messages(string serverid, string memberid, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_BulkClear_Messages", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return ClearMessages(serverid, memberid).Result;
        }

        [About("Sends a message directly to the user [They must be in the server]\n This command requires the SERVER MEMBERS INTENT found in discord app dev")]
        [ReturnHints("Discord client not ready")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [ArgHints("memberid", "URLARG", "who we are giving it to")]
        [ArgHints("message", "Text", "what we are sending")]
        [Route(HttpVerbs.Post, "/Discord_Dm_Member/{serverid}/{memberid}/{token}")]
        public object Discord_Dm_Member(string serverid, string memberid, [FormField] string message, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_Dm_Member", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(MessageMember(serverid, memberid, message).Result.ToString());
        }

        [About("Returns a list of members in a server \n collection is userid: username \n if the user has set a nickname: userid: nickname|username \n" +
            " This command requires Discord full client mode enabled and connected\n !!!! This command also requires: Privileged Gateway Intents / " +
            "SERVER MEMBERS INTENT set to true on the discord bot api area !!!")]
        [ReturnHints("Discord client not ready")]
        [ReturnHints("mixed array of userid: nickname|username  or   userid:username")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [Route(HttpVerbs.Get, "/Discord_MembersList/{serverid}/{token}")]
        public object Discord_MembersList(string serverid, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_MembersList", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return ListMembers(serverid).Result;
        }

        [About("Sends a message to the selected channel - Optional TTS usage")]
        [ReturnHints("Discord client not ready")]
        [ReturnHints("true|false")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [ArgHints("channelid", "URLARG", "the channel id to apply this action to")]
        [ArgHints("tts", "URLARG", "shoud tts be enabled true or false")]
        [ArgHints("message", "Text", "what we are sending")]
        [Route(HttpVerbs.Post, "/Discord_MembersList/{serverid}/{tts}/{token}")]
        public object Discord_MessageChannel(string serverid, string channelid, string tts, [FormField] string message,  string token)
        {
            if (tokens.Allow(token, "discord", "Discord_MessageChannel", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            if (helpers.notempty(message) == false)
            {
                return Failure("message empty");
            }
            return BasicReply(MessageChannel(serverid, channelid, message, tts).Result.ToString());
        }

        [About("Sends a message to the selected channel - Optional TTS usage")]
        [ReturnHints("mixed array of userid: nickname|username  or   userid:username")]
        [ReturnHints("Discord client not ready")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [ArgHints("memberid", "URLARG", "who we are giving it to")]
        [ArgHints("mode", "URLARG", "should we mute them \"true\" or unmute \"false\"")]
        [Route(HttpVerbs.Get, "/Discord_MuteMember/{serverid}/{memberid}/{mode}/{token}")]
        public object Discord_MuteMember(string serverid, string memberid, string mode, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_MuteMember", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(MuteMember(serverid, memberid, mode).Result.ToString());
        }

        [About("returns a collection of settings for the given role \n This command requires Discord full client mode enabled and connected")]
        [ReturnHints("KeyPair of status: KeyPair[] item = value")]
        [ReturnHints("Discord client not ready")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [ArgHints("memberid", "URLARG", "who we are giving it to")]
        [ArgHints("mode", "URLARG", "should we mute them \"true\" or unmute \"false\"")]
        [Route(HttpVerbs.Get, "/Discord_Role_GetSettings/{serverid}/{roleid}/{token}")]
        public object Discord_Role_GetSettings(string serverid, string roleid, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_Role_GetSettings", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return RoleSettings(serverid,roleid);
        }

        [About("Updates perm flags for the selected role \n example CSV format: Speak=True,SendMessages=False \n for a full list of perms see output of Discord_Role_GetSettings \n" +
            " This command requires Discord full client mode enabled and connected")]
        [ReturnHints("true|false")]
        [ReturnHints("Discord client not ready")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [ArgHints("roleid", "URLARG", "who we are giving it to")]
        [ArgHints("flagscsv", "Text", "what we are setting")]
        [Route(HttpVerbs.Post, "/Discord_Role_UpdatePerms/{serverid}/{roleid}/{token}")]
        public object Discord_Role_UpdatePerms(string serverid, string roleid, [FormField] string flagscsv, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_Role_UpdatePerms", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(UpdateRolePerms(serverid, roleid, flagscsv).Result.ToString());
        }

        [About("Updates perm flags for the selected role \n example CSV format: Speak=True,SendMessages=False \n for a full list of perms see output of Discord_Role_GetSettings \n" +
    " This command requires Discord full client mode enabled and connected")]
        [ReturnHints("KeyPair of statusmessage=roleid or 0")]
        [ReturnHints("Discord client not ready")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [ArgHints("role", "URLARG", "the name of the role we are creating")]
        [Route(HttpVerbs.Get, "/Discord_RoleCreate/{serverid}/{role}/{token}")]
        public object Discord_RoleCreate(string serverid, string role, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_RoleCreate", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return CreateRole(serverid,role).Result;
        }

        [About("Returns a list of roles and their ids in collection \n This command requires Discord full client mode enabled and connected")]
        [ReturnHints("KeyPair of status: KeyPair of roleid: rolename")]
        [ReturnHints("Discord client not ready")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [Route(HttpVerbs.Get, "/Discord_RoleList/{serverid}/{token}")]
        public object Discord_RoleList(string serverid, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_RoleList", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(ListRoles(serverid).ToString());
        }

        [About("Remove a role from a server \n This command requires Discord full client mode enabled and connected")]
        [ReturnHints("true|false")]
        [ReturnHints("Discord client not ready")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [Route(HttpVerbs.Get, "/Discord_RoleRemove/{serverid}/{roleid}/{token}")]
        public object Discord_RoleRemove(string serverid, string roleid, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_RoleRemove", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return BasicReply(RemoveRole(serverid, roleid).Result.ToString());
        }

        [About("Returns a list of text channels in a server")]
        [ReturnHints("array of channelid: name")]
        [ReturnHints("Discord client not ready")]
        [ArgHints("serverid", "URLARG", "the server id to apply this action to")]
        [Route(HttpVerbs.Get, "/Discord_TextChannels_List/{serverid}/{roleid}/{token}")]
        public object Discord_TextChannels_List(string serverid, string token)
        {
            if (tokens.Allow(token, "discord", "Discord_TextChannels_List", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.discordReady() == false)
            {
                return Failure("Discord client not ready");
            }
            return ListTextChannels(serverid);
        }


        #region discord helper functions

        protected KeyValuePair<bool, Dictionary<string, string>> ListTextChannels(string givenserverid)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == true)
            {
                return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
            }
            SocketGuild server = Discord.GetGuild(serverid);
            Dictionary<string, string> textChannels = new Dictionary<string, string>();
            foreach (SocketTextChannel channel in server.TextChannels)
            {
                textChannels.Add(channel.Id.ToString(), channel.Name);
            }
            return new KeyValuePair<bool, Dictionary<string, string>>(true, textChannels);
        }

        protected async Task<bool> RemoveRole(string givenserverid, string givenroleid)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenroleid, out ulong roleid) == false)
            {
                return false;
            }
            SocketGuild server = Discord.GetGuild(serverid);
            SocketRole role = server.GetRole(roleid);
            await role.DeleteAsync().ConfigureAwait(true);
            return true;
        }

        protected KeyValuePair<bool, Dictionary<string, string>> ListRoles(string givenserverid)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
            }
            SocketGuild server = Discord.GetGuild(serverid);
            Dictionary<string, string> rolesList = new Dictionary<string, string>();
            foreach (SocketRole role in server.Roles)
            {
                rolesList.Add(role.Id.ToString(), role.Name);
            }
            return new KeyValuePair<bool, Dictionary<string, string>>(true, rolesList);
        }

        protected async Task<KeyValuePair<string, ulong>> CreateRole(string givenserverid, string givenrole)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == true)
            {
                return new KeyValuePair<string, ulong>("Unable to process server id", 0);
            }
            SocketGuild server = Discord.GetGuild(serverid);
            RestRole role = await server.CreateRoleAsync(givenrole, new GuildPermissions(), Color.DarkRed, false, null).ConfigureAwait(true);
            return new KeyValuePair<string, ulong>("ok", role.Id);
        }

        protected async Task<bool> UpdateRolePerms(string givenserverid,string givenroleid,string givenflagscsv)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenroleid, out ulong roleid) == true)
            {
                return false;
            }
            SocketGuild server = Discord.GetGuild(serverid);
            SocketRole role = server.GetRole(roleid);
            string[] bits = givenflagscsv.Split(',');
            GuildPermissions NewPerms = role.Permissions;
            foreach (String bit in bits)
            {
                string[] subbits = bit.Split('=');
                if (subbits.Length == 2)
                {
                    if (bool.TryParse(subbits[1], out bool flagValue) == true)
                    {
                        NewPerms = UpdateFlag(NewPerms, subbits[0], flagValue);
                    }
                }
            }
            await role.ModifyAsync(Rp =>
            {
                Rp.Permissions = NewPerms;
            }).ConfigureAwait(true);
            return true;
        }

        protected GuildPermissions UpdateFlag(GuildPermissions NewPerms, string flag, bool flagValue)
        {
            switch (flag)
            {
                case "CreateInstantInvite":
                    NewPerms = NewPerms.Modify(flagValue);
                    break;
                case "KickMembers":
                    NewPerms = NewPerms.Modify(null, flagValue);
                    break;
                case "BanMembers":
                    NewPerms = NewPerms.Modify(null, null, flagValue);
                    break;
                case "Administrator":
                    NewPerms = NewPerms.Modify(null, null, null, flagValue);
                    break;
                case "ManageChannels":
                    NewPerms = NewPerms.Modify(null, null, null, null, flagValue);
                    break;
                case "ManageGuild":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, flagValue);
                    break;
                case "AddReactions":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, flagValue);
                    break;
                case "ViewAuditLog":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, flagValue);
                    break;
                case "SendTTSMessages":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "AttachFiles":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "ReadMessageHistory":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "MentionEveryone":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "UseExternalEmojis":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "Connect":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "Speak":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "MuteMembers":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "UseVAD":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "MoveMembers":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "EmbedLinks":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "PrioritySpeaker":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "Stream":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "ChangeNickname":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "ManageNicknames":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "ManageRoles":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "DeafenMembers":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "ManageMessages":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "ViewChannel":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "SendMessages":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "ManageWebhooks":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                case "ManageEmojis":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                default:
                    break;
            }
            return NewPerms;
        }

        protected KeyValuePair<bool, Dictionary<string, string>> RoleSettings(string givenserverid, string givenroleid)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
            }
            if (ulong.TryParse(givenroleid, out ulong roleid) == false)
            {
                return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
            }
            SocketGuild server = Discord.GetGuild(serverid);
            SocketRole role = server.GetRole(roleid);
            Dictionary<string, string> settings = new Dictionary<string, string>
            {
                { "Config|Color", String.Join(",", new byte[3] { role.Color.R, role.Color.G, role.Color.B }) },
                { "Config|Name", role.Name },
                { "Info|AssignedCount", role.Members.ToString() },
                { "Perm|AttachFiles", role.Permissions.AttachFiles.ToString() },
                { "Perm|ReadMessageHistory", role.Permissions.ReadMessageHistory.ToString() },
                { "Perm|MentionEveryone", role.Permissions.MentionEveryone.ToString() },
                { "Perm|UseExternalEmojis", role.Permissions.UseExternalEmojis.ToString() },
                { "Perm|Connect", role.Permissions.Connect.ToString() },
                { "Perm|Speak", role.Permissions.Speak.ToString() },
                { "Perm|MuteMembers", role.Permissions.MuteMembers.ToString() },
                { "Perm|UseVAD", role.Permissions.UseVAD.ToString() },
                { "Perm|MoveMembers", role.Permissions.MoveMembers.ToString() },
                { "Perm|EmbedLinks", role.Permissions.EmbedLinks.ToString() },
                { "Perm|PrioritySpeaker", role.Permissions.PrioritySpeaker.ToString() },
                { "Perm|Stream", role.Permissions.Stream.ToString() },
                { "Perm|ChangeNickname", role.Permissions.ChangeNickname.ToString() },
                { "Perm|ManageNicknames", role.Permissions.ManageNicknames.ToString() },
                { "Perm|ManageRoles", role.Permissions.ManageRoles.ToString() },
                { "Perm|DeafenMembers", role.Permissions.DeafenMembers.ToString() },
                { "Perm|ManageMessages", role.Permissions.ManageMessages.ToString() },
                { "Perm|ViewChannel", role.Permissions.ViewChannel.ToString() },
                { "Perm|SendMessages", role.Permissions.SendMessages.ToString() },
                { "Perm|CreateInstantInvite", role.Permissions.CreateInstantInvite.ToString() },
                { "Perm|BanMembers", role.Permissions.BanMembers.ToString() },
                { "Perm|SendTTSMessages", role.Permissions.SendTTSMessages.ToString() },
                { "Perm|Administrator", role.Permissions.Administrator.ToString() },
                { "Perm|ManageChannels", role.Permissions.ManageChannels.ToString() },
                { "Perm|KickMembers", role.Permissions.KickMembers.ToString() },
                { "Perm|AddReactions", role.Permissions.AddReactions.ToString() },
                { "Perm|ViewAuditLog", role.Permissions.ViewAuditLog.ToString() },
                { "Perm|ManageWebhooks", role.Permissions.ManageWebhooks.ToString() },
                { "Perm|ManageGuild", role.Permissions.ManageGuild.ToString() },
                { "Perm|ManageEmojis", role.Permissions.ManageEmojis.ToString() }
            };
            return new KeyValuePair<bool, Dictionary<string, string>>(true, settings);
        }

        protected async Task<bool> MuteMember(string givenserverid, string givenmemberid, string mode)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == true)
            {
                return false;
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == true)
            {
                return false;
            }
            SocketGuild server = Discord.GetGuild(serverid);
            SocketGuildUser user = server.GetUser(memberid);
            bool.TryParse(mode, out bool status);
            await user.ModifyAsync(pr =>
            {
                pr.Mute = status;
            });
            return true;
        }

        protected async Task<bool> MessageChannel(string givenserverid, string givenchannelid, string givenmessage, string tts)
        {
            if (bool.TryParse(tts, out bool useTTS) == false)
            {
                return false;
            }
            return await bot.SendMessageToDiscord(givenserverid, givenchannelid, givenmessage, useTTS);
        }


        protected async Task<KeyValuePair<bool, Dictionary<string, string>>> ListMembers(string givenserverid)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
            }
            SocketGuild server = Discord.GetGuild(serverid);
            IEnumerable<IGuildUser> users = await server.GetUsersAsync().FlattenAsync().ConfigureAwait(true);
            Dictionary<string, string> membersList = new Dictionary<string, string>();

            foreach (IGuildUser user in users)
            {
                if (user.Nickname != null)
                {
                    membersList.Add(user.Id.ToString(), user.Nickname + "|" + user.Username);
                }
                else
                {
                    membersList.Add(user.Id.ToString(), user.Username);
                }
            }
            return new KeyValuePair<bool, Dictionary<string, string>>(true, membersList);            
        }


        protected async Task<bool> MessageMember(string givenserverid, string givenmemberid, string givenmessage)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == false)
            {
                return false;
            }
            try
            {
                SocketGuild server = Discord.GetGuild(serverid);
                IEnumerable<IGuildUser> users = await server.GetUsersAsync().FlattenAsync().ConfigureAwait(true);
                bool sent = false;
                foreach (IGuildUser user in users)
                {
                    if (user.Id == memberid)
                    {
                        sent = true;
                        await user.SendMessageAsync(givenmessage);
                        break;
                    }
                }
                return sent;
            }
            catch
            {
                return false;
            }
        }

        protected async Task<KeyValuePair<bool, int>> ClearMessages(string givenserverid, string givenmemberid)
        {
            int messagesDeleted = 0;
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return new KeyValuePair<bool, int>(false, 0);
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == false)
            {
                return new KeyValuePair<bool, int>(false, 0);
            }
            SocketGuild server = Discord.GetGuild(serverid);
            SocketGuildUser user = server.GetUser(memberid);
            TimeSpan somedays = new TimeSpan(13, 22, 59, 59, 0);
            DateTimeOffset justundertwosweeks = DateTimeOffset.Now;
            justundertwosweeks = justundertwosweeks.Subtract(somedays);
            long unixtimelimit = justundertwosweeks.ToUnixTimeSeconds();
            foreach (SocketTextChannel channel in server.TextChannels)
            {
                List<IMessage> DeleteMessages = new List<IMessage>();
                IEnumerable<IMessage> messages = await channel.GetMessagesAsync(100).FlattenAsync();
                foreach (IMessage message in messages)
                {
                    if (message.CreatedAt.ToUnixTimeSeconds() > unixtimelimit)
                    {
                        if (message.Author.Id == user.Id)
                        {
                            DeleteMessages.Add(message);
                        }
                    }
                }
                if (DeleteMessages.Count > 0)
                {
                    messagesDeleted += DeleteMessages.Count;
                    await channel.DeleteMessagesAsync(DeleteMessages);
                }
            }
            return new KeyValuePair<bool, int>(true, messagesDeleted);
        }



        protected async Task<bool> BanMember(string givenserverid, string givenmemberid, string why)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == false)
            {
                return false;
            }
            SocketGuild server = Discord.GetGuild(serverid);
            SocketGuildUser user = server.GetUser(memberid);
            await user.BanAsync(7, why);
            return true;
        }


        protected async Task<bool> addRoleToMember(string givenserverid, string givenroleid, string givenmemberid)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenroleid, out ulong roleid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == false)
            {
                return false;
            }
            SocketGuild server = Discord.GetGuild(serverid);
            SocketRole role = server.GetRole(roleid);
            SocketGuildUser user = server.GetUser(memberid);
            await user.AddRoleAsync(role); // ? Irole seems to accept SocketRole ?
            return true;
        }


        #endregion
    }
}
