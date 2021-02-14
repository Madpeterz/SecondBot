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
    class Discord_TextChannels_List : CoreCommand_SmartReply_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Discord server id" }; } }
        public override string Helpfile { get { return "Returns a list of text channels in a server \n collection is channelid: name"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                KeyValuePair<bool, Dictionary<string, string>> reply = ListTextChannels(args);
                return bot.GetCommandsInterface.SmartCommandReply(reply.Key, args[0], "see status for state", CommandName, reply.Value);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected KeyValuePair<bool, Dictionary<string, string>> ListTextChannels(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                SocketGuild server = Discord.GetGuild(serverid);
                Dictionary<string, string> textChannels = new Dictionary<string, string>();
                foreach(SocketTextChannel channel in server.TextChannels)
                {
                    textChannels.Add(channel.Id.ToString(), channel.Name);
                }
                return new KeyValuePair<bool, Dictionary<string, string>>(true, textChannels);
            }
            return new KeyValuePair<bool, Dictionary<string, string>>(false, new Dictionary<string, string>());
        }
    }
}
