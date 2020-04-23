using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;

namespace BSB.Commands.Self
{
    class BotVersion : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Mixed" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply target: Channel,UUID,HTTP" }; } }
        public override string Helpfile { get { return "Returns the bots build version"; } }
        public override int MinArgs { get { return 0; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                string target = "0";
                if (args.Length > 0)
                {
                    target = args[0];
                }
                return bot.GetCommandsInterface.SmartCommandReply(target, bot.MyVersion, CommandName);
            }
            return false;
        }
    }
}
