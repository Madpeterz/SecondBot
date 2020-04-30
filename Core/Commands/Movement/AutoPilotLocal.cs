using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
namespace BSB.Commands.Movement
{
    public class AutoPilotLocal : CoreCommand_3arg
    {
        public override string[] ArgTypes { get { return new[] { "Number","Number","number" }; } }
        public override string[] ArgHints { get { return new[] { "0 to 255", "0 to 255","0 to 5000" }; } }
        public override string Helpfile { get { return "Make the bot auto pilot to X,Y,Z"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (int.TryParse(args[0], out int x) == true)
                {
                    if (int.TryParse(args[1], out int y) == true)
                    {
                        if (float.TryParse(args[2], out float z) == true)
                        {
                            if ((x >= 0) && (x <= 255) && (y >= 0) && (y <= 255) && (z >= 0) && (z <= 5000))
                            {
                                bot.GetClient.Self.Chat("Attempting to walk to " + x.ToString() + " " + y.ToString() + " " + z.ToString() + "",0,ChatType.Normal);
                                bot.GetClient.Self.AutoPilotLocal(x, y, z);
                                return true;
                            }
                            else
                            {
                                return Failed("x,y,z cords are out of spec");
                            }
                        }
                        else
                        {
                            return Failed("arg 3 \"z\" is not vaild)");
                        }
                    }
                    else
                    {
                        return Failed("arg 2 \"y\" is not vaild)");
                    }
                }
                else
                {
                    return Failed("arg 1 \"x\" is not vaild)");
                }
            }
            return false;
        }
    }

}
