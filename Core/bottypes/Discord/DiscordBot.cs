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
                if (helpers.notempty(myconfig.DiscordFull_Token) == false)
                {
                    myconfig.DiscordFull_Enable = false;
                }
                else if (myconfig.DiscordFull_ServerID <= 0)
                {
                    myconfig.DiscordFull_Enable = false;
                }
                if (myconfig.DiscordFull_Enable == false)
                {
                    if (helpers.notempty(myconfig.DiscordRelay_URL) == true)
                    {
                        if (helpers.notempty(myconfig.DiscordRelay_GroupUUID) == true)
                        {
                            if (UUID.TryParse(myconfig.DiscordRelay_GroupUUID, out UUID targetgroup) == true)
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
                    DiscordClient = new DiscordSocketClient();
                    DiscordClient.Ready += DiscordClientReady;
                    DiscordClient.LoggedOut += DiscordClientLoggedOut;
                    DiscordClient.MessageReceived += DiscordClientMessageReceived;
                    await DiscordClient.LoginAsync(TokenType.Bot, myconfig.DiscordFull_Token);
                    _ = DiscordClient.StartAsync();
                }
            }
        }

        protected Task DiscordClientReady()
        {
            DiscordServer = DiscordClient.GetGuild(myconfig.DiscordFull_ServerID);
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
