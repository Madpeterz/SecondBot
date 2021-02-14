using OpenMetaverse;

namespace BetterSecondBot.Commands.Group
{
    class GroupchatGroupHasUnread : CoreGroupCommand_SmartReply_Group_auto
    {
        public override string Helpfile { get { return "Returns True|False via the smart reply target if there are any unread group ims for the group [ARG 2]"; } }

        protected override string RunFunction(UUID targetgroup)
        {
            return ""+targetgroup.ToString()+"=" + bot.GroupHasUnread(targetgroup).ToString();
        }

    }
}
