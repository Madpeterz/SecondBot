using System;
using System.Collections.Generic;
using OpenMetaverse;
using Discord;
using BetterSecondBotShared.Static;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;
using System.Timers;
using BetterSecondBotShared.logs;

namespace BetterSecondBot.DiscordSupervisor
{
    public abstract class DiscordBot : DiscordInbound
    {
        protected Timer ReconnectTimer;
        protected virtual async Task StartDiscordClientService()
        {
            var config = new DiscordSocketConfig()
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildIntegrations | GatewayIntents.MessageContent
            };
            DiscordClient = new DiscordSocketClient(config);
            DiscordClient.Ready += DiscordClientReady;
            DiscordClient.LoggedOut += DiscordClientLoggedOut;
            DiscordClient.LoggedIn += DicordClientLoggedin;
            DiscordClient.MessageReceived += DiscordClientMessageReceived;
            ReconnectTimer = new Timer();
            ReconnectTimer.Interval = 10000;
            ReconnectTimer.Elapsed += ReconnectTimerEvent;
            ReconnectTimer.AutoReset = false;
            ReconnectTimer.Enabled = false;
            LogFormater.Info("Discord is starting", true);
            await DiscordClient.LoginAsync(TokenType.Bot, myconfig.DiscordFull_Token);
            
        }

        protected override Task DiscordClientLoggedOut()
        {
            LogFormater.Info("Discord has logged out", true);
            ReconnectTimer = new Timer();
            ReconnectTimer.Interval = 10000;
            ReconnectTimer.Elapsed += ReconnectTimerEvent;
            ReconnectTimer.AutoReset = false;
            ReconnectTimer.Enabled = false;
            return Task.CompletedTask;
        }

        protected virtual async Task<Task> DicordClientLoggedin()
        {
            LogFormater.Info("Discord has logged in", true);
            await DiscordClient.StartAsync().ConfigureAwait(false);
            ReconnectTimer = new Timer();
            ReconnectTimer.Interval = 10000;
            ReconnectTimer.Elapsed += ReconnectTimerEvent;
            ReconnectTimer.AutoReset = false;
            ReconnectTimer.Enabled = false;
            return Task.CompletedTask;
        }

        private void ReconnectTimerEvent(Object source, System.Timers.ElapsedEventArgs e)
        {
            ReconnectTimer.Enabled = false;
            _ = StartDiscordClientService().ConfigureAwait(false);
        }

        protected virtual async Task<Task> DiscordClientReady()
        {
            ReconnectTimer.Enabled = false;
            bool hasAdmin = false;
            foreach(SocketRole Role in DiscordClient.GetGuild(myconfig.DiscordFull_ServerID).CurrentUser.Roles)
            {
                if(Role.Permissions.Administrator == true)
                {
                    hasAdmin = true;
                    break;
                }
            }
            if (hasAdmin == true)
            {
                LogFormater.Info("Discord is active", true);
                await DiscordClient.SetStatusAsync(Discord.UserStatus.Idle);
                await DiscordClient.SetGameAsync("Not connected", null, ActivityType.CustomStatus);
                DiscordServer = DiscordClient.GetGuild(myconfig.DiscordFull_ServerID);
                if (DiscordServer != null)
                {
                    if (myconfig.DiscordFull_Enable == true)
                    {
                        login_bot(false);
                    }
                    DiscordUnixTimeOnine = helpers.UnixTimeNow();
                    DiscordClientConnected = true;
                    await DiscordRebuildChannels();
                }
                else
                {
                    DiscordClientConnected = false;
                }
            }
            else
            {
                LogFormater.Info("Discord has failed - missing admin perm", true);
            }
            return Task.CompletedTask;
        }

    }
}
