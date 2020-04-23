using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BSB.Commands.Notecard
{
    class NotecardAdd : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Text","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Collection","Content" }; } }
        public override string Helpfile { get { return "Adds [ARG 2] to the Collection [ARG 1]<br/> Also creates the collection if it does not exist"; } }
        public override int MinArgs { get { return 2; } }

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
