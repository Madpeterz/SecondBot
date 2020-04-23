using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BSB.Commands.Group
{
    public class GetGroupRoles : CoreGroupCommand_SmartReply_Group
    {
        public override string Helpfile { get { return "Returns group=UUID@@@rolename=UUID,rolename=UUID ... "; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[1], out _) == true)
                {
                    if (bot.GetCommandsInterface.SmartCommandReply(args[0], "Vaildate", CommandName, true) == true)
                    {
                        if (bot.CreateAwaitEventReply("grouproles", this, args) == true)
                        {
                            return true;
                        }
                        else
                        {
                            return Failed("Unable to await reply");
                        }
                    }
                    else
                    {
                        return Failed("Smart reply [ARG 1] is not vaild");
                    }
                }
                else
                {
                    return Failed("Invaild group UUID");
                }
            }
            return false;
        }

        public override void Callback(string[] args, EventArgs e)
        {
            GroupRolesDataReplyEventArgs rolesData = (GroupRolesDataReplyEventArgs)e;
            string reply = "group=" + args[1] + "@@@";
            string addon = "";
            foreach(KeyValuePair<UUID,GroupRole> data in rolesData.Roles)
            {
                if (data.Key != UUID.Zero)
                {
                    reply += addon;
                    reply += data.Value.Name;
                    reply += "=";
                    reply += data.Key.ToString();
                    addon = ",";
                }
            }
            bot.GetCommandsInterface.SmartCommandReply(args[0], reply, CommandName);
        }
    }
}
