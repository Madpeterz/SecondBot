namespace BSB.Commands.Self
{
    class BotVersion : CoreCommand_SmartReply_1arg
    {
        public override string Helpfile { get { return "Returns the bots build version"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(true,args[0], bot.MyVersion, CommandName);
            }
            return false;
        }
    }
}
