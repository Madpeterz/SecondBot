using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Discord;
using Discord.Webhook;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;

namespace BSB.bottypes
{
    public abstract class DiscordBot : ChatRelay
    {
        // Full fat Discord client
        protected bool discord_group_relay;
        protected DiscordSocketClient DiscordClient;
        protected bool DiscordClientConnected;
        protected IGuild DiscordServer;
        protected Dictionary<string, ICategoryChannel> catmap = new Dictionary<string, ICategoryChannel>();
        protected string LastSendDiscordStatus = "";
        protected long DiscordUnixTimeOnine;

        protected bool DiscordLock;
        protected Task WaitForUnlock()
        {
            while (DiscordLock == true)
            {
                Thread.Sleep(100);
            }
            return Task.CompletedTask;
        }

        public override string GetStatus()
        {
            string reply = "";
            if(myconfig.DiscordFullServer == true)
            {
                if (DiscordClientConnected == true)
                {
                    reply = "[Discord: connected]";
                    DiscordStatusUpdate();
                }
                else
                {
                    reply = "[Discord: Disconnected]";
                }

            }
            else if(discord_group_relay == true)
            {
                reply = " [Discord: Group Relay]";
            }
            if(reply != "")
            {
                reply = " " + reply;
            }
            return base.GetStatus() + reply;
        }

        protected bool AllowNewOutbound()
        {
            if (DiscordUnixTimeOnine != 0)
            {
                long dif = helpers.UnixTimeNow() - DiscordUnixTimeOnine;
                if (dif > 8)
                {
                    return true;
                }
            }
            return false;
        }

        protected async Task SendMessageToChannelAsync(string channelname,string message,string catmapid,UUID sender_id,string sendername, string TopicType)
        {
            if (AllowNewOutbound() == true)
            {
                channelname = channelname.ToLowerInvariant();
                channelname = String.Concat(channelname.Where(char.IsLetterOrDigit));
                ITextChannel Channel = await FindTextChannel(channelname, catmap[catmapid], sender_id, sendername, TopicType).ConfigureAwait(false);
                if (Channel != null)
                {
                    await Channel.SendMessageAsync(message);
                }
            }
        }

        protected void DiscordStatusUpdate()
        {
            if (AllowNewOutbound() == true)
            {
                if (LastSendDiscordStatus != LastStatusMessage)
                {
                    LastSendDiscordStatus = LastStatusMessage;
                    _ = SendMessageToChannelAsync("status", LastSendDiscordStatus, "bot", UUID.Zero, myconfig.userName, "bot");
                }
            }
        }

        protected async Task<ITextChannel> CreateChannel(string channelname, string channeltopictype, string sender_id)
        {
            channelname = channelname.ToLowerInvariant();
            channelname = String.Concat(channelname.Where(char.IsLetterOrDigit));
            string display_topic = "" + channeltopictype + ":" + sender_id + "";
            if (channelname == "interface")
            {
                display_topic = "Actions -> !clear, !commands, !help command or command";
            }
            else if (channelname == "status")
            {
                display_topic = "Actions -> !clear";
            }
            else if (sender_id == UUID.Zero.ToString())
            {
                display_topic = "" + myconfig.userName + " #" + MyVersion + "";
            }
            else if (channeltopictype == "Group")
            {
                display_topic = "" + channeltopictype + ":" + sender_id + ": Actions -> !clear, !notice title|||message";
            }
            else if (channeltopictype == "IM")
            {
                display_topic = "" + channeltopictype + ":" + sender_id + ": Actions -> !clear, !close";
            }

            IGuildChannel channel = await DiscordServer.CreateTextChannelAsync(channelname, X => DiscordGetNewChannelProperies(X, channelname, display_topic, channeltopictype.ToLowerInvariant()));
            ITextChannel Txtchan = await DiscordServer.GetTextChannelAsync(channel.Id);
            return Txtchan;
        }

