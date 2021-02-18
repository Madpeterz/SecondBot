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
                return Failure("Token not accepted");
            }
            return BasicReply(bot.getJsonFriendlist());
        }

        [About("Updates the friend perms for avatar avatar to State \n if true grants (Online/Map/Modify) perms")]
        [ArgHints("avatar", "URLARG", "A avatar uuid or Firstname Lastname")]
        [ArgHints("state", "URLARG", "true: Grant perms, false: Remove perms")]
        [ReturnHints("granted")]
        [ReturnHints("removed")]
        [ReturnHints("Not A friend")]
        [ReturnHints("state invaild")]
        [ReturnHints("avatar lookup")]
        [Route(HttpVerbs.Get, "/FriendFullPerms/{avatar}/{state}/{token}")]
        public object FriendFullPerms(string avatar, string state, string token)
        {
            if (tokens.Allow(token, "friends", "FriendFullPerms", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup");
            }
            bool status = false;
            if (bool.TryParse(state, out status) == false)
            {
                return Failure("state invaild");
            }
            if (bot.GetClient.Friends.FriendList.ContainsKey(avataruuid) == false)
            {
                return Failure("Not A friend");
            }
            if (status == true)
            {
                bot.GetClient.Friends.GrantRights(avataruuid, FriendRights.CanSeeOnline | FriendRights.CanSeeOnMap | FriendRights.CanModifyObjects);
                return BasicReply("granted");
            }
            bot.GetClient.Friends.GrantRights(avataruuid, FriendRights.None);
            return BasicReply("removed");
        }

        [About("Updates the friend perms for avatar avatar to State \n if true grants (Online/Map/Modify) perms")]
        [ReturnHints("Request sent")]
        [ReturnHints("Removed")]
        [ReturnHints("Already a friend")]
        [ReturnHints("Not in friendslist")]
        [ReturnHints("state invaild")]
        [ReturnHints("avatar lookup")]
        [ArgHints("avatar", "URLARG", "A avatar uuid or Firstname Lastname")]
        [ArgHints("state", "URLARG", "true: Send invite, false: Remove from friendslist")]
        [Route(HttpVerbs.Get, "/FriendRequest/{avatar}/{state}/{token}")]
        public object FriendRequest(string avatar, string state, string token)
        {
            if (tokens.Allow(token, "friends", "FriendRequest", handleGetClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup");
            }
            bool status = false;
            if (bool.TryParse(state, out status) == false)
            {
                return Failure("state invaild");
            }
            if (bot.GetClient.Friends.FriendList.ContainsKey(avataruuid) == true)
            {
                if (status == false)
                {
                    bot.GetClient.Friends.TerminateFriendship(avataruuid);
                    return BasicReply("Removed");
                }
            }
            if (status == false)
            {
                return Failure("Not in friendslist");
            }
            bot.GetClient.Friends.OfferFriendship(avataruuid);
            return BasicReply("Request sent");
            
        }
    }


}
