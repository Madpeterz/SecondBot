using System;
using OpenMetaverse;
namespace BSB.Commands.Friends
{
    public class Friendslist : CoreCommand_SmartReply_1arg
    {
        public override string Helpfile { get { return "Returns a copy of the friends list"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(true, args[0], bot.getJsonFriendlist(), CommandName);
            }
            return false;
        }
    }

}
