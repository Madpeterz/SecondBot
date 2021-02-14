using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BetterSecondBot.RLV.Miscellaneous
{
    public class Permissive : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            string action = args[0];
            if ((action == "y") || (action == "n"))
            {
                if (action == "n")
                {
                    bot.SetPermissiveMode(caller_uuid, true);
                }
                else if (action == "y")
                {
                    bot.SetPermissiveMode(caller_uuid, false);
                }
                return true;
            }
            return Failed("Permissive requires y/n!");
        }
    }
}
