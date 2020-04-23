using BSB.bottypes;
using System;
using System.Collections.Generic;
using System.Text;
using static BetterSecondBot.Program;

namespace BetterSecondBot.HttpServer.Control.CNC
{
    public class CNCStartBot : HTTP_commands_post
    {
        public override string Helpfile { get { return "[C&C Mode only]<br/>Starts bot"; } }
        public override string[] CallFunction(string[] args)
        {
            if (httpserver.HTTPCnCmode == true)
            {
                if (bot == null)
                {
                    httpserver.NewBot();
                    httpserver.GetBot.Setup(httpserver.GetConfig, AssemblyInfo.GetGitHash());
                    httpserver.GetBot.Start();
                    return new string[] { "OK","Starting bot now" };
                }
                return Failed("Unable to Login the bot (it might already be running)");
            }
            return Failed("Not running as CNC");
        }
    }
}
