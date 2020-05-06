namespace BSB.Commands.Home
{
    public class AtHomeSameSimRange : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Float" }; } }
        public override string[] ArgHints { get { return new[] { "Range" }; } }
        public override string Helpfile { get { return "Temporary change the @home systems recovery max range from home (in m) [min 3] <br/> resets on next start up"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (float.TryParse(args[0], out float range) == true)
                {
                    bot.TempUpdateAtHomeSimRange(range);
                    return true;
                }
                else
                {
                    return Failed("Unable to process arg 1");
                }
            }
            return false;
        }
    }
}
