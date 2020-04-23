using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.RLV.Blacklist
{
    public class GetBlacklist : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                int channel;
                string filter = "";
                if(args.Length == 2)
                {
                    if(int.TryParse(args[1], out channel) == true)
                    {
                        filter = args[0];
                    }
                }
                else
                {
                    int.TryParse(args[0], out channel);
                }
                if (channel >= 0)
                {
                    string sendim = "";
                    string recvim = "";
                    if (filter == "")
                    {
                        sendim = String.Join('|', bot.Getblacklist_sendim);
                        recvim = String.Join('|', bot.Getblacklist_recvim);
                    }
                    else
                    {
                        string addon = "";
                        foreach (string a in bot.Getblacklist_sendim)
                        {
                            if (a.Contains(filter) == true)
                            {
                                sendim = "" + sendim + "" + addon + "" + a + "";
                                addon = "|";
                            }
                        }
                        addon = "";
                        foreach (string a in bot.Getblacklist_recvim)
                        {
                            if (a.Contains(filter) == true)
                            {
                                recvim = "" + recvim + "" + addon + "" + a + "";
                                addon = "|";
                            }
                        }
                    }
                    if (sendim == "") sendim = "#";
                    if (recvim == "") recvim = "#";
                    bot.GetClient.Self.Chat("" + sendim + "," + recvim + "", channel, OpenMetaverse.ChatType.Normal);
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
