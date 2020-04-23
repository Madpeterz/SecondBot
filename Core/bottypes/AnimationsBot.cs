using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BSB.bottypes
{
    public abstract class AnimationsBot : VirtualsBot
    {
        protected List<UUID> active_animations = new List<UUID>();

        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            if (reconnect == false)
            {
                Client.Avatars.AvatarAnimation += AvatarAnimationHandler;
            }
        }

        public override void ResetAnimations()
        {
            foreach (UUID ani in active_animations)
            {
                Client.Self.AnimationStop(ani, true);
            }
            active_animations.Clear();
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