        protected async Task DiscordGroupMessage(UUID group_uuid, string sender_name, string message)
        {
            if ((helpers.notempty(sender_name) == true) && (helpers.notempty(message) == true))
            {
                if (mygroups.ContainsKey(group_uuid) == true)
                {
                    await SendMessageToChannelAsync(mygroups[group_uuid].Name, "" + sender_name + ": " + message + "", "group", group_uuid, sender_name, "Group").ConfigureAwait(false);
                }
            }
        }
        protected async Task DiscordIMMessage(UUID sender_uuid, string sender_name, string message)
        {
            if ((helpers.notempty(sender_name) == true) && (helpers.notempty(message) == true))
            {
                await SendMessageToChannelAsync(sender_name, "" + sender_name + ": " + message + "", "im", sender_uuid, sender_name, "IM").ConfigureAwait(false);
            }
        }

        protected async Task<ITextChannel> FindTextChannel(string channelname, ICategoryChannel cat, UUID sender, string sendername, string TopicType)
        {
            await WaitForUnlock();
            channelname = channelname.ToLowerInvariant();
            channelname = String.Concat(channelname.Where(char.IsLetterOrDigit));
            DiscordLock = true;
            IReadOnlyCollection<ITextChannel> found_chans = await DiscordServer.GetTextChannelsAsync(CacheMode.AllowDownload);
            ITextChannel result = null;
            foreach(ITextChannel ITC in found_chans)
            {
                if(ITC.CategoryId == cat.Id)
                {
                    if(ITC.Name == channelname)
                    {
                        result = ITC;
                        break;
                    }
                }
            }
            if(result == null)
            {
                result = await CreateChannel(channelname, TopicType, sender.ToString());
            }
            else
            {
                await CleanDiscordChannel(result, myconfig.DiscordServerImHistoryHours).ConfigureAwait(false);
            }
            DiscordLock = false;
            return result;
        }

        protected async Task DiscordBotAfterLogin()
        {
            if (reconnect == false)
            {
                if (helpers.notempty(myconfig.DiscordClientToken) == false)
                {
                    myconfig.DiscordFullServer = false;
                }
                else if (myconfig.DiscordServerID <= 0)
                {
                    myconfig.DiscordFullServer = false;
                }
                if (myconfig.DiscordFullServer == false)
                {
                    if (helpers.notempty(myconfig.discord) == true)
                    {
                        if (helpers.notempty(myconfig.discordGroupTarget) == true)
                        {
                            discord_group_relay = true;
                        }
                    }
                }
                else
                {
                    if (myconfig.DiscordServerImHistoryHours > 48)
                    {
                        myconfig.DiscordServerImHistoryHours = 48;
                    }
                    else if (myconfig.DiscordServerImHistoryHours < 1)
                    {
                        myconfig.DiscordServerImHistoryHours = 1;
                    }
                    DiscordClient = new DiscordSocketClient();
                    DiscordClient.Ready += DiscordClientReady;
                    DiscordClient.LoggedOut += DiscordClientLoggedOut;
                    DiscordClient.MessageReceived += DiscordClientMessageReceived;
                    await DiscordClient.LoginAsync(TokenType.Bot, myconfig.DiscordClientToken);
                    _ = DiscordClient.StartAsync();
                }
            }
        }



        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            _ = DiscordBotAfterLogin();
        }

        protected async Task DiscordKillMePlease()
        {
            DiscordUnixTimeOnine = 0;
            await DiscordClient.LogoutAsync();
        }

        public override void KillMePlease()
        {
            base.KillMePlease();
            if(DiscordClient != null)
            {
                _ = DiscordKillMePlease();
            }
            
        }

        protected void DiscordGetNewChannelProperies(TextChannelProperties C,string channelname,string channeltopic,string catname)
        {
            if (catname != null)
            {
                if(catmap.ContainsKey(catname) == true)
                {
                    C.CategoryId = catmap[catname].Id;
                }
            }
            C.Name = channelname;
            C.Topic = channeltopic;
        }

