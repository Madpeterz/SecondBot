using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BSB.RLV.Movement
{
    public class SetRot : RLV_command
    {
        public override int MinArgs { get { return 2; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (args[1] == "force")
                {
                    return bot.GetCommandsInterface.Call("rotateto", args[0], UUID.Zero);
                }
                return Failed("magic word force is required [dont ask me]");
            }
            return false;
        }
    }
}
