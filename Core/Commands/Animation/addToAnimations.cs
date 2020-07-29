namespace BSB.Commands.Animation
{
    public class AddToAllowAnimations : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Text" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar name"}; } }
        public override string Helpfile { get { return "Toggles if animation requests from this avatar (used for remote poseballs) are accepted<br/>Case sensitive also requires Lastname"; } }
        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                bot.ToggleAutoAcceptAnimations(args[0]);
                return true;
            }
            return false;
        }
    }
}
