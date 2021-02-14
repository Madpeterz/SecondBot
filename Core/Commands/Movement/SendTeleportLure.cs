using OpenMetaverse;

namespace BSB.Commands.Movement
{
    public class SendTeleportLure : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar UUID or Firstname Lastname" }; } }
        public override string Helpfile { get { return "Make the bot request the target avatar teleport to the bot"; } }

        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                if(UUID.TryParse(args[0],out UUID avatar) == true)
                {
                    bot.GetClient.Self.SendTeleportLure(avatar);
                    return true;
                }
                return Failed("Unable to process avatar on arg 1");
            }
            return false;
        }
    }
}
