namespace BSB.Commands.Animation
{
    public class ResetAnimations : CoreCommand
    {
        public override string Helpfile { get { return "Resets the animation stack for the bot"; } }
        public override bool CallFunction(string[] args)
        {
            bot.ResetAnimations();
            return true;
        }
    }
}
