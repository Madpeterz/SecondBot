using Discord;
using Discord.Rest;
using Discord.WebSocket;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SecondBotEvents.Services
{
    public class DiscordService : BotServices
    {
        protected Config.DiscordConfig myConfig = null;
        protected bool AcceptEventsFromSL = false;
        protected bool DiscordIsReady = false;
        protected bool DiscordDisconnectExpected = false;
        protected bool DiscordServerChannelsSetup = false;
        protected bool DiscordDoingLogin = false;
        protected DiscordSocketClient DiscordClient;

        public DiscordService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new Config.DiscordConfig(master.fromEnv, master.fromFolder);
        }

        public override string Status()
        {
            if (myConfig == null)
            {
                return "Config broken";
            }
            else if(myConfig.GetEnabled() == false)
            {
                return "Disabled";
            }
            else if(DiscordServerChannelsSetup == false)
            {
                return "Setting up channel";
            }
            else if(DiscordDoingLogin == true)
            {
                return "Logging in";
            }
            else if(DiscordIsReady == false)
            {
                return "Disconnected";
            }
            return "Active";
        }

        public bool DiscordReady()
        {
            return GetReadyForDiscordActions();
        }

        protected Task DiscordClientLoggedOut()
        {
            Console.WriteLine("Discord service [Logged out]");
            DiscordIsReady = false;
            return Task.CompletedTask;
        }

        protected Task DicordClientLoggedin()
        {
            Console.WriteLine("Discord service [Logged in]");
            DiscordDoingLogin = false;
            _ = DiscordClient.StartAsync();
            return Task.CompletedTask;   
        }

        protected Task DiscordClientReady()
        {
            Console.WriteLine("Discord service [Ready]");
            DiscordIsReady = true;
            DoServerChannelSetup();
            return Task.CompletedTask;
        }

        protected Task DiscordClientMessageReceived(SocketMessage message)
        {
            return Task.CompletedTask;
        }

        protected Task DiscordDisconnected(Exception e)
        {
            Console.WriteLine("Discord service [Disconnected: "+e.Message+"]");
            DiscordIsReady = false;
            if(DiscordDisconnectExpected == false)
            {
                Restart();
            }
            return Task.CompletedTask;
        }

        protected void StatusMessageUpdate(object o, SystemStatusMessage e)
        {
            if(GetReadyForDiscordActions() == false)
            {
                return;
            }

        }

        protected bool GetReadyForDiscordActions()
        {
            if (DiscordIsReady == false)
            {
                return false;
            }
            if (DiscordServerChannelsSetup == false)
            {
                return false;
            }
            return true;
        }

        public override void Start()
        {
            if(myConfig.GetEnabled() == false)
            {
                Console.WriteLine("Discord service [Disabled]");
                return;
            }
            if(myConfig.GetServerID() == 0)
            {
                Console.WriteLine("Discord service [Invaild server id]");
                return;
            }
            DiscordServerChannelsSetup = false;
            DiscordDisconnectExpected = false;
            DiscordIsReady = false;
            Console.WriteLine("Discord service [Starting]");
            master.SystemStatusMessagesEvent += SystemStatusEvent;
            master.BotClientNoticeEvent += BotClientRestart;
            master.SystemStatusMessagesEvent += StatusMessageUpdate;
            DiscordClient = new DiscordSocketClient();
            DiscordClient.Ready += DiscordClientReady;
            DiscordClient.LoggedOut += DiscordClientLoggedOut;
            DiscordClient.LoggedIn += DicordClientLoggedin;
            DiscordClient.MessageReceived += DiscordClientMessageReceived;
            DiscordClient.Disconnected += DiscordDisconnected;
            _ = DiscordClient.LoginAsync(TokenType.Bot, myConfig.GetClientToken()).ConfigureAwait(false);
        }

        public override void Stop()
        {
            Console.WriteLine("Discord service [Stopping]");
            master.BotClientNoticeEvent -= BotClientRestart;
            master.SystemStatusMessagesEvent -= StatusMessageUpdate;
            AcceptEventsFromSL = false;
            if(DiscordClient != null)
            {
                DiscordClient.Dispose();
                DiscordClient = null;
            }
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isRestart == false)
            {
                Console.WriteLine("Discord service [Avi link]");
                AcceptEventsFromSL = false;
                GetClient().Network.LoggedOut += BotLoggedOut;
                GetClient().Self.ChatFromSimulator -= LocalChat;
                GetClient().Self.IM -= BotImMessage;
                GetClient().Network.SimConnected += BotLoggedIn;
            }

        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            GetClient().Network.SimConnected += BotLoggedIn;
            GetClient().Self.ChatFromSimulator -= LocalChat;
            GetClient().Self.IM -= BotImMessage;
            AcceptEventsFromSL = false;
            Console.WriteLine("Discord service [Avi link - standby]");
        }

        protected void BotImMessage(object o, InstantMessageEventArgs e)
        {
            if(AcceptEventsFromSL == false)
            {
                return;
            }
            switch (e.IM.Dialog)
            {
                case InstantMessageDialog.MessageFromObject:
                    {
                        ObjectIMChat(e.IM.FromAgentName, e.IM.Message);
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        if(e.IM.GroupIM == false)
                        {
                            AvatarIMChat(e.IM.FromAgentID, e.IM.FromAgentName, e.IM.Message);
                            break;
                        }
                        GroupIMChat(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.Message);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        protected Dictionary<string, ulong> ChannelMap = new Dictionary<string, ulong>();
        protected Dictionary<string, ulong> CategoryMap = new Dictionary<string, ulong>();

        protected async void DoServerChannelSetup()
        {
            LogFormater.Info("Setting up channels");
            Dictionary<string, string> WantedTextChannels = new Dictionary<string, string>
            {
                { "status", StatusPrefill() },
                { "commands", "Send a command to the bot as if you are a master" },
                { "localchat", "Talk and view localchat" }
            };

            SocketGuild server = DiscordClient.GetGuild(myConfig.GetServerID());
            foreach (ITextChannel channel in server.TextChannels)
            {
                if(WantedTextChannels.ContainsKey(channel.Name) == false)
                {
                    continue;
                }
                if(channel.Name != "commands")
                {
                    CleanChannel(channel);
                }
                WantedTextChannels.Remove(channel.Name);
                ChannelMap.Add(channel.Name, channel.Id);
            }

            List<string> WantedCategoryChannels = new List<string>
            {
                "Bot",
                "IM",
                "Group"
            };
            foreach (ICategoryChannel category in server.CategoryChannels)
            {
                if (WantedCategoryChannels.Contains(category.Name) == false)
                {
                    continue;
                }
                WantedCategoryChannels.Remove(category.Name);
                CategoryMap.Add(category.Name, category.Id);
            }
            foreach(String a in WantedCategoryChannels)
            {
                RestCategoryChannel reply = await server.CreateCategoryChannelAsync(a);
                CategoryMap.Add(a, reply.Id);
            }
            foreach(KeyValuePair<string,string> a in WantedTextChannels)
            {
                createTextChannel(a.Key, a.Value, "Bot");
            }
            DiscordServerChannelsSetup = true;
        }

        protected async void createTextChannel(string ChannelName, string ChannelInfo,  string Group)
        {
            SocketGuild server = DiscordClient.GetGuild(myConfig.GetServerID());
            IGuildChannel channel = await server.CreateTextChannelAsync(ChannelName, X => DiscordGetNewChannelProperies(X, ChannelName, ChannelInfo, Group));
            ChannelMap.Add(channel.Name, channel.Id);
        }

        protected void DiscordGetNewChannelProperies(TextChannelProperties C, string channelname, string channeltopic, string catname)
        {
            if (catname != null)
            {
                if (CategoryMap.ContainsKey(catname) == true)
                {
                    C.CategoryId = CategoryMap[catname];
                }
            }
            C.Name = channelname;
            C.Topic = channeltopic;
        }

        protected void GroupIMChat(UUID group, string name, string message)
        {
        }

        protected void AvatarIMChat(UUID avatar, string name, string message)
        {

        }

        protected void ObjectIMChat(string objectName, string message)
        {

        }

        protected void StatusChat(string message)
        {
        }

        readonly string[] hard_blocked_agents = new[] { "secondlife", "second life" };
        protected void LocalChat(object o, ChatEventArgs e)
        {
            switch (e.Type)
            {
                case ChatType.OwnerSay:
                case ChatType.Whisper:
                case ChatType.Normal:
                case ChatType.Shout:
                case ChatType.RegionSayTo:
                {
                        if (hard_blocked_agents.Contains(e.FromName.ToLowerInvariant()) == true)
                        {
                            break;
                        }
                        string source = ":person_bald:";
                        if (e.SourceType == ChatSourceType.Object)
                        {
                            source = ":diamond_shape_with_a_dot_inside:";
                        }
                        else if (e.SourceType == ChatSourceType.System)
                        {
                            source = ":comet:";
                        }
                        if(e.Type == ChatType.OwnerSay)
                        {
                            source = ":robot:";
                        }
                        else if (e.Type == ChatType.RegionSayTo)
                        {
                            source = ":dart:";
                        }
                        SendMessageToChannel("localchat", "Talk and view localchat", LogFormater.GetClockStamp()+" "+source + "" + e.FromName + ": " + e.Message);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        protected bool AcceptMessageSigned(string message)
        {
            return false;
        }

        public ulong SendMessageToChannel(string ChannelName, string ChannelInfo, string message)
        {
            return SendMessageToChannel(GetChannel(ChannelName, ChannelInfo, message), message);
        }

        public ulong SendMessageToChannel(ITextChannel channel, string message)
        {
            IMessage reply = channel.SendMessageAsync(message).GetAwaiter().GetResult();
            return reply.Id;
        }

        protected ITextChannel GetChannel(string ChannelName, string ChannelInfo, string Group)
        {
            if(ChannelMap.ContainsKey(ChannelName) == true)
            {
                return (ITextChannel) DiscordClient.GetChannel(ChannelMap[ChannelName]);
            }
            createTextChannel(ChannelName, ChannelInfo, Group);
            return (ITextChannel)DiscordClient.GetChannel(ChannelMap[ChannelName]);
        }

        protected ulong LastStatusMessageId = 0;
        protected string LastMessageContent = "";
        protected bool messageHasEndDot = false;
        protected long LastUpdatedSystemStatus = 0;

        protected async void SystemStatusEvent(object o, SystemStatusMessage e)
        {
            if(GetReadyForDiscordActions() == false)
            {
                return;
            }
            ITextChannel statusChannel = GetChannel("status", StatusPrefill(), "bot");
            if ((e.changed == false) && (LastStatusMessageId != 0))
            {
                long dif = SecondbotHelpers.UnixTimeNow() - LastUpdatedSystemStatus;
                if (dif < 15)
                {
                    return;
                }
                LastUpdatedSystemStatus = SecondbotHelpers.UnixTimeNow();

                if (messageHasEndDot == false)
                {
                    await statusChannel.
                        ModifyMessageAsync(LastStatusMessageId, m => m.Content = LastMessageContent + ".");
                    messageHasEndDot = true;
                    return;
                }
                await statusChannel.
                    ModifyMessageAsync(LastStatusMessageId, m => m.Content = LastMessageContent);
                messageHasEndDot = false;
                return;
            }
            LastStatusMessageId = SendMessageToChannel(statusChannel, e.message);
            LastMessageContent = e.message;
        }

        protected async void CleanChannel(ITextChannel channel)
        {
            IEnumerable<IMessage> messages = await channel.GetMessagesAsync(100, CacheMode.AllowDownload).FlattenAsync();
            IEnumerable<IMessage> filtered = messages.Where(x => (DateTimeOffset.UtcNow - x.Timestamp).TotalDays <= 14);
            if(filtered.Count() == 0)
            {
                return;
            }
            await channel.DeleteMessagesAsync(filtered);
        }

        protected string StatusPrefill()
        {
            return "State of the bot service";
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            AcceptEventsFromSL = true;
            GetClient().Network.SimConnected -= BotLoggedIn;
            GetClient().Self.IM += BotImMessage;
            GetClient().Self.ChatFromSimulator += LocalChat;
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


        public async Task<bool> AddRoleToMember(string givenserverid, string givenroleid, string givenmemberid)
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
