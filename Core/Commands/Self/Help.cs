using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Self
{
    public class Help : CoreCommand
    {

        public override string Helpfile { get { return "Basic list of commands"; } }
        public override bool CallFunction(string[] args)
        {
            if (bot.getMaster_uuid != UUID.Zero)
            {
              
                StringBuilder sb = new StringBuilder();
                sb.Append("Second Bot - Basic Command List\n\n");
                sb.Append(bot.GetCommandsInterface.GetCommandHelp("im"));
                sb.Append("\n\n");
                sb.Append(bot.GetCommandsInterface.GetCommandHelp("say"));
                sb.Append("\n\n");
                sb.Append(bot.GetCommandsInterface.GetCommandHelp("botsit"));
                sb.Append("\n\n");
                sb.Append("\nType: 'morehelp' for a full list of commands\n");
                sb.Replace("<br/>", "\n");
                bot.sendIM(bot.getMaster_uuid, sb.ToString());

                return true;
            }
            else
            {
                return Failed("UUID is not vaild");
            }
        }
    }
    public class MoreHelp : CoreCommand
    {
     
        public override string Helpfile { get { return "A Full list of supported commands"; } }
        public override bool CallFunction(string[] args)
        {
            if (bot.getMaster_uuid != UUID.Zero)
            {


                StringBuilder reply = new StringBuilder();
                reply.Append("Second Bot - Full Command List\n\n");
                foreach (string a in bot.GetCommandsInterface.GetCommandsList())
                {
                    reply.Append(a);
                    reply.Append("\n");
                    reply.Append(bot.GetCommandsInterface.GetCommandHelp(a));
                    reply.Append("\n\n");
                }

                bot.sendIM(bot.getMaster_uuid, reply.ToString());

                return true;
            }
            else
            {
                return Failed("UUID is not vaild");
            }
        }
    }
}
