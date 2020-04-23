using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Home
{
    public class GoHome : CoreCommand
    {
        public override string Helpfile { get { return "Makes the bot teleport to its home region"; } }
        public override int MinArgs { get { return 0; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                bot.GotoNextHomeRegion();
            }
            return false;
        }
    }
}
