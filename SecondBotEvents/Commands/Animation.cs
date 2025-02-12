using OpenMetaverse;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecondBotEvents.Commands
{
    [ClassInfo("Animations and Gestures")]
    internal class AnimationCommands(EventsSecondBot setmaster) : CommandsAPI(setmaster)
    {
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
                return BasicReply("avatar lookup", [avatar]);
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
                return BasicReply("Error with gesture", [gesture]);
            }
            InventoryItem itm = GetClient().Inventory.FetchItem(gestureUUID, GetClient().Self.AgentID, TimeSpan.FromSeconds(15));
            GetClient().Self.PlayGesture(itm.AssetUUID);
            return BasicReply("Accepted", [gesture]);
        }

        [About("Resets the animation stack for the bot")]
        [ReturnHints("Accepted - X stopped animations")]
        public object ResetAnimations()
        {
            List<UUID> animations = [.. GetClient().Self.SignaledAnimations.Copy().Keys];
            foreach (UUID anim in animations)
            {
                GetClient().Self.AnimationStop(anim, true);
            }
            return BasicReply("Accepted - "+animations.Count.ToString()+" stopped animations");
        }

    }
}
