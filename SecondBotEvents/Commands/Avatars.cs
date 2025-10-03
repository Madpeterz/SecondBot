using System;
using System.Collections.Generic;
using OpenMetaverse;
using System.Text.Json;
using SecondBotEvents.Services;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Avatars near me and Key2Name and Name2Key lookups")]
    public class Avatars(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
        [About("Adds avatars to the one time accept list for interactions")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHintsFailure("Unknown store type")]
        [ReturnHintsFailure("Unable to process request store")]
        [ArgHints("store", "What type of request to accept", "Text", "GroupInvite", ["FriendRequest", "InventoryOffer", "GroupInvite", "Teleport"])]
        [ArgHints("avatar", "Who to accept the request from", "AVATAR")]
        [ArgHints("remove", "Should this remove the entry", "BOOL")]
        [CmdTypeDo()]
        public Object AcceptNext(string store, string avatar, string remove)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [store, avatar, remove]);
            }
            bool toberemoved = false;
            if((remove == "true") || (remove == "True"))
            {
                toberemoved = true;
            }
            if(master.DataStoreService.KnownStoreType(store) == false)
            {
                return Failure("Unknown store type", [store, avatar, remove]);
            }
            if (master.DataStoreService.AddToAcceptNext(store, avataruuid, toberemoved) == false)
            {
                return Failure("Unable to process request store", [store, avatar, remove]);
            }
            return BasicReply("ok", [store, avatar, remove]);
        }
        [About("Checks if the avatar is on the accept next list (without using the token) for the given store")]
        [ReturnHints("true")]
        [ReturnHints("false")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHintsFailure("Unknown store type")]
        [ArgHints("store", "What type of request to accept", "Text", "GroupInvite", ["FriendRequest", "InventoryOffer", "GroupInvite", "Teleport"])]
        [ArgHints("avatar", "Who to accept the request from", "AVATAR")]
        [CmdTypeDo()]
        public Object IsOnAcceptNext(string store, string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [store, avatar]);
            }
            if (master.DataStoreService.KnownStoreType(store) == false)
            {
                return Failure("Unknown store type", [store, avatar]);
            }
            if(master.DataStoreService.IsInAcceptNext(store,avataruuid) == false)
            {
                return BasicReply("false", [store, avatar]);
            }
            return BasicReply("true", [store, avatar]);
        }


        [About("an improved version of near me with extra details<br/>NearMeDetails is a object formated as follows<br/><ul><li>id</li><li>name</li><li>x</li><li>y</li><li>z</li><li>range</li></ul>")]
        [ReturnHints("array NearMeDetails")]
        [ReturnHintsFailure("Error not in a sim")]
        [CmdTypeGet()]
        public object NearmeWithDetails()
        {
            if (GetClient().Network.CurrentSim == null)
            {
                return BasicReply("Error not in a sim");
            }
            List<NearMeDetails> BetterNearMe = [];
            List<Avatar> avcopy = [.. GetClient().Network.CurrentSim.ObjectsAvatars.Values];
            foreach (Avatar av in avcopy)
            {
                if (av.ID != GetClient().Self.AgentID)
                {
                    NearMeDetails details = new()
                    {
                        id = av.ID.ToString(),
                        name = av.Name,
                        x = (int)Math.Round(av.Position.X),
                        y = (int)Math.Round(av.Position.Y),
                        z = (int)Math.Round(av.Position.Z),
                        range = (int)Math.Round(Vector3.Distance(av.Position, GetClient().Self.SimPosition))
                    };
                    BetterNearMe.Add(details);
                }
            }
            return BasicReply(JsonSerializer.Serialize(BetterNearMe));
        }

        [About("Requests the given avatars profile image")]
        [ReturnHints("profile UUID")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHints("Requesting avatar details  [Retry later]")]
        [ArgHints("avatar", "Who are we getting the profile image of", "AVATAR")]
        [CmdTypeGet()]
        public object GetProfileImage(string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [avatar]);
            }
            KeyValuePair<bool, Avatar.AvatarProperties> reply = master.DataStoreService.GetAvatarAvatarProperties(avataruuid);
            if(reply.Key == false)
            {
                return Failure("Requesting avatar details  [Retry later]", [avatar]);
            }
            return BasicReply(reply.Value.ProfileImage.ToString());
        }

        [About("Requests the bot updates its profile image")]
        [ReturnHints("ok")]
        [ReturnHints("Requesting avatar details [Retry later]")]
        [ReturnHintsFailure("Invaild texture uuid")]
        [ArgHints("texture", "What texture to use", "UUID")]
        [CmdTypeSet()]
        public object SetProfileImage(string texture)
        {
            if (UUID.TryParse(texture, out UUID textureUUID) == false)
            {
                return Failure("Invaild texture uuid\"", [texture]);
            }
            KeyValuePair<bool, Avatar.AvatarProperties> reply = master.DataStoreService.GetAvatarAvatarProperties(GetClient().Self.AgentID);
            if (reply.Key == false)
            {
                return Failure("Requesting avatar details  [Retry later]", [texture]);
            }
            Avatar.AvatarProperties props = reply.Value;
            props.ProfileImage = textureUUID;
            GetClient().Self.UpdateProfile(props);
            return BasicReply("ok");
        }

        [About("Requests the given avatars profile about me")]
        [ReturnHints("profile UUID")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHints("Requesting avatar details  [Retry later]")]
        [ArgHints("avatar", "Who are we getting the profile about of", "AVATAR")]
        [CmdTypeGet()]
        public object GetProfileAbout(string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [avatar]);
            }
            KeyValuePair<bool, Avatar.AvatarProperties> reply = master.DataStoreService.GetAvatarAvatarProperties(avataruuid);
            if (reply.Key == false)
            {
                return Failure("Requesting avatar details  [Retry later]", [avatar]);
            }
            return BasicReply(reply.Value.AboutText.ToString());
        }

        [About("Requests the given avatars display name")]
        [ReturnHints("display name if set or ?")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHints("Requesting avatar details  [Retry later]")]
        [ArgHints("avatar", "Who are we getting the profile about of", "AVATAR")]
        [CmdTypeGet()]
        public object GetDisplayName(string avatar)
        {
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", [avatar]);
            }
            return BasicReply(master.DataStoreService.GetDisplayName(avataruuid));
        }

        [About("Requests the bot updates its about me")]
        [ReturnHints("ok")]
        [ReturnHints("Requesting avatar details [Retry later]")]
        [ReturnHintsFailure("about text to short")]
        [ReturnHintsFailure("about text to long")]
        [ArgHints("abouttext", "text to use in the about me area, length 10 to 300", "Text", "Hello world!")]
        [CmdTypeSet()]
        public object SetProfileAbout(string abouttext)
        {
            if(abouttext.Length < 10)
            {
                return Failure("About text to short", [abouttext]);
            }
            else if (abouttext.Length > 300)
            {
                return Failure("About text to long", [abouttext]);
            }
            KeyValuePair<bool, Avatar.AvatarProperties> reply = master.DataStoreService.GetAvatarAvatarProperties(GetClient().Self.AgentID);
            if (reply.Key == false)
            {
                return Failure("Requesting avatar details  [Retry later]", [abouttext]);
            }
            Avatar.AvatarProperties props = reply.Value;
            props.AboutText = abouttext;
            GetClient().Self.UpdateProfile(props);
            return BasicReply("ok");
        }



        [About("returns a list of all known avatars nearby")]
        [ReturnHints("array UUID = Name")]
        [ReturnHintsFailure("Error not in a sim")]
        [CmdTypeGet()]
        public object Nearme()
        {
            if (GetClient().Network.CurrentSim == null)
            {
                return BasicReply("Error not in a sim");
            }
            Dictionary<UUID, string> NearMe = [];
            List<Avatar> avcopy = [.. GetClient().Network.CurrentSim.ObjectsAvatars.Values];
            foreach (Avatar av in avcopy)
            {
                if (av.ID != GetClient().Self.AgentID)
                {
                    NearMe.Add(av.ID, av.Name);
                }
            }
            return BasicReply(JsonSerializer.Serialize(NearMe));
        }

        [About("searchs the AV database if not found triggers a lookup")]
        [ArgHints("uuid", "What uuid are we getting the name for", "UUID")]
        [ReturnHints("the name of the avatar")]
        [ReturnHints("lookup")]
        [ReturnHintsFailure("Not a vaild UUID")]
        [CmdTypeDo()]
        public object Key2Name(string uuid)
        {
            if (UUID.TryParse(uuid, out _) == false)
            {
                return Failure("Not a vaild UUID");
            }
            return BasicReply(master.DataStoreService.GetAvatarName(UUID.Zero));
        }

        [About("searchs the AV database if not found triggers a lookup")]
        [ReturnHints("the uuid of the avatar")]
        [ReturnHints("lookup")]
        [ReturnHintsFailure("Not a vaild name")]
        [ArgHints("name", "the name to find the uuid for", "Text", "Madpeter Zond")]
        [CmdTypeDo()]
        public object Name2Key(string name)
        {
            if(name == null)
            {
                return Failure("Not a vaild name");
            }
            return BasicReply(master.DataStoreService.GetAvatarUUID(name));
        }
    }
}
