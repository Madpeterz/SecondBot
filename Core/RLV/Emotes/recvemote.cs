using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BetterSecondBot.RLV.Emotes
{
    public class RecvEmote : RLV_UUID_flag_optional_arg_yn
    {
        public override bool AsExceptionRule { get { return true; } }
        public override bool CallFunction(string[] args)
        {
            if (args.Length == 1)
            {
                if (args[0] == "y")
                {
                    bot.SetLock("recvemote_lock", false);
                    return true;
                }
                else if (args[0] == "n")
                {
                    bot.SetLock("recvemote_lock", true);
                    return true;
                }
                return Failed("y/n only");
            }
            else if (args.Length == 2)
            {
                if (UUID.TryParse(args[0], out UUID rec_uuid) == true)
                {
                    return SetFlag(args[1], "set", rec_uuid);
                }
                return Failed("Unable to process target UUID");
            }
            return Failed("Unsupported number of args");
        }
    }
}
