namespace BSB.Commands.Movement
{
    public class AutoPilotStop : CoreCommand
    {
        public override string Helpfile { get { return "Stops any current AutoPilot"; } }
        public override bool CallFunction(string[] args)
        {
            bot.GetClient.Self.AutoPilotCancel();
            return true;
        }
    }

}
