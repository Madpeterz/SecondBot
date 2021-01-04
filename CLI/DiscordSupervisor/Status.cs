using OpenMetaverse;

namespace BetterSecondBot.DiscordSupervisor
{
    public abstract class DiscordStatus : DiscordChannelControl
    {
        protected void DiscordStatusUpdate()
        {
            if (AllowNewOutbound() == true)
            {
                if (LastSendDiscordStatus != LastStatusMessage)
                {
                    LastSendDiscordStatus = LastStatusMessage;
                    _ = SendMessageToChannelAsync("status", LastSendDiscordStatus, "bot", UUID.Zero, "bot");
                }
            }
        }
        string DiscordBotStatus_laststatus = "";
        public string GetStatus()
        {
            string reply = "";
            if (myconfig.DiscordFull_Enable == true)
            {
                if (DiscordClientConnected == true)
                {
                    reply = "[Discord-V: connected]";
                }
                else
                {
                    reply = "[Discord-V: Disconnected]";
                }
            }
            if (HasBasicBot() == false)
            {
                reply = reply + " (Not logged in)";
            }
            if (reply != "")
            {
                reply = " " + reply;
            }
            if(reply == DiscordBotStatus_laststatus)
            {
                reply = "";
            }
            else
            {
                DiscordBotStatus_laststatus = reply;
            }
            if(HasBasicBot() == true)
            {
                controler.Bot.LastStatusMessage = controler.Bot.GetStatus();
                reply = controler.Bot.LastStatusMessage + reply;
                if (controler.Bot.KillMe == true)
                {
                    controler.Bot.GetClient.Network.Logout();
                    controler = null;
                    if (DiscordClientConnected == true)
                    {
                        reply = "[Discord-V: connected] (Logging out)";
                    }
                    else
                    {
                        reply = "[Discord-V: Disconnected] (Logging out)";
                    }
                }

            }
            return reply;
        }

    }
}
