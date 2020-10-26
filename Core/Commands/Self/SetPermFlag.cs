using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BSB.Commands.Self
{
    public class SetPermFlag : CoreCommand_3arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar", "Text","Text (True|Flase)", "Text (True|Flase)" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]", "Flag: friend, group, animation, teleport or command", "State to set the flag to","Make the permission sticky" }; } }
        public override string Helpfile { get { return "Makes the bot pay a avatar"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if(UUID.TryParse(args[0],out UUID targetUUID) == true)
                {
                    string[] AcceptedFlags = new string[] { "friend", "group", "animation", "teleport", "command" };
                    if(AcceptedFlags.Contains(args[1]) == true)
                    {
                        if(bool.TryParse(args[2],out bool stateflag) == true)
                        {
                            bool sticky = false;
                            if(args.Length == 4)
                            {
                                bool.TryParse(args[3], out sticky);
                            }
                            if(stateflag == true)
                            {
                                bot.Add_action_from(args[1], targetUUID, sticky);
                                InfoBlob = "Added perm: "+args[1]+" Sticky: " + sticky.ToString();
                            }
                            else
                            {
                                bot.Remove_action_from(args[1], targetUUID, sticky);
                                InfoBlob = "Removed perm: " + args[1] + " Sticky: " + sticky.ToString();
                            }
                            return true;
                        }
                        return Failed("Failed to process state flag value");
                    }
                    return Failed("Not an accepted flag");
                }
                return Failed("Unable to process UUID");
            }
            return false;
        }
    }
}
