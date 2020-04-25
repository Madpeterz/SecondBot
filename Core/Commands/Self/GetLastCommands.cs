using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.Self
{
    public class GetLastCommands : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Mixed" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply [Channel|Avatar|http url]", }; } }
        public override int MinArgs { get { return 1; } }
        public override string Helpfile
        {
            get
            {
                return "";
            }
        }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(args[0], String.Join(',',bot.GetLastCommands(5)), CommandName);
            }
            return false;
        }
    }
}
