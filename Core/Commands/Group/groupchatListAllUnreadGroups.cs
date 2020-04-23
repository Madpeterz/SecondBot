using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.Group
{
    public class GroupchatListAllUnreadGroups : CoreGroupCommand_SmartReply_auto
    {
        public override string Helpfile { get { return "Returns a csv of groups with unread messages: UUID,UUID... "; } }

        protected override string RunFunction()
        {
            string reply = "";
            string addon = "";
            foreach (UUID group in bot.UnreadGroupchatGroups())
            {
                reply += addon;
                reply += group;
                addon = ",";
            }
            return reply;
        }
    }
}