        protected async static Task CleanDiscordChannel(ITextChannel chan)
        {
            await CleanDiscordChannel(chan, 48).ConfigureAwait(false);
        }
        protected async static Task CleanDiscordChannel(ITextChannel chan, int HistoryHours)
        {
            await CleanDiscordChannel(chan, HistoryHours,false).ConfigureAwait(false);
        }

        protected async static Task CleanDiscordChannel(ITextChannel chan,int HistoryHours, bool forceempty)
        {
            DateTimeOffset Now = new DateTimeOffset(new DateTime());
            IEnumerable<IMessage> messages;
            bool empty = false;
            while(empty == false)
            {
                empty = true;
                messages = await chan.GetMessagesAsync(50).FlattenAsync();
                List<ulong> deleteMessages = new List<ulong>();
                foreach (IMessage mess in messages)
                {
                    var hours = ((Now.ToUnixTimeSeconds() - mess.Timestamp.ToUnixTimeSeconds()) / 60) / 60;
                    if ((hours > HistoryHours) || (forceempty == true))
                    {
                        empty = false;
                        deleteMessages.Add(mess.Id);
                    }
                }
                if(deleteMessages.Count > 0)
                {
                    await chan.DeleteMessagesAsync(deleteMessages);
                }
            }
        }

        protected async Task DiscordRebuildChannels()
        {
            List<string> required_cats = new List<string>() { "bot", "group", "im" };
            IReadOnlyCollection<ICategoryChannel> found_cats = await DiscordServer.GetCategoriesAsync(CacheMode.AllowDownload);
            foreach(ICategoryChannel fcat in found_cats)
            {
                if(required_cats.Contains(fcat.Name) == true)
                {
                    required_cats.Remove(fcat.Name);
                    catmap.Add(fcat.Name, fcat);
                }
            }
            foreach (string A in required_cats)
            {
                ICategoryChannel newcat = await DiscordServer.CreateCategoryAsync(A).ConfigureAwait(true);
                catmap.Add(A, newcat);
            }
            List<string> required_channels = new List<string>() {"status","interface" };
            IReadOnlyCollection<ITextChannel> found_chans = await DiscordServer.GetTextChannelsAsync(CacheMode.AllowDownload);
            List<string> GroupChannels = new List<string>();
            foreach (ITextChannel chan in found_chans)
            {
                if (chan.CategoryId == catmap["bot"].Id)
                {
                    required_channels.Remove(chan.Name);
                }
                else
                {
                    if (chan.CategoryId == catmap["group"].Id)
                    {
                        GroupChannels.Add(chan.Name);
                    }
                }
            }
            foreach (string A in required_channels)
            {
                _ = await FindTextChannel(A, catmap["bot"], UUID.Zero, A, "bot").ConfigureAwait(false);
            }
            foreach (Group G in mygroups.Values)
            {
                string groupname = G.Name.ToLowerInvariant();
                groupname = String.Concat(groupname.Where(char.IsLetterOrDigit));
                if (GroupChannels.Contains(groupname) == false)
                {
                    _ = await FindTextChannel(groupname, catmap["group"], G.ID, groupname, "Group").ConfigureAwait(false);
                }
            }
        }
        protected async Task<Task> DiscordClientMessageReceived(SocketMessage message)
        {
            if (AllowNewOutbound() == true)
            {
                if (message.Author.IsBot == false)
                {
                    if (Client.Network.Connected == false)
                    {
                        await message.DeleteAsync();
                    }
                    else
                    {
                        ITextChannel Chan = (ITextChannel)message.Channel;
                        if(Chan.CategoryId == catmap["bot"].Id)
                        {
                            await message.DeleteAsync();
                            if (message.Content == "!clear")
                            {
                                await CleanDiscordChannel(Chan, 0, true);
                            }
                            if (Chan.Name == "interface")
                            {
                                if (message.Content.StartsWith("!"))
                                {
                                    if (message.Content == "!commands")
                                    {
                                        string reply = "";
                                        int counter = 0;
                                        string addon = "";
                                        foreach(string a in CommandsInterface.GetCommandsList())
                                        {
                                            reply += addon;
                                            reply += a;
                                            counter++;
                                            if (counter == 5)
                                            {
                                                reply += "\n";
                                                addon = "";
                                                counter = 0;
                                            }
                                            else
                                            {
                                                addon = " , ";
                                            }
                                        }
                                        _ = SendMessageToChannelAsync("interface", reply, "bot", UUID.Zero, myconfig.userName, "bot");
                                    }
                                    else if (message.Content.StartsWith("!help") == true)
                                    {
                                        string[] bits = message.Content.Split(' ');
                                        if (bits.Length == 2)
                                        {
                                            string command = bits[1].ToLowerInvariant();
                                            if (CommandsInterface.GetCommandsList().Contains(command) == true)
                                            {
                                                _ = SendMessageToChannelAsync("interface", "\n=========================\nCommand: " + command + "\n" +
                                                    "Workspace: " + CommandsInterface.GetCommandWorkspace(command) + "\n" +
                                                    "Min args: " + CommandsInterface.GetCommandArgs(command).ToString() + "\n" +
                                                    "Arg types" + String.Join(",", CommandsInterface.GetCommandArgTypes(command)) + "\n" +
                                                    "\n" +
                                                    "About: " + CommandsInterface.GetCommandHelp(command) + "", "bot", UUID.Zero, myconfig.userName, "bot");
                                            }
                                            else
                                            {
                                                _ = SendMessageToChannelAsync("interface", "Unable to find command: " + command + " please use !commands for a full list", "bot", UUID.Zero, myconfig.userName, "bot");
                                            }
                                        }
                                        else
                                        {
                                            _ = SendMessageToChannelAsync("interface", "Please format help request as follows: !help commandname", "bot", UUID.Zero, myconfig.userName, "bot");
                                        }
                                    }
                                    else if (message.Content != "!clear")
                                    {
                                        _ = SendMessageToChannelAsync("interface", "Unknown request: " + message.Content + "", "bot", UUID.Zero, myconfig.userName, "bot");
                                    }
                                }
                                else
                                {
                                    string[] bits = message.Content.Split("|||");
                                    string command = bits[0].ToLowerInvariant();
                                    if (CommandsInterface.GetCommandsList().Contains(command) == true)
                                    {
                                        bool status = false;
                                        if (bits.Length == 2)
                                        {
                                            status = CommandsInterface.Call(command, bits[1],UUID.Zero);
                                        }
                                        else
                                        {
                                            status = CommandsInterface.Call(command);
                                        }
                                    }
                                    else
                                    {
                                        _ = SendMessageToChannelAsync("interface", "Unable to find command: " + command + " please use !commands for a full list", "bot", UUID.Zero, myconfig.userName, "bot");
                                    }
                                }
                            }
                        }
                        else if (Chan.CategoryId == catmap["im"].Id)
                        {
                            // Avatar
                            if (message.Content == "!close")
                            {
                                await Chan.DeleteAsync();
                            }
                            else if (message.Content == "!clear")
                            {
                                await CleanDiscordChannel(Chan, 0, true);
                            }
                            else
                            {
                                string[] bits = Chan.Topic.Split(':');
                                if (bits.Length >= 2)
                                {
                                    if (bits[0] == "IM")
                                    {
                                        if (message.Content == "!clear")
                                        {
                                            await CleanDiscordChannel(Chan, 0, true);
                                        }
                                        else if (UUID.TryParse(bits[1], out UUID avatar) == true)
                                        {
                                            Client.Self.InstantMessage(avatar, "["+message.Author.Username+"]->"+message.Content);
                                        }
                                    }
                                }
                            }
                        }
                        else if (Chan.CategoryId == catmap["group"].Id)
                        {
                            // Group
                            string[] bits = Chan.Topic.Split(':');
                            if (bits.Length >= 2)
                            {
                                if (bits[0] == "Group")
                                {
                                    if (UUID.TryParse(bits[1], out UUID group) == true)
                                    {
                                        if(mygroups.ContainsKey(group) == true)
                                        {
                                            if (message.Content == "!clear")
                                            {
                                                await CleanDiscordChannel(Chan, 0, true);
                                            }
                                            else if (message.Content.StartsWith("!notice") == true)
                                            {
                                                string Noticetitle = "Notice";
                                                string Noticemessage = "";
                                                bits = message.Content.Split("|||", StringSplitOptions.None);
                                                if (bits.Length == 2)
                                                {
                                                    Noticetitle = bits[0];
                                                    Noticemessage = bits[1];
                                                }
                                                else
                                                {
                                                    Noticemessage = bits[0];
                                                }
                                                Noticetitle = Noticetitle.Replace("!notice ", "");
                                                Noticetitle = Noticetitle.Trim();
                                                CommandsInterface.Call("GroupNotice", ""+ group.ToString()+"~#~" + Noticetitle + "~#~" + Noticemessage,UUID.Zero);
                                            }
                                            else
                                            {
                                                CommandsInterface.Call("Groupchat", "" + group.ToString() + "~#~" + "[" + message.Author.Username + "]->" + message.Content, UUID.Zero);
                                            }
                                            await message.DeleteAsync();
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        protected Task DiscordClientReady()
        {
            DiscordServer = DiscordClient.GetGuild(myconfig.DiscordServerID);
            if (DiscordServer != null)
            {
                DiscordUnixTimeOnine = helpers.UnixTimeNow();
                DiscordClientConnected = true;
                _ = DiscordRebuildChannels();
            }
            else
            {
                DiscordClientConnected = false;
            }
            return Task.CompletedTask;
        }
        protected Task DiscordClientLoggedOut()
        {
            DiscordClientConnected = false;
            return Task.CompletedTask;
        }

        protected async override void BotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            base.BotChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, fromme);
            await DiscordBotChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, fromme);
        }

        public override void CommandHistoryAdd(string command, string arg, bool status)
        {
            base.CommandHistoryAdd(command, arg, status);
            if (status == true)
            {
                _ = SendMessageToChannelAsync("interface", "Command running: " + command + " ["+arg+"]", "bot", UUID.Zero, myconfig.userName, "bot");
            }
            else
            {
                _ = SendMessageToChannelAsync("interface", "Command rejected: " + command + " [" + arg + "]", "bot", UUID.Zero, myconfig.userName, "bot");
            }
        }

        public async override void sendIM(UUID avatar, string message)
        {
            base.sendIM(avatar, message);
            if(AvatarKey2Name.ContainsKey(avatar) == true)
            {
                if (DiscordClientConnected == true)
                {
                    await DiscordIMMessage(avatar, AvatarKey2Name[avatar], message).ConfigureAwait(false);
                }
            }
        }

        protected async Task DiscordBotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            if (myconfig.DiscordFullServer == false)
            {
                if (discord_group_relay == true)
                {
                    // GroupChat Relay only
                    if (group == true)
                    {
                        if (myconfig.discord != "")
                        {
                            if ((myconfig.discordGroupTarget == group_uuid.ToString()) || (myconfig.discordGroupTarget == "*") || (myconfig.discordGroupTarget == "all"))
                            {
                                Group Gr = mygroups[group_uuid];
                                using (var DWHclient = new DiscordWebhookClient(myconfig.discord))
                                {
                                    string SendMessage = "(" + Gr.Name + ") @" + sender_name + ":" + message + "";
                                    await DWHclient.SendMessageAsync(text: SendMessage).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }
            }
            else if (DiscordClientConnected == true)
            {
                if (avatar == true)
                {
                    if (localchat == false)
                    {
                        if (group == true)
                        {
                            await DiscordGroupMessage(group_uuid, sender_name, message).ConfigureAwait(false);
                        }
                        else
                        {
                            await DiscordIMMessage(sender_uuid,sender_name, message).ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }
}
