using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace BSB.bottypes
{
    public abstract class Values : ChatRelay
    {
        protected bool discord_group_relay;
        protected DiscordSocketClient DiscordClient;
        protected bool DiscordClientConnected;
        protected IGuild DiscordServer;
        protected string LastSendDiscordStatus = "";
        protected long DiscordUnixTimeOnine;
        protected bool DiscordLock;
        protected Dictionary<string, ICategoryChannel> catmap = new Dictionary<string, ICategoryChannel>();
    }
}
