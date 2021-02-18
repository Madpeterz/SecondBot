using BetterSecondBotShared.Static;
using Discord.Webhook;
using OpenMetaverse;
using System.Threading.Tasks;

namespace BetterSecondBot.DiscordSupervisor
{
    public abstract class DiscordRelay : DiscordStatus
    {
        public void SendIM(UUID avatar, string message)
        {
            if (HasBot() == true)
            {
                controler.Bot.SendIM(avatar, message);
            }
        }
        protected async Task DiscordGroupMessage(UUID group_uuid, string sender_name, string message)
        {
            if (HasBot() == true)
            {
                if ((helpers.notempty(sender_name) == true) && (helpers.notempty(message) == true))
                {
                    if (controler.Bot.MyGroups.ContainsKey(group_uuid) == true)
                    {
                        await SendMessageToChannelAsync(controler.Bot.MyGroups[group_uuid].Name, "" + sender_name + ": " + message + "", "group", group_uuid, "Group").ConfigureAwait(false);
                    }
                }
            }
        }
        protected virtual async Task DiscordIMMessage(UUID sender_uuid, string sender_name, string message)
        {
            if ((helpers.notempty(sender_name) == true) && (helpers.notempty(message) == true))
            {
                await SendMessageToChannelAsync(sender_name, "" + sender_name + ": " + message + "", "im", sender_uuid, "IM").ConfigureAwait(false);
            }
        }

        protected async void BotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            await DiscordBotChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, fromme).ConfigureAwait(false);
        }

        public string CommandHistoryAdd(string command, string arg, bool status, string WhyFailed)
        {
            if (HasBot() == true)
            {
                string output = controler.Bot.CommandHistoryAdd(command, arg, status, WhyFailed);
                if (output != "")
                {
                    _ = SendMessageToChannelAsync("interface", output, "bot", UUID.Zero, "bot");
                }
                return output;
            }
            return "";
        }

        protected async Task<Task> DiscordBotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            if (HasBot() == true)
            {
                if (fromme == false)
                {
                    if (DiscordClientConnected == true)
                    {
                        if (avatar == true)
                        {
                            if (localchat == false)
                            {
                                if (group == true)
                                {
                                    controler.Bot.Debug("DiscordBotChatControler - GroupChat:(" + group_uuid.ToString() + " -> " + sender_name + ":" + message);
                                    await DiscordGroupMessage(group_uuid, sender_name, message).ConfigureAwait(false);
                                }
                                else
                                {
                                    controler.Bot.Debug("DiscordBotChatControler - IM:" + sender_name + ":" + message);
                                    await DiscordIMMessage(sender_uuid, sender_name, message).ConfigureAwait(false);
                                }
                            }
                            else
                            {
                                controler.Bot.Debug("DiscordBotChatControler - Localchat:" + sender_name + ":" + message);
                                await SendMessageToChannelAsync("localchat", "" + sender_name + ": " + message, "bot", UUID.Zero, "bot").ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            controler.Bot.Debug("DiscordBotChatControler - Not from an avatar");
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
