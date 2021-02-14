namespace BetterSecondBot.Commands.Info
{
    public class GetPosition : CoreCommand_SmartReply_1arg
    {
        public override string Helpfile { get { return "Requests the Position and sends that via the smart reply target"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(true,args[0], bot.GetClient.Self.RelativePosition.ToString(), CommandName);
            }
            return false;
        }
    }
}
