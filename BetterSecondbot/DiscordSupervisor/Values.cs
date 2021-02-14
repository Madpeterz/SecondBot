using BetterSecondBotShared.Json;
using Discord;
using Discord.WebSocket;
using System.Collections.Generic;

namespace BetterSecondBot.DiscordSupervisor
{
    public abstract class DiscordValues
    {
        protected JsonConfig myconfig;
        protected bool exit_super = false;
        protected CliExitOnLogout controler;
        protected bool running_as_docker = false;

        protected bool discord_group_relay;
        protected DiscordSocketClient DiscordClient;
        protected bool DiscordClientConnected;

        protected IGuild DiscordServer;
        protected string LastSendDiscordStatus = "";
        protected long DiscordUnixTimeOnine;
        protected bool DiscordLock;
        protected string LastStatusMessage = "";

        protected Dictionary<string, ICategoryChannel> catmap = new Dictionary<string, ICategoryChannel>();

        protected virtual void login_bot(bool self_keepalive)
        {

        }

        protected virtual void keep_alive()
        {

        }

        protected bool HasBot()
        {
            if (HasBasicBot() == true)
            {
                return controler.Bot.GetClient.Network.Connected;
            }
            return false;
        }

        protected bool HasBasicBot()
        {
            if (controler != null)
            {
                if (controler.Bot != null)
                {
                    if (controler.Bot.GetClient != null)
                    {
                        if (controler.Bot.GetClient.Network != null)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
    }
}
