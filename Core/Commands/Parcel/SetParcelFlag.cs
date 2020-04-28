using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.CMD_Parcel
{

    public class SetParcelFlag : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Text=True|False" }; } }
        public override string[] ArgHints { get { return new[] { "[Repeatable] flag args" }; } }
        public override int MinArgs { get { return 1; } }

        public override string Helpfile { get { return "Sets the parcel flags of the parcel the bot is currently standing on<br/>" +
                    "Example:AllowFly=False~#~AllowDamage=True<br/>" + helpers.create_dirty_table(parcel_static.get_flag_names()) +"<br/>"; } }


        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                List<string> acceptablewords = new List<string>();
                Dictionary<string, ParcelFlags> flags = parcel_static.get_flags_list();
                acceptablewords.AddRange(new[] { "True", "False" });

                Dictionary<string, bool> setflags = new Dictionary<string, bool>();
                foreach (string a in args)
                {
                    string[] parts = a.Split('=',StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length == 2)
                    {
                        if (flags.ContainsKey(parts[0]) == true)
                        {
                            if (acceptablewords.Contains(parts[1]))
                            {
                                setflags.Add(parts[0], Convert.ToBoolean(parts[1]));
                            }
                            else
                            {
                                ConsoleLog.Crit("Unable to set flag " + parts[0] + " to : " + parts[1] + "");
                            }
                        }
                        else
                        {
                            ConsoleLog.Warn("Flag: " + parts[0] + " is unknown");
                        }
                    }
                    else
                    {
                        ConsoleLog.Warn("Flag: " + a + " missing = True|False");
                    }
                }
                if (setflags.Count > 0)
                {
                    int localid = bot.GetClient.Parcels.GetParcelLocalID(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimPosition);
                    if (bot.GetClient.Network.CurrentSim.Parcels.ContainsKey(localid) == true)
                    {
                        Parcel p = bot.GetClient.Network.CurrentSim.Parcels[localid];
                        if (parcel_static.has_parcel_perm(p, bot) == true)
                        {
                            foreach (KeyValuePair<string, bool> cfg in setflags)
                            {
                                if (flags.ContainsKey(cfg.Key) == true)
                                {
                                    parcel_static.ParcelSetFlag(flags[cfg.Key], p, cfg.Value);
                                }
                            }
                            p.Update(bot.GetClient.Network.CurrentSim, false);
                            return true;
                        }
                        else
                        {
                            return Failed("Incorrect perms to control parcel");
                        }
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
