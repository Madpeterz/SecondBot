using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BSB.Commands.Notecard
{
    class NotecardClear : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Text" }; } }
        public override string[] ArgHints { get { return new[] { "Collection" }; } }
        public override string Helpfile { get { return "Clears Collection [ARG 1]"; } }
        public override int MinArgs { get { return 1; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                bot.ClearNotecardStorage(args[0]);
                return true;
            }
            return false;
        }
    }
}
