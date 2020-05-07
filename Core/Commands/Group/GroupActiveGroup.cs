using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.Group
{
    class GroupActiveGroup : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Group"}; } }
        public override string[] ArgHints { get { return new[] { "Group UUID or name" }; } }
        public override string Helpfile { get { return "Sets the selected group to the active group"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                UUID group_uuid = UUID.Zero;
                if(UUID.TryParse(args[0],out group_uuid) == false)
                {
                    foreach(KeyValuePair<UUID,OpenMetaverse.Group> G in bot.MyGroups)
                    {
                        if(G.Value.Name == args[0])
                        {
                            group_uuid = G.Key;
                            break;
                        }
                    }
                    if (group_uuid == UUID.Zero)
                    {
                        return Failed("Unable to find group");
                    }
                }
                if (group_uuid != UUID.Zero)
                {
                    bot.GetClient.Groups.ActivateGroup(group_uuid);
                }
            }
            return false;
        }
    }
}
