using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BSB.RLV.Miscellaneous
{
    public class GetStatus : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            string part = "#";
            string split = "/";
            int channel = -1;
            if (args.Length == 1)
            {
                _ = int.TryParse(args[0], out channel);
            }
            else if (args.Length == 2)
            {
                part = args[0];
                _ = int.TryParse(args[1], out channel);
            }
            else if (args.Length == 3)
            {
                part = args[0];
                split = args[1];
                _ = int.TryParse(args[2], out channel);
            }
            if (channel > -1)
            {
                string reply = "";
                string addon = "";
                if (bot.Getuuid_rules.ContainsKey(caller_uuid) == true)
                {
                    foreach (string rule in bot.Getuuid_rules[caller_uuid].Keys)
                    {
                        if ((part == "#") || (rule.Contains(part) == true))
                        {
                            reply = "" + reply + "" + addon + "" + rule + "";
                            addon = split;
                        }
                    }
                }
                bot.GetClient.Self.Chat(reply, channel, ChatType.Normal);
                return true;
            }
            return Failed("Invaild channel given");
        }
    }
}
