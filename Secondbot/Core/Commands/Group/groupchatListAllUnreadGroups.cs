using OpenMetaverse;
using System.Text;

namespace BSB.Commands.Group
{
    public class GroupchatListAllUnreadGroups : CoreGroupCommand_SmartReply_auto
    {
        public override string Helpfile { get { return "Returns what groups have unread messages "; } }

        protected override string RunFunction()
        {
            int entry = 1;
            foreach (UUID group in bot.UnreadGroupchatGroups())
            {
                collection.Add(entry.ToString(),group.ToString());
                entry++;
            }
            return "count="+collection.Count.ToString();
        }
    }
}
