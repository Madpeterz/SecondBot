using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;

namespace BSB.Commands.Group
{
    class GroupchatHistory : CoreGroupCommand_SmartReply_Group_auto
    {
        public override string Helpfile { get { return "Returns the chat history for the selected group [ARG 2] via smart reply [ARG 1]"; } }
        protected override string RunFunction(UUID targetgroup)
        {
            return String.Join("###", bot.GetGroupchat(targetgroup));
        }
    }
}
