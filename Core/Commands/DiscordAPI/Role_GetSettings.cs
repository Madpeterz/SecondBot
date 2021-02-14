using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using Newtonsoft.Json;
using Discord.WebSocket;
using Discord;
using Discord.Rest;
using System.Threading.Tasks;

namespace BSB.Commands.DiscordAPI
{
    class Discord_Role_GetSettings : CoreCommand_SmartReply_3arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number", "Number" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Discord server id", "Role id" }; } }
        public override string Helpfile { get { return "returns a collection of settings for the given role \n This command requires Discord full client mode enabled and connected"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                KeyValuePair<bool, Dictionary<string, string>> reply = RoleSettings(args);
                return bot.GetCommandsInterface.SmartCommandReply(reply.Key, args[0], "see status for state", CommandName,reply.Value);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected KeyValuePair<bool, Dictionary<string, string>> RoleSettings(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                if (ulong.TryParse(args[2], out ulong roleid) == true)
                {
                    SocketGuild server = Discord.GetGuild(serverid);
                    SocketRole role = server.GetRole(roleid);
                    Dictionary<string, string> settings = new Dictionary<string, string>
                    {
                        { "Config|Color", String.Join(",", new byte[3] { role.Color.R, role.Color.G, role.Color.B }) },
                        { "Config|Name", role.Name },
                        { "Info|AssignedCount", role.Members.Count().ToString() },
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
            }
            return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
        }
    }
}
