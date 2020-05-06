using System;

namespace BSB.Commands.Self
{
    public class GetLastCommands : CoreCommand_SmartReply_1arg
    {
        public override string Helpfile { get { return "Gets the last 5 commands issued to the bot and returns them via the smart target"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(args[0], String.Join(',',bot.GetLastCommands(5)), CommandName);
            }
            return false;
        }
    }
}
