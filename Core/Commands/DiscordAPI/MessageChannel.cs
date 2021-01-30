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
    class Discord_MessageChannel : CoreCommand_4arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number", "Number", "Message", "True|False" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Discord server id", "channel id", "Message to send", "Use TTS defaults to False" }; } }
        public override string Helpfile { get { return "Sends a message to the selected channel - Optional TTS usage"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.discordReady() == false)
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], bot.getDiscordWhyFailed(), CommandName);
                }
                bool reply = MessageChannel(args).Result;
                return bot.GetCommandsInterface.SmartCommandReply(reply, args[0], "see status for state", CommandName);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "incorrect args", CommandName);
        }

        protected async Task<bool> MessageChannel(string[] args)
        {
            DiscordSocketClient Discord = bot.getDiscordClient();
            if (ulong.TryParse(args[1], out ulong serverid) == true)
            {
                if (ulong.TryParse(args[2], out ulong channelid) == true)
                {
                    bool useTTS = false;
                    if(args.Length == 5)
                    {
                        bool.TryParse(args[4], out useTTS);
                    }
                    SocketGuild server = Discord.GetGuild(serverid);
                    SocketTextChannel Channel = null;
                    foreach (SocketTextChannel channel in server.TextChannels)
                    {
                        if(channel.Id == channelid)
                        {
                            Channel = channel;
                            break;
                        }
                    }
                    if(Channel != null)
                    {
                        await Channel.SendMessageAsync(args[3], useTTS);
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
