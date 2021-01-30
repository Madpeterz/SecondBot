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
    class Discord_BulkClear_Messages : CoreCommand_3arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number", "Number" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Discord server id", "Member id" }; } }
        public override string Helpfile { get { return "Clears messages on the server sent by the member in the last 13 days, 22hours 59mins"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                KeyValuePair<bool, int> reply = ClearMessages(args).Result;
                Dictionary<string, string> collection = new Dictionary<string, string>();
                collection.Add("Deleted", reply.Value.ToString());
                return bot.GetCommandsInterface.SmartCommandReply(reply.Key, args[0], "See status for state, see collection for total deleted", CommandName, collection);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected async Task<KeyValuePair<bool,int>> ClearMessages(string[] args)
        {
            int messagesDeleted = 0;
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                if (ulong.TryParse(args[2], out ulong memberid) == true)
                {
                    SocketGuild server = Discord.GetGuild(serverid);
                    SocketGuildUser user = server.GetUser(memberid);
                    TimeSpan somedays = new TimeSpan(13, 22, 59, 59, 0);
                    DateTimeOffset justundertwosweeks = DateTimeOffset.Now;
                    justundertwosweeks = justundertwosweeks.Subtract(somedays);
                    long unixtimelimit = justundertwosweeks.ToUnixTimeSeconds();
                    foreach (SocketTextChannel channel in server.TextChannels)
                    {
                        List<IMessage> DeleteMessages = new List<IMessage>();
                        IEnumerable<IMessage> messages = await channel.GetMessagesAsync(100).FlattenAsync();
                        foreach (IMessage message in messages)
                        {
                            if (message.CreatedAt.ToUnixTimeSeconds() > unixtimelimit)
                            {
                                if (message.Author.Id == user.Id)
                                {
                                    DeleteMessages.Add(message);
                                }
                            }
                        }
                        if(DeleteMessages.Count() > 0)
                        {
                            messagesDeleted += DeleteMessages.Count();
                            await channel.DeleteMessagesAsync(DeleteMessages);
                        }
                    }
                    return new KeyValuePair<bool, int>(true, messagesDeleted);
                }
            }
            return new KeyValuePair<bool, int>(false,0);
        }
    }
}
