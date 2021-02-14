using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using Newtonsoft.Json;
using Discord.WebSocket;
using Discord;
using Discord.Rest;
using System.Threading.Tasks;

namespace BetterSecondBot.Commands.DiscordAPI
{
    class Discord_MuteMember : CoreCommand_3arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number", "Number", "True|False" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Discord server id", "Member id", "{Optional} Mute status (defaults to true) set this to False to unmute" }; } }
        public override string Helpfile { get { return "Updates the Mute status of the member"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                bool reply = MuteMember(args).Result;
                return bot.GetCommandsInterface.SmartCommandReply(reply, args[0], "see status for state", CommandName);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected async Task<bool> MuteMember(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                if (ulong.TryParse(args[2], out ulong memberid) == true)
                {
                    SocketGuild server = Discord.GetGuild(serverid);
                    SocketGuildUser user = server.GetUser(memberid);
                    bool status = true;
                    if (args.Length == 4)
                    {
                        bool.TryParse(args[3], out status);
                    }
                    await user.ModifyAsync(pr =>
                    {
                        pr.Mute = status;
                    });
                    return true;
                }
            }
            return false;
        }
    }
}
