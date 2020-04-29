using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Self
{ 
    public class PointAt : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]" }; } }
        public override string Helpfile { get { return "Makes the bot turn to face [ARG 1] and point at them (if found)"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID testavatar) == true)
                {
                    if (bot.GetClient.Network.CurrentSim.AvatarPositions.ContainsKey(testavatar) == true)
                    {
                        bot.ResetAnimations();
                        bot.GetClient.Self.Stand();
                        bot.GetClient.Self.Movement.TurnToward(bot.GetClient.Network.CurrentSim.AvatarPositions[testavatar]);
                        bot.GetClient.Self.Movement.SendUpdate();
                        bot.GetClient.Self.AnimationStart(Animations.POINT_YOU, true);
                        bot.GetClient.Self.PointAtEffect(bot.GetClient.Self.AgentID, testavatar, new Vector3d(0, 0, 0), PointAtType.Select, UUID.Random());
                        bot.GetClient.Self.BeamEffect(bot.GetClient.Self.AgentID, testavatar, new Vector3d(0, 0, 2), new Color4(255, 255, 255, 1), (float)3.0, UUID.Random());
                        return true;
                    }
                    else
                    {
                        return Failed("Cant find UUID in sim");
                    }
                }
                else
                {
                    return Failed("UUID is not vaild");
                }
            }
            return false;
        }
    }
}
