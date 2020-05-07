using System;
using OpenMetaverse;
namespace BSB.Commands.Movement
{
    public class FriendRequest : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar UUID or Firstname Lastname" }; } }
        public override string Helpfile { get { return "sends a friend request to the target avatar"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID avatar) == true)
                {
                    if (bot.GetClient.Friends.FriendList.ContainsKey(avatar) == false)
                    {
                        bot.GetClient.Friends.OfferFriendship(avatar);
                    }
                    return true;
                }
                return Failed("Unable to process avatar on arg 1");
            }
            return false;
        }
    }

}
