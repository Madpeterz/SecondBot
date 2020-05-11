using OpenMetaverse;
using System.Collections.Generic;

namespace BSB.Commands.Group
{
    public abstract class CoreGroupCommand_SmartReply_auto : CoreCommand_SmartReply_1arg
    {
        protected virtual string RunFunction()
        {
            return "";
        }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(true, args[0], RunFunction(), CommandName, collection);
            }
            return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "", CommandName);
        }
    }

    public abstract class CoreGroupCommand_SmartReply_Group : CoreCommand_2arg
    {
        protected Dictionary<string, string> collection = new Dictionary<string, string>();
        public override string[] ArgTypes { get { return new[] { "Mixed", "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply [Channel|IM uuid|http url]", "Group" }; } }
    }

    public abstract class CoreGroupCommand_SmartReply_Group_auto : CoreGroupCommand_SmartReply_Group
    {
        protected virtual string RunFunction(UUID group)
        {
            return "";
        }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[1], out UUID targetgroup) == true)
                {
                    if (bot.MyGroups.ContainsKey(targetgroup) == true)
                    {
                        return bot.GetCommandsInterface.SmartCommandReply(true,args[0], RunFunction(targetgroup), CommandName);
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
    }
}
