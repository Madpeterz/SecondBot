using OpenMetaverse;
using System;

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

        public string lastStatus = "";
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
            if(reply != lastStatus)
            {
                lastStatus = reply;
            }
            else
            {
                reply = "";
            }
            if(HasBasicBot() == true)
            {
                string newreply = controler.getBot().GetStatus();
                if(newreply != controler.getBot().LastStatusMessage)
                {
                    controler.getBot().LastStatusMessage = newreply;
                }
                else
                {
                    newreply = "";
                }
                reply = newreply + reply;
                if (controler.getBot().KillMe == true)
                {
                    if (controler.getBot().GetClient.Network.Connected == true)
                    {
                        controler.getBot().GetClient.Network.Logout();
                    }
                    controler = null;
                    reply = "[Discord-V: connected] (Logging out)";
                }
            }
            return reply;
        }
    }
}
