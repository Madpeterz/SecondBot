using OpenMetaverse;

namespace BSB.bottypes
{
    public abstract class DiscordBotStatus : DiscordBotChannelControl
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
        public override string GetStatus()
        {
            string reply = "";
            if (myconfig.DiscordFull_Enable == true)
            {
                if (DiscordClientConnected == true)
                {
                    reply = "[Discord: connected]";
                    DiscordStatusUpdate();
                }
                else
                {
                    reply = "[Discord: Disconnected]";
                }
            }
            else if (discord_group_relay == true)
            {
                reply = " [Discord: Group Relay]";
            }
            if (reply != "")
            {
                reply = " " + reply;
            }
            if (reply != DiscordBotStatus_laststatus)
            {
                DiscordBotStatus_laststatus = reply;
                return base.GetStatus() + reply;
            }
            return base.GetStatus();
        }

    }
}
