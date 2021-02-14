using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BetterSecondBot.bottypes
{
    public abstract class AnimationsBot : VirtualsBot
    {
        protected List<UUID> active_animations = new List<UUID>();

        public override void AfterBotLoginHandler()
        {
            if (reconnect == false)
            {
                Client.Avatars.AvatarAnimation += AvatarAnimationHandler;
            }
            base.AfterBotLoginHandler();
        }

        public override void ResetAnimations()
        {
            List<UUID> copyAnyimations = active_animations;
            foreach (UUID ani in copyAnyimations)
            {
                Client.Self.AnimationStop(ani, true);
            }
            lock (active_animations)
            {
                active_animations.Clear();
            }
        }

        protected override void AvatarAnimationHandler(object sender, AvatarAnimationEventArgs e)
        {
            if (e.AvatarID == Client.Self.AgentID)
            {
                foreach (Animation startani in e.Animations)
                {
                    if (active_animations.Contains(startani.AnimationID) == false)
                    {
                        active_animations.Add(startani.AnimationID);
                    }
                }
            }
        }
    }
}
