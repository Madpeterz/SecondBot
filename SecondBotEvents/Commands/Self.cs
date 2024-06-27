using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Services;
using System.Collections.Generic;
using System.Linq;

namespace SecondBotEvents.Commands
{
    [ClassInfo("The more we know about our self the better we are")]
    public class Self : CommandsAPI
    {
        public Self(EventsSecondBot setmaster) : base(setmaster)
        {
        }

        [About("Makes the bot teleport to its home region")]
        [ReturnHints("ok")]
        public object GoHome()
        {
            master.HomeboundService.GoHome();
            return BasicReply("ok");
        }

        [About("Makes the bot turn to face a avatar and point at it (if found)")]
        [ReturnHints("ok")]
        [ArgHints("targetUUID", "UUID of a avatar to point at")]
        [ReturnHintsFailure("Cant find UUID in sim")]
        public object PointAt(string targetUUID)
        {
            if (GetClient().Network.CurrentSim.AvatarPositions.ContainsKey(avataruuid) == false)
            {
                return Failure("Cant find UUID in sim", new [] { targetUUID });
            }
            ProcessAvatar(targetUUID);
            GetClient().Self.Stand();
            GetClient().Self.Movement.TurnToward(GetClient().Network.CurrentSim.AvatarPositions[avataruuid]);
            GetClient().Self.Movement.SendUpdate();
            GetClient().Self.AnimationStart(Animations.POINT_YOU, true);
            GetClient().Self.PointAtEffect(GetClient().Self.AgentID, avataruuid, new Vector3d(0, 0, 0), PointAtType.Select, UUID.Random());
            GetClient().Self.BeamEffect(GetClient().Self.AgentID, avataruuid, new Vector3d(0, 0, 2), new Color4(255, 255, 255, 1), (float)3.0, UUID.Random());
            return BasicReply("ok", new [] { targetUUID });
        }

        [About("Reads a value from the KeyValue storage (temp unless SQL is enabled)")]
        [ReturnHints("value")]
        [ReturnHintsFailure("Unknown Key: KeyName")]
        [ArgHints("Key", "the key we are trying to read from")]
        public object ReadKeyValue(string Key)
        {
            return BasicReply(master.DataStoreService.GetKeyValue(Key));
        }

        [About("sets a value for KeyValue storage (temp unless SQL is enabled)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Key is empty")]
        [ReturnHintsFailure("Value is empty")]
        [ArgHints("Key", "the key we are trying to set")]
        [ArgHints("Value", "the value we are tring to put on the key")]
        public object SetKeyValue(string Key, string Value)
        {
            if (SecondbotHelpers.isempty(Key) == false)
            {
                return Failure("Key is empty", new[] { Key, Value });
            }
            if (SecondbotHelpers.isempty(Value) == false)
            {
                return Failure("Value is empty", new[] { Key, Value });
            }
            master.DataStoreService.SetKeyValue(Key, Value);
            return BasicReply("ok");
        }

        [About("Reads a value from the KeyValue storage (temp unless SQL is enabled)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Key is empty")]
        [ArgHints("Key", "the key we are trying to clear")]
        public object ClearKeyValue(string Key)
        {
            if (SecondbotHelpers.isempty(Key) == false)
            {
                return Failure("Key is empty", new[] { Key });
            }
            master.DataStoreService.ClearKeyValue(Key);
            return BasicReply("ok");
        }

        [About("Makes the bot sit on the ground or on a object if it can see it")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild object UUID")]
        [ArgHints("target", "a object UUID anything or nothing will also work")]
        public object Sit(string target)
        {
            if ((target == "ground") || (target == UUID.Zero.ToString()))
            {
                GetClient().Self.SitOnGround();
                return BasicReply("ok", new [] { target });
            }
            if(UUID.TryParse(target,out UUID objectuuid) == false)
            {
                return Failure("Invaild object uuid and not ground");
            }
            GetClient().Self.RequestSit(objectuuid, Vector3.Zero);
            return BasicReply("ok", new [] { target });
        }

        [About("Makes the bot stand up if sitting (also resets animations)")]
        [ReturnHints("ok")]
        public object Stand()
        {
            master.HomeboundService.MarkStandup();
            GetClient().Self.Stand();
            return BasicReply("ok");
        }

        [About("Makes the bot sit on the ground or on a object if it can see it")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Invaild object UUID")]
        [ReturnHintsFailure("Unable to see object")]
        [ArgHints("target", "object UUID")]
        public object ClickObject(string target)
        {
            if (UUID.TryParse(target, out UUID objectuuid) == false)
            {
                return Failure("Invaild object UUID", new [] { target });
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
            return BasicReply(found_object.ToString(), new [] { target });
        }

        [About("Makes the bot kill itself you monster")]
        [ReturnHints("ok")]
        public object Logoff()
        {
            GetClient().Network.BeginLogout();
            return BasicReply("ok");
        }

        [About("Gets the last 5 commands issued to the bot")]
        [ReturnHints("list of commands")]
        public object GetLastCommands()
        {
            List<string> reply = new List<string>();
            foreach(CommandHistory A in master.DataStoreService.GetCommandHistory())
            {
                reply.Add(JsonConvert.SerializeObject(A));
                if(reply.Count >= 5)
                {
                    break;
                }
            }
            return BasicReply("ok", reply.ToArray<string>());
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
        [ArgHints("avatar", "avatar uuid or Firstname Lastname")]
        [ArgHints("flag", "friend, group, animation, teleport, inventory or command")]
        [ArgHints("state", "State to set the flag to true or false")]
        [ArgHints("sticky", "if true the permissing will not expire after the first use otherwise false")]
        public object SetPermFlag(string avatar, string flag, string state, string sticky)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", new [] { avatar, flag, state, sticky });
            }
            bool stateflag = false;
            bool stickyflag = false;
            if(bool.TryParse(state,out stateflag) == false)
            {
                return Failure("Invaild state", new [] { avatar, flag, state, sticky });
            }
            if (bool.TryParse(sticky, out stickyflag) == false)
            {
                return Failure("Invaild sticky", new [] { avatar, flag, state, sticky });
            }
            string[] AcceptedFlags = new [] { "friend", "group", "animation", "teleport", "command", "inventory" };
            if (AcceptedFlags.Contains(flag) == false)
            {
                return Failure("Invaild flag", new [] { avatar, flag, state, sticky });
            }
            if (stateflag == true)
            {
                // @todo add stick/normal accept dynamic actions
                return BasicReply("Added perm: " + flag + " Sticky: " + stickyflag.ToString(), new [] { avatar, flag, state, sticky });
            }
            // @todo remove stick/normal accept dynamic actions
            return BasicReply("Removed perm: " + flag + " Sticky: " + stickyflag.ToString(), new [] { avatar, flag, state, sticky });
        }



    }
}
