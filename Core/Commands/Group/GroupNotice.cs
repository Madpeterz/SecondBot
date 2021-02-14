using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BetterSecondBot.Commands.Group
{
    class Groupnotice : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "TEXT","TEXT","TEXT" }; } }
        public override string[] ArgHints { get { return new[] { "Group", "Message/Title", "Message","Inventory UUID" }; } }
        public override string Helpfile { get { return "Sends a group notice to [ARG 1] with the message [ARG 2]<hr/>if given 3 Args: Sends a group notice to [ARG 1] with the title [ARG 2] and the message [ARG 3]<br/>if [ARG 4] is given then it also sends the notice with an attachment"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID targetgroup) == true)
                {
                    if (bot.MyGroups.ContainsKey(targetgroup) == true)
                    {
                        string Noticetitle = "GroupNotice@"+ helpers.UnixTimeNow().ToString()+"";
                        string Noticemessage = args[1];
                        if (args.Length >= 3)
                        {
                            Noticetitle = args[1];
                            Noticemessage = args[2];
                        }
                        GroupNotice NewNotice = new GroupNotice();
                        NewNotice.Subject = Noticetitle;
                        NewNotice.Message = Noticemessage;
                        NewNotice.OwnerID = bot.GetClient.Self.AgentID;
                        if (args.Length == 4)
                        {
                            if (UUID.TryParse(args[3], out UUID result) == true)
                            {
                                NewNotice.AttachmentID = result;
                            }
                        }
                        bot.GetClient.Groups.SendGroupNotice(targetgroup, NewNotice);
                        return true;
                    }
                    else
                    {
                        return Failed("Unkown group");
                    }
                }
                else
                {
                    return Failed("Invaild UUID");
                }
            }
            return false;
        }
    }
}