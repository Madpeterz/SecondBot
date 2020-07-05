namespace BSB.Commands.Home
{
    public class GoHome : CoreCommand
    {
        public override string Helpfile { get { return "Makes the bot teleport to its home region"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                bot.GotoNextHomeRegion();
                return true;
            }
            return false;
        }
    }
}
