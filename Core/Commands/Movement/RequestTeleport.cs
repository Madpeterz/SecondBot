using OpenMetaverse;

namespace BSB.Commands.Movement
{
    public class RequestTeleport : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar UUID or Firstname Lastname" }; } }
        public override string Helpfile { get { return "Sends a teleport lure request (Move the bot to the avatar)"; } }

        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                if(UUID.TryParse(args[0],out UUID avatar) == true)
                {
                    bot.Add_uuid_to_teleport_list(avatar);
                    bot.GetClient.Self.SendTeleportLureRequest(avatar, "I would like to teleport to you");
                    return true;
                }
                return Failed("Unable to process avatar on arg 1");
            }
            return false;
        }
    }
}
