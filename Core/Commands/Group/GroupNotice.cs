using BetterSecondBotShared.Static;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.Group
{
    class Groupnotice : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "TEXT","TEXT" }; } }
        public override string[] ArgHints { get { return new[] { "Group", "Message/Title", "Message" }; } }
        public override string Helpfile { get { return "Sends a group notice to [ARG 1] with the message [ARG 2]<hr/>if given 3 Args: Sends a group notice to [ARG 1] with the title [ARG 2] and the message [ARG 3]"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID targetgroup) == true)
                {
                    if (bot.MyGroups.ContainsKey(targetgroup) == true)
                    {
                        string Noticetitle = "GroupNotice@"+ helpers.UnixTimeNow().ToString()+"";
                        string Noticemessage = "";
                        if (args.Length == 3)
                        {
                            Noticetitle = args[1];
                        }
                        Noticemessage = args[^1];
                        GroupNotice NewNotice = new GroupNotice();
                        NewNotice.Subject = Noticetitle;
                        NewNotice.Message = Noticemessage;
                        NewNotice.OwnerID = bot.GetClient.Self.AgentID;
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