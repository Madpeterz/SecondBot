namespace BetterSecondBot.Commands.Notecard
{
    class NotecardAdd : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Text","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Collection","Content" }; } }
        public override string Helpfile { get { return "Adds [ARG 2] to the Collection [ARG 1]<br/> Also creates the collection if it does not exist"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                bot.NotecardAddContent(args[0], args[1]);
                return true;
            }
            return false;
        }
    }
}
