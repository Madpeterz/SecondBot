using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.Commands.Group
{
    public abstract class CoreGroupCommand_SmartReply : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Smart" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]"}; } }
        public override int MinArgs { get { return 1; } }
    }
    public abstract class CoreGroupCommand_SmartReply_auto : CoreGroupCommand_SmartReply
    {
        protected virtual string RunFunction()
        {
            return "";
        }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                return bot.GetCommandsInterface.SmartCommandReply(args[0], RunFunction(), CommandName);
            }
            return false;
        }
    }

    public abstract class CoreGroupCommand_SmartReply_Group : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "Mixed", "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply [Channel|IM uuid|http url]", "Group" }; } }
        public override int MinArgs { get { return 2; } }
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
                        return bot.GetCommandsInterface.SmartCommandReply(args[0], RunFunction(targetgroup), CommandName);
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
