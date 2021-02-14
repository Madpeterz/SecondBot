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
    class Discord_Dm_Member : CoreCommand_4arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number", "Number", "Text" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Server id", "Member id", "Message" }; } }
        public override string Helpfile { get { return "Sends a message directly to the user [They must be in the server]\n This command requires the SERVER MEMBERS INTENT found in discord app dev"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                bool reply = MessageMember(args).Result;
                return bot.GetCommandsInterface.SmartCommandReply(reply, args[0], "see status for state", CommandName);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected async Task<bool> MessageMember(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                if (ulong.TryParse(args[2], out ulong memberid) == true)
                {
                    try
                    {
                        SocketGuild server = Discord.GetGuild(serverid);
                        IEnumerable<IGuildUser> users = await server.GetUsersAsync().FlattenAsync().ConfigureAwait(true);
                        bool sent = false;
                        foreach(IGuildUser user in users)
                        {
                            if(user.Id == memberid)
                            {
                                sent = true;
                                await user.SendMessageAsync(args[3]);
                                break;
                            }
                        }
                        return sent;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }
            return false;
        }
    }
}
