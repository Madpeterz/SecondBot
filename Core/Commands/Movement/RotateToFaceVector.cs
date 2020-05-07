using System;
using BetterSecondBotShared.Static;
using OpenMetaverse;
namespace BSB.Commands.Movement
{
    public class RotateToFaceVector : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Vector" }; } }
        public override string[] ArgHints { get { return new[] { "The location to face Example: <25,123,33>" }; } }
        public override string Helpfile { get { return "Rotates the bot to face a vector from its current location"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if(Vector3.TryParse(args[0],out Vector3 pos) == true)
                {
                    if (helpers.inrange(pos.X, 0, 255) == true)
                    {
                        if (helpers.inrange(pos.Y, 0, 255) == true)
                        {
                            if (helpers.inrange(pos.Z, 0, 5000) == true)
                            {
                                return bot.GetClient.Self.Movement.TurnToward(pos, true);
                            }
                            return Failed("Arg 1 - Vector Z is out of range");
                        }
                        return Failed("Arg 2 - Vector Y is out of range");
                    }
                    return Failed("Arg 1 - Vector X is out of range");
                }
                return Failed("Unable to process vector on arg 1");
            }
            return false;
        }
    }

}
