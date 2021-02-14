using System;
using System.Linq;
using Microsoft.VisualBasic.CompilerServices;
using OpenMetaverse;

namespace BetterSecondBot.Commands.Group
{
    class GroupchatHistory : CoreGroupCommand_SmartReply_Group_auto
    {
        public override string Helpfile { get { return "Returns the chat history for the selected group [ARG 2] via smart reply [ARG 1]"; } }
        protected override string RunFunction(UUID targetgroup)
        {
            int entry = 1;
            foreach(string A in bot.GetGroupchat(targetgroup))
            {
                collection.Add(entry.ToString(),A);
                entry++;
            }
            return "group="+targetgroup.ToString()+"";
        }
    }
}
