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
        [ArgHints("avatar", "UUID (or Firstname Lastname)")]
        public object AddToAllowAnimations(string avatar)
        {
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
        [ArgHints("gesture", "Inventory UUID of the gesture")]
        public object PlayGesture(string gesture)
        {
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
        public object ResetAnimations()
        {
            // @todo Reset animations function from old version
            return Failure("@todo", "ResetAnimations");
        }

    }
}
