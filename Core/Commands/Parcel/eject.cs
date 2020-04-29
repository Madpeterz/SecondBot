using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.CMD_Parcel
{
    public class ParcelEject : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar", "True|False" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]", "Also Ban" }; } }
        public override string Helpfile { get { return "Ejects an avatar [ARG 1] from a parcel<br/>You can also ban them at the same time if you wish"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID target) == true)
                {
                    bool banuser = false;
                    if (args.Length == 2)
                    {
                        if (args[1] == "True")
                        {
                            banuser = true;
                        }
                    }
                    bot.GetClient.Parcels.EjectUser(target, banuser);
                    return true;
                }
                else
                {
                    return Failed("Arg 0 requires UUID");
                }
            }
            return false;
        }
    }
}
