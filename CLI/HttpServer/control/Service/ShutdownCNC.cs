using BSB.bottypes;
using System;
using System.Collections.Generic;
using System.Text;
using static BetterSecondBot.Program;

namespace BetterSecondBot.HttpServer.Control.CNC
{
    public class ShutdownCNC : HTTP_commands_post
    {
        public override string Helpfile { get { return "[C&C Mode only]<br/>Shutsdown the C&C process"; } }
        public override string[] CallFunction(string[] args)
        {
            if (httpserver.HTTPCnCmode == true)
            {
                httpserver.ShutdownHTTP = true;
                return new [] { "OK", "Shutting down" };
            }
            return Failed("Not running as CNC");
        }
    }
}
