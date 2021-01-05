using BSB.bottypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.DiscordSupervisor
{
    public class DiscordEvents : DiscordBot
    {
        protected StatusMessageEvent LastNetworkEventArgs = null;
        protected void attach_events()
        {
            controler.Bot.MessageEvent += BotChatControlerHandler;
            controler.Bot.StatusMessageEvent += StatusMessageHandler;
            controler.Bot.GroupsReadyEvent += GroupsReady;
        }

        protected async void GroupsReady(object sender, GroupEventArgs e)
        {
            await DiscordRebuildChannels();
        }

        protected void BotChatControlerHandler(object sender, MessageEventArgs e)
        {
            BotChatControler(e.message, e.sender_name, e.sender_uuid, e.avatar, e.group, e.group_uuid, e.localchat, e.fromme);
        }

        protected async void StatusMessageHandler(object sender, StatusMessageEvent e)
        {
            bool apply_update = true;
            if(LastNetworkEventArgs != null)
            {
                if((LastNetworkEventArgs.sim == e.sim) && (LastNetworkEventArgs.connected == e.connected))
                {
                    apply_update = false;
                }
            }
            if (apply_update == true)
            {
                LastNetworkEventArgs = e;
                if (e.connected == true)
                {
                    await DiscordClient.SetGameAsync("Sim:" + e.sim);
                    await DiscordClient.SetStatusAsync(Discord.UserStatus.Online);

                }
                else
                {
                    await DiscordClient.SetGameAsync("Logged out");
                    await DiscordClient.SetStatusAsync(Discord.UserStatus.Idle);
                }
            }
        }
    }
}
