using System;
using OpenMetaverse;
using BetterSecondBotShared.Static;
namespace BSB.Commands.Movement
{
    public class AutoPilotLocal : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Mixed","Number","number" }; } }
        public override string[] ArgHints { get { return new[] { "Vector [OR] 0 to 255", "0 to 255","0 to 5000" }; } }
        public override string Helpfile { get { return "Make the bot auto pilot to X,Y,Z<br/>Example AutoPilotLocal|||<124,55,22>"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                bool ok = false;
                int x = 0;
                int y = 0;
                float z = 0;
                if (args.Length == 3)
                {
                    if (int.TryParse(args[0], out x) == true)
                    {
                        if (int.TryParse(args[1], out y) == true)
                        {
                            if (float.TryParse(args[2], out z) == true)
                            {
                                ok = true;
                            }
                            else
                            {
                                return Failed("Unable to process Z cord");
                            }
                        }
                        else
                        {
                            return Failed("Unable to process Y cord");
                        }
                    }
                    else
                    {
                        return Failed("Unable to process X cord");
                    }
                }
                else if (args.Length == 1)
                {
                    if (Vector3.TryParse(args[0], out Vector3 pos) == true)
                    {
                        x = (int)Math.Round(pos.X);
                        y = (int)Math.Round(pos.Y);
                        z = pos.Z;
                        ok = true;
                    }
                    else
                    {
                        return Failed("Unable to process vector");
                    }
                }
                if (ok == true)
                {
                    if (helpers.inrange(x, 0, 255) == true)
                    {
                        if (helpers.inrange(y, 0, 255) == true)
                        {
                            if (helpers.inrange(z, 0, 5000) == true)
                            {
                                bot.GetClient.Self.AutoPilotLocal(x, y, z);
                                return true;
                            }
                            else
                            {
                                return Failed("Z cord is out of range");
                            }
                        }
                        else
                        {
                            return Failed("Y cord is out of range");
                        }
                    }
                    else
                    {
                        return Failed("X cord is out of range");
                    }
                }
                else
                {
                    return Failed("Unknown issue");
                }
            }
            return false;
        }
    }

}
