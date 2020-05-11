using Discord;
using Discord.WebSocket;
using OpenMetaverse;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

namespace BSB.bottypes
{
    public abstract class DiscordBotInbound : DiscordBotRelay
    {
        protected async Task<Task> InboundLocalchatMessage(SocketMessage message)
        {
            Client.Self.Chat("[" + message.Author.Username + "]->" + message.Content, 0, ChatType.Normal);
            return await MarkMessage(message, "✅");
        }
        protected async Task<Task> InboundInterfaceMessage(SocketMessage message)
        {
            StringBuilder output = new StringBuilder();
            if (message.Content.StartsWith("!"))
            {
                if (message.Content == "!commands")
                {
                    int counter = 0;
                    string addon = "";
                    foreach (string a in CommandsInterface.GetCommandsList())
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

                        if (CommandsInterface.GetCommandsList().Contains(command) == true)
                        {
                            output.Append("\n=========================\nCommand:");
                            output.Append(command);
                            output.Append("\n");
                            output.Append("Workspace: ");
                            output.Append(CommandsInterface.GetCommandWorkspace(command));
                            output.Append("\n");
                            output.Append("Min args: ");
                            output.Append(CommandsInterface.GetCommandArgs(command).ToString());
                            output.Append("\n");
                            output.Append("Arg types: ");
                            output.Append(String.Join(",", CommandsInterface.GetCommandArgTypes(command)));
                            output.Append("\n");
                            output.Append("\n");
                            output.Append("About: ");
                            output.Append(CommandsInterface.GetCommandHelp(command));
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
                else if (message.Content != "!clear")
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
                if (CommandsInterface.GetCommandsList().Contains(command) == true)
                {
                    bool process_status = false;
                    if (bits.Length == 2)
                    {
                        process_status = CommandsInterface.Call(command, bits[1], UUID.Zero);
                    }
                    else
                    {
                        process_status = CommandsInterface.Call(command);
                    }
                    if (process_status == true)
                    {
                        await MarkMessage(message, "✅");
                    }
                    else
                    {
                        await MarkMessage(message, "❌");
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
            return Task.CompletedTask;
        }
        protected async Task<Task> InboundImMessage(ITextChannel Chan, SocketMessage message)
        {
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
                            Client.Self.InstantMessage(avatar, "[" + message.Author.Username + "]->" + message.Content);
                            await MarkMessage(message, "✅");
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        protected async Task<Task> InboundGroupImMessage(ITextChannel Chan, SocketMessage message)
        {
            string[] bits = Chan.Topic.Split(':');
            if (bits.Length >= 2)
            {
                if (bits[0] == "Group")
                {
                    if (UUID.TryParse(bits[1], out UUID group) == true)
                    {
                        if (mygroups.ContainsKey(group) == true)
                        {
                            if (message.Content == "!clear")
                            {
                                await CleanDiscordChannel(Chan, 0, true);
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
                                CommandsInterface.Call("GroupNotice", "" + group.ToString() + "~#~" + Noticetitle + "~#~" + Noticemessage, UUID.Zero);
                                await MarkMessage(message, "✅");
                            }
                            else
                            {
                                CommandsInterface.Call("Groupchat", "" + group.ToString() + "~#~" + "[" + message.Author.Username + "]->" + message.Content, UUID.Zero);
                                await MarkMessage(message, "✅");
                            }
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        protected async Task<Task> MarkMessage(SocketMessage message,string emote)
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
                    if (Client.Network.Connected == false)
                    {
                        await message.DeleteAsync();
                    }
                    else
                    {
                        ITextChannel Chan = (ITextChannel)message.Channel;
                        if (Chan.CategoryId == catmap["bot"].Id)
                        {
                            if (message.Content == "!clear")
                            {
                                await CleanDiscordChannel(Chan, 0, true);
                            }
                            else if (Chan.Name == "interface")
                            {
                                await InboundInterfaceMessage(message);
                            }
                            else if(Chan.Name == "localchat")
                            {
                                await InboundLocalchatMessage(message);
                            }
                        }
                        else if (Chan.CategoryId == catmap["im"].Id)
                        {
                            await InboundImMessage(Chan, message);
                        }
                        else if (Chan.CategoryId == catmap["group"].Id)
                        {
                            await InboundGroupImMessage(Chan, message);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
