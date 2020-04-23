using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.Group
{
    class GroupchatAnyUnread : CoreGroupCommand_SmartReply_auto
    {
        public override string Helpfile { get { return "Returns True|False via the smart reply target if there are any unread group ims"; } }
        protected override string RunFunction()
        {
            return bot.HasUnreadGroupchats().ToString();
        }
    }
}