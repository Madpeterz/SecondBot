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
    class Discord_Dm_Member : CoreCommand_3arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "Number", "Text" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]", "Member id", "Message" }; } }
        public override string Helpfile { get { return "Sends a message directly to the user \n This will fail if they are not friends or dont share a server!"; } }
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
            if (ulong.TryParse(args[1], out ulong memberid) == true)
            {
                SocketUser user = Discord.GetUser(memberid);
                await user.SendMessageAsync(args[2]);
                return true;
            }
            return false;
        }
    }
}
