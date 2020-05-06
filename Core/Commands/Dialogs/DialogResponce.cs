namespace BSB.Commands.Dialogs
{
    class DialogResponce : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Number","Text" }; } }
        public override string[] ArgHints { get { return new[] { "DialogID","Button to click" }; } }
        public override string Helpfile { get { return "Makes the bot interact with the dialog [ARG 1] with the button [ARG 2]"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (int.TryParse(args[0], out int dialogid) == true)
                {
                    return bot.DialogReply(dialogid, args[1]);
                }
                else
                {
                    return Failed("Invaild dialog id");
                }
            }
            return false;
        }
    }

}
