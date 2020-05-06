using OpenMetaverse;
using System.Text;

namespace BSB.Commands.Group
{
    public class GroupchatListAllUnreadGroups : CoreGroupCommand_SmartReply_auto
    {
        public override string Helpfile { get { return "Returns a csv of groups with unread messages: UUID,UUID... "; } }

        protected override string RunFunction()
        {
            StringBuilder reply = new StringBuilder();
            string addon = "";
            foreach (UUID group in bot.UnreadGroupchatGroups())
            {
                reply.Append(addon);
                addon = ",";
                reply.Append(group);
            }
            return reply.ToString();
        }
    }
}
