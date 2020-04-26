using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using BetterSecondBotShared.logs;

namespace BSB.Commands.Group
{
    public class GroupInvite : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "Avatar", "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "GROUP", "Avatar [UUID or Firstname Lastname]", "Role" }; } }
        public override string Helpfile { get { return "Invites the avatar [ARG 2] to the Group [ARG 1] {optional with the Role [ARG 3]} otherwise to role Everyone"; } }
        public override int MinArgs { get { return 2; } }

        protected void ForRealGroupInvite(string[] args,UUID group,UUID avatar)
        {
            UUID target_role = UUID.Zero;
            if (args.Length == 3)
            {
                UUID.TryParse(args[2], out target_role);
            }
            List<UUID> invite_roles = new List<UUID>();
            if (target_role != UUID.Zero) invite_roles.Add(target_role);
            else invite_roles.Add(UUID.Zero);
            bot.GetClient.Groups.Invite(group, invite_roles, avatar);
        }
        public override void Callback(string[] args, EventArgs e)
        {
            GroupMembersReplyEventArgs reply = (GroupMembersReplyEventArgs)e;
            if (UUID.TryParse(args[0], out UUID target_group) == true)
            {
                if (UUID.TryParse(args[1], out UUID target_avatar) == true)
                {
                    if (reply.GroupID == target_group)
                    {
                        if (reply.Members.ContainsKey(target_avatar) == false)
                        {
                            ForRealGroupInvite(args, target_group, target_avatar);
                            base.Callback(args, e);
                        }
                        else
                        {
                            base.Callback(args, e, false);
                        }
                    }
                    else
                    {
                        ConsoleLog.Crit("Callback sent for the wrong group as part of group invite!");
                        base.Callback(args, e, false);
                    }
                }
                else
                {
                    base.Callback(args, e, false);
                }
            }
            else
            {
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
                        if (bot.MyGroups.ContainsKey(target_group) == true)
                        {
                            if (bot.GetAllowGroupInvite(target_avatar) == true)
                            {
                                if (bot.FastCheckInGroup(target_group, target_avatar) == false)
                                {
                                    if (bot.NeedReloadGroupData(target_group) == true)
                                    {
                                        if (bot.CreateAwaitEventReply("groupmembersreply", this, args) == true)
                                        {
                                            bot.GroupInviteLockoutArm(target_avatar); // enable 120 sec cooldown
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
                                        bot.GroupInviteLockoutArm(target_avatar); // enable 120 sec cooldown
                                        ForRealGroupInvite(args, target_group, target_avatar);
                                        return true;
                                    }
                                }
                                else
                                {
                                    return true; // they are in the group (or we have old data) retry in like 2 mins
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
