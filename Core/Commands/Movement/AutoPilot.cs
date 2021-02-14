using System;
using OpenMetaverse;
using BetterSecondBotShared.Static;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BSB.Commands.Movement
{
    public class AutoPilot : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Vector" }; } }
        public override string[] ArgHints { get { return new[] { "<0,0,0>" }; } }
        public override string Helpfile { get { return "Make the bot auto pilot to <X,Y,Z><br/>Example AutoPilot|||<124,55,22>"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (Vector3.TryParse(args[0], out Vector3 pos) == true)
                {
                    if (helpers.inrange(pos.X, 0, 255) == true)
                    {
                        if (helpers.inrange(pos.Y, 0, 255) == true)
                        {
                            if (helpers.inrange(pos.Z, 0, 5000) == true)
                            {
                                bot.GetClient.Self.AutoPilotCancel();
                                bot.GetClient.Self.Movement.TurnToward(pos, true);
                                Thread.Sleep(500);
                                uint Globalx, Globaly;
                                Utils.LongToUInts(bot.GetClient.Network.CurrentSim.Handle, out Globalx, out Globaly);
                                bot.GetClient.Self.AutoPilot((ulong)(Globalx + pos.X), (ulong)(Globaly + pos.Y), pos.Z);
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
                    return Failed("Unable to process vector");
                }
            }
            return false;
        }
    }

}
