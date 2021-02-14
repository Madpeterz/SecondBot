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

namespace BetterSecondBot.Commands.DiscordAPI
{
    class Discord_Role_UpdatePerms : CoreCommand_4arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number", "Number","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Discord server id", "Role id","CSV list of PermFlag=Bool" }; } }
        public override string Helpfile { get { return "Updates perm flags for the selected role \n example CSV format: Speak=True,SendMessages=False \n for a full list of perms see output of DiscordRoleGetSettings \n This command requires Discord full client mode enabled and connected"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                bool status = UpdateRolePerms(args).Result;
                return bot.GetCommandsInterface.SmartCommandReply(status, args[0], "see status for update state", CommandName);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected GuildPermissions UpdateFlag(GuildPermissions NewPerms,string flag, bool flagValue)
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

        protected async Task<bool> UpdateRolePerms(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                if (ulong.TryParse(args[2], out ulong roleid) == true)
                {
                    SocketGuild server = Discord.GetGuild(serverid);
                    SocketRole role = server.GetRole(roleid);
                    string[] bits = args[3].Split(',');
                    GuildPermissions NewPerms = role.Permissions;
                    foreach (String bit in bits)
                    {
                        string[] subbits = bit.Split('=');
                        if(subbits.Length == 2)
                        {
                            if(bool.TryParse(subbits[1],out bool flagValue) == true)
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
            }
            return false;
        }
    }
}
