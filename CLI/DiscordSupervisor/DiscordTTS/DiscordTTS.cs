using System;
using System.Collections.Generic;
using OpenMetaverse;
using Discord;
using BetterSecondBotShared.Static;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Linq;
using BetterSecondBotShared.Json;

namespace BetterSecondBot.DiscordSupervisor
{
    public class DiscordTTS : Discord_super
    {
        public DiscordTTS(JsonConfig configfile, bool as_docker) : base(configfile, as_docker)
        {

        }
        protected ITextChannel TTSchannel;
        protected IGuild TTSDiscordServer;
        protected override async Task<Task> DiscordClientReady()
        {
            await base.DiscordClientReady();
            if (myconfig.DiscordTTS_Enable == true)
            {
                TTSDiscordServer = DiscordClient.GetGuild(myconfig.DiscordTTS_server_id);
                if (TTSDiscordServer != null)
                {
                    IGuildUser user = await TTSDiscordServer.GetCurrentUserAsync();
                    await user.ModifyAsync(x =>
                    {
                        x.Nickname = myconfig.DiscordTTS_Nickname;
                    });
                    FindTTsChannel();
                }
            }
            return Task.CompletedTask;
        }

        protected async void FindTTsChannel()
        {
            if (myconfig.DiscordTTS_Enable == true)
            {
                TTSchannel = await FindTextChannel(TTSDiscordServer, myconfig.DiscordTTS_channel_name);
            }
        }

        protected override async Task DiscordIMMessage(UUID sender_uuid, string sender_name, string message)
        {
            if (myconfig.DiscordTTS_Enable == true)
            {
                if ((helpers.notempty(sender_name) == true) && (helpers.notempty(message) == true))
                {
                    if (sender_uuid.ToString() == myconfig.DiscordTTS_avatar_uuid)
                    {
                        if (TTSchannel == null)
                        {
                            FindTTsChannel();
                        }
                        if (TTSchannel != null)
                        {
                            await TTSchannel.SendMessageAsync(message, true);
                        }
                    }
                }
            }
        }
    }
}
