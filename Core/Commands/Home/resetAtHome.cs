namespace BetterSecondBot.Commands.Home
{
    public class ResetAtHome : CoreCommand
    {
        public override string Helpfile { get { return "Resets the at home lockouts"; } }
        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                bot.ResetAtHome();
                return true;
            }
            return false;
        }
    }
}
