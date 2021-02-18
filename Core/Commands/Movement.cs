using BetterSecondBot.bottypes;
using BetterSecondBot.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading;
using BetterSecondBotShared.Static;

namespace BetterSecondBot.HttpService
{
    public class HTTP_Movement : WebApiControllerWithTokens
    {
        public HTTP_Movement(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("uses the AutoPilot to move to a location")]
        [ReturnHints("Error Unable to AutoPilot to location")]
        [ReturnHints("Accepted")]
        [ReturnHints("Convert to vector has failed")]
        [ReturnHints("?  value out of range 0-?")]
        [ArgHints("x", "URLARG", "X location to AutoPilot to")]
        [ArgHints("y", "URLARG", "y location to AutoPilot to")]
        [ArgHints("z", "URLARG", "z location to AutoPilot to")]
        [Route(HttpVerbs.Get, "/AutoPilot/{x}/{y}/{z}/{token}")]
        public object AutoPilot(string x, string y, string z, string token)
        {
            if (tokens.Allow(token, "movement", "AutoPilot", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (Vector3.TryParse("<" + x + "," + y + "," + z + ">", out Vector3 pos) == false)
            {
                return Failure("Convert to vector has failed");
            }
            if (helpers.inrange(pos.X, 0, 255) == false)
            {
                return Failure("x value out of range 0-255");
            }
            if (helpers.inrange(pos.Y, 0, 255) == false)
            {
                return Failure("y value out of range 0-255");
            }
            if (helpers.inrange(pos.Z, 0, 5000) == false)
            {
                return Failure("z value out of range 0-5000");
            }
            bot.GetClient.Self.AutoPilotCancel();
            bot.GetClient.Self.Movement.TurnToward(pos, true);
            Thread.Sleep(500);
            uint Globalx, Globaly;
            Utils.LongToUInts(bot.GetClient.Network.CurrentSim.Handle, out Globalx, out Globaly);
            bot.GetClient.Self.AutoPilot((ulong)(Globalx + pos.X), (ulong)(Globaly + pos.Y), pos.Z);
            return true;
        }

        [About("Attempt to teleport to a new region")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/AutoPilotStop/{token}")]
        public object AutoPilotStop(string token)
        {
            if (tokens.Allow(token, "movement", "AutoPilotStop", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            bot.GetClient.Self.AutoPilotCancel();
            return BasicReply("ok");
        }

        [About("Make the bot request the target avatar teleport to the bot")]
        [ReturnHints("ok")]
        [ReturnHints("Invaild avatar UUID")]
        [ArgHints("avatar", "URLARG", "Avatar UUID or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/SendTeleportLure/{avatar}/{token}")]
        public object SendTeleportLure(string avatar,string token)
        {
            if (tokens.Allow(token, "movement", "SendTeleportLure", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID");
            }
            bot.GetClient.Self.SendTeleportLure(avataruuid);
            return BasicReply("ok");
        }

        [About("Sends a teleport request (Move the bot to the avatar)")]
        [ReturnHints("ok")]
        [ReturnHints("Invaild avatar UUID")]
        [ArgHints("avatar", "URLARG", "Avatar UUID or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/RequestTeleport/{avatar}/{token}")]
        public object RequestTeleport(string avatar, string token)
        {
            if (tokens.Allow(token, "movement", "RequestTeleport", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID");
            }
            bot.Add_action_from("teleport", avataruuid);
            bot.GetClient.Self.SendTeleportLureRequest(avataruuid, "I would like to teleport to you");
            return BasicReply("ok");
        }

        [About("Makes the bot fly (or not)")]
        [ReturnHints("ok")]
        [ReturnHints("Invaild mode")]
        [ArgHints("mode", "URLARG", "true: Start flying, false: stop flying (super fun at height)")]
        [Route(HttpVerbs.Get, "/Fly/{mode}/{token}")]
        public object Fly(string mode, string token)
        {
            if (tokens.Allow(token, "movement", "Fly", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bool.TryParse(mode, out bool flymode) == false)
            {
                return Failure("Invaild mode");
            }
            bot.GetClient.Self.Movement.Fly = flymode;
            bot.GetClient.Self.Movement.SendUpdate();
            return BasicReply("ok");
        }



        [About("Rotates the bot to face a vector from its current location")]
        [ReturnHints("true|false")]
        [ReturnHints("Invaild vector")]
        [ReturnHints("Vector ? value is out of range 0-?")]
        [ArgHints("vector", "Text", "a vector to face eg <123,45,44>")]
        [Route(HttpVerbs.Post, "/RotateToFaceVector/{token}")]
        public object RotateToFaceVector([FormField] string vector, string token)
        {
            if (tokens.Allow(token, "movement", "RotateToFaceVector", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (Vector3.TryParse(vector, out Vector3 pos) == false)
            {
                return Failure("Invaild vector");
            }
            if (helpers.inrange(pos.X, 0, 255) == false)
            {
                return Failure("Vector x value is out of range 0-255");
            }
            if (helpers.inrange(pos.Y, 0, 255) == false)
            {
                return Failure("Vector y value is out of range 0-255");
            }
            if (helpers.inrange(pos.Z, 0, 5000) == false)
            {
                return Failure("Vector z value is out of range 0-5000");
            }
            return BasicReply(bot.GetClient.Self.Movement.TurnToward(pos, true).ToString());
        }

        [About("Rotates the bot to face a avatar")]
        [ReturnHints("true|false")]
        [ReturnHints("Invaild avatar UUID")]
        [ReturnHints("Unable to see avatar")]
        [ArgHints("avatar", "URLARG", "An avatar UUID or Firstname Lastname")]
        [Route(HttpVerbs.Post, "/RotateToFace/{avatar}/{token}")]
        public object RotateToFace(string avatar, string token)
        {
            if (tokens.Allow(token, "movement", "RotateToFace", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID");
            }
            if (bot.GetClient.Network.CurrentSim.AvatarPositions.ContainsKey(avataruuid) == false)
            {
                return Failure("Unable to see avatar");
            }
            return BasicReply(bot.GetClient.Self.Movement.TurnToward(bot.GetClient.Network.CurrentSim.AvatarPositions[avataruuid], true).ToString());
        }


        [About("Rotates the avatar to face a rotation from north in Degrees")]
        [ReturnHints("ok")]
        [ReturnHints("Unable to process rotation")]
        [ArgHints("deg", "URLARG", "0 to 360")]
        [Route(HttpVerbs.Post, "/RotateTo/{deg}/{token}")]
        public object RotateTo(string deg, string token)
        {
            if (tokens.Allow(token, "movement", "RotateTo", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (float.TryParse(deg, out float target_yaw) == true)
            {
                return Failure("Unable to process rotation");
            }
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
            return BasicReply("ok");
        }



        [About("Attempt to teleport to a new region")]
        [ReturnHints("Error Unable to Teleport to location")]
        [ReturnHints("Accepted")]
        [ArgHints("region", "URLARG", "the name of the region we are going to")]
        [ArgHints("x", "URLARG", "X location to teleport to")]
        [ArgHints("y", "URLARG", "y location to teleport to")]
        [ArgHints("z", "URLARG", "z location to teleport to")]
        [Route(HttpVerbs.Get, "/Teleport/{region}/{x}/{y}/{z}/{token}")]
        public object Teleport(string region, string x, string y, string z, string token)
        {
            if (tokens.Allow(token, "movement", "Teleport", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            bool status = TeleportRequest(new string[] { region, x, y, z });
            if (status == false)
            {
                return Failure("Error Unable to Teleport to location");
            }
            return BasicReply("Accepted");
        }



        protected bool TeleportRequest(string[] args)
        {
            bot.GetClient.Self.AutoPilotCancel();
            if (args[0].Contains("http://maps.secondlife.com/secondlife/") == true)
            {
                bot.TeleportWithSLurl(args[0]);
                return true;
            }
            else
            {
                float posX = 128;
                float posY = 128;
                float posZ = 0;
                string regionName = bot.GetClient.Network.CurrentSim.Name;
                bool ok = true;
                int offset = 0;
                string[] tp_args = args[0].Split('/');
                if ((tp_args.Length == 4) || (tp_args.Length == 1))
                {
                    regionName = tp_args[0];
                    offset = 1;
                }
                if (tp_args.Length >= 3)
                {
                    float.TryParse(tp_args[offset + 0], out posX);
                    float.TryParse(tp_args[offset + 1], out posY);
                    float.TryParse(tp_args[offset + 2], out posZ);
                }
                else if (tp_args.Length == 2)
                {
                    ok = false;
                }
                if (ok == true)
                {
                    bot.SetTeleported();
                    bool status = bot.GetClient.Self.Teleport(regionName, new Vector3(posX, posY, posZ));
                    bot.ResetAnimations();
                    return status;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
