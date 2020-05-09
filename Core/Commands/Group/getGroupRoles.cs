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
                if (UUID.TryParse(args[1], out UUID group) == true)
                {
                    if (bot.CreateAwaitEventReply("grouproles", this, args) == true)
                    {
                        InfoBlob = "Requesting now";
                        bot.GetClient.Groups.RequestGroupRoles(group);
                        return true;
                    }
                    else
                    {
                        return Failed("Unable to await reply");
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
            StringBuilder reply = new StringBuilder();
            reply.Append("group=");
            reply.Append(args[1]);
            reply.Append("@@@");
            string addon = "";
            foreach(KeyValuePair<UUID,GroupRole> data in rolesData.Roles)
            {
                if (data.Key != UUID.Zero)
                {
                    reply.Append(addon);
                    addon = ",";
                    reply.Append(data.Value.Name);
                    reply.Append("=");
                    reply.Append(data.Key.ToString());
                }
            }
            bot.GetCommandsInterface.SmartCommandReply(args[0], reply.ToString(), CommandName);
            base.Callback(args, e);
        }
    }
}
