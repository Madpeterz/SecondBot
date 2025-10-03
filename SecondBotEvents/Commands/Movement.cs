using OggVorbisEncoder.Setup.Templates;
using OpenMetaverse;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Dont like this place lets try somewhere else")]
    public class Movement(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Adjusts Hover height")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Convert to float has failed")]
        [ReturnHintsFailure("value out of range (-2.0, 2.0)")]
        [ArgHints("height", "what height to hover at from (-2.0, 2.0)","Number","0.5")]
        [CmdTypeSet()]
        public object HoverHeight(string height)
        {
            if (double.TryParse(height, out double level) == false)
            {
                return Failure("Convert to float has failed", [height]);
            }
            if (SecondbotHelpers.InRange(level, -2.0, 2.0) == false)
            {
                return Failure("height value out of range(-2.0 to 2.0)", [height]);
            }
            GetClient().Self.SetHoverHeight(level);
            return BasicReply("ok", [height]);
        }

        [About("uses the AutoPilot to move to a location")]
        [ReturnHints("Error Unable to AutoPilot to location")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Convert to vector has failed")]
        [ReturnHintsFailure("?  value out of range 0-?")]
        [ArgHints("x", "X location to AutoPilot to","Number","44")]
        [ArgHints("y", "Y location to AutoPilot to","Number","78")]
        [ArgHints("z", "Z location to AutoPilot to","Number","26")]
        [CmdTypeDo()]
        public object AutoPilot(string x, string y, string z)
        {
            if (Vector3.TryParse("<" + x + "," + y + "," + z + ">", out Vector3 pos) == false)
            {
                return Failure("Convert to vector has failed", [x, y, z]);
            }
            if (SecondbotHelpers.InRange(pos.X, 0, 255) == false)
            {
                return Failure("x value out of range 0-255", [x, y, z]);
            }
            if (SecondbotHelpers.InRange(pos.Y, 0, 255) == false)
            {
                return Failure("y value out of range 0-255", [x, y, z]);
            }
            if (SecondbotHelpers.InRange(pos.Z, 0, 5000) == false)
            {
                return Failure("z value out of range 0-5000", [x, y, z]);
            }
            GetClient().Self.AutoPilotCancel();
            GetClient().Self.Movement.TurnToward(pos, true);
            Thread.Sleep(500);
            Utils.LongToUInts(GetClient().Network.CurrentSim.Handle, out uint Globalx, out uint Globaly);
            GetClient().Self.AutoPilot((ulong)(Globalx + pos.X), (ulong)(Globaly + pos.Y), pos.Z);
            return BasicReply("ok", [x, y, z]);
        }

        [About("Stop the auto pilot system from walking")]
        [ReturnHints("ok")]
        [CmdTypeDo()]
        public object AutoPilotStop()
        {
            GetClient().Self.AutoPilotCancel();
            return BasicReply("ok");
        }

        [About("Make the bot request the target avatar teleport to the bot")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild avatar UUID")]
        [ArgHints("avatar", "Who to send the join me via teleport to","AVATAR")]
        [CmdTypeDo()]
        public object SendTeleportLure(string avatar)
        {
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID", [avatar]);
            }
            GetClient().Self.SendTeleportLure(avataruuid);
            return BasicReply("ok", [avatar]);
        }

        [About("Sends a teleport request (Move the bot to the avatar)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild avatar UUID")]
        [ArgHints("avatar", "Who to request a teleport from","AVATAR")]
        [CmdTypeDo()]
        public object RequestTeleport(string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID", [avatar]);
            }
            // @todo action from event log with accept
            GetClient().Self.SendTeleportLureRequest(avataruuid, "I would like to teleport to you");
            return BasicReply("ok", [avatar]);
        }

        [About("Makes the bot fly (or not)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild mode")]
        [ArgHints("mode", "true: Start flying, false: stop flying (super fun at height)","BOOL")]
        [CmdTypeSet()]
        public object Fly(string mode)
        {
            if (bool.TryParse(mode, out bool flymode) == false)
            {
                return Failure("Invaild mode", [mode]);
            }
            GetClient().Self.Movement.Fly = flymode;
            GetClient().Self.Movement.SendUpdate();
            return BasicReply("ok", [mode]);
        }

        [About("Rotates the bot to face a vector from its current location")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Invaild vector")]
        [ReturnHintsFailure("Vector ? value is out of range 0-?")]
        [ArgHints("vector", "What direction to face","Vector", "<123,45,44>")]
        [CmdTypeSet()]
        public object RotateToFaceVector(string vector)
        {
            if (Vector3.TryParse(vector, out Vector3 pos) == false)
            {
                return Failure("Invaild vector", [vector]);
            }
            if (SecondbotHelpers.InRange(pos.X, 0, 255) == false)
            {
                return Failure("Vector x value is out of range 0-255", [vector]);
            }
            if (SecondbotHelpers.InRange(pos.Y, 0, 255) == false)
            {
                return Failure("Vector y value is out of range 0-255", [vector]);
            }
            if (SecondbotHelpers.InRange(pos.Z, 0, 5000) == false)
            {
                return Failure("Vector z value is out of range 0-5000", [vector]);
            }
            return BasicReply(GetClient().Self.Movement.TurnToward(pos, true).ToString(), [vector]);
        }

        [About("Rotates the bot to face a avatar")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Invaild avatar UUID")]
        [ReturnHintsFailure("Unable to see avatar")]
        [ArgHints("avatar", "Who to turn to look at","AVATAR")]
        [CmdTypeSet()]
        public object RotateToFace(string avatar)
        {
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar UUID", [avatar]);
            }
            if (GetClient().Network.CurrentSim.AvatarPositions.ContainsKey(avataruuid) == false)
            {
                return Failure("Unable to see avatar", [avatar]);
            }
            return BasicReply(GetClient().Self.Movement.TurnToward(GetClient().Network.CurrentSim.AvatarPositions[avataruuid], true).ToString(), [avatar]);
        }

        [About("Rotates the avatar to face a rotation from north in Degrees")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Unable to process rotation")]
        [ArgHints("deg", "0 to 360","Number","180")]
        [CmdTypeSet()]
        public object RotateTo(string deg)
        {
            if (float.TryParse(deg, out float target_yaw) == true)
            {
                return Failure("Unable to process rotation", [deg]);
            }
            float yaw = target_yaw;
            GetClient().Self.Movement.BodyRotation.GetEulerAngles(out float roll, out float pitch, out _);
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
            GetClient().Self.Movement.BodyRotation = result;
            GetClient().Self.Movement.SendUpdate();
            return BasicReply("ok", [deg]);
        }



        [About("Attempt to teleport to a new region")]
        [ReturnHintsFailure("Error Unable to Teleport to location")]
        [ReturnHints("Accepted")]
        [ArgHints("region", "the name of the region we are going to")]
        [ArgHints("x", "X location to teleport to","Number","55")]
        [ArgHints("y", "y location to teleport to","Number","78")]
        [ArgHints("z", "z location to teleport to","Number","244")]
        [CmdTypeDo()]
        public object Teleport(string region, string x, string y, string z)
        {
            bool status = TeleportRequest(region+"/"+x+"/"+y+"/"+z);
            if (status == false)
            {
                return Failure("Error Unable to Teleport to location", [region, x, y, z]);
            }
            return BasicReply("Accepted", [region, x, y, z]);
        }


        [About("Attempt to teleport to a new region via a SL url")]
        [ReturnHintsFailure("slurl is empty")]
        [ReturnHints("True|False")]
        [ArgHints("slurl", "a full SLurl","Text", "secondlife://Example%20Land/115/130/24")]
        [CmdTypeDo()]
        public object TeleportSLURL(string slurl)
        {
            if (SecondbotHelpers.NotEmpty(slurl) == false)
            {
                return Failure("slurl is empty", [slurl]);
            }
            return BasicReply(TeleportRequest(slurl).ToString(), [slurl]);
        }

        protected bool TeleportRequest(string url)
        {
            // Viserion/66/166/23
            GetClient().Self.AutoPilotCancel();
            SimSlURL A = new(url);
            if(A.name == null)
            {
                return false;
            }
            if(GetClient().Self.Teleport(A.name, new Vector3(A.x, A.y, A.z)) == true)
            {
                master.HomeboundService.MarkTeleport();
                return true;
            }
            return false;
        }
    }
}
