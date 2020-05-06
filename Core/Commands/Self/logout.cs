using OpenMetaverse;

namespace BSB.Commands.Self
{
    public class Logout : CoreCommand
    {
        public override string Helpfile { get { return "Makes the bot kill itself you monster"; } }
        public override bool CallFunction(string[] args)
        {
            bot.GetClient.Self.Chat("I dont want to go :( ", 0, ChatType.Normal);
            bot.KillMePlease();          
            return true;
        }
    }
}
