using System;
using OpenMetaverse;
namespace BSB.Commands.Movement
{
    public class RotateToFace : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar UUID or Firstname Lastname" }; } }
        public override string Helpfile { get { return "Rotates the bot to face an avatar"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if(UUID.TryParse(args[0],out UUID avatar) == true)
                {
                    if (bot.GetClient.Network.CurrentSim.AvatarPositions.ContainsKey(avatar) == true)
                    {
                        return bot.GetClient.Self.Movement.TurnToward(bot.GetClient.Network.CurrentSim.AvatarPositions[avatar], true);
                    }
                    Failed("Unable to find avatar in local sim memory");
                }
                return Failed("Unable to process avatar on arg 1");
            }
            return false;
        }
    }

}
