using Discord;
using Discord.WebSocket;
using OpenMetaverse;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace BetterSecondBot.DiscordSupervisor
{

    public class DiscordMessageEvent : EventArgs
    {
        public ulong channel { get; }
        public ulong server { get; }
        public string name { get; }
        public string message { get; }

        public DiscordMessageEvent(ulong server, ulong channel, string name,string message)
        {
            this.channel = channel;
            this.server = server;
            this.name = name;
            this.message = message;
        }
    }

    public abstract class DiscordInbound : DiscordRelay
    {
        private EventHandler<DiscordMessageEvent> _MessageEvent;
        private readonly object _MessageEventLock = new object();
        public event EventHandler<DiscordMessageEvent> MessageEvent
        {
            add { lock (_MessageEventLock) { _MessageEvent += value; } }
            remove { lock (_MessageEventLock) { _MessageEvent -= value; } }
        }
        protected void On_MessageEvent(DiscordMessageEvent e)
        {
            EventHandler<DiscordMessageEvent> handler = _MessageEvent;
            handler?.Invoke(this, e);
        }


        protected async Task<Task> InboundLocalchatMessage(IUserMessage message)
        {
            if (HasBot() == true)
            {
                IGuildUser user = (IGuildUser)message.Author;
                controler.getBot().GetClient.Self.Chat(message.Content, 0, ChatType.Normal);
                return await MarkMessage(message, "✅");
            }
            return await MarkMessage(message, "❌");
        }
        protected async Task<Task> InboundInterfaceMessage(IUserMessage message)
        {
            if (message.Content == "login")
            {
                if (HasBot() == true)
                {
                    return await MarkMessage(message, "❌");
                }
                login_bot(false);
                return await MarkMessage(message, "✅").ConfigureAwait(false);
            }
            else if (message.Content == "logout")
            {
                if(HasBot() == false)
                {
                    return await MarkMessage(message, "❌");
                }
                controler.getBot().KillMePlease();
                controler = null;
                return await MarkMessage(message, "✅").ConfigureAwait(false);
            }
            else if(message.Content == "exit")
            {
                if (HasBot() == true)
                {
                    controler.getBot().KillMePlease();
                    controler.getBot().GetClient.Network.Logout();
                }
                exit_super = true;
                return await MarkMessage(message, "✅").ConfigureAwait(false);
            }
            else
            {
                if (HasBot() == true)
                {
                    StringBuilder output = new StringBuilder();
                    if (message.Content.StartsWith("!"))
                    {
                        if (message.Content == "!commands")
                        {
                            string[] commands = controler.getBot().getFullListOfCommands();
                            await SendMessageToChannelAsync("interface", string.Join("\n",commands)+ " \n For more details please see the wiki \n https://wiki.blackatom.live/", "bot", UUID.Zero, "bot");
                        }
                        else
                        {
                            await MarkMessage(message, "❌").ConfigureAwait(false);
                            output.Append("Unknown request: ");
                            output.Append(message.Content);
                            await SendMessageToChannelAsync("interface", output.ToString(), "bot", UUID.Zero, "bot");
                        }
                    }
                    else
                    {
                        string[] bits = message.Content.Split("|||");
                        string target = "None";
                        if (bits.Length == 3)
                        {
                            target = bits[2];
                        }
                        string args = "";
                        if (bits.Length >= 2)
                        {
                            args = bits[1];
                        }
                        if (controler.getBot().GetFullListOfCommandsWithCustoms().Contains(bits[0].ToLowerInvariant()) == true)
                        {
                            string reply = "";
                            if (args == "")
                            {
                                reply = controler.getBot().CallAPI(bits[0], new string[] { }, target);
                            }
                            else
                            {
                                reply = controler.getBot().CallAPI(bits[0], args.Split("~#~"), target);
                            }
                            await MarkMessage(message, "✅").ConfigureAwait(false);

                            foreach(string A in Split(reply, 1250))
                            {
                                await message.Channel.SendMessageAsync(A, false, null, null, null, new MessageReference(message.Id)).ConfigureAwait(false);
                            }
                            
                        }
                        else
                        {
                            await MarkMessage(message, "❌").ConfigureAwait(false);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        public string[] Split(string input,int size)
        {
            if (input.Length < size)
            {
                return new string[] { input };
            }
            else
            {
                int index = 0;
                int length = input.Length;
                List<string> bits = new List<string>();
                while((index+size) < length)
                {
                    bits.Add(input.Substring(index, size));
                    index += size;
                }
                if(index != length)
                {
                    // last bit
                    int remain = length - index;
                    bits.Add(input.Substring(index, remain));
                }
                return bits.ToArray();
            }
        }
        protected virtual async Task<Task> InboundImMessage(ITextChannel Chan, IUserMessage message)
        {
            if (HasBot() == true)
            {
                if (message.Content.StartsWith("!close"))
                {
                    await Chan.DeleteAsync();
                }
                else if (message.Content.StartsWith("!clear"))
                {
                    await CleanDiscordChannel(Chan, 0, true);
                }
                else
                {
                    await MarkMessage(message, "⏳").ConfigureAwait(true);
                    Thread.Sleep(400);
                    string[] bits = Chan.Topic.Split(':');
                    if (bits.Length >= 2)
                    {
                        if (bits[0] == "IM")
                        {
                            if (UUID.TryParse(bits[1], out UUID avatar) == true)
                            { 
                                string content = message.Content;
                                await message.DeleteAsync().ConfigureAwait(true);
                                SendIM(avatar, content);
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        protected async Task<Task> InboundGroupImMessage(ITextChannel Chan, IUserMessage message)
        {
            if (HasBot() == true)
            {
                string[] bits = Chan.Topic.Split(':');
                if (bits.Length >= 2)
                {
                    if (bits[0] == "Group")
                    {
                        if (UUID.TryParse(bits[1], out UUID group) == true)
                        {
                            if (controler.getBot().MyGroups.ContainsKey(group) == true)
                            {
                                if (message.Content == "!clear")
                                {
                                    await CleanDiscordChannel(Chan, 0, true).ConfigureAwait(false);
                                }
                                else if (message.Content.StartsWith("!notice") == true)
                                {
                                    string Noticetitle = "Notice";
                                    string Noticemessage;
                                    bits = message.Content.Split("|||", StringSplitOptions.None);
                                    if (bits.Length == 2)
                                    {
                                        Noticetitle = bits[0];
                                        Noticemessage = bits[1];
                                    }
                                    else
                                    {
                                        Noticemessage = bits[0];
                                        Noticemessage = Noticemessage.Replace("!notice ", "");
                                    }
                                    if (Noticemessage.Length > 5)
                                    {
                                        Noticetitle = Noticetitle.Replace("!notice ", "");
                                        Noticetitle = Noticetitle.Trim();
                                        controler.getBot().CallAPI("Groupnotice", new[] { group.ToString(), Noticetitle, Noticemessage });
                                        await MarkMessage(message, "✅").ConfigureAwait(false);
                                    }
                                    else
                                    {
                                        await MarkMessage(message, "❌").ConfigureAwait(false);
                                    }
                                }
                                else
                                {
                                    IGuildUser user = (IGuildUser)message.Author;
                                    controler.getBot().CallAPI("Groupchat", new [] { group.ToString(), message.Content });
                                    await MarkMessage(message, "✅").ConfigureAwait(false);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                await MarkMessage(message, "❌").ConfigureAwait(false);
            }
            return Task.CompletedTask;
        }

        protected async Task<Task> MarkMessage(IUserMessage message,string emote)
        {
            var Tickmark = new Emoji(emote);
            await message.AddReactionAsync(Tickmark, RequestOptions.Default);
            return Task.CompletedTask;
        }



        protected async Task<Task> DiscordClientMessageReceived(SocketMessage message)
        {
            if (AllowNewOutbound() == true)
            {
                if (message.Author.IsBot == false)
                {
                    ITextChannel Chan = (ITextChannel)message.Channel;
                    IGuildUser user = await Chan.Guild.GetUserAsync(message.Author.Id).ConfigureAwait(true);
                    string username = user.Nickname;
                    if(username == null) {
                        username = message.Author.Username;
                    }
                    On_MessageEvent(new DiscordMessageEvent(Chan.GuildId, message.Channel.Id, username, message.Content));
                    if (message.Content.StartsWith("!clear") == true)
                    {
                        await CleanDiscordChannel(Chan, 0, true).ConfigureAwait(false);
                    }
                    else if (Chan.CategoryId == catmap["bot"].Id)
                    {
                        if (Chan.Name == "interface")
                        {
                            await InboundInterfaceMessage((IUserMessage)message).ConfigureAwait(false);
                        }
                        else if (Chan.Name == "localchat")
                        {
                            await InboundLocalchatMessage((IUserMessage)message).ConfigureAwait(false);
                        }
                        else if(Chan.Name == "status")
                        {
                            IUserMessage Message = await SendMessageToChannelAsync("interface", message.Content, "bot", UUID.Zero, "bot").ConfigureAwait(true);
                            if (Message == null)
                            {
                                await MarkMessage((IUserMessage)message, "❌").ConfigureAwait(false);
                            }
                            else
                            {
                                ITextChannel channel = await FindTextChannel(DiscordServer, "interface");
                                if (channel != null)
                                {
                                    await message.DeleteAsync().ConfigureAwait(false);
                                    await InboundInterfaceMessage(Message).ConfigureAwait(false);
                                }
                                else
                                {
                                    await MarkMessage((IUserMessage)message, "❌").ConfigureAwait(false);
                                }
                            }

                        }
                    }
                    else if (Chan.CategoryId == catmap["im"].Id)
                    {
                        await InboundImMessage(Chan, (IUserMessage)message).ConfigureAwait(false);
                    }
                    else if (Chan.CategoryId == catmap["group"].Id)
                    {
                        await InboundGroupImMessage(Chan, (IUserMessage)message).ConfigureAwait(false);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
