using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.RLV.Blacklist
{
    public class VersionNumBL : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                _ = int.TryParse(args[0], out int channel);
                if (channel >= 0)
                {
                    string sendim = String.Join('|', bot.Getblacklist_sendim);
                    string recvim = String.Join('|', bot.Getblacklist_recvim);
                    if (sendim == "") { sendim = "#"; }
                    if (recvim == "") { recvim = "#"; }
                    bot.GetClient.Self.Chat("209000," + sendim + "," + recvim + "", channel, OpenMetaverse.ChatType.Normal);
                    return true;
                }
                else
                {
                    return Failed("Invaild channel given");
                }
            }
            return false;
        }
    }
}
