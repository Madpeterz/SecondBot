using System;
using System.Collections.Generic;
using OpenMetaverse;
using Discord;
using BetterSecondBotShared.Static;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;

namespace BetterSecondBot.DiscordSupervisor
{
    public abstract class DiscordBot : DiscordInbound
    {
        protected virtual async Task StartDiscordClientService()
        {
            DiscordClient = new DiscordSocketClient();
            DiscordClient.Ready += DiscordClientReady;
            DiscordClient.LoggedOut += DiscordClientLoggedOut;
            DiscordClient.MessageReceived += DiscordClientMessageReceived;
            await DiscordClient.LoginAsync(TokenType.Bot, myconfig.DiscordFull_Token);
            _ = DiscordClient.StartAsync();
        }

        protected virtual async Task<Task> DiscordClientReady()
        {
            await DiscordClient.SetStatusAsync(Discord.UserStatus.Idle);
            await DiscordClient.SetGameAsync("Not connected", null, ActivityType.CustomStatus);
            DiscordServer = DiscordClient.GetGuild(myconfig.DiscordFull_ServerID);
            if (DiscordServer != null)
            {
                DiscordUnixTimeOnine = helpers.UnixTimeNow();
                DiscordClientConnected = true;
                await DiscordRebuildChannels();
            }
            else
            {
                DiscordClientConnected = false;
            }
            return Task.CompletedTask;
        }

    }
}
