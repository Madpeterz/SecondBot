using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BetterSecondBot.Commands.CMD_Parcel
{
    public class GetParcelFlags : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "String" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply [Channel|Avatar|http url]", "[Repeatable] flag name" }; } }
        public override string Helpfile { get { return "Returns the value of the parcel flags (At the parcel on the bot is currently on)<br/>" +
                    "requested on [ARG 2+] via [ARG 1] smart reply target<br/>" +
                    "If you request multiple Flags they are split with CSV<br/>You can also get all the flags skipping [ARG 2]<br/><br/>" +
                    "" + helpers.create_dirty_table(parcel_static.get_flag_names()) + "<br/><br/>Example: getparcelflag|||0~#~ForSale<br/>Example: getparcelflag|||12"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                Dictionary<string, ParcelFlags> flags = parcel_static.get_flags_list();
                List<string> get_flags = new List<string>();
                List<string> otherargs = new List<string>(args);
                string target = otherargs[0];
                otherargs.RemoveAt(0);
                if (otherargs.Count == 0)
                {
                    otherargs.Add("ALL");
                }
                if (otherargs[0] == "ALL")
                {
                    foreach (string A in flags.Keys)
                    {
                        get_flags.Add(A);
                    }
                }
                else
                {
                    foreach (string a in otherargs)
                    {
                        if (flags.ContainsKey(a) == true)
                        {
                            get_flags.Add(a);
                        }
                        else
                        {
                            LogFormater.Warn("Flag: " + a + " is unknown");
                        }
                    }
                }

                if (get_flags.Count > 0)
                {
                    int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                    if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == true)
                    {
                        OpenMetaverse.Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
                        Dictionary<string, string> collection = new Dictionary<string, string>();
                        foreach (string cfg in get_flags)
                        {
                            if (flags.ContainsKey(cfg) == true)
                            {
                                collection.Add(cfg, p.Flags.HasFlag(flags[cfg]).ToString());
                            }
                        }
                        return bot.GetCommandsInterface.SmartCommandReply(true,target, p.Name, CommandName, collection);
                    }
                    else
                    {
                        return Failed("Unable to find parcel in memory, please wait and try again");
                    }
                }
                else
                {
                    return Failed("No accepted flags");
                }
            }
            return false;
        }
    }
}
