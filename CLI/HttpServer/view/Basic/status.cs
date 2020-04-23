using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.HttpServer.View.Basic
{
    public class Status : HTTP_commands_get
    {
        public override int MinArgs { get { return 0; } }

        public override string Helpfile { get { return "returns the status of the bot"; } }

        public override string[] CallFunction(string[] args)
        {
            if (bot != null)
            {
                if (ArgsCheck(args) == true)
                {
                    return new string[] { bot.GetStatus() };
                }
                else
                {
                    return Failed("Incorrect number of args");
                }
            }
            else
            {
                return Failed("No Bot");
            }
        }
    }
}
