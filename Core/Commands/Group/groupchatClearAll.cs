namespace BetterSecondBot.Commands.Group
{
    class GroupchatClearAll : CoreCommand
    {
        public override string Helpfile { get { return "Clears all group chat buffers at once"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                bot.ClearAllGroupchat();
                return true;
            }
            return false;
        }
    }
}
