using OpenMetaverse;
using SecondBotEvents.Services;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace SecondBotEvents.Commands
{
    [ClassInfo("The power of friendship")]
    public class Friends : CommandsAPI
    {
        public Friends(EventsSecondBot setmaster) : base(setmaster)
        {
        }

        [About("Gets the location of a friend if map access was given \n also requests a update")]
        [ReturnHints("a json object of global region X,Y  sim X,Y,Z and time it was updated")]
        [ReturnHintsFailure("avatar lookup")]
        [ReturnHintsFailure("not in friends list")]
        [ReturnHintsFailure("no map access or updating")]
        [ArgHints("avatar", "A avatar uuid or Firstname Lastname")]
        public object FriendsGetLocation(string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", new[] { avatar });
            }
            Dictionary<UUID, FriendInfo> FriendListCopy = GetClient().Friends.FriendList.Copy();
            if(FriendListCopy.ContainsKey(avataruuid) == false)
            {
                return Failure("not in friends list (updating)", new[] { avatar });
            }
            GetClient().Friends.MapFriend(avataruuid);
            FriendLocations loc = master.DataStoreService.GetFriendMap(avataruuid);
            if(loc == null)
            {
                return Failure("no map access or updating", new[] { avatar });
            }
            return BasicReply(JsonConvert.SerializeObject(loc), new[] { avatar });
        }

        [About("Gets the friendslist")]
        [ReturnHints("array of FriendListEntry")]
        public object Friendslist()
        {
            Dictionary<UUID, FriendInfo> FriendListCopy = GetClient().Friends.FriendList.Copy();
            List< FriendListEntry > CleanedFriendsList = new List< FriendListEntry >();
            int index = 0;
            foreach (FriendInfo A in FriendListCopy.Values)
            {
                FriendListEntry entry = new FriendListEntry();
                entry.id = A.UUID.ToString();
                entry.name = A.Name;
                entry.online = A.IsOnline;
                CleanedFriendsList.Add(entry);
                index++;
            }
            return BasicReply(JsonConvert.SerializeObject(CleanedFriendsList));
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
                return Failure("avatar lookup", new [] { avatar, state });
            }
            bool status = false;
            if (bool.TryParse(state, out status) == false)
            {
                return Failure("state invaild", new [] { avatar, state });
            }
            if (GetClient().Friends.FriendList.ContainsKey(avataruuid) == false)
            {
                return Failure("Not A friend", new [] { avatar, state });
            }
            if (status == true)
            {
                GetClient().Friends.GrantRights(avataruuid, FriendRights.CanSeeOnline | FriendRights.CanSeeOnMap | FriendRights.CanModifyObjects);
                return BasicReply("granted", new [] { avatar, state });
            }
            GetClient().Friends.GrantRights(avataruuid, FriendRights.None);
            return BasicReply("removed", new [] { avatar, state });
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
                return Failure("avatar lookup", new [] { avatar, state });
            }
            bool status = false;
            if (bool.TryParse(state, out status) == false)
            {
                return Failure("state invaild", new [] { avatar, state });
            }
            if (GetClient().Friends.FriendList.ContainsKey(avataruuid) == true)
            {
                if (status == false)
                {
                    GetClient().Friends.TerminateFriendship(avataruuid);
                    return BasicReply("Removed", new [] { avatar, state });
                }
            }
            if (status == false)
            {
                return Failure("Not in friendslist", new [] { avatar, state });
            }
            GetClient().Friends.OfferFriendship(avataruuid);
            return BasicReply("Request sent", new [] { avatar, state });
            
        }
    }

    class FriendListEntry
    {
        public string name;
        public string id;
        public bool online = false;
    }
}
