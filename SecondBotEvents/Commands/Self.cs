using Newtonsoft.Json;
using OpenMetaverse;
using OpenMetaverse.ImportExport.Collada14;
using SecondBotEvents.Services;
using System.Collections.Generic;
using System.Linq;
using static NRedisStack.Search.Schema;

namespace SecondBotEvents.Commands
{
    [ClassInfo("The more we know about our self the better we are")]
    public class Self(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("use the SMTP service to send a email (requires AllowCommandSendMail to be true)")]
        [ReturnHints("sending")]
        [ReturnHintsFailure("Send was rejected because: X")]
        [ReturnHintsFailure("ARG is empty")]
        [ArgHints("to", "the email addres to send to", "Text", "me@email.com")]
        [ArgHints("subject", "the subject of the email", "Text", "This is the subject")]
        [ArgHints("body", "the contents of the email", "Text", "Email is fun")]
        [CmdTypeDo()]
        public object SendEmail(string to, string subject, string body)
        {
            if (SecondbotHelpers.isempty(to) == true) {
                return Failure("to is empty");
            }
            else if (SecondbotHelpers.isempty(subject) == true)
            {
                return Failure("subject is empty", [to]);
            }
            else if (SecondbotHelpers.isempty(body) == true)
            {
                return Failure("body is empty", [to, subject]);
            }
            KeyValuePair<bool, string> reply = master.SmtpService.commandEmail(to, subject, body);
            if (reply.Key == false)
            {
                return Failure("Send was rejected because: " + reply.Value, [to, subject, body]);
            }
            return BasicReply("sending", [to, subject, body]);
        }

        [About("Makes the bot teleport to its home region")]
        [ReturnHints("ok")]
        [CmdTypeDo()]
        public object GoHome()
        {
            master.HomeboundService.GoHome();
            return BasicReply("ok");
        }

        [About("Makes the bot turn to face a avatar and point at it (if found)")]
        [ReturnHints("ok")]
        [ArgHints("targetUUID", "The avatar to point at", "AVATAR")]
        [ReturnHintsFailure("Cant find UUID in sim")]
        [CmdTypeDo()]
        public object PointAt(string targetUUID)
        {
            if (GetClient().Network.CurrentSim.AvatarPositions.ContainsKey(avataruuid) == false)
            {
                return Failure("Cant find UUID in sim", [targetUUID]);
            }
            ProcessAvatar(targetUUID);
            GetClient().Self.Stand();
            GetClient().Self.Movement.TurnToward(GetClient().Network.CurrentSim.AvatarPositions[avataruuid]);
            GetClient().Self.Movement.SendUpdate();
            GetClient().Self.AnimationStart(Animations.POINT_YOU, true);
            GetClient().Self.PointAtEffect(GetClient().Self.AgentID, avataruuid, new Vector3d(0, 0, 0), PointAtType.Select, UUID.Random());
            GetClient().Self.BeamEffect(GetClient().Self.AgentID, avataruuid, new Vector3d(0, 0, 2), new Color4(255, 255, 255, 1), (float)3.0, UUID.Random());
            return BasicReply("ok", [targetUUID]);
        }

        [About("Reads a value from the KeyValue storage (temp unless SQL is enabled)")]
        [ReturnHints("value")]
        [ReturnHintsFailure("Unknown Key: KeyName")]
        [ArgHints("Key", "the key we are trying to read from", "Text", "ExampleKey")]
        [CmdTypeGet()]
        public object ReadKeyValue(string Key)
        {
            return BasicReply(master.DataStoreService.GetKeyValue(Key));
        }

        [About("sets a value for KeyValue storage (temp unless SQL is enabled)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Key is empty")]
        [ReturnHintsFailure("Value is empty")]
        [ArgHints("Key", "the key we are trying to set", "Text", "ExampleKey")]
        [ArgHints("Value", "the value we are tring to put on the key", "Text", "ExampleValue")]
        [CmdTypeSet()]
        public object SetKeyValue(string Key, string Value)
        {
            if (SecondbotHelpers.isempty(Key) == false)
            {
                return Failure("Key is empty", [Key, Value]);
            }
            if (SecondbotHelpers.isempty(Value) == false)
            {
                return Failure("Value is empty", [Key, Value]);
            }
            master.DataStoreService.SetKeyValue(Key, Value);
            return BasicReply("ok");
        }

        [About("Reads a value from the KeyValue storage (temp unless SQL is enabled)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Key is empty")]
        [ArgHints("Key", "the key we are trying to clear", "Text", "ExampleKey")]
        [CmdTypeSet()]
        public object ClearKeyValue(string Key)
        {
            if (SecondbotHelpers.isempty(Key) == false)
            {
                return Failure("Key is empty", [Key]);
            }
            master.DataStoreService.ClearKeyValue(Key);
            return BasicReply("ok");
        }

        [About("Makes the bot sit on the ground or on a object if it can see it")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild object UUID")]
        [ArgHints("target", "a object UUID or the word ground", "Text", "ground")]
        [CmdTypeDo()]
        public object Sit(string target)
        {
            if ((target == "ground") || (target == UUID.Zero.ToString()))
            {
                GetClient().Self.SitOnGround();
                return BasicReply("ok", [target]);
            }
            if (UUID.TryParse(target, out UUID objectuuid) == false)
            {
                return Failure("Invaild object uuid and not ground");
            }
            GetClient().Self.RequestSit(objectuuid, Vector3.Zero);
            return BasicReply("ok", [target]);
        }

        [About("Makes the bot stand up if sitting (also resets animations)")]
        [ReturnHints("ok")]
        [CmdTypeDo()]
        public object Stand()
        {
            master.HomeboundService.MarkStandup();
            GetClient().Self.Stand();
            master.CommandsService.CommandInterfaceCaller("ResetAnimations", false, false, "Stand command");
            return BasicReply("ok");
        }

        [About("Makes the bot sit on the ground or on a object if it can see it")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Invaild object UUID")]
        [ReturnHintsFailure("Unable to see object")]
        [ArgHints("target", "the object to click on", "UUID")]
        [CmdTypeDo()]
        public object ClickObject(string target)
        {
            if (UUID.TryParse(target, out UUID objectuuid) == false)
            {
                return Failure("Invaild object UUID", [target]);
            }

            GetClient().Self.PointAtEffect(GetClient().Self.AgentID, objectuuid, new Vector3d(0, 0, 0), PointAtType.Select, new UUID("1df9eb92-62fa-15e5-4bfb-5931f1525274"));

            Dictionary<uint, Primitive> objectsentrys = GetClient().Network.CurrentSim.ObjectsPrimitives.Copy();

            bool found_object = false;
            foreach (KeyValuePair<uint, Primitive> entry in objectsentrys)
            {
                if (entry.Value.ID == objectuuid)
                {
                    GetClient().Objects.ClickObject(GetClient().Network.CurrentSim, entry.Key);
                    found_object = true;
                    break;
                }
            }
            return BasicReply(found_object.ToString(), [target]);
        }

        [About("Makes the bot kill itself you monster")]
        [ReturnHints("ok")]
        [CmdTypeDo()]
        public object Logoff()
        {
            master.BotClient.flagLogoutExpected();
            GetClient().Network.BeginLogout();
            return BasicReply("ok");
        }

        [About("Gets the last 5 commands issued to the bot")]
        [ReturnHints("list of commands")]
        [CmdTypeGet()]
        public object GetLastCommands()
        {
            List<string> reply = [];
            foreach (CommandHistory A in master.DataStoreService.GetCommandHistory())
            {
                reply.Add(JsonConvert.SerializeObject(A));
                if (reply.Count >= 5)
                {
                    break;
                }
            }
            return BasicReply("ok", [.. reply]);
        }

        [About("Sets the bot to accept a request type from the avatar (or a object owned by the avatar)\n " +
            "friend: friend request \n " +
            "group: group invite \n " +
            "animation: trigger animation request [from a object]\n" +
            "teleport: teleport lure\n" +
            "inventory: Inventory transfer\n" +
            "command: A non signed command")]
        [ReturnHintsFailure("avatar lookup")]
        [ReturnHintsFailure("Invaild state")]
        [ReturnHintsFailure("Invaild sticky")]
        [ReturnHintsFailure("Invaild flag")]
        [ArgHints("avatar", "Who to asign the flag to", "AVATAR")]
        [ArgHints("flag", "What flag to assign", "Text", "group", 
            new string[] { "friend", "group", "animation", "teleport", "inventory","command" })]
        [ArgHints("state", "What state to put the flag into","BOOL")]
        [ArgHints("sticky", "Should this be repeatable","BOOL")]
        [CmdTypeSet()]
        public object SetPermFlag(string avatar, string flag, string state, string sticky)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", [avatar, flag, state, sticky]);
            }
            if (bool.TryParse(state, out bool stateflag) == false)
            {
                return Failure("Invaild state", [avatar, flag, state, sticky]);
            }
            if (bool.TryParse(sticky, out bool stickyflag) == false)
            {
                return Failure("Invaild sticky", [avatar, flag, state, sticky]);
            }
            string[] AcceptedFlags = ["friend", "group", "animation", "teleport", "command", "inventory"];
            if (AcceptedFlags.Contains(flag) == false)
            {
                return Failure("Invaild flag", [avatar, flag, state, sticky]);
            }
            if (stateflag == true)
            {
                // @todo add stick/normal accept dynamic actions
                return BasicReply("Added perm: " + flag + " Sticky: " + stickyflag.ToString(), [avatar, flag, state, sticky]);
            }
            // @todo remove stick/normal accept dynamic actions
            return BasicReply("Removed perm: " + flag + " Sticky: " + stickyflag.ToString(), [avatar, flag, state, sticky]);
        }



    }
}
