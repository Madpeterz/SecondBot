using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.HttpServer.Control.Interface
{
    public class CoreCMD : HTTP_commands_post
    {
        public override string[] ArgTypes { get { return new [] { "Text","Text" }; } }
        public override string[] ArgHints { get { return new [] { "Command","Args (~#~ split)" }; } }
        public override string Helpfile { get { return "Calls the Commands interface via HTTP (returns OK if not rejected)"; } }
        public override int MinArgs { get { return 2; } }

        public override string[] CallFunction(string[] args)
        {
            if (bot != null)
            {
                if (ArgsCheck(args) == true)
                {
                    if (bot.GetCommandsInterface.Call(args[0], args[1], UUID.Zero) == true)
                    {
                        return new string[] { "OK" };
                    }
                    else
                    {
                        return Failed("Something went wrong with the call");
                    }
                }
                else
                {
                    return Failed("Incorrect number of args");
                }
            }
            else
            {
                return Failed("No bot");
            }
        }
    }
}
