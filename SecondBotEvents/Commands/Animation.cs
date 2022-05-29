using EmbedIO;
using EmbedIO.Routing;
using OpenMetaverse;
using SecondBotEvents.Services;

namespace SecondBotEvents.Commands
{
    internal class AnimationCommands : CommandsAPI
    {
        public AnimationCommands(EventsSecondBot setmaster) : base(setmaster)
        {
        }

        [About("Toggles if animation requests from this avatar (used for remote poseballs) are accepted")]
        [ReturnHints("Granted perm animation")]
        [ReturnHints("Removed perm animation")]
        [ReturnHintsFailure("avatar lookup")]
        [ArgHints("avatar", "URLARG", "UUID (or Firstname Lastname)")]
        [Route(HttpVerbs.Get, "/AddToAllowAnimations/{avatar}/{token}")]
        public object AddToAllowAnimations(string avatar, string token)
        {
            if(AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return BasicReply("avatar lookup", "AddToAllowAnimations", new [] { avatar });
            }
            // @todo accept storage
            return Failure("@todo", "AddToAllowAnimations");
        }

        [About("Attempts to play a gesture")]
        [ReturnHintsFailure("Error with gesture")]
        [ReturnHints("Accepted")]
        [ArgHints("gesture", "URLARG", "Inventory UUID of the gesture")]
        [Route(HttpVerbs.Get, "/PlayGesture/{gesture}/{token}")]
        public object PlayGesture(string gesture, string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            if (UUID.TryParse(gesture, out UUID gestureUUID) == false)
            {
                return BasicReply("Error with gesture", "PlayGesture", new [] { gesture });
            }
            InventoryItem itm = getClient().Inventory.FetchItem(gestureUUID, getClient().Self.AgentID, (3 * 1000));
            getClient().Self.PlayGesture(itm.AssetUUID);
            return BasicReply("Accepted", "PlayGesture", new [] { gesture });
        }

        [About("Resets the animation stack for the bot")]
        [ReturnHints("Accepted")]
        [Route(HttpVerbs.Get, "/ResetAnimations/{token}")]
        public object ResetAnimations(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            // @todo Reset animations function from old version
            return Failure("@todo", "ResetAnimations");
        }

    }
}
