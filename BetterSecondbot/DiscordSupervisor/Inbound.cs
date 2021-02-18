using Discord;
using Discord.WebSocket;
using OpenMetaverse;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Threading;

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
                controler.Bot.GetClient.Self.Chat(message.Content, 0, ChatType.Normal);
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
                controler.Bot.KillMePlease();
                controler = null;
                return await MarkMessage(message, "✅").ConfigureAwait(false);
            }
            else if(message.Content == "exit")
            {
                if (HasBot() == true)
                {
                    controler.Bot.KillMePlease();
                    controler.Bot.GetClient.Network.Logout();
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
                            await SendMessageToChannelAsync("interface", "broken", "bot", UUID.Zero, "bot");
                        }
                        else if (message.Content.StartsWith("!help") == true)
                        {
                            await SendMessageToChannelAsync("interface", "broken", "bot", UUID.Zero, "bot");
                        }
                        else
                        {
                            output.Append("Unknown request: ");
                            output.Append(message.Content);
                            await SendMessageToChannelAsync("interface", output.ToString(), "bot", UUID.Zero, "bot");
                        }
                    }
                    else
                    {
                        await MarkMessage(message, "❌").ConfigureAwait(false);
                    }
                }
            }
            return Task.CompletedTask;
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
                    Thread.Sleep(200);
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
                            if (controler.Bot.MyGroups.ContainsKey(group) == true)
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
                                    }
                                    Noticetitle = Noticetitle.Replace("!notice ", "");
                                    Noticetitle = Noticetitle.Trim();
                                    //controler.Bot.GetCommandsInterface.Call("GroupNotice", "" + group.ToString() + "~#~" + Noticetitle + "~#~" + Noticemessage, UUID.Zero);
                                    //await MarkMessage(message, "✅");
                                    await MarkMessage((IUserMessage)message, "❌").ConfigureAwait(false);
                                }
                                else
                                {
                                    IGuildUser user = (IGuildUser)message.Author;
                                    //controler.Bot.GetCommandsInterface.Call("Groupchat", "" + group.ToString() + "~#~" + message.Content, UUID.Zero);
                                    //await MarkMessage(message, "✅");
                                    await MarkMessage((IUserMessage)message, "❌").ConfigureAwait(false);
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
