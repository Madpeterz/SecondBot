namespace BetterSecondBot.Commands.Self
{
    public class Stand : CoreCommand
    {
        public override string Helpfile { get { return "Makes the bot stand up if sitting (also resets animations)"; } }
        public override bool CallFunction(string[] args)
        {
            bot.GetClient.Self.Stand();
            bot.ResetAnimations();
            return true;
        }
    }
}