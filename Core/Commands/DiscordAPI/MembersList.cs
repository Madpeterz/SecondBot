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
    class Discord_MembersList : CoreCommand_SmartReply_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Discord server id" }; } }
        public override string Helpfile { get { return "Returns a list of members in a server \n collection is userid: username \n if the user has set a nickname: userid: nickname|username \n This command requires Discord full client mode enabled and connected\n !!!! This command also requires: Privileged Gateway Intents / SERVER MEMBERS INTENT set to true on the discord bot api area !!!"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                KeyValuePair<bool, Dictionary<string, string>> reply = ListMembers(args).Result;
                return bot.GetCommandsInterface.SmartCommandReply(reply.Key, args[0], "see status for state", CommandName, reply.Value);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected async Task<KeyValuePair<bool, Dictionary<string, string>>> ListMembers(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                SocketGuild server = Discord.GetGuild(serverid);
                IEnumerable<IGuildUser> users = await server.GetUsersAsync().FlattenAsync().ConfigureAwait(true);
                Dictionary<string, string> membersList = new Dictionary<string, string>();

                foreach (IGuildUser user in users)
                {
                    if (user.Nickname != null)
                    {
                        membersList.Add(user.Id.ToString(), user.Nickname+"|"+user.Username);
                    }
                    else
                    {
                        membersList.Add(user.Id.ToString(), user.Username);
                    }
                }
                return new KeyValuePair<bool, Dictionary<string, string>>(true, membersList);
            }
            return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
        }
    }
}
