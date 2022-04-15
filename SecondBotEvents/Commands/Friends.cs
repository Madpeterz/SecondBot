using EmbedIO;
using EmbedIO.Routing;
using OpenMetaverse;
using SecondBotEvents.Services;
using Newtonsoft.Json;


namespace SecondBotEvents.Commands
{
    public class Friends : CommandsAPI
    {
        public Friends(EventsSecondBot setmaster) : base(setmaster)
        {
        }
        [About("Gets the friendslist")]
        [ReturnHints("array UUID = friendreplyobject")]
        [Route(HttpVerbs.Get, "/Friendslist/{token}")]
        public object Friendslist(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            return BasicReply(JsonConvert.SerializeObject(getClient().Friends.FriendList));
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
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
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
            if (getClient().Friends.FriendList.ContainsKey(avataruuid) == false)
            {
                return Failure("Not A friend", "FriendFullPerms", new [] { avatar, state });
            }
            if (status == true)
            {
                getClient().Friends.GrantRights(avataruuid, FriendRights.CanSeeOnline | FriendRights.CanSeeOnMap | FriendRights.CanModifyObjects);
                return BasicReply("granted", "FriendFullPerms", new [] { avatar, state });
            }
            getClient().Friends.GrantRights(avataruuid, FriendRights.None);
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
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
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
            if (getClient().Friends.FriendList.ContainsKey(avataruuid) == true)
            {
                if (status == false)
                {
                    getClient().Friends.TerminateFriendship(avataruuid);
                    return BasicReply("Removed", "FriendRequest", new [] { avatar, state });
                }
            }
            if (status == false)
            {
                return Failure("Not in friendslist", "FriendRequest", new [] { avatar, state });
            }
            getClient().Friends.OfferFriendship(avataruuid);
            return BasicReply("Request sent", "FriendRequest", new [] { avatar, state });
            
        }
    }


}
