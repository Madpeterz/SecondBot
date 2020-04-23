using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.RLV.Sitting
{
    public class Sit : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            if (args.Length == 1)
            {
                return SetFlag(args[0], "set");
            }
            else if (args.Length == 2)
            {
                if (args[1] == "force")
                {
                    return bot.GetCommandsInterface.Call("sit", args[0]);
                }
                else
                {
                    return Failed("Magic keyword force missing.");
                }
            }
            return Failed("Badly formatted command.");
        }
    }
}
