using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.HttpServer.Control.Actions
{
    public class Logout : HTTP_commands
    {
        public override string Helpfile { get { return "Makes the bot logout<br/> Returns OK,I dont want to go if accepted"; } }

        public override string[] CallFunction(string[] args)
        {
            if (bot != null)
            {
                if (httpserver.HTTPCnCmode == true)
                {
                    httpserver.KillBot();
                    return new [] { "OK", "Bot Shutting down" };
                }
                else
                {
                    bot.KillMePlease();
                    return new [] { "OK", "I dont want to go" };
                }
            }
            else
            {
                return Failed("No bot logged in");
            }
        }
    }
}
