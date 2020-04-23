using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
namespace BSB.Commands.Movement
{
    public class RotateTo : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Number" }; } }
        public override string[] ArgHints { get { return new[] { "0 to 360" }; } }
        public override string Helpfile { get { return "Rotates the avatar to face a rotation from north in Degrees"; } }
        public override int MinArgs { get { return 1; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (float.TryParse(args[0], out float target_yaw) == true)
                {
                    float yaw = target_yaw;
                    bot.GetClient.Self.Movement.BodyRotation.GetEulerAngles(out float roll, out float pitch, out _);
                    roll *= 57.2958f;
                    pitch *= 57.2958f;
                    yaw *= 57.2958f;
                    float rollOver2 = roll * 0.5f;
                    float sinRollOver2 = (float)Math.Sin((double)rollOver2);
                    float cosRollOver2 = (float)Math.Cos((double)rollOver2);
                    float pitchOver2 = pitch * 0.5f;
                    float sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
                    float cosPitchOver2 = (float)Math.Cos((double)pitchOver2);
                    float yawOver2 = yaw * 0.5f;
                    float sinYawOver2 = (float)Math.Sin((double)yawOver2);
                    float cosYawOver2 = (float)Math.Cos((double)yawOver2);
                    Quaternion result;
                    result.W = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
                    result.X = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
                    result.Y = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
                    result.Z = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;
                    bot.GetClient.Self.Movement.BodyRotation = result;
                    bot.GetClient.Self.Movement.SendUpdate();
                    return true;
                }
                else
                {
                    return Failed("Unable to process rotation");
                }
            }
            return false;
        }
    }

}
