using System;
using OpenMetaverse;
namespace BSB.Commands.Friends
{
    public class FriendFullPerms : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar","True|False" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar UUID or Firstname Lastname","What status should it be" }; } }
        public override string Helpfile { get { return "Updates the friend perms for avatar [Arg 1] to [Arg 2] (Online/Map/Modify)"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if(UUID.TryParse(args[0],out UUID avatar) == true)
                {
                    if(bool.TryParse(args[1],out bool status) == true)
                    {
                        if (bot.GetClient.Friends.FriendList.ContainsKey(avatar) == true)
                        {
                            if (status == true)
                            {
                                bot.GetClient.Friends.GrantRights(avatar, FriendRights.CanSeeOnline | FriendRights.CanSeeOnMap | FriendRights.CanModifyObjects);
                                InfoBlob = "Friendship rights granted";
                            }
                            else
                            {
                                bot.GetClient.Friends.GrantRights(avatar, FriendRights.None);
                                InfoBlob = "Friendship rights removed";
                            }
                            return true;
                        }
                        return Failed("Avatar not on friends list or friends list has not updated yet");
                    }
                    return Failed("Unable to process arg 2");
                }
                return Failed("Unable to process avatar on arg 1");
            }
            return false;
        }
    }

}
