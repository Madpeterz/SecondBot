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
            if (UUID.TryParse(bot.FindAvatarName2Key(bot.OwnerName), out UUID owner) == true)
            {
                string reply = "";
                int counter = 0;
                string addon = "";
                foreach (string a in bot.GetCommandsInterface.GetCommandsList())
                {
                    reply += addon;
                    reply += a;
                    counter++;
                    if (counter == 5)
                    {
                        reply += "\n";
                        addon = "";
                        counter = 0;
                    }
                    else
                    {
                        addon = " , ";
                    }
                }

                bot.sendIM(owner, reply);

                return true;
            }
            else
            {
                return Failed("UUID is not vaild");
            }
        }
    }
}
