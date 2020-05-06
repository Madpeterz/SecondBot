namespace BSB.Commands.Self
{
    public class Balance : CoreCommand_SmartReply_1arg
    {
        public override string Helpfile { get { return "Requests the current balance and sends that via the smart reply target and then requests the balance to update."; } }
        public override bool CallFunction(string[] args)
        {
            if(bot.GetAllowFunds == true)
            {
                bool status = bot.GetCommandsInterface.SmartCommandReply(args[0], bot.GetClient.Self.Balance.ToString(), CommandName);
                bot.GetClient.Self.RequestBalance();
                return status;
            }
            return bot.GetCommandsInterface.SmartCommandReply(args[0], "0", CommandName);
        }
    }
}
