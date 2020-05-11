using BetterSecondBotShared.Static;

namespace BSB.Commands.Info
{
    public class UnixTimeNow : CoreCommand_SmartReply_1arg
    {
        public override string Helpfile { get { return "Requests the current unixtime at the bot and sends that via the smart reply target"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(true,args[0], helpers.UnixTimeNow().ToString(), CommandName);
            }
            return false;
        }
    }
}
