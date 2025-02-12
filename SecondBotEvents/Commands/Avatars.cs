using System;
using System.Collections.Generic;
using OpenMetaverse;
using Newtonsoft.Json;
using SecondBotEvents.Services;
using System.Threading.Tasks;

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
        [ArgHints("store", "What type of request to accept: \"FriendRequest\", \"InventoryOffer\", \"GroupInvite\", \"Teleport\"")]
        [ArgHints("avatar", "a UUID or Firstname Lastname")]
        [ArgHints("remove", "set to \"true\" or \"True\" to remove that avatar from the accept next for that store type")]
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
        [ArgHints("store", "What type of request to check: \"FriendRequest\", \"InventoryOffer\", \"GroupInvite\", \"Teleport\"")]
        [ArgHints("avatar", "a UUID or Firstname Lastname")]
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
        public object NearmeWithDetails()
        {
            if (GetClient().Network.CurrentSim == null)
            {
                return BasicReply("Error not in a sim");
            }
            List<NearMeDetails> BetterNearMe = [];
            Dictionary<uint, Avatar> avcopy = GetClient().Network.CurrentSim.ObjectsAvatars.Copy();
            foreach (Avatar av in avcopy.Values)
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
            return BasicReply(JsonConvert.SerializeObject(BetterNearMe));
        }

        [About("Requests the given avatars profile image")]
        [ReturnHints("profile UUID")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHints("Requesting avatar details  [Retry later]")]
        [ArgHints("avatar", "a UUID or Firstname Lastname")]
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
        [ArgHints("texture", "a UUID of a texture to use")]
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
        [ArgHints("avatar", "a UUID or Firstname Lastname")]
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

        [About("Requests the bot updates its about me")]
        [ReturnHints("ok")]
        [ReturnHints("Requesting avatar details [Retry later]")]
        [ReturnHintsFailure("about text to short")]
        [ReturnHintsFailure("about text to long")]
        [ArgHints("abouttext", "text to use in the about me area, length 10 to 300")]
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
        public object Nearme()
        {
            if (GetClient().Network.CurrentSim == null)
            {
                return BasicReply("Error not in a sim");
            }
            Dictionary<UUID, string> NearMe = [];
            Dictionary<uint, Avatar> avcopy = GetClient().Network.CurrentSim.ObjectsAvatars.Copy();
            foreach (Avatar av in avcopy.Values)
            {
                if (av.ID != GetClient().Self.AgentID)
                {
                    NearMe.Add(av.ID, av.Name);
                }
            }
            return BasicReply(JsonConvert.SerializeObject(NearMe));
        }

        [About("searchs the AV database if not found triggers a lookup")]
        [ArgHints("uuid", "the UUID to find the name for")]
        [ReturnHints("the name of the avatar")]
        [ReturnHints("lookup")]
        [ReturnHintsFailure("Not a vaild UUID")]
        public object Key2Name(string uuid)
        {
            UUID avUUID = UUID.Zero;
            if (UUID.TryParse(uuid, out avUUID) == false)
            {
                return Failure("Not a vaild UUID");
            }
            return BasicReply(master.DataStoreService.GetAvatarName(avUUID));
        }

        [About("searchs the AV database if not found triggers a lookup")]
        [ReturnHints("the uuid of the avatar")]
        [ReturnHints("lookup")]
        [ReturnHintsFailure("Not a vaild name")]
        [ArgHints("name", "the name to find the uuid for")]
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
