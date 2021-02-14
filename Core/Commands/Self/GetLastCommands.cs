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
                int entry = 1;
                foreach(string a in bot.GetLastCommands(5))
                {
                    collection.Add(entry.ToString(), a);
                    entry++;
                }
                return bot.GetCommandsInterface.SmartCommandReply(true,args[0], "last=5", CommandName,collection);
            }
            return false;
        }
    }
}
