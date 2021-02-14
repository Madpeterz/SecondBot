using System.Linq;
using Newtonsoft.Json;
using OpenMetaverse;

namespace BSB.Commands.Chat
{
    class LocalChatHistory : CoreCommand_SmartReply_1arg
    {
        public override string Helpfile { get { return "Gets the last 20 localchat messages the bot has."; } }
        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(true, args[0], JsonConvert.SerializeObject(bot.getLocalChatHistory()), CommandName);
            }
            return false;
        }
    }
}
