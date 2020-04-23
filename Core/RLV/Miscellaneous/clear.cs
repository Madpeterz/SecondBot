using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BSB.RLV.Miscellaneous
{
    public class Clear : RLV_command
    {
        public override bool CallFunction(string[] args)
        {
            if(args.Length == 1)
            {
                string target = args[0];
                bot.Clearrule(false, caller_uuid);
                bot.Clearrule(true, caller_uuid);
            }
            else
            {
                bot.Clearrule(false, caller_uuid);
                bot.Clearrule(true, caller_uuid);
            }
            return true;
        }
    }
}
