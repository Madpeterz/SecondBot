using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace BSB.Commands.Group
{
    public class GetGroupRoles : CoreGroupCommand_SmartReply_Group
    {
        public override string Helpfile { get { return "Gets a list of group roles Json encoded "; } }
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
            foreach(KeyValuePair<UUID,GroupRole> data in rolesData.Roles)
            {
                if (data.Key != UUID.Zero)
                {
                    collection.Add(data.Value.Name, data.Key.ToString());
                }
            }
            bot.GetCommandsInterface.SmartCommandReply(true,args[0], "group=" + args[1], CommandName,collection);
            base.Callback(args, e);
        }
    }
}
