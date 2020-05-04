using System;
using System.Collections.Generic;
using OpenMetaverse;
using Discord;
using BetterSecondBotShared.Static;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;

namespace BSB.bottypes
{
    public abstract class DiscordBot : DiscordBotInbound
    {
        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            _ = DiscordBotAfterLogin();
        }
        protected async Task DiscordBotAfterLogin()
        {
            if (reconnect == false)
            {
                if (helpers.notempty(myconfig.DiscordClientToken) == false)
                {
                    myconfig.DiscordFullServer = false;
                }
                else if (myconfig.DiscordServerID <= 0)
                {
                    myconfig.DiscordFullServer = false;
                }
                if (myconfig.DiscordFullServer == false)
                {
                    if (helpers.notempty(myconfig.discordWebhookURL) == true)
                    {
                        if (helpers.notempty(myconfig.discordGroupTarget) == true)
                        {
                            if (UUID.TryParse(myconfig.discordGroupTarget, out UUID targetgroup) == true)
                            {
                                if (targetgroup != UUID.Zero)
                                {
                                    discord_group_relay = true;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (myconfig.DiscordServerImHistoryHours > 48)
                    {
                        myconfig.DiscordServerImHistoryHours = 48;
                    }
                    else if (myconfig.DiscordServerImHistoryHours < 1)
                    {
                        myconfig.DiscordServerImHistoryHours = 1;
                    }
                    DiscordClient = new DiscordSocketClient();
                    DiscordClient.Ready += DiscordClientReady;
                    DiscordClient.LoggedOut += DiscordClientLoggedOut;
                    DiscordClient.MessageReceived += DiscordClientMessageReceived;
                    await DiscordClient.LoginAsync(TokenType.Bot, myconfig.DiscordClientToken);
                    _ = DiscordClient.StartAsync();
                }
            }
        }

        protected Task DiscordClientReady()
        {
            DiscordServer = DiscordClient.GetGuild(myconfig.DiscordServerID);
            if (DiscordServer != null)
            {
                DiscordUnixTimeOnine = helpers.UnixTimeNow();
                DiscordClientConnected = true;
                _ = DiscordRebuildChannels();
            }
            else
            {
                DiscordClientConnected = false;
            }
            return Task.CompletedTask;
        }

    }
}
