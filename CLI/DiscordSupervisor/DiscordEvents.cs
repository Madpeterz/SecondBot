using BSB.bottypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.DiscordSupervisor
{
    public class DiscordEvents : DiscordBot
    {
        protected void attach_events()
        {
            controler.Bot.MessageEvent += BotChatControlerHandler;
        }

        protected void BotChatControlerHandler(object sender, MessageEventArgs e)
        {
            BotChatControler(e.message, e.sender_name, e.sender_uuid, e.avatar, e.group, e.group_uuid, e.localchat, e.fromme);
        }
    }
}
