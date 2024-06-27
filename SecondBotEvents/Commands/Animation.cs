using OpenMetaverse;
using SecondBotEvents.Services;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Animations and Gestures")]
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
                return BasicReply("avatar lookup", new [] { avatar });
            }
            // @todo accept storage
            return Failure("@todo");
        }

        [About("Attempts to play a gesture")]
        [ReturnHintsFailure("Error with gesture")]
        [ReturnHints("Accepted")]
        [ArgHints("gesture", "Inventory UUID of the gesture")]
        public object PlayGesture(string gesture)
        {
            if (UUID.TryParse(gesture, out UUID gestureUUID) == false)
            {
                return BasicReply("Error with gesture", new [] { gesture });
            }
            InventoryItem itm = GetClient().Inventory.FetchItem(gestureUUID, GetClient().Self.AgentID, (3 * 1000));
            GetClient().Self.PlayGesture(itm.AssetUUID);
            return BasicReply("Accepted", new [] { gesture });
        }

        [About("Resets the animation stack for the bot")]
        [ReturnHints("Accepted")]
        public object ResetAnimations()
        {
            // @todo Reset animations function from old version
            return Failure("@todo");
        }

    }
}
