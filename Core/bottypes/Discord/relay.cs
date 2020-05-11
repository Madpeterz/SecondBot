using BetterSecondBotShared.Static;
using Discord.Webhook;
using OpenMetaverse;
using System.Threading.Tasks;

namespace BSB.bottypes
{
    public abstract class DiscordBotRelay : DiscordBotStatus
    {
        public async override void SendIM(UUID avatar, string message)
        {
            base.SendIM(avatar, message);
            if (AvatarKey2Name.ContainsKey(avatar) == true)
            {
                if (DiscordClientConnected == true)
                {
                    await DiscordIMMessage(avatar, AvatarKey2Name[avatar], message).ConfigureAwait(false);
                }
            }
        }
        protected async Task DiscordGroupMessage(UUID group_uuid, string sender_name, string message)
        {
            if ((helpers.notempty(sender_name) == true) && (helpers.notempty(message) == true))
            {
                if (mygroups.ContainsKey(group_uuid) == true)
                {
                    await SendMessageToChannelAsync(mygroups[group_uuid].Name, "" + sender_name + ": " + message + "", "group", group_uuid, "Group").ConfigureAwait(false);
                }
            }
        }
        protected async Task DiscordIMMessage(UUID sender_uuid, string sender_name, string message)
        {
            if ((helpers.notempty(sender_name) == true) && (helpers.notempty(message) == true))
            {
                await SendMessageToChannelAsync(sender_name, "" + sender_name + ": " + message + "", "im", sender_uuid, "IM").ConfigureAwait(false);
            }
        }
        protected async override void BotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            base.BotChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, fromme);
            await DiscordBotChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, fromme).ConfigureAwait(false);
        }

        public override string CommandHistoryAdd(string command, string arg, bool status, string WhyFailed)
        {
            string output = base.CommandHistoryAdd(command, arg, status, WhyFailed);
            if (output != "")
            {
                _ = SendMessageToChannelAsync("interface", output, "bot", UUID.Zero, "bot");
            }
            return output;
        }
        protected async Task DiscordBotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat, bool fromme)
        {
            if (fromme == false)
            {
                if (myconfig.DiscordFullServer == false)
                {
                    if (discord_group_relay == true)
                    {
                        // GroupChat Relay only
                        if (group == true)
                        {
                            if (myconfig.discordWebhookURL != "")
                            {
                                if ((myconfig.discordGroupTarget == group_uuid.ToString()) || (myconfig.discordGroupTarget == "*") || (myconfig.discordGroupTarget == "all"))
                                {
                                    Group Gr = mygroups[group_uuid];
                                    using var DWHclient = new DiscordWebhookClient(myconfig.discordWebhookURL);
                                    string SendMessage = "(" + Gr.Name + ") @" + sender_name + ":" + message + "";
                                    await DWHclient.SendMessageAsync(text: SendMessage).ConfigureAwait(false);
                                    DWHclient.Dispose();
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
                                await DiscordIMMessage(sender_uuid, sender_name, message).ConfigureAwait(false);
                            }
                        }
                        else
                        {
                            _ = SendMessageToChannelAsync("localchat", ""+sender_name+":"+message, "bot", UUID.Zero, "bot");
                        }
                    }
                }
            }
        }
    }
}
