using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;

namespace BSB.Commands.Chat
{
    class Say : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Mixed","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Message or channel","Message" }; } }
        public override string Helpfile { get { return "Makes the bot talk via chat<br/>Example: say|||Hello<br/>Example: say|||12~#~Goodbye"; } }
        public override int MinArgs { get { return 1; } }
        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                string message = args[0];
                int channel = 0;
                if (args.Count() == 2)
                {
                    message = args[1];
                    int.TryParse(args[0], out channel);
                }
                bot.GetClient.Self.Chat(message, channel, ChatType.Normal);
                return true;
            }
            return false;
        }
    }
}
