namespace BetterSecondBot.Commands.Dialogs
{
    class DialogTrack : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "True|False" }; } }
        public override string[] ArgHints { get { return new[] { "Status" }; } }
        public override string Helpfile { get { return "Should the bot track dialogs and send them to the relays setup?"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if(bool.TryParse(args[0],out bool status) == true)
                {
                    bot.SetTrackDialogs(status);
                    return true;
                }
            }
            return false;
        }
    }

}
