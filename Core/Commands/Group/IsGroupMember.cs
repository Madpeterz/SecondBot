using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using OpenMetaverse;

namespace BSB.Commands.Group
{
    public class IsGroupMember : CoreCommand_3arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "Mixed", "Avatar" }; } }
        public override string[] ArgHints { get { return new[] { "Group", "Smart reply [Channel|IM uuid|http url]", "Avatar [UUID or Firstname Lastname]" }; } }
        public override string Helpfile { get { return "Returns if the selected user is in the group or not"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID targetgroup) == true)
                {
                    if (UUID.TryParse(args[2], out UUID testavatar) == true)
                    {
                        if (bot.FastCheckInGroup(targetgroup, testavatar) == true)
                        {
                            Dictionary<string, string> reply = new Dictionary<string, string>
                            {
                                { "group", args[0] },
                                { "avatar", args[2] },
                                { "ingroup", "1" }
                            };
                            return bot.GetCommandsInterface.SmartCommandReply(true, args[1], JsonConvert.SerializeObject(reply), CommandName);
                        }
                        else
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
                    }
                    else
                    {
                        return Failed("Unable to find avatar");
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
            if (UUID.TryParse(args[2], out UUID testavatar) == true)
            {
                GroupMembersReplyEventArgs members_reply = (GroupMembersReplyEventArgs)e;
                string return_status = "0";
                foreach (KeyValuePair<UUID, GroupMember> data in members_reply.Members)
                {
                    if (data.Value.ID == testavatar)
                    {
                        return_status = "1";
                        break;
                    }
                }
                Dictionary<string, string> reply = new Dictionary<string, string>
                {
                    { "group", args[0] },
                    { "avatar", args[2] },
                    { "ingroup", return_status }
                };
                bot.GetCommandsInterface.SmartCommandReply(true, args[1], JsonConvert.SerializeObject(reply), CommandName);
            }
            base.Callback(args, e);
        }
    }
}
