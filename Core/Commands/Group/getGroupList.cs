using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BetterSecondBot.Commands.Group
{
    public class GetGroupList : CoreGroupCommand_SmartReply_auto
    {
        public override string Helpfile { get { return "Gets a list of groups Json encoded"; } }
        protected override string RunFunction()
        {
            foreach (KeyValuePair<UUID, OpenMetaverse.Group> groupdata in bot.MyGroups)
            {
                collection.Add(groupdata.Value.Name, groupdata.Key.ToString());
            }
            return "grouplist=loaded";
        }
    }
}
