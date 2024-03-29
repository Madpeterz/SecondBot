﻿using BetterSecondBot.bottypes;
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

namespace BetterSecondBot.HttpService
{
    public class HTTP_Friends : WebApiControllerWithTokens
    {
        public HTTP_Friends(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("Gets the friendslist <br/>Formated as follows<br/>friendreplyobject<br/><ul><li>name: String</li><li>id: String</li><li>online: bool</li></ul>")]
        [ReturnHints("array UUID = friendreplyobject")]
        [Route(HttpVerbs.Get, "/Friendslist/{token}")]
        public object Friendslist(string token)
        {
            if (tokens.Allow(token, "friends", "Friendslist", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "Friendslist");
            }
            return BasicReply(bot.getJsonFriendlist(), "Friendslist");
        }

        [About("Updates the friend perms for avatar avatar to State \n if true grants (Online/Map/Modify) perms")]
        [ArgHints("avatar", "URLARG", "A avatar uuid or Firstname Lastname")]
        [ArgHints("state", "URLARG", "true: Grant perms, false: Remove perms")]
        [ReturnHints("granted")]
        [ReturnHints("removed")]
        [ReturnHintsFailure("Not A friend")]
        [ReturnHintsFailure("state invaild")]
        [ReturnHintsFailure("avatar lookup")]
        [Route(HttpVerbs.Get, "/FriendFullPerms/{avatar}/{state}/{token}")]
        public object FriendFullPerms(string avatar, string state, string token)
        {
            if (tokens.Allow(token, "friends", "FriendFullPerms", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "FriendFullPerms", new [] { avatar , state });
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", "FriendFullPerms", new [] { avatar, state });
            }
            bool status = false;
            if (bool.TryParse(state, out status) == false)
            {
                return Failure("state invaild", "FriendFullPerms", new [] { avatar, state });
            }
            if (bot.GetClient.Friends.FriendList.ContainsKey(avataruuid) == false)
            {
                return Failure("Not A friend", "FriendFullPerms", new [] { avatar, state });
            }
            if (status == true)
            {
                bot.GetClient.Friends.GrantRights(avataruuid, FriendRights.CanSeeOnline | FriendRights.CanSeeOnMap | FriendRights.CanModifyObjects);
                return BasicReply("granted", "FriendFullPerms", new [] { avatar, state });
            }
            bot.GetClient.Friends.GrantRights(avataruuid, FriendRights.None);
            return BasicReply("removed", "FriendFullPerms", new [] { avatar, state });
        }

        [About("Updates the friend perms for avatar avatar to State \n if true grants (Online/Map/Modify) perms")]
        [ReturnHints("Request sent")]
        [ReturnHints("Removed")]
        [ReturnHintsFailure("Already a friend")]
        [ReturnHintsFailure("Not in friendslist")]
        [ReturnHintsFailure("state invaild")]
        [ReturnHintsFailure("avatar lookup")]
        [ArgHints("avatar", "URLARG", "A avatar uuid or Firstname Lastname")]
        [ArgHints("state", "URLARG", "true: Send invite, false: Remove from friendslist")]
        [Route(HttpVerbs.Get, "/FriendRequest/{avatar}/{state}/{token}")]
        public object FriendRequest(string avatar, string state, string token)
        {
            if (tokens.Allow(token, "friends", "FriendRequest", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted", "FriendRequest", new [] { avatar, state });
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", "FriendRequest", new [] { avatar, state });
            }
            bool status = false;
            if (bool.TryParse(state, out status) == false)
            {
                return Failure("state invaild", "FriendRequest", new [] { avatar, state });
            }
            if (bot.GetClient.Friends.FriendList.ContainsKey(avataruuid) == true)
            {
                if (status == false)
                {
                    bot.GetClient.Friends.TerminateFriendship(avataruuid);
                    return BasicReply("Removed", "FriendRequest", new [] { avatar, state });
                }
            }
            if (status == false)
            {
                return Failure("Not in friendslist", "FriendRequest", new [] { avatar, state });
            }
            bot.GetClient.Friends.OfferFriendship(avataruuid);
            return BasicReply("Request sent", "FriendRequest", new [] { avatar, state });
            
        }
    }


}
