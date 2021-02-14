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

namespace BSB.Commands.DiscordAPI
{
    class Discord_AddRole : CoreCommand_4arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number", "Number", "Number" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Discord server id", "Role id", "Member id" }; } }
        public override string Helpfile { get { return "Adds the selected role to the member"; } }
        public override bool CallFunction(string[] args) 
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                bool reply = addRoleToMember(args).Result;
                return bot.GetCommandsInterface.SmartCommandReply(reply, args[0], "see status for state", CommandName);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected async Task<bool> addRoleToMember(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                if(ulong.TryParse(args[2], out ulong roleid) == true)
                {
                    if (ulong.TryParse(args[3], out ulong memberid) == true)
                    {
                        SocketGuild server = Discord.GetGuild(serverid);
                        SocketRole role = server.GetRole(roleid);
                        SocketGuildUser user = server.GetUser(memberid);
                        await user.AddRoleAsync(role); // ? Irole seems to accept SocketRole ?
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
