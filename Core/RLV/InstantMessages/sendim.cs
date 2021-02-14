using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.RLV.InstantMessages
{
    public class SendIm : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            if (args.Length == 1)
            {
                if (args[0] == "y")
                {
                    bot.SetLock("sendim_lock", false);
                    return true;
                }
                else if (args[0] == "n")
                {
                    bot.SetLock("sendim_lock", true);
                    return true;
                }
                return Failed("y/n only");
            }
            else if (args.Length == 2)
            {
                if (UUID.TryParse(args[0], out UUID rec_uuid) == true)
                {
                    return SetFlag(args[1], "set", rec_uuid);
                }
                return Failed("Unable to process target UUID");
            }
            return Failed("Unsupported number of args");
        }
    }
}
