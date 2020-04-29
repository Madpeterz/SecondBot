using BetterSecondBotShared.Static;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.CMD_Parcel
{
    class SetParcelMusic : ParcelCommand_RequirePerms
    {
        public override string[] ArgTypes { get { return new[] { "String" }; } }
        public override string[] ArgHints { get { return new[] { "A vaild url" }; } }
        public override string Helpfile { get { return "Updates the current parcels music url"; } }

        protected bool set_parcel_music(string url)
        {
            if (targetparcel.MusicURL != url)
            {
                targetparcel.MusicURL = url;
                targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
            }
            return true;
        }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (args.Length == 0)
                {
                    return set_parcel_music("");
                }
                else
                {
                    if (args[0].StartsWith("http") == true)
                    {
                        return set_parcel_music(args[0]);
                    }
                    else
                    {
                        return Failed("Invaild url");
                    }
                }
            }
            return false;
        }
    }
}
