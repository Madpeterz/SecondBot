using BetterSecondBot.bottypes;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using OpenMetaverse;
using System.Linq;
using BetterSecondBotShared.Static;

namespace BetterSecondBot.HttpService
{
    public class HTTP_Self : WebApiControllerWithTokens
    {
        public HTTP_Self(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Makes the bot teleport to its home region")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/GoHome/{token}")]
        public object GoHome(string token)
        {
            if (tokens.Allow(token, "self", "GoHome", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GoHome");
            }
            bot.GotoNextHomeRegion();
            return BasicReply("ok", "GoHome");
        }

        [About("Makes the bot turn to face avatar and point at them (if found)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Cant find UUID in sim")]
        [Route(HttpVerbs.Get, "/PointAt/{avatar}/{token}")]
        public object PointAt(string avatar, string token)
        {
            if (tokens.Allow(token, "self", "PointAt", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "PointAt", new [] { avatar });
            }
            ProcessAvatar(avatar);
            if (bot.GetClient.Network.CurrentSim.AvatarPositions.ContainsKey(avataruuid) == false)
            {
                return Failure("Cant find UUID in sim", "PointAt", new [] { avatar });
            }
            bot.ResetAnimations();
            bot.GetClient.Self.Stand();
            bot.GetClient.Self.Movement.TurnToward(bot.GetClient.Network.CurrentSim.AvatarPositions[avataruuid]);
            bot.GetClient.Self.Movement.SendUpdate();
            bot.GetClient.Self.AnimationStart(Animations.POINT_YOU, true);
            bot.GetClient.Self.PointAtEffect(bot.GetClient.Self.AgentID, avataruuid, new Vector3d(0, 0, 0), PointAtType.Select, UUID.Random());
            bot.GetClient.Self.BeamEffect(bot.GetClient.Self.AgentID, avataruuid, new Vector3d(0, 0, 2), new Color4(255, 255, 255, 1), (float)3.0, UUID.Random());
            return BasicReply("ok", "PointAt", new [] { avatar });
        }

        [About("Reads a value from the KeyValue storage (temp unless SQL is enabled)")]
        [ReturnHints("value")]
        [ReturnHintsFailure("Unknown Key: KeyName")]
        [ArgHints("Key", "URLARG", "the key we are trying to read from")]
        [Route(HttpVerbs.Get, "/ReadKeyValue/{Key}/{token}")]
        public object ReadKeyValue(string Key, string token)
        {
            if (tokens.Allow(token, "self", "ReadKeyValue", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "ReadKeyValue", new[] { Key });
            }
            if(bot.HaveLocalKeyStorage(Key) == true)
            {
                return BasicReply(bot.ReadLocalKeyStorage(Key), "ReadKeyValue");
            }
            return Failure("Unknown Key: "+Key, "ReadKeyValue");
        }

        [About("sets a value for KeyValue storage (temp unless SQL is enabled)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Key is empty")]
        [ReturnHintsFailure("Value is empty")]
        [ArgHints("Key", "URLARG", "the key we are trying to set")]
        [ArgHints("Value", "string", "the value we are tring to put on the key")]
        [Route(HttpVerbs.Post, "/SetKeyValue/{Key}/{token}")]
        public object SetKeyValue(string Key, [FormField] string Value, string token)
        {
            if (tokens.Allow(token, "self", "SetKeyValue", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "SetKeyValue", new[] { Key, Value });
            }
            if (helpers.isempty(Key) == false)
            {
                return Failure("Key is empty", "SetKeyValue", new[] { Key, Value });
            }
            if (helpers.isempty(Value) == false)
            {
                return Failure("Value is empty", "SetKeyValue", new[] { Key, Value });
            }
            bot.setLocalKeyStorage(Key, Value);
            return BasicReply("ok", "SetKeyValue");
        }

        [About("Reads a value from the KeyValue storage (temp unless SQL is enabled)")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Key is empty")]
        [ArgHints("Key", "URLARG", "the key we are trying to clear")]
        [Route(HttpVerbs.Get, "/ClearKeyValue/{Key}/{token}")]
        public object ClearKeyValue(string Key, string token)
        {
            if (tokens.Allow(token, "self", "ClearKeyValue", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "ClearKeyValue", new[] { Key });
            }
            if (helpers.isempty(Key) == false)
            {
                return Failure("Key is empty", "SetKeyValue", new[] { Key });
            }
            bot.clearLocalKeyStorage(Key);
            return BasicReply("ok", "SetKeyValue");
        }

        [About("Makes the bot sit on the ground or on a object if it can see it")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild object UUID")]
        [ArgHints("target", "URLARG", "ground or a object UUID")]
        [Route(HttpVerbs.Get, "/Sit/{target}/{token}")]
        public object Sit(string target, string token)
        {
            if (tokens.Allow(token, "self", "Sit", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Sit", new [] { target });
            }
            if(target == "ground")
            {
                bot.GetClient.Self.SitOnGround();
                return BasicReply("ok", "Sit", new [] { target });
            }
            if(UUID.TryParse(target,out UUID objectuuid) == false)
            {
                return Failure("Invaild object UUID", "Sit", new [] { target });
            }
            bot.GetClient.Self.RequestSit(objectuuid, Vector3.Zero);
            return BasicReply("ok", "Sit", new [] { target });
        }

        [About("Makes the bot stand up if sitting (also resets animations)")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/Stand/{token}")]
        public object Stand(string token)
        {
            if (tokens.Allow(token, "self", "Stand", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Stand");
            }
            bot.GetClient.Self.Stand();
            bot.ResetAnimations();
            return BasicReply("ok", "Stand");
        }

        [About("Makes the bot sit on the ground or on a object if it can see it")]
        [ReturnHints("true|false")]
        [ReturnHintsFailure("Invaild object UUID")]
        [ReturnHintsFailure("Unable to see object")]
        [ArgHints("target", "URLARG", "object UUID")]
        [Route(HttpVerbs.Get, "/ClickObject/{object}/{token}")]
        public object ClickObject(string target, string token)
        {
            if (tokens.Allow(token, "self", "ClickObject", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "ClickObject", new [] { target });
            }
            if (UUID.TryParse(target, out UUID objectuuid) == false)
            {
                return Failure("Invaild object UUID", "ClickObject", new [] { target });
            }

            bot.GetClient.Self.PointAtEffect(bot.GetClient.Self.AgentID, objectuuid, new Vector3d(0, 0, 0), PointAtType.Select, new UUID("1df9eb92-62fa-15e5-4bfb-5931f1525274"));

            Dictionary<uint, Primitive> objectsentrys = bot.GetClient.Network.CurrentSim.ObjectsPrimitives.Copy();

            bool found_object = false;
            foreach (KeyValuePair<uint, Primitive> entry in objectsentrys)
            {
                if (entry.Value.ID == objectuuid)
                {
                    bot.GetClient.Objects.ClickObject(bot.GetClient.Network.CurrentSim, entry.Key);
                    found_object = true;
                    break;
                }
            }
            return BasicReply(found_object.ToString(), "ClickObject", new [] { target });
        }

        [About("Makes the bot kill itself you monster")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/Logoff/{token}")]
        public object Logoff(string token)
        {
            if (tokens.Allow(token, "self", "Logoff", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Logoff");
            }
            if (bot.KillMe == false)
            {
                bot.GetClient.Self.Chat("Laters im out", 0, ChatType.Normal);
                bot.KillMePlease();
            }
            return BasicReply("ok", "Logoff");
        }

        [About("Makes the bot kill itself you monster")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/Logout/{token}")]
        public object Logout(string token)
        {
            if (tokens.Allow(token, "self", "Logout", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Logout");
            }
            if (bot.KillMe == false)
            {
                bot.GetClient.Self.Chat("Laters im out", 0, ChatType.Normal);
                bot.KillMePlease();
            }
            return BasicReply("ok", "Logout");
        }



        [About("Makes the bot kill itself you monster - without making a sound")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/Bye/{token}")]
        public object Bye(string token)
        {
            if (tokens.Allow(token, "self", "Bye", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Bye");
            }
            if (bot.KillMe == false)
            {
                bot.KillMePlease();
            }
            return BasicReply("ok", "Bye");
        }

        [About("Gets the last 5 commands issued to the bot")]
        [ReturnHints("list of commands")]
        [Route(HttpVerbs.Get, "/GetLastCommands/{token}")]
        public object GetLastCommands(string token)
        {
            if (tokens.Allow(token, "self", "GetLastCommands", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "GetLastCommands");
            }
            SuccessNoReturn("GetLastCommands");
            return bot.GetLastCommands(5);
        }

        [About("Gets the last 5 commands issued to the bot")]
        [ReturnHintsFailure("avatar lookup")]
        [ReturnHintsFailure("Invaild state")]
        [ReturnHintsFailure("Invaild sticky")]
        [ReturnHintsFailure("Invaild flag")]
        [ArgHints("avatar", "URLARG", "avatar uuid or Firstname Lastname")]
        [ArgHints("flag", "URLARG", "friend, group, animation, teleport or command")]
        [ArgHints("state", "URLARG", "State to set the flag to true or false")]
        [ArgHints("sticky", "URLARG", "if true the permissing will not expire after the first use otherwise false")]
        [Route(HttpVerbs.Get, "/SetPermFlag/{avatar}/{flag}/{state}/{sticky}/{token}")]
        public object SetPermFlag(string avatar, string flag, string state, string sticky, string token)
        {
            if (tokens.Allow(token, "self", "SetPermFlag", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "SetPermFlag", new [] { avatar, flag, state, sticky });
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", "SetPermFlag", new [] { avatar, flag, state, sticky });
            }
            bool stateflag = false;
            bool stickyflag = false;
            if(bool.TryParse(state,out stateflag) == false)
            {
                return Failure("Invaild state", "SetPermFlag", new [] { avatar, flag, state, sticky });
            }
            if (bool.TryParse(sticky, out stickyflag) == false)
            {
                return Failure("Invaild sticky", "SetPermFlag", new [] { avatar, flag, state, sticky });
            }
            string[] AcceptedFlags = new [] { "friend", "group", "animation", "teleport", "command" };
            if (AcceptedFlags.Contains(flag) == false)
            {
                return Failure("Invaild flag", "SetPermFlag", new [] { avatar, flag, state, sticky });
            }
            if (stateflag == true)
            {
                bot.Add_action_from(flag, avataruuid, bot.FindAvatarKey2Name(avataruuid), stickyflag);
                return BasicReply("Added perm: " + flag + " Sticky: " + stickyflag.ToString(), "SetPermFlag", new [] { avatar, flag, state, sticky });
            }
            bot.Remove_action_from(flag, avataruuid, stickyflag);
            return BasicReply("Removed perm: " + flag + " Sticky: " + stickyflag.ToString(), "SetPermFlag", new [] { avatar, flag, state, sticky });
        }



    }
}
