using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.RLV.Version
{
    public class Version : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            _ = int.TryParse(args[0], out int channel);
            if (channel >= 0)
            {
                bot.GetClient.Self.Chat("RestrainedLife BOT (RLVa 2.9)", channel, OpenMetaverse.ChatType.Normal);
                return true;
            }
            return Failed("Invaild channel given");
        }
    }
}
