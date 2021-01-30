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
    class Discord_RoleRemove: CoreCommand_SmartReply_3arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number", "Number" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Discord server id", "Role id" }; } }
        public override string Helpfile { get { return "Remove a new role from a server \n This command requires Discord full client mode enabled and connected"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                bool status = RemoveRole(args).Result;
                return bot.GetCommandsInterface.SmartCommandReply(status, args[0], "see status for removal state", CommandName);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected async Task<bool> RemoveRole(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                if (ulong.TryParse(args[2], out ulong roleid) == true)
                {
                    SocketGuild server = Discord.GetGuild(serverid);
                    SocketRole role = server.GetRole(roleid);
                    await role.DeleteAsync().ConfigureAwait(true);
                    return true;
                }
            }
            return false;
        }
    }
}
