using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BSB.Commands.Group
{
    public class GetGroupList : CoreGroupCommand_SmartReply_auto
    {
        public override string Helpfile { get { return "Returns a csv of known groups NAME=UUID,NAME=UUID... "; } }
        protected override string RunFunction()
        {
            StringBuilder reply = new StringBuilder();
            string addon = "";
            foreach (KeyValuePair<UUID, OpenMetaverse.Group> groupdata in bot.MyGroups)
            {
                reply.Append(addon);
                addon = ",";
                reply.Append(groupdata.Value.Name);
                reply.Append("=");
                reply.Append(groupdata.Key);
            }
            return reply.ToString();
        }
    }
}
