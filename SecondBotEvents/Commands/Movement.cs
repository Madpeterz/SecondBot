using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using OpenMetaverse;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SecondBotEvents.Commands
{
    public class Movement : CommandsAPI
    {
        public Movement(EventsSecondBot setmaster) : base(setmaster)
        {
        }


        [About("uses the AutoPilot to move to a location")]
        [ReturnHints("Error Unable to AutoPilot to location")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Convert to vector has failed")]
        [ReturnHintsFailure("?  value out of range 0-?")]
        [ArgHints("x", "URLARG", "X location to AutoPilot to")]
        [ArgHints("y", "URLARG", "y location to AutoPilot to")]
        [ArgHints("z", "URLARG", "z location to AutoPilot to")]
        [Route(HttpVerbs.Get, "/AutoPilot/{x}/{y}/{z}/{token}")]
        public object AutoPilot(string x, string y, string z, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (Vector3.TryParse("<" + x + "," + y + "," + z + ">", out Vector3 pos) == false)
            {
                return Failure("Convert to vector has failed", "AutoPilot", new [] { x, y, z });
            }
            if (SecondbotHelpers.inrange(pos.X, 0, 255) == false)
            {
                return Failure("x value out of range 0-255", "AutoPilot", new [] { x, y, z });
            }
            if (SecondbotHelpers.inrange(pos.Y, 0, 255) == false)
            {
                return Failure("y value out of range 0-255", "AutoPilot", new [] { x, y, z });
            }
            if (SecondbotHelpers.inrange(pos.Z, 0, 5000) == false)
            {
                return Failure("z value out of range 0-5000", "AutoPilot", new [] { x, y, z });
            }
            getClient().Self.AutoPilotCancel();
            getClient().Self.Movement.TurnToward(pos, true);
            Thread.Sleep(500);
            uint Globalx, Globaly;
            Utils.LongToUInts(getClient().Network.CurrentSim.Handle, out Globalx, out Globaly);
            getClient().Self.AutoPilot((ulong)(Globalx + pos.X), (ulong)(Globaly + pos.Y), pos.Z);
            return BasicReply("ok", "AutoPilot", new [] { x, y, z });
        }

        [About("Attempt to teleport to a new region")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/AutoPilotStop/{token}")]
        public object AutoPilotStop(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            getClient().Self.AutoPilotCancel();
            return BasicReply("ok", "AutoPilotStop");
        }

        [About("Make the bot request the target avatar teleport to the bot")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild avatar UUID")]
        [ArgHints("avatar", "URLARG", "Avatar UUID or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/SendTeleportLure/{avatar}/{token}")]
        public object SendTeleportLure(string avatar,string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID", "SendTeleportLure", new [] { avatar });
            }
            getClient().Self.SendTeleportLure(avataruuid);
            return BasicReply("ok", "SendTeleportLure", new [] { avatar });
        }

        [About("Sends a teleport request (Move the bot to the avatar)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild avatar UUID")]
        [ArgHints("avatar", "URLARG", "Avatar UUID or Firstname Lastname")]
        [Route(HttpVerbs.Get, "/RequestTeleport/{avatar}/{token}")]
        public object RequestTeleport(string avatar, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID", "RequestTeleport", new [] { avatar });
            }
            bot.Add_action_from("teleport", avataruuid, bot.FindAvatarKey2Name(avataruuid));
            getClient().Self.SendTeleportLureRequest(avataruuid, "I would like to teleport to you");
            return BasicReply("ok", "RequestTeleport", new [] { avatar });
        }

        [About("Makes the bot fly (or not)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild mode")]
        [ArgHints("mode", "URLARG", "true: Start flying, false: stop flying (super fun at height)")]
        [Route(HttpVerbs.Get, "/Fly/{mode}/{token}")]
        public object Fly(string mode, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (bool.TryParse(mode, out bool flymode) == false)
            {
                return Failure("Invaild mode", "Fly", new [] { mode });
            }
            getClient().Self.Movement.Fly = flymode;
            getClient().Self.Movement.SendUpdate();
            return BasicReply("ok", "Fly", new [] { mode });
        }



        [About("Rotates the bot to face a vector from its current location")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Invaild vector")]
        [ReturnHintsFailure("Vector ? value is out of range 0-?")]
        [ArgHints("vector", "Text", "a vector to face eg <123,45,44>")]
        [Route(HttpVerbs.Post, "/RotateToFaceVector/{token}")]
        public object RotateToFaceVector([FormField] string vector, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (Vector3.TryParse(vector, out Vector3 pos) == false)
            {
                return Failure("Invaild vector", "RotateToFaceVector", new [] { vector });
            }
            if (SecondbotHelpers.inrange(pos.X, 0, 255) == false)
            {
                return Failure("Vector x value is out of range 0-255", "RotateToFaceVector", new [] { vector });
            }
            if (SecondbotHelpers.inrange(pos.Y, 0, 255) == false)
            {
                return Failure("Vector y value is out of range 0-255", "RotateToFaceVector", new [] { vector });
            }
            if (SecondbotHelpers.inrange(pos.Z, 0, 5000) == false)
            {
                return Failure("Vector z value is out of range 0-5000", "RotateToFaceVector", new [] { vector });
            }
            return BasicReply(getClient().Self.Movement.TurnToward(pos, true).ToString(), "RotateToFaceVector", new [] { vector });
        }

        [About("Rotates the bot to face a avatar")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Invaild avatar UUID")]
        [ReturnHintsFailure("Unable to see avatar")]
        [ArgHints("avatar", "URLARG", "An avatar UUID or Firstname Lastname")]
        [Route(HttpVerbs.Post, "/RotateToFace/{avatar}/{token}")]
        public object RotateToFace(string avatar, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID", "RotateToFace", new [] { avatar });
            }
            if (getClient().Network.CurrentSim.AvatarPositions.ContainsKey(avataruuid) == false)
            {
                return Failure("Unable to see avatar", "RotateToFace", new [] { avatar });
            }
            return BasicReply(getClient().Self.Movement.TurnToward(getClient().Network.CurrentSim.AvatarPositions[avataruuid], true).ToString(), "RotateToFace", new [] { avatar });
        }


        [About("Rotates the avatar to face a rotation from north in Degrees")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Unable to process rotation")]
        [ArgHints("deg", "URLARG", "0 to 360")]
        [Route(HttpVerbs.Post, "/RotateTo/{deg}/{token}")]
        public object RotateTo(string deg, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (float.TryParse(deg, out float target_yaw) == true)
            {
                return Failure("Unable to process rotation", "RotateTo", new [] { deg });
            }
            float yaw = target_yaw;
            getClient().Self.Movement.BodyRotation.GetEulerAngles(out float roll, out float pitch, out _);
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
            getClient().Self.Movement.BodyRotation = result;
            getClient().Self.Movement.SendUpdate();
            return BasicReply("ok", "RotateTo", new [] { deg });
        }



        [About("Attempt to teleport to a new region")]
        [ReturnHintsFailure("Error Unable to Teleport to location")]
        [ReturnHints("Accepted")]
        [ArgHints("region", "URLARG", "the name of the region we are going to")]
        [ArgHints("x", "URLARG", "X location to teleport to")]
        [ArgHints("y", "URLARG", "y location to teleport to")]
        [ArgHints("z", "URLARG", "z location to teleport to")]
        [Route(HttpVerbs.Get, "/Teleport/{region}/{x}/{y}/{z}/{token}")]
        public object Teleport(string region, string x, string y, string z, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            bool status = TeleportRequest(new [] { region, x, y, z });
            if (status == false)
            {
                return Failure("Error Unable to Teleport to location", "Teleport", new [] { region, x, y, z });
            }
            bot.SetTeleported();
            return BasicReply("Accepted", "Teleport", new [] { region, x, y, z });
        }


        [About("Attempt to teleport to a new region via a SL url")]
        [ReturnHintsFailure("slurl is empty")]
        [ReturnHints("True|False")]
        [ArgHints("slurl", "Text", "a full SLurl")]
        [Route(HttpVerbs.Post, "/TeleportSLURL/{token}")]
        public object TeleportSLURL([FormField] string slurl, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (SecondbotHelpers.notempty(slurl) == false)
            {
                return Failure("slurl is empty", "TeleportSLURL", new [] { slurl });
            }
            bot.SetTeleported();
            return BasicReply(TeleportRequest(new [] { slurl }).ToString(), "TeleportSLURL", new [] { slurl });
        }

        protected bool TeleportRequest(string[] args)
        {
            getClient().Self.AutoPilotCancel();
            if (args[0].Contains("http://maps.secondlife.com/secondlife/") == true)
            {
                if (bot.TeleportWithSLurl(args[0]) == "ok")
                {
                    return true;
                }
                return false;
            }
            else
            {
                List<string> argvalues = new List<string>(args);
                if(argvalues.Count == 3)
                {
                    string regionName = getClient().Network.CurrentSim.Name;
                    argvalues = new List<string>() { regionName };
                    argvalues.AddRange(args);
                }
                if (argvalues.Count == 4)
                {
                    string regionName = argvalues[0];
                    float.TryParse(argvalues[1], out float posX);
                    float.TryParse(argvalues[2], out float posY);
                    float.TryParse(argvalues[3], out float posZ);
                    bot.SetTeleported();
                    bool status = getClient().Self.Teleport(regionName, new Vector3(posX, posY, posZ));
                    bot.ResetAnimations();
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
