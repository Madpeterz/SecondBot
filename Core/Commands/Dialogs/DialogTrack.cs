using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.Dialogs
{
    class DialogTrack : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "True|False" }; } }
        public override string[] ArgHints { get { return new[] { "Status" }; } }
        public override string Helpfile { get { return "Should the bot track dialogs and send them to the relays setup?"; } }
        public override int MinArgs { get { return 1; } }
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
