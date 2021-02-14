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
    class Discord_BanMember : CoreCommand_3arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number", "Number","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Discord server id", "Member id","Optional why message" }; } }
        public override string Helpfile { get { return "Adds the selected user to the ban list - Disallows rejoining untill they are removed from the list "; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                bool reply = BanMember(args).Result;
                return bot.GetCommandsInterface.SmartCommandReply(reply, args[0], "see status for state", CommandName);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected async Task<bool> BanMember(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                if (ulong.TryParse(args[2], out ulong memberid) == true)
                {
                    SocketGuild server = Discord.GetGuild(serverid);
                    SocketGuildUser user = server.GetUser(memberid);
                    string why = "Secondbot API ban - see your own logs";
                    if (args.Length == 4)
                    {
                        why = args[3];
                    }
                    await user.BanAsync(7, why);
                    return true;
                }
            }
            return false;
        }
    }
}
