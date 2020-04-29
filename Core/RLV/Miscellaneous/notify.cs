using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.RLV.Miscellaneous
{
    public class Notify : RLV_command_2arg
    {
        public override bool CallFunction(string[] args)
        {
            _ = int.TryParse(args[0], out int channel);
            if (channel >= 0)
            {
                string word = "#";
                string action;
                if (args.Length == 3)
                {
                    word = args[1];
                    action = args[2];
                }
                else
                {
                    action = args[1];
                }
                if ((action == "add") || (action == "n"))
                {
                    bot.NotifyUpdate(true, word, channel);
                    return true;
                }
                else if ((action == "rem") || (action == "y"))
                {
                    bot.NotifyUpdate(false, word, channel);
                    return true;
                }
                else
                {
                    return Failed("unknown action add/rem only! Action:" + action + " Channel:" + channel.ToString() + " Word: " + word + "");
                }
            }
            return Failed("Invaild channel given");
        }
    }
}
