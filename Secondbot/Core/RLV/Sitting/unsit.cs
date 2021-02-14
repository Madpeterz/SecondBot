using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.RLV.Sitting
{
    public class UnSit : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            if (args[0] == "force")
            {
                return bot.GetCommandsInterface.Call("stand", "", UUID.Zero);
            }
            else if ((args[0] == "y") || (args[0] == "n"))
            {
                return SetFlag(args[0], "set");
            }
            return Failed("Magic keyword force missing.");
        }
    }
}
