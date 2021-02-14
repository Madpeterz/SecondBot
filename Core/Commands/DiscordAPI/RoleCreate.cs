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
    class Discord_RoleCreate : CoreCommand_SmartReply_3arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart","Number","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]","Discord server id","Role name" }; } }
        public override string Helpfile { get { return "Creates a new role and returns its role ID via smart target\n This command requires Discord full client mode enabled and connected"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if(bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                KeyValuePair<string, ulong> reply = CreateRole(args).Result;
                Dictionary<string, string> collection = new Dictionary<string, string>();
                collection.Add("roleid", reply.Value.ToString());
                return bot.GetCommandsInterface.SmartCommandReply(true, args[0], "ok", CommandName, collection);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected async Task<KeyValuePair<string,ulong>> CreateRole(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                SocketGuild server = Discord.GetGuild(serverid);
                RestRole role = await server.CreateRoleAsync(args[2], new GuildPermissions(), Color.DarkRed, false, null).ConfigureAwait(true);
                return new KeyValuePair<string, ulong>("ok", role.Id);
            }
            return new KeyValuePair<string, ulong>("Unable to process server id", 0);
        }
    }
}
