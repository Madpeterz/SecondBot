using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Self
{
    public class Stand : CoreCommand
    {
        public override string Helpfile { get { return "Makes the bot stand up if sitting (also resets animations)"; } }
        public override int MinArgs { get { return 0; } }
        public override bool CallFunction(string[] args)
        {
            bot.GetClient.Self.Stand();
            bot.ResetAnimations();
            return true;
        }
    }
}
