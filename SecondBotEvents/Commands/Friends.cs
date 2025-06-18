using OpenMetaverse;
using SecondBotEvents.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace SecondBotEvents.Commands
{
    [ClassInfo("The power of friendship")]
    public class Friends(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Gets the location of a friend if map access was given \n also requests a update")]
        [ReturnHints("a json object of global region X,Y  sim X,Y,Z and time it was updated")]
        [ReturnHintsFailure("avatar lookup")]
        [ReturnHintsFailure("not in friends list")]
        [ReturnHintsFailure("no map access or updating")]
        [ArgHints("avatar", "Who to get the location of","AVATAR")]
        [CmdTypeGet()]
        public object FriendsGetLocation(string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", [avatar]);
            }
            Dictionary<UUID, FriendInfo> FriendListCopy = GetClient().Friends.FriendList.ToDictionary(k => k.Key, v => v.Value);
            {
                return Failure("not in friends list (updating)", [avatar]);
            }
            GetClient().Friends.MapFriend(avataruuid);
            FriendLocations loc = master.DataStoreService.GetFriendMap(avataruuid);
            if(loc == null)
            {
                return Failure("no map access or updating", [avatar]);
            }
            return BasicReply(JsonConvert.SerializeObject(loc), [avatar]);
        }

        [About("Gets the friendslist")]
        [ReturnHints("array of FriendListEntry")]
        [CmdTypeGet()]
        public object Friendslist()
        {
            Dictionary<UUID, FriendInfo> FriendListCopy = GetClient().Friends.FriendList.ToDictionary(k => k.Key, v => v.Value);
            List< FriendListEntry > CleanedFriendsList = [];
            int index = 0;
            foreach (FriendInfo A in FriendListCopy.Values)
            {
                FriendListEntry entry = new()
                {
                    id = A.UUID.ToString(),
                    name = A.Name,
                    online = A.IsOnline
                };
                CleanedFriendsList.Add(entry);
                index++;
            }
            return BasicReply(JsonConvert.SerializeObject(CleanedFriendsList));
        }

        [About("Updates the friend perms for avatar avatar to State \n if true grants (Online/Map/Modify) perms")]
        [ArgHints("avatar", "Who we are talking about","AVATAR")]
        [ArgHints("state", "true: Grant perms, false: Remove perms","BOOL")]
        [ReturnHints("granted")]
        [ReturnHints("removed")]
        [ReturnHintsFailure("Not A friend")]
        [ReturnHintsFailure("state invaild")]
        [ReturnHintsFailure("avatar lookup")]
        [CmdTypeSet()]
        public object FriendFullPerms(string avatar, string state)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", [avatar, state]);
            }
            if (bool.TryParse(state, out bool status) == false)
            {
                return Failure("state invaild", [avatar, state]);
            }
            if (GetClient().Friends.FriendList.ContainsKey(avataruuid) == false)
            {
                return Failure("Not A friend", [avatar, state]);
            }
            if (status == true)
            {
                GetClient().Friends.GrantRights(avataruuid, FriendRights.CanSeeOnline | FriendRights.CanSeeOnMap | FriendRights.CanModifyObjects);
                return BasicReply("granted", [avatar, state]);
            }
            GetClient().Friends.GrantRights(avataruuid, FriendRights.None);
            return BasicReply("removed", [avatar, state]);
        }

        [About("Updates the friend perms for avatar avatar to State \n if true grants (Online/Map/Modify) perms")]
        [ReturnHints("Request sent")]
        [ReturnHints("Removed")]
        [ReturnHintsFailure("Already a friend")]
        [ReturnHintsFailure("Not in friendslist")]
        [ReturnHintsFailure("state invaild")]
        [ReturnHintsFailure("avatar lookup")]
        [ArgHints("avatar", "Who to request/remove from friends list","AVATAR")]
        [ArgHints("state", "true: Send invite, false: Remove from friendslist","BOOL")]
        [CmdTypeDo()]
        public object FriendRequest(string avatar, string state)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("avatar lookup", [avatar, state]);
            }
            if (bool.TryParse(state, out bool status) == false)
            {
                return Failure("state invaild", [avatar, state]);
            }
            if (GetClient().Friends.FriendList.ContainsKey(avataruuid) == true)
            {
                if (status == false)
                {
                    GetClient().Friends.TerminateFriendship(avataruuid);
                    return BasicReply("Removed", [avatar, state]);
                }
            }
            if (status == false)
            {
                return Failure("Not in friendslist", [avatar, state]);
            }
            GetClient().Friends.OfferFriendship(avataruuid);
            return BasicReply("Request sent", [avatar, state]);
            
        }
    }

    class FriendListEntry
    {
        public string name;
        public string id;
        public bool online = false;
    }
}
