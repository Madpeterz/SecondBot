namespace BSB.Commands.Movement
{
    public class BotFly : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "True|False" }; } }
        public override string[] ArgHints { get { return new[] { "Should the bot be flying" }; } }
        public override string Helpfile { get { return "Makes the bot fly (or not)"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bool.TryParse(args[0], out bool status) == true)
                {
                    bot.GetClient.Self.Movement.Fly = status;
                    bot.GetClient.Self.Movement.SendUpdate();
                    return true;
                }
                else
                {
                    return Failed("Unable to process rotation");
                }
            }
            return false;
        }
    }

}
