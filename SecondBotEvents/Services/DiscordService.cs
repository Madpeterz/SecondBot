using Discord;
using Discord.Rest;
using Discord.WebSocket;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SecondBotEvents.Services
{
    public class DiscordService : BotServices
    {
        public Config.DiscordConfig myConfig = null;
        public bool acceptChat = false;
        public bool discordIsReady = false;
        protected DiscordSocketClient DiscordClient;

        public DiscordService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new Config.DiscordConfig(master.fromEnv, master.fromFolder);
        }

        public bool DiscordReady()
        {
            if(acceptChat == false)
            {
                return false;
            }
            return discordIsReady;
        }

        protected Task DiscordClientLoggedOut()
        {
            Console.WriteLine("Discord service [Logged out]");
            discordIsReady = false;
            return Task.CompletedTask;
        }

        protected Task DicordClientLoggedin()
        {
            Console.WriteLine("Discord service [Logged in]");
            return Task.CompletedTask;   
        }

        protected Task DiscordClientReady()
        {
            Console.WriteLine("Discord service [Ready]");
            discordIsReady = true;
            return Task.CompletedTask;
        }

        protected Task DiscordClientMessageReceived(SocketMessage message)
        {
            return Task.CompletedTask;
        }

        protected Task DiscordDisconnected(Exception e)
        {
            Console.WriteLine("Discord service [Disconnected: "+e.Message+"]");
            discordIsReady = false;
            return Task.CompletedTask;
        }

        public async override void Start()
        {
            if(myConfig.GetEnabled() == false)
            {
                Console.WriteLine("Discord service [Disabled]");
                return;
            }
            Console.WriteLine("Discord service [Starting]");
            master.BotClientNoticeEvent += BotClientRestart;
            DiscordClient = new DiscordSocketClient();
            DiscordClient.Ready += DiscordClientReady;
            DiscordClient.LoggedOut += DiscordClientLoggedOut;
            DiscordClient.LoggedIn += DicordClientLoggedin;
            DiscordClient.MessageReceived += DiscordClientMessageReceived;
            DiscordClient.Disconnected += DiscordDisconnected;
            await DiscordClient.LoginAsync(TokenType.Bot, myConfig.GetClientToken());
        }

        public override void Stop()
        {
            Console.WriteLine("Discord service [Stopping]");
            master.BotClientNoticeEvent -= BotClientRestart;
            acceptChat = false;
            if(DiscordClient != null)
            {
                DiscordClient.Dispose();
                DiscordClient = null;
            }
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            Console.WriteLine("Discord service [Avi link - Restarting]");
            acceptChat = false;
            getClient().Network.LoggedOut += BotLoggedOut;
            getClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            getClient().Network.SimConnected += BotLoggedIn;
            acceptChat = false;
            Console.WriteLine("Discord service [Avi link - standby]");
        }

        protected void BotImMessage(object o, InstantMessageEventArgs e)
        {
            if(acceptChat == false)
            {
                return;
            }
            switch (e.IM.Dialog)
            {
                case InstantMessageDialog.MessageFromObject:
                    {
                        objectChat(e.IM.FromAgentName, e.IM.Message);
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        if(e.IM.GroupIM == false)
                        {
                            avatarChat(e.IM.FromAgentID, e.IM.FromAgentName, e.IM.Message);
                            break;
                        }
                        groupChat(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.Message);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }

        }

        protected void groupChat(UUID group, string name, string message)
        {
        }

        protected void avatarChat(UUID avatar, string name, string message)
        {
        }

        protected void objectChat(string objectName, string message)
        {
        }

        protected void statusChat(string message)
        {
        }

        protected bool acceptMessageSigned(string message)
        {
            return false;
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            acceptChat = true;
            getClient().Network.SimConnected -= BotLoggedIn;
            getClient().Self.IM += BotImMessage;
            Console.WriteLine("Discord service [Avi link - Active]");
        }

        #region discord helper functions

        public KeyValuePair<bool, Dictionary<string, string>> ListTextChannels(string givenserverid)
        {
            if (ulong.TryParse(givenserverid, out ulong serverid) == true)
            {
                return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
            Dictionary<string, string> textChannels = new Dictionary<string, string>();
            foreach (SocketTextChannel channel in server.TextChannels)
            {
                textChannels.Add(channel.Id.ToString(), channel.Name);
            }
            return new KeyValuePair<bool, Dictionary<string, string>>(true, textChannels);
        }

        public async Task<bool> RemoveRole(string givenserverid, string givenroleid)
        {
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenroleid, out ulong roleid) == false)
            {
                return false;
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
            SocketRole role = server.GetRole(roleid);
            await role.DeleteAsync().ConfigureAwait(true);
            return true;
        }

        public KeyValuePair<bool, Dictionary<string, string>> ListRoles(string givenserverid)
        {
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
            Dictionary<string, string> rolesList = new Dictionary<string, string>();
            foreach (SocketRole role in server.Roles)
            {
                rolesList.Add(role.Id.ToString(), role.Name);
            }
            return new KeyValuePair<bool, Dictionary<string, string>>(true, rolesList);
        }

        public async Task<KeyValuePair<string, ulong>> CreateRole(string givenserverid, string givenrole)
        {
            if (ulong.TryParse(givenserverid, out ulong serverid) == true)
            {
                return new KeyValuePair<string, ulong>("Unable to process server id", 0);
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
            RestRole role = await server.CreateRoleAsync(givenrole, new GuildPermissions(), Color.DarkRed, false, false).ConfigureAwait(true);
            return new KeyValuePair<string, ulong>("ok", role.Id);
        }

        public async Task<bool> UpdateRolePerms(string givenserverid, string givenroleid, string givenflagscsv)
        {
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenroleid, out ulong roleid) == true)
            {
                return false;
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
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
                case "ManageEmojisAndStickers":
                    NewPerms = NewPerms.Modify(null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, flagValue);
                    break;
                default:
                    break;
            }
            return NewPerms;
        }

        public KeyValuePair<bool, Dictionary<string, string>> RoleSettings(string givenserverid, string givenroleid)
        {
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
            }
            if (ulong.TryParse(givenroleid, out ulong roleid) == false)
            {
                return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
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
                { "Perm|ManageEmojisAndStickers", role.Permissions.ManageEmojisAndStickers.ToString() }
            };
            return new KeyValuePair<bool, Dictionary<string, string>>(true, settings);
        }

        public async Task<bool> MuteMember(string givenserverid, string givenmemberid, string mode)
        {
            if (ulong.TryParse(givenserverid, out ulong serverid) == true)
            {
                return false;
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == true)
            {
                return false;
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
            SocketGuildUser user = server.GetUser(memberid);
            bool.TryParse(mode, out bool status);
            await user.ModifyAsync(pr =>
            {
                pr.Mute = status;
            });
            return true;
        }

        public async Task<bool> MessageChannel(string givenserverid, string givenchannelid, string givenmessage, string tts)
        {
            if (bool.TryParse(tts, out bool useTTS) == false)
            {
                return false;
            }
            return await SendMessageToDiscord(givenserverid, givenchannelid, givenmessage, useTTS);
        }

        protected async Task<bool> SendMessageToDiscord(string target_serverid, string target_channelid, string message, bool useTTS)
        {
            if (ulong.TryParse(target_serverid, out ulong serverid) == true)
            {
                if (ulong.TryParse(target_channelid, out ulong channelid) == true)
                {
                    SocketGuild server = DiscordClient.GetGuild(serverid);
                    SocketTextChannel Channel = null;
                    foreach (SocketTextChannel channel in server.TextChannels)
                    {
                        if (channel.Id == channelid)
                        {
                            Channel = channel;
                            break;
                        }
                    }
                    if (Channel != null)
                    {
                        await Channel.SendMessageAsync(message, useTTS);
                        return true;
                    }
                }
            }
            return false;
        }

        public async Task<KeyValuePair<bool, Dictionary<string, string>>> ListMembers(string givenserverid)
        {
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
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


        public async Task<bool> MessageMember(string givenserverid, string givenmemberid, string givenmessage)
        {
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
                SocketGuild server = DiscordClient.GetGuild(serverid);
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

        public async Task<KeyValuePair<bool, int>> ClearMessages(string givenserverid, string givenmemberid)
        {
            int messagesDeleted = 0;
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return new KeyValuePair<bool, int>(false, 0);
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == false)
            {
                return new KeyValuePair<bool, int>(false, 0);
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
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



        public async Task<bool> BanMember(string givenserverid, string givenmemberid, string why)
        {
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return false;
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == false)
            {
                return false;
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
            SocketGuildUser user = server.GetUser(memberid);
            await user.BanAsync(7, why);
            return true;
        }


        public async Task<bool> addRoleToMember(string givenserverid, string givenroleid, string givenmemberid)
        {
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
            SocketGuild server = DiscordClient.GetGuild(serverid);
            SocketRole role = server.GetRole(roleid);
            SocketGuildUser user = server.GetUser(memberid);
            await user.AddRoleAsync(role); // ? Irole seems to accept SocketRole ?
            return true;
        }


        #endregion
    }
}
