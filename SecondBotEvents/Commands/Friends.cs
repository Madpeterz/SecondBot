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
        public object Friendslist()
        {
            return BasicReply(JsonConvert.SerializeObject(getClient().Friends.FriendList));
        }

        [About("Updates the friend perms for avatar avatar to State \n if true grants (Online/Map/Modify) perms")]
        [ArgHints("avatar", "A avatar uuid or Firstname Lastname")]
        [ArgHints("state", "true: Grant perms, false: Remove perms")]
        [ReturnHints("granted")]
        [ReturnHints("removed")]
        [ReturnHintsFailure("Not A friend")]
        [ReturnHintsFailure("state invaild")]
        [ReturnHintsFailure("avatar lookup")]
        public object FriendFullPerms(string avatar, string state)
        {
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
        [ArgHints("avatar", "A avatar uuid or Firstname Lastname")]
        [ArgHints("state", "true: Send invite, false: Remove from friendslist")]
        public object FriendRequest(string avatar, string state)
        {
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
