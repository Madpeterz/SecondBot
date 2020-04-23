using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Self
{
    public class BotSit : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Mixed" }; } }
        public override string[] ArgHints { get { return new[] { "Text \"ground\" or a object UUID" }; } }
        public override string Helpfile { get { return "Makes the bot sit on the ground or on a object if it can see it"; } }
        public override int MinArgs { get { return 1; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                bot.ResetAnimations();
                bot.GetClient.Self.Stand();
                if (args[0] == "ground")
                {
                    bot.GetClient.Self.SitOnGround();
                    return true;
                }
                else
                {
                    if (UUID.TryParse(args[0], out UUID arg_is_uuid) == true)
                    {
                        bot.GetClient.Self.RequestSit(arg_is_uuid, Vector3.Zero);
                        return true;
                    }
                    else
                    {
                        return Failed("UUID is not vaild");
                    }
                }
            }
            return false;
        }
    }
}
