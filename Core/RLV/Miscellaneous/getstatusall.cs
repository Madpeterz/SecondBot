using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BSB.RLV.Miscellaneous
{
    public class GetStatusAll : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                string part = "#";
                string split = "/";
                int channel = -1;
                if (args.Length == 1)
                {
                    int.TryParse(args[0], out channel);
                }
                else if (args.Length == 2)
                {
                    part = args[0];
                    int.TryParse(args[1], out channel);
                }
                else if (args.Length == 3)
                {
                    part = args[0];
                    split = args[1];
                    int.TryParse(args[2], out channel);
                }
                if (channel > -1)
                {
                    string reply = "";
                    string addon = "";
                    foreach (UUID a in bot.Getuuid_rules.Keys)
                    {
                        foreach (string rule in bot.Getuuid_rules[a].Keys)
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
                else
                {
                    Failed("Invaild channel given");
                }
            }
            return false;
        }
    }
}
