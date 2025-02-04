using Discord;
using Discord.Rest;
using Discord.WebSocket;
using OpenMetaverse;
using SecondBotEvents.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SecondBotEvents.Services
{
    public class DiscordService : BotServices
    {
        protected new Config.DiscordConfig myConfig = null;
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
                return "No Config";
            }
            else if (myConfig.GetHideStatusOutput() == true)
            {
                return "hidden";
            }
            else if (myConfig.GetEnabled() == false)
            {
                return "Disabled";
            }
            else if (DiscordServerChannelsSetup == false)
            {
                return "Setting up channel";
            }
            else if (DiscordDoingLogin == true)
            {
                return "Logging in";
            }
            else if (DiscordIsReady == false)
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
            LogFormater.Info("Discord service [Logged out]");
            DiscordIsReady = false;
            return Task.CompletedTask;
        }

        protected Task DicordClientLoggedin()
        {
            LogFormater.Info("Discord service [Logged in]");
            DiscordDoingLogin = false;
            _ = DiscordClient.StartAsync();
            return Task.CompletedTask;
        }

        protected Task DiscordClientReady()
        {
            LogFormater.Info("Discord service [Ready]");
            DiscordIsReady = true;
            DoServerChannelSetup();
            if (master.BotClient.IsConnected() == true)
            {
                BotLoggedIn(this, new SimConnectedEventArgs(GetClient().Network.CurrentSim));
            }
            return Task.CompletedTask;
        }

        private readonly object m_discordEventChatLock = new object();
        private EventHandler<SocketMessage> m_discordChatEvent;

        /// <summary>Raised when a scripted object or agent within range sends a public message</summary>
        public event EventHandler<SocketMessage> DiscordMessageEvent
        {
            add { lock (m_discordEventChatLock) { m_discordChatEvent += value; } }
            remove { lock (m_discordEventChatLock) { m_discordChatEvent -= value; } }
        }

        public KeyValuePair<bool, List<string>> DiscordMemerRoles(string givenserverid, string givenmemberid)
        {
            if (GetReadyForDiscordActions() == false)
            {
                // bot is not ready this message should not have got here yet.
                return new KeyValuePair<bool, List<string>>(false, new List<string>() { "not ready" });
            }
            if (ulong.TryParse(givenserverid, out ulong serverid) == false)
            {
                return new KeyValuePair<bool, List<string>>(false, new List<string>() { "invaild server" });
            }
            if (ulong.TryParse(givenmemberid, out ulong memberid) == false)
            {
                return new KeyValuePair<bool, List<string>>(false, new List<string>() { "invaild member" });
            }
            SocketGuild server = DiscordClient.GetGuild(serverid);
            if (server == null)
            {
                return new KeyValuePair<bool, List<string>>(false, new List<string>() { "Cant get server" });
            }
            SocketGuildUser user = server.GetUser(memberid);
            if (user == null)
            {
                return new KeyValuePair<bool, List<string>>(false, new List<string>() { "Cant get user" });
            }
            List<string> roleids = new List<string>();
            foreach (SocketRole role in user.Roles)
            {
                roleids.Add(role.Id.ToString());
            }
            return new KeyValuePair<bool, List<string>>(true, roleids);
        }

        protected HttpClient HTTPclient = new HttpClient();
        protected bool MessageInteractionEvent(SocketMessage message, SocketChannel socketChannel)
        {
            if (myConfig.GetInteractionEnabled() != true)
            {
                return false;
            }
            if (myConfig.GetInteractionChannelNumber() != socketChannel.Id.ToString())
            {
                return false;
            }
            if (message.Content.StartsWith(myConfig.GetInteractionCommandName()) == false)
            {
                return false;
            }
            if (myConfig.GetInteractionHttpTarget().StartsWith("http") == false)
            {
                myConfig.SetInteractionEnabled(false);
                return false;
            }
            string getmessage = message.Content.Replace(myConfig.GetInteractionCommandName(), "");
            long unixtime = SecondbotHelpers.UnixTimeNow();
            string hash = SecondbotHelpers.GetSHA1(unixtime.ToString() + getmessage + master.CommandsService.myConfig.GetSharedSecret());
            Dictionary<string, string> values = new Dictionary<string, string>
                    {
                        { "message", getmessage },
                        { "username", message.Author.Username },
                        { "userid", message.Author.Id.ToString() },
                        { "unixtime", unixtime.ToString() },
                        { "hash", hash }
                    };
            var content = new FormUrlEncodedContent(values);
            try
            {
                HTTPclient.PostAsync(myConfig.GetInteractionHttpTarget(), content);
                _ = MarkMessage((IUserMessage)message, "✅");
            }
            catch (Exception e)
            {
                LogFormater.Crit("[MessageInteractionEvent] HTTP failed: " + e.Message + "");
                _ = MarkMessage((IUserMessage)message, "❌");
                return false;
            }
            return true;

        }

        protected Task DiscordClientMessageReceived(SocketMessage message)
        {
            if (GetReadyForDiscordActions() == false)
            {
                // bot is not ready this message should not have got here yet.
                return Task.CompletedTask;
            }
            SocketChannel socketChannel = DiscordClient.GetChannel(message.Channel.Id);
            if (MessageInteractionEvent(message, socketChannel) == true)
            {
                return Task.CompletedTask;
            }
            if (socketChannel.GetChannelType() != Discord.ChannelType.Text)
            {
                // bot only does stuff on text channels.
                return Task.CompletedTask;
            }
            ITextChannel TextChannel = (ITextChannel)socketChannel;
            if (TextChannel.CategoryId == null)
            {
                // bot only does stuff on text channels that are in a category
                return Task.CompletedTask;
            }
            if (ulong.TryParse(TextChannel.CategoryId.ToString(), out ulong CategoryId) == false)
            {
                // a value that is null or ulong, that was not null is some how not a ulong I have no idea
                // whats going on anymore :/ this should be dead code.
                return Task.CompletedTask;
            }
            if (CategoryMap.ContainsValue(CategoryId) == false)
            {
                // message is not on a Category the bot looks after so we ignore it.
                return Task.CompletedTask;
            }
            if (ChannelMap.ContainsKey(message.Channel.Name) == false)
            {
                // unknown channel add it to map
                ChannelMap.Add(message.Channel.Name, message.Channel.Id);
            }
            string CatName = MapCatToName(CategoryId);
            switch (CatName)
            {
                case "bot":
                    {
                        HandleDiscordBotInput(TextChannel, message);
                        break;
                    }
                case "im":
                    {
                        HandleDiscordAvatarImInput(TextChannel, message);
                        break;
                    }
                case "group":
                    {
                        HandleDiscordGroupImInput(TextChannel, message);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            return Task.CompletedTask;
        }

        protected async Task<Task> MarkMessage(IUserMessage message, string emote)
        {
            var Tickmark = new Emoji(emote);
            await message.AddReactionAsync(Tickmark, RequestOptions.Default);
            return Task.CompletedTask;
        }

        protected void ReplyToMessage(IMessage message, string reply)
        {
            if(reply.Length > 2000)
            {
                reply = reply.Substring(0, 1900)+"...";
            }
            message.Channel.SendMessageAsync(reply, false, null, null, null, new MessageReference(message.Id));
        }

        protected void HandleDiscordGroupImInput(ITextChannel TextChannel, SocketMessage message)
        {
            if (AcceptEventsFromSL == false)
            {
                _ = message.DeleteAsync();
                return;
            }
            if (message.Author.Id == DiscordClient.CurrentUser.Id)
            {
                return;
            }
            // group=UUID||| !clear !notice {Title}@@@{Message}[@@@{Inventory UUID}] !eject {Avatar} !invite {Avatar}
            string[] bits = TextChannel.Topic.Split("|||", StringSplitOptions.RemoveEmptyEntries);
            // group=UUID
            // !clear !notice {Title}@@@{Message}[@@@{Inventory UUID}] !eject {Avatar} !invite {Avatar} !ban {Avatar}
            if (bits.Length != 2)
            {
                _ = TextChannel.DeleteAsync();
                return;
            }
            bits = bits[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            // group
            // UUID
            if (bits[0] != "group")
            {
                _ = TextChannel.DeleteAsync();
                return;
            }
            if (UUID.TryParse(bits[1], out UUID groupuuid) == false)
            {
                _ = TextChannel.DeleteAsync();
                return;
            }
            if(GetClient().Groups.GroupName2KeyCache.ContainsKey(groupuuid) == false)
            {
                _ = MarkMessage((IUserMessage)message, "❌");
                ReplyToMessage(message, "Unknown group [Maybe im still loading]");
                return;
            }
            string cleaned = message.Content.Trim();
            if(cleaned == "!clear")
            {
                CleanChannel(TextChannel);
                master.DataStoreService.CloseChat(groupuuid);
                return;
            }
            else if(cleaned.StartsWith("!notice") == true)
            {
                // handle group notice
                // !notice {Title}@@@{Message}[@@@{Inventory UUID}]
                List<string> args = new List<string>();
                args.Add(groupuuid.ToString());
                cleaned = cleaned.Replace("!notice ", "");
                bits = cleaned.Split("@@@", StringSplitOptions.RemoveEmptyEntries);
                args.AddRange(bits);
                if (bits.Length < 2)
                {
                    _ = MarkMessage((IUserMessage)message, "❌");
                    ReplyToMessage(message, "Notices require title and a message split with @@@!");
                    return;
                }
                SignedCommand C = new SignedCommand(
                    master.CommandsService,
                    "discordapi",
                    "Groupnotice", null, args.ToArray(), 0, "none", false, 0, "", false);
                if (bits.Length == 3)
                {
                    C.command = "GroupnoticeWithAttachment";
                }
                RunCommandFromMessage(C, message);
                return;
            }
            else if(cleaned.StartsWith("!eject") == true)
            {
                // handle group eject
                cleaned = cleaned.Replace("!eject ", "");
                List<string> args = new List<string>();
                args.Add(groupuuid.ToString());
                args.Add(cleaned);
                RunCommandFromMessage(new SignedCommand(master.CommandsService,
                    "discordapi", "GroupEject", null, args.ToArray(), 0, "none", false, 0, "", false), message);
                return;
            }
            else if(cleaned.StartsWith("!invite") == true)
            {
                // hangle group invite
                cleaned = cleaned.Replace("!invite ", "");
                List<string> args = new List<string>();
                args.Add(groupuuid.ToString());
                args.Add(cleaned);
                args.Add("everyone");
                RunCommandFromMessage(new SignedCommand(master.CommandsService,
                    "discordapi", "GroupInvite", null, args.ToArray(), 0, "none", false, 0, "", false), message);
                return;
            }
            else if(cleaned.StartsWith("!ban") == true)
            {
                // hangle group ban
                cleaned = cleaned.Replace("!ban ", "");
                List<string> args = new List<string>();
                args.Add(groupuuid.ToString());
                args.Add(cleaned);
                args.Add("true");
                RunCommandFromMessage(new SignedCommand(master.CommandsService,
                    "discordapi", "GroupBan", null, args.ToArray(), 0, "none", false, 0, "", false), message);
                return;
            }
            if (myConfig.GethideChatterName() == false)
            {
                GetClient().Self.InstantMessageGroup(groupuuid, message.Author.Username + ": " + message.CleanContent);
            }
            else
            {
                GetClient().Self.InstantMessageGroup(groupuuid, message.CleanContent);
            }
            _ = message.DeleteAsync();
        }

        protected void RunCommandFromChat(SocketMessage message)
        {
            KeyValuePair<bool, string> reply = master.CommandsService.CommandInterfaceCaller(message.Content, false, false, "DiscordAPI");
            string replyEmote = "✅";
            if (reply.Key == false)
            {
                replyEmote = "❌";
            }
            _ = MarkMessage((IUserMessage)message, replyEmote);
            ReplyToMessage(message, reply.Value);
        }

        protected void RunCommandFromMessage(SignedCommand C, SocketMessage message)
        {
            KeyValuePair<bool, string> reply = master.CommandsService.RunCommand("Discord",C);
            string replyEmote = "✅";
            if (reply.Key == false)
            {
                replyEmote = "❌";
            }
            _ = MarkMessage((IUserMessage)message, replyEmote);
            ReplyToMessage(message, reply.Value);
        }

        protected void HandleDiscordAvatarImInput(ITextChannel TextChannel, SocketMessage message)
        {
            if (AcceptEventsFromSL == false)
            {
                _ = message.DeleteAsync();
                return;
            }
            if (message.Author.Id == DiscordClient.CurrentUser.Id)
            {
                return;
            }
            // avatar=UUID||| !close !offertp !clear
            string[] bits = TextChannel.Topic.Split("|||", StringSplitOptions.RemoveEmptyEntries);
            // avatar=UUID
            // !close !offertp !clear
            if (bits.Length != 2)
            {
                _ = TextChannel.DeleteAsync();
                return;
            }
            bits = bits[0].Split("=", StringSplitOptions.RemoveEmptyEntries);
            // avatar
            // UUID
            if (bits[0] != "avatar")
            {
                _ = TextChannel.DeleteAsync();
                return;
            }
            if (UUID.TryParse(bits[1], out UUID avataruuid) == false)
            {
                _ = TextChannel.DeleteAsync();
                return;
            }
            string cleaned = message.Content.Trim();
            if (cleaned == "!close")
            {
                master.DataStoreService.CloseChat(avataruuid);
                _ = TextChannel.DeleteAsync();
                return;
            }
            else if (cleaned == "!clear")
            {
                CleanChannel(TextChannel);
                return;
            }
            else if (cleaned == "!offertp")
            {
                _ = MarkMessage((IUserMessage)message, "✅");
                GetClient().Self.SendTeleportLure(avataruuid);
                return;
            }
            if (myConfig.GethideChatterName() == false)
            {
                master.DataStoreService.GetAvatarName(avataruuid); // add avatar to lookup service just in case.
                GetClient().Self.InstantMessage(avataruuid, message.Author.Username + ": " + message.CleanContent);
            }
            else
            {
                GetClient().Self.InstantMessage(avataruuid, message.CleanContent);
            }
            _ = MarkMessage((IUserMessage)message, "✅");
        }

        protected void HandleDiscordBotInput(ITextChannel TextChannel, SocketMessage message)
        {
            if (TextChannel.Name == "status")
            {
                if (message.Author.Id != DiscordClient.CurrentUser.Id)
                {
                    _ = message.DeleteAsync();
                }
                return;
            }
            else if (TextChannel.Name == "commands")
            {
                if(AcceptEventsFromSL == false)
                {
                    _ = message.DeleteAsync();
                    return;
                }
                if (message.Author.Id != DiscordClient.CurrentUser.Id)
                {
                    // do command
                    RunCommandFromChat(message);
                    return;
                }
            }
            else if (TextChannel.Name == "localchat")
            {
                if (AcceptEventsFromSL == false)
                {
                    _ = message.DeleteAsync();
                    return;
                }
                if (message.Author.Id != DiscordClient.CurrentUser.Id)
                {
                    string cleaned = message.Content.Trim();
                    if (cleaned == "!clear")
                    {
                        CleanChannel(TextChannel);
                        return;
                    }
                    GetClient().Self.Chat(message.CleanContent, 0, ChatType.Normal);
                    _ = message.DeleteAsync();
                }
            }
        }

        protected string MapCatToName(ulong CatID)
        {
            string reply = "none";
            foreach(KeyValuePair<string, ulong> entry in CategoryMap)
            {
                if(entry.Value == CatID)
                {
                    reply = entry.Key;
                    break;
                }
            }
            return reply;
        }

        protected Task DiscordDisconnected(Exception e)
        {
            LogFormater.Info("Discord service [Disconnected: "+e.Message+"]");
            DiscordIsReady = false;
            if(DiscordDisconnectExpected == false)
            {
                Restart();
            }
            return Task.CompletedTask;
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

        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            if (updateEnabled)
            {
                myConfig.setEnabled(setEnabledTo);
            }
            running = true;
            if (myConfig.GetEnabled() == false)
            {
                LogFormater.Info("Discord service [Disabled]");
                return;
            }
            if(myConfig.GetServerID() == 0)
            {
                LogFormater.Info("Discord service [Invaild server id]");
                return;
            }
            DiscordServerChannelsSetup = false;
            DiscordDisconnectExpected = false;
            DiscordIsReady = false;
            LogFormater.Info("Discord service [Starting]");
            master.SystemStatusMessagesEvent += SystemStatusEvent;
            master.BotClientNoticeEvent += BotClientRestart;
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildIntegrations | GatewayIntents.MessageContent
            };
            DiscordClient = new DiscordSocketClient(config);
            DiscordClient.Ready += DiscordClientReady;
            DiscordClient.LoggedOut += DiscordClientLoggedOut;
            DiscordClient.LoggedIn += DicordClientLoggedin;
            DiscordClient.MessageReceived += DiscordClientMessageReceived;
            DiscordClient.Disconnected += DiscordDisconnected;
            _ = DiscordClient.LoginAsync(TokenType.Bot, myConfig.GetClientToken()).ConfigureAwait(false);
        }

        protected Task DiscordInteraction(SocketMessageCommand msg)
        {
            if (myConfig.GetInteractionEnabled() == false)
            {
                return Task.CompletedTask;
            }
            if (msg.CommandName != myConfig.GetInteractionCommandName())
            {
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }

        public override void Stop()
        {
            if(running == false)
            {
                return;
            }
            LogFormater.Info("Discord service [Stopping]");
            running = false;
            if (master == null)
            {
                LogFormater.Info("Discord service [lost connection to master]");
            }
            else
            {
                master.BotClientNoticeEvent -= BotClientRestart;
                master.SystemStatusMessagesEvent -= SystemStatusEvent;
            }
            AcceptEventsFromSL = false;
            if (DiscordClient != null)
            {
                LogFormater.Info("Discord service [Discord client cleanup]");
                DiscordClient.Dispose();
                DiscordClient = null;
            }
            LogFormater.Info("Discord service [Stop done]");
        }

        protected void GroupCurrent(object o, CurrentGroupsEventArgs e)
        {
            if(GetReadyForDiscordActions() == false)
            {
                return;
            }
            lock (ChannelMap)
            {
                foreach (KeyValuePair<UUID, OpenMetaverse.Group> a in e.Groups)
                {
                    GetChannel(
                        a.Value.Name,
                        GroupPrefill(a.Value.ID),
                        "group",
                        true
                    );
                }
            }
        }


        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isStart == false)
            {
                return;
            }
            LogFormater.Info("Discord service [Avi link - waiting]");
            AcceptEventsFromSL = false;
            LoginEventsAttached = false;
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Self.ChatFromSimulator -= LocalChat;
            GetClient().Self.IM -= BotImMessage;
            GetClient().Groups.CurrentGroups -= GroupCurrent;
            GetClient().Network.SimConnected += BotLoggedIn;

        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            LoginEventsAttached = false;
            if (GetClient() != null)
            {
                GetClient().Network.SimConnected += BotLoggedIn;
                GetClient().Self.ChatFromSimulator -= LocalChat;
                GetClient().Self.IM -= BotImMessage;
                GetClient().Groups.CurrentGroups -= GroupCurrent;
            }
            AcceptEventsFromSL = false;
            LogFormater.Info("Discord service [Avi link - lost connection to SL]");
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
                        if(master.DataStoreService.GetIsGroup(e.IM.IMSessionID) == false)
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

        protected string GroupPrefill(UUID groupuuid)
        {
            return "group=" + groupuuid.ToString() + "||| !clear !notice {Title}@@@{Message}[@@@{Inventory UUID}] !eject {Avatar} !invite {Avatar} !ban {Avatar}";
        }

        protected Dictionary<string, ulong> ChannelMap = new Dictionary<string, ulong>();
        protected Dictionary<string, ulong> CategoryMap = new Dictionary<string, ulong>();

        protected async void DoServerChannelSetup()
        {
            LogFormater.Info("Setting up channels");
            LoginEvents();
            Dictionary<string, string> WantedTextChannels = new Dictionary<string, string>
            {
                { "status", StatusPrefill() },
                { "commands", "Send a command to the bot as if you are a master" },
                { "localchat", LocalChatPrefill() }
            };
            ChannelMap = new Dictionary<string, ulong>();

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
                if (ChannelMap.ContainsKey(channel.Name) == true)
                {
                    ChannelMap[channel.Name] = channel.Id;
                    continue;
                }
                ChannelMap.Add(channel.Name, channel.Id);
                

            }

            List<string> WantedCategoryChannels = new List<string>
            {
                "bot",
                "im",
                "group"
            };
            foreach (ICategoryChannel category in server.CategoryChannels)
            {
                if (WantedCategoryChannels.Contains(category.Name) == false)
                {
                    continue;
                }
                WantedCategoryChannels.Remove(category.Name);
                if (CategoryMap.ContainsKey(category.Name) == true)
                {
                    CategoryMap[category.Name] = category.Id;
                    continue;
                }
                CategoryMap.Add(category.Name, category.Id);
            }
            foreach(String a in WantedCategoryChannels)
            {
                RestCategoryChannel reply = await server.CreateCategoryChannelAsync(a);
                CategoryMap.Add(a, reply.Id);
            }
            foreach(KeyValuePair<string,string> a in WantedTextChannels)
            {
                createTextChannel(a.Key, a.Value, "bot");
            }
            DiscordServerChannelsSetup = true;
        }

        protected void createTextChannel(string ChannelName, string ChannelInfo,  string Group, bool SkipDelay = false)
        {
            SocketGuild server = DiscordClient.GetGuild(myConfig.GetServerID());
            Group = Group.ToLower();
            IGuildChannel channel = server.CreateTextChannelAsync(ChannelName, X => DiscordGetNewChannelProperies(X, ChannelName, ChannelInfo, Group)).GetAwaiter().GetResult();
            lock (ChannelMap)
            {
                if (ChannelMap.ContainsKey(ChannelName) == true)
                {
                    ChannelMap.Remove(ChannelName);
                }
                ChannelMap.Add(ChannelName, channel.Id);
            }
            if (SkipDelay == false)
            {
                Thread.Sleep(500); // wait 300ms for channel to be ready
            }
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
            string groupname = master.DataStoreService.GetGroupName(group);
            if(groupname == "lookup")
            {
                return;
            }
            SendMessageToChannel(groupname, GroupPrefill(group), name + ": " + message, "group");
        }

        protected void AvatarIMChat(UUID avatar, string name, string message)
        {
            SendMessageToChannel(name.ToLower().Replace(" ",""), "avatar=" + avatar.ToString() + "||| !close !offertp !clear", name+":"+message, "im");
        }

        protected void ObjectIMChat(string objectName, string message)
        {
            SendMessageToChannel("localchat", LocalChatPrefill(), ":white_small_square: "+ objectName + ": " + message, "bot");
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
                        else if(e.FromName == GetClient().Self.Name)
                        {
                            source = ":disguised_face:";
                        }
                        SendMessageToChannel("localchat", LocalChatPrefill(), source + "" + e.FromName + ": " + e.Message, "bot");
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        protected string LocalChatPrefill()
        {
            return "Talk and view localchat !clear";
        }

        protected bool AcceptMessageSigned(string message)
        {
            return false;
        }

        public ulong SendMessageToChannel(string ChannelName, string ChannelInfo, string message, string group)
        {
            return SendMessageToChannel(GetChannel(ChannelName, ChannelInfo, group), message);
        }

        public ulong SendMessageToChannel(ITextChannel channel, string message)
        {
            IMessage reply = channel.SendMessageAsync(message).GetAwaiter().GetResult();
            return reply.Id;
        }

        public ulong? SendMessageToChannel(ulong serverid, ulong channelid, string message)
        {
            try
            {
                SocketGuild server = DiscordClient.GetGuild(serverid);
                ITextChannel channel = server.GetTextChannel(channelid);
                IMessage reply = channel.SendMessageAsync(message).GetAwaiter().GetResult();
                return reply.Id;
            }
            catch (Exception e)
            {
                LogFormater.Crit("Discord service: Failed to send message to channel "+channelid.ToString()+" on server: "+serverid.ToString()+" because: "+e.Message);
                return null;
            }
        }

        protected string GetChannelName(string input)
        {
            input = input.Replace("-", "");
            string DiscordChannelName = input.ToLower();
            DiscordChannelName = Regex.Replace(DiscordChannelName, "[^a-z0-9 ]", "");
            DiscordChannelName = DiscordChannelName.Replace(" ", "-");
            string old = "";
            while (old != DiscordChannelName)
            {
                old = DiscordChannelName;
                DiscordChannelName = DiscordChannelName.Replace("--", "-");
            }
            return DiscordChannelName;
        }

        protected ITextChannel GetChannel(string ChannelName, string ChannelInfo, string Group, bool SkipDelay=false)
        {
            // known channel?
            ChannelName = GetChannelName(ChannelName);
            if (ChannelMap.ContainsKey(ChannelName) == true)
            {
                ITextChannel KwownChannel = (ITextChannel)DiscordClient.GetChannel(ChannelMap[ChannelName]);
                if(KwownChannel != null)
                {
                    return KwownChannel;
                }
            }
            // Channel already on server?
            SocketGuild server = DiscordClient.GetGuild(myConfig.GetServerID());
            Group = Group.ToLower();
            bool found = false;
            foreach (ITextChannel E in server.TextChannels)
            {
                if (ulong.TryParse(E.CategoryId.ToString(), out ulong CategoryId) == false)
                {
                    continue;
                }
                string CatName = MapCatToName(CategoryId);
                if (CatName != Group)
                {
                    // not in the group we are looking for continue
                    continue;
                }
                if (E.Name == ChannelName)
                {
                    lock (ChannelMap)
                    {
                        if (ChannelMap.ContainsKey(ChannelName) == true)
                        {
                            ChannelMap.Remove(ChannelName);
                        }
                        ChannelMap.Add(ChannelName, E.Id);
                    }
                    found = true;
                    break;
                }
            }
            if (found == false)
            {
                // create the channel
                createTextChannel(ChannelName, ChannelInfo, Group, SkipDelay);
            }
            return (ITextChannel)DiscordClient.GetChannelAsync(ChannelMap[ChannelName]).GetAwaiter().GetResult();
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
            long dif = SecondbotHelpers.UnixTimeNow() - LastUpdatedSystemStatus;
            if (dif < 5)
            {
                return;
            }
            try
            {
                ITextChannel statusChannel = GetChannel("status", StatusPrefill(), "bot");
                if ((e.changed == false) && (LastStatusMessageId != 0))
                {
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
                if (dif < 15)
                {
                    return;
                }
                LastStatusMessageId = SendMessageToChannel(statusChannel, e.message);
                LastMessageContent = e.message;
            }
            catch
            {
                return;
            }
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
            LogFormater.Info("Discord service [Avi link - Active]");
            LoginEvents();
            BotCurrentSim(o, e);
        }

        protected void BotCurrentSim(object o, SimConnectedEventArgs e)
        {
            if (DiscordIsReady == true)
            {
                DiscordClient.SetGameAsync(e.Simulator.Name, null, ActivityType.Playing);
                if(LoginEventsAttached == false)
                {
                    LoginEvents();
                }
            }
        }

        protected bool LoginEventsAttached = false;
        protected void LoginEvents()
        {
            if((AcceptEventsFromSL == true) && (DiscordIsReady == true) && (LoginEventsAttached == false))
            {
                LoginEventsAttached = true;
                GetClient().Self.IM += BotImMessage;
                GetClient().Self.ChatFromSimulator += LocalChat;
                GetClient().Groups.CurrentGroups += GroupCurrent;
                GetClient().Groups.RequestCurrentGroups();
            }
            else if(DiscordIsReady == true)
            {
                DiscordClient.SetGameAsync("Waiting for login", null, ActivityType.Listening);
            }
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
