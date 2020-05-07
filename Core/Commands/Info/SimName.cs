namespace BSB.Commands.Info
{
    public class SimName : CoreCommand_SmartReply_1arg
    {
        public override string Helpfile { get { return "Requests the current Sim Position and sends that via the smart reply target"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(args[0], bot.GetClient.Network.CurrentSim.Name, CommandName);
            }
            return false;
        }
    }
}
