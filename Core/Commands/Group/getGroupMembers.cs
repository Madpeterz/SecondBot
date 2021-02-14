using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BetterSecondBot.Commands.Group
{
    public class GetGroupMembers : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "Mixed" }; } }
        public override string[] ArgHints { get { return new[] { "Group", "Smart reply [Channel|IM uuid|http url]" }; } }
        public override string Helpfile { get { return "Gets membership of a group plus online status "; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID targetgroup) == true)
                {
                    if (bot.MyGroups.ContainsKey(targetgroup) == true)
                    {
                        if (bot.CreateAwaitEventReply("groupmembersreply", this, args) == true)
                        {
                            bot.GetClient.Groups.RequestGroupMembers(targetgroup);
                            InfoBlob = "Requesting group membership";
                            return true;
                        }
                        else
                        {
                            return Failed("Unable to await reply");
                        }

                    }
                    else
                    {
                        return Failed("Unknown group");
                    }
                }
                else
                {
                    return Failed("Invaild UUID");
                }
            }
            return false;
        }

        public override void Callback(string[] args, EventArgs e)
        {
            GroupMembersReplyEventArgs members_reply = (GroupMembersReplyEventArgs)e;
            Dictionary<string, string> collection = new Dictionary<string, string>();
            foreach (KeyValuePair<UUID, GroupMember> data in members_reply.Members)
            {
                collection.Add(data.Key.ToString(), data.Value.OnlineStatus);
            }
            bot.GetCommandsInterface.SmartCommandReply(true, args[1], "group=" + args[0], CommandName, collection);
            base.Callback(args, e);
        }
    }
}
