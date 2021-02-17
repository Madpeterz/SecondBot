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
using BetterSecondBotShared.Static;

namespace BetterSecondBot.HttpService
{
    public class Http_Self : WebApiControllerWithTokens
    {
        public Http_Self(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Makes the bot turn to face avatar and point at them (if found)")]
        [ReturnHints("ok")]
        [ReturnHints("Cant find UUID in sim")]
        [Route(HttpVerbs.Get, "/PointAt/{avatar}/{token}")]
        public object PointAt(string avatar, string token)
        {
            if (tokens.Allow(token, "self", "PointAt", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if (bot.GetClient.Network.CurrentSim.AvatarPositions.ContainsKey(avataruuid) == false)
            {
                return Failure("Cant find UUID in sim");
            }
            bot.ResetAnimations();
            bot.GetClient.Self.Stand();
            bot.GetClient.Self.Movement.TurnToward(bot.GetClient.Network.CurrentSim.AvatarPositions[avataruuid]);
            bot.GetClient.Self.Movement.SendUpdate();
            bot.GetClient.Self.AnimationStart(Animations.POINT_YOU, true);
            bot.GetClient.Self.PointAtEffect(bot.GetClient.Self.AgentID, avataruuid, new Vector3d(0, 0, 0), PointAtType.Select, UUID.Random());
            bot.GetClient.Self.BeamEffect(bot.GetClient.Self.AgentID, avataruuid, new Vector3d(0, 0, 2), new Color4(255, 255, 255, 1), (float)3.0, UUID.Random());
            return BasicReply("ok");
        }

        [About("Makes the bot sit on the ground or on a object if it can see it")]
        [ReturnHints("ok")]
        [ReturnHints("Invaild object UUID")]
        [ArgHints("target", "URLARG", "ground or a object UUID")]
        [Route(HttpVerbs.Get, "/Sit/{target}/{token}")]
        public object Sit(string target, string token)
        {
            if (tokens.Allow(token, "self", "Sit", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if(target == "ground")
            {
                bot.GetClient.Self.SitOnGround();
                return BasicReply("ok");
            }
            if(UUID.TryParse(target,out UUID objectuuid) == false)
            {
                return Failure("Invaild object UUID");
            }
            bot.GetClient.Self.RequestSit(objectuuid, Vector3.Zero);
            return BasicReply("ok");
        }

        [About("Makes the bot stand up if sitting (also resets animations)")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/Stand/{token}")]
        public object Stand(string target, string token)
        {
            if (tokens.Allow(token, "self", "Stand", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            bot.GetClient.Self.Stand();
            bot.ResetAnimations();
            return BasicReply("ok");
        }

        [About("Makes the bot sit on the ground or on a object if it can see it")]
        [ReturnHints("true|false")]
        [ReturnHints("Invaild object UUID")]
        [ReturnHints("Unable to see object")]
        [ArgHints("target", "URLARG", "object UUID")]
        [Route(HttpVerbs.Get, "/ClickObject/{object}/{token}")]
        public object ClickObject(string target, string token)
        {
            if (tokens.Allow(token, "self", "ClickObject", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(target, out UUID objectuuid) == false)
            {
                return Failure("Invaild object UUID");
            }
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
            return BasicReply(found_object.ToString());
        }

        [About("Makes the bot sit on the ground or on a object if it can see it")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/Logoff/{token}")]
        public object Logoff(string token)
        {
            if (tokens.Allow(token, "self", "Logoff", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (bot.KillMe == false)
            {
                bot.GetClient.Self.Chat("Laters im out", 0, ChatType.Normal);
                bot.KillMePlease();
            }
            return BasicReply("ok");
        }


    }
}
