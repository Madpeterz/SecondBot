using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Self
{ 
    public class Help : CoreCommand
    {
     
        public override string Helpfile { get { return "Makes the bot turn to face [ARG 1] and point at them (if found)"; } }
        public override bool CallFunction(string[] args)
        {
            if (UUID.TryParse(bot, out UUID testavatar) == true)
            {

            }
            else
            {
                return Failed("UUID is not vaild");
            }
        }
    }
}
