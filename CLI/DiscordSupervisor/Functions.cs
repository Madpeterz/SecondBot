using BetterSecondBotShared.Static;
using System.Threading;
using System.Threading.Tasks;

namespace BetterSecondBot.DiscordSupervisor
{
    public abstract class DiscordFunctions : DiscordValues
    {
        protected Task WaitForUnlock()
        {
            while (DiscordLock == true)
            {
                Thread.Sleep(100);
            }
            return Task.CompletedTask;
        }
        protected bool AllowNewOutbound()
        {
            if (DiscordUnixTimeOnine != 0)
            {
                long dif = helpers.UnixTimeNow() - DiscordUnixTimeOnine;
                if (dif > 8)
                {
                    return true;
                }
            }
            return false;
        }
        protected Task DiscordClientLoggedOut()
        {
            DiscordClientConnected = false;
            return Task.CompletedTask;
        }

        protected async Task DiscordKillMePlease()
        {
            DiscordUnixTimeOnine = 0;
            await DiscordClient.LogoutAsync();
        }

    }
}
