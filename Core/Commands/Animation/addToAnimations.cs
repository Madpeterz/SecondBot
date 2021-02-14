using OpenMetaverse;

namespace BetterSecondBot.Commands.Animation
{
    public class AddToAllowAnimations : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar UUID or Firstname Lastname" }; } }
        public override string Helpfile { get { return "Toggles if animation requests from this avatar (used for remote poseballs) are accepted<br/>Case sensitive also requires Lastname"; } }
        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                if(UUID.TryParse(args[0],out UUID avatar) == true)
                {
                    if(bot.Accept_action_from("animation",avatar) == false)
                    {
                        bot.Add_action_from("animation", avatar);
                        InfoBlob = "Granted perm animation";
                    }
                    else
                    {
                        bot.Remove_action_from("animation", avatar,true);
                        InfoBlob = "Removed perm animation";
                    }
                    return true;
                }
                return Failed("Invaild avatar for arg 1");
            }
            return false;
        }
    }
}
