using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Home
{
    public class AtHomeSameSimMode : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "True|False"}; } }
        public override string[] ArgHints { get { return new[] { "Target mode" }; } }
        public override string Helpfile { get { return "Temporary change the @home systems recovery mode from position+sim (False) or Sim only (True) <br/> resets on next start up"; } }
        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                if (bool.TryParse(args[0], out bool status) == true)
                {
                    bot.TempUpdateAtHomeSimOnly(status);
                    return true;
                }
                else
                {
                    return Failed("Unable to process arg 1");
                }
            }
            return false;
        }
    }
}
