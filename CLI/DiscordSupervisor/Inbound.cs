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
    public abstract class DiscordInbound : DiscordRelay
    {
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
                            int counter = 0;
                            string addon = "";
                            foreach (string a in controler.Bot.GetCommandsInterface.GetCommandsList())
                            {
                                output.Append(addon);
                                addon = " , ";
                                output.Append(a);
                                counter++;
                                if (counter == 5)
                                {
                                    output.Append("\n");
                                    addon = "";
                                    counter = 0;
                                }
                            }
                            await SendMessageToChannelAsync("interface", output.ToString(), "bot", UUID.Zero, "bot");
                        }
                        else if (message.Content.StartsWith("!help") == true)
                        {
                            string[] bits = message.Content.Split(' ');
                            if (bits.Length == 2)
                            {
                                string command = bits[1].ToLowerInvariant();

                                if (controler.Bot.GetCommandsInterface.GetCommandsList().Contains(command) == true)
                                {
                                    output.Append("\n=========================\nCommand:");
                                    output.Append(command);
                                    output.Append("\n");
                                    output.Append("Workspace: ");
                                    output.Append(controler.Bot.GetCommandsInterface.GetCommandWorkspace(command));
                                    output.Append("\n");
                                    output.Append("Min args: ");
                                    output.Append(controler.Bot.GetCommandsInterface.GetCommandArgs(command).ToString());
                                    output.Append("\n");
                                    output.Append("Arg types: ");
                                    output.Append(String.Join(",", controler.Bot.GetCommandsInterface.GetCommandArgTypes(command)));
                                    output.Append("\n");
                                    output.Append("\n");
                                    output.Append("About: ");
                                    output.Append(controler.Bot.GetCommandsInterface.GetCommandHelp(command));
                                    await SendMessageToChannelAsync("interface", output.ToString(), "bot", UUID.Zero, "bot");
                                }
                                else
                                {
                                    output.Append("Unable to find command: ");
                                    output.Append(command);
                                    output.Append(" please use !commands for a full list");
                                    await SendMessageToChannelAsync("interface", output.ToString(), "bot", UUID.Zero, "bot");
                                }
                            }
                            else
                            {
                                await SendMessageToChannelAsync("interface", "Please format help request as follows: !help commandname", "bot", UUID.Zero, "bot");
                            }
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
                        string[] bits = message.Content.Split("|||");
                        string command = bits[0].ToLowerInvariant();
                        bool known_command = controler.Bot.GetCommandsInterface.GetCommandsList().Contains(command);
                        if (known_command == false)
                        {
                            known_command = controler.Bot.getCustomCommands.ContainsKey(command);
                        }
                        if (known_command == true)
                        {
                            bool process_status = false;
                            if (bits.Length == 1)
                            {
                                bits = new string[] { bits[0], "" };
                            }
                            if (controler.Bot.getCustomCommands.ContainsKey(command) == false)
                            {
                                process_status = controler.Bot.GetCommandsInterface.Call(command, bits[1], UUID.Zero);
                            }
                            else
                            {
                                process_status = true;
                                Thread t = new Thread(() => controler.Bot.custom_commands_loop(command, bits[1], UUID.Zero));
                                t.Start();
                            }
                            if (process_status == true)
                            {
                                await MarkMessage(message, "✅").ConfigureAwait(false);
                            }
                            else
                            {
                                await MarkMessage(message, "❌").ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            output.Append("Unable to find command: ");
                            output.Append(command);
                            output.Append(" please use !commands for a full list");
                            await SendMessageToChannelAsync("interface", output.ToString(), "bot", UUID.Zero, "bot");
                        }
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
                                    controler.Bot.GetCommandsInterface.Call("GroupNotice", "" + group.ToString() + "~#~" + Noticetitle + "~#~" + Noticemessage, UUID.Zero);
                                    await MarkMessage(message, "✅");
                                }
                                else
                                {
                                    IGuildUser user = (IGuildUser)message.Author;
                                    controler.Bot.GetCommandsInterface.Call("Groupchat", "" + group.ToString() + "~#~" + message.Content, UUID.Zero);
                                    await MarkMessage(message, "✅");
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
