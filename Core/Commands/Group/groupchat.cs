using OpenMetaverse;
using System;

namespace BSB.Commands.Group
{
    class Groupchat : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID","TEXT" }; } }
        public override string[] ArgHints { get { return new[] { "Group","Message" }; } }
        public override string Helpfile { get { return "Sends a message [ARG 2] to the group [ARG 1]"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID targetgroup) == true)
                {
                    if (bot.MyGroups.ContainsKey(targetgroup) == true)
                    {
                        if (bot.GetActiveGroupchatSessions.Contains(targetgroup) == true)
                        {
                            Callback(args, null);
                            return true;
                        }
                        else
                        {
                            if (bot.CreateAwaitEventReply("groupchatjoin", this, args) == true)
                            {
                                bot.GetClient.Self.RequestJoinGroupChat(targetgroup);
                                return true;
                            }
                            else
                            {
                                return Failed("Unable to await reply");
                            }
                        }
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
        public override void Callback(string[] args, EventArgs e)
        {
            if (UUID.TryParse(args[0], out UUID targetgroup) == true)
            {
                bot.GetClient.Self.InstantMessageGroup(targetgroup, args[1]);
                base.Callback(args, e);
            }
            else
            {
                base.Callback(args, e,false);
            }
        }
    }
}