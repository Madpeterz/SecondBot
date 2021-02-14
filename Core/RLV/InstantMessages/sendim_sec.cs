using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.RLV.InstantMessages
{
    public class SendIm_Sec : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            if (args[0] == "y") { bot.SetLock("sendim_lock", false); }
            else if (args[0] == "n") { bot.SetLock("sendim_lock", true); }
            else { return Failed("y/n only"); }
            return true;
        }
    }
}