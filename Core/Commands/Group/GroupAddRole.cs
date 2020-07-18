using System;
using System.Collections.Generic;
using OpenMetaverse;
using BetterSecondBotShared.logs;
using System.Linq;

namespace BSB.Commands.Group
{
    public class GroupAddRole : CoreCommand_3arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "Avatar", "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "GROUP", "Avatar [UUID or Firstname Lastname]", "Role" }; } }
        public override string Helpfile { get { return "Adds the avatar [ARG 2] to the Group [ARG 1] with the role [ARG 3] if they are not in the group then it invites them"; } }

        public override void Callback(string[] args, EventArgs e)
        {
            GroupMembersReplyEventArgs reply = (GroupMembersReplyEventArgs)e;
            if (UUID.TryParse(args[0], out UUID target_group) == true)
            {
                if (reply.GroupID == target_group)
                {
                    if (UUID.TryParse(args[1], out UUID target_avatar) == true)
                    {
                        if (UUID.TryParse(args[2], out UUID target_role) == true)
                        {
                            if(reply.Members.ContainsKey(target_avatar) == false)
                            {
                                bot.GetClient.Groups.AddToRole(target_group, target_role, target_avatar);
                                base.Callback(args, e, true);
                            }
                            else
                            {
                                ConsoleLog.Info("Member not in group to get role, passing to invite");
                                bot.GetCommandsInterface.Call("GroupInvite", String.Join("~#~", args), caller, "~#~");
                                base.Callback(args, e, true);
                            }

                        }
                        else
                        {
                            ConsoleLog.Crit("{GroupAddRole} Unable to unpack role as part of the callback");
                            base.Callback(args, e, false);
                        }
                    }
                    else
                    {
                        ConsoleLog.Crit("{GroupAddRole} Unable to unpack avatar as part of the callback");
                        base.Callback(args, e, false);
                    }
                }
                else
                {
                    ConsoleLog.Crit("Callback sent for the wrong group as part of GroupAddRole!");
                    base.Callback(args, e, false);
                }
            }
            else
            {
                ConsoleLog.Crit("{GroupAddRole} Unable to unpack group as part of the callback");
                base.Callback(args, e, false);
            }
        }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID target_group) == true)
                {
                    if (UUID.TryParse(args[1], out UUID target_avatar) == true)
                    {
                        if (UUID.TryParse(args[2], out UUID target_role) == true)
                        {
                            if (bot.MyGroups.ContainsKey(target_group) == true)
                            {
                                if (bot.GetAllowGroupInvite(target_avatar) == true)
                                {
                                    if (bot.CreateAwaitEventReply("groupmembersreply", this, args) == true)
                                    {
                                        bot.GetClient.Groups.RequestGroupMembers(target_group);
                                        return true;
                                    }
                                    else
                                    {
                                        return Failed("Unable to await reply");
                                    }
                                }
                                else
                                {
                                    return Failed("Group invite to this avatar is on 120sec cooldown");
                                }
                            }
                            else
                            {
                                return Failed("I am not a member of that group");
                            }
                        }
                        else
                        {
                            return Failed("Invaild role UUID");
                        }
                    }
                    else
                    {
                        return Failed("Invaild avatar UUID");
                    }
                }
                else
                {
                    return Failed("Invaild group UUID");
                }
            }
            return false;
        }
    }
}
