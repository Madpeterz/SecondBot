using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Home
{
    public class ResetAtHome : CoreCommand
    {
        public override string Helpfile { get { return "Resets the at home lockouts"; } }
        public override int MinArgs { get { return 0; } }
        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                bot.ResetAtHome();
                return true;
            }
            return false;
        }
    }
}
