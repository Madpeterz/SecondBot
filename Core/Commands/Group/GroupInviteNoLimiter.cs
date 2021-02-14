using BetterSecondBotShared.logs;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.Commands.Group
{
    class GroupInviteNoLimiter: CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "Avatar", "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "GROUP", "Avatar [UUID or Firstname Lastname]", "Role" }; } }
        public override string Helpfile { get { return "see GroupInvite, note: This command bypasses the limiter use at your own risk!"; } }

        protected void ForRealGroupInvite(string[] args, UUID group, UUID avatar)
        {
            UUID target_role = UUID.Zero;
            if (args.Length == 3)
            {
                if (UUID.TryParse(args[2], out target_role) == false)
                {
                    LogFormater.Warn("GroupInviteNoLimiter: Role uuid not vaild using everyone");
                }
            }
            bot.GetClient.Groups.Invite(group, new List<UUID>() { target_role }, avatar);
        }
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
                            ForRealGroupInvite(args, target_group, target_avatar);
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
