using System;
using System.Collections.Generic;
using OpenMetaverse;
using BetterSecondBotShared.logs;

namespace BetterSecondBot.Commands.Group
{
    public class GroupEject : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "GROUP", "Avatar [UUID or Firstname Lastname]"}; } }
        public override string Helpfile { get { return "Eject selected avatar from group"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID target_group) == true)
                {
                    if (UUID.TryParse(args[1], out UUID target_avatar) == true)
                    {
                        if (bot.MyGroups.ContainsKey(target_group) == true)
                        {
                            bot.GetClient.Groups.EjectUser(target_group, target_avatar);
                            return true;
                        }
                        else
                        {
                            return Failed("I am not a member of that group");
                        }
                    }
                    else
                    {
                        return Failed("Invaild avatar UUID");
                    }
                }
                else
                {
                    return Failed("Invaild group UUID");
                }
            }
            return false;
        }
    }
}
