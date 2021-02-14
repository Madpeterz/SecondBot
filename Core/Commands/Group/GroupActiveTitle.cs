using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.Commands.Group
{
    class GroupActiveTitle : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "UUID"}; } }
        public override string[] ArgHints { get { return new[] { "UUID of the group","UUID of the role" }; } }
        public override string Helpfile { get { return "Activates the selected title"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if(UUID.TryParse(args[0],out UUID group_uuid) == true)
                {
                    if (UUID.TryParse(args[1], out UUID group_role) == true)
                    {
                        bot.GetClient.Groups.ActivateTitle(group_uuid, group_role);
                        return true;
                    }
                    return Failed("Unable to process group Role on arg 1");
                }
                return Failed("Unable to process group UUID on arg 1");
            }
            return false;
        }
    }
}
