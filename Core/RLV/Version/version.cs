using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.RLV.Version
{
    public class Version : RLV_command
    {
        public override bool CallFunction(string[] args)
        {
            int channel = 0;
            if (args.Length == 1)
            {
                _ = int.TryParse(args[0], out channel);
            }
            if (channel >= 0)
            {
                bot.GetClient.Self.Chat("RestrainedLife BOT (RLVa 2.9)", channel, OpenMetaverse.ChatType.Normal);
                return true;
            }
            else
            {
                return Failed("Invaild channel given");
            }
        }
    }
}
