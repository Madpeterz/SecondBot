using OpenMetaverse;

namespace BSB.Commands.Self
{
    public class Bye : Logout
    {
        public override string Helpfile { get { return "Makes the bot kill itself you monster, without saying anything"; } }
        public override bool CallFunction(string[] args)
        {
            bot.KillMePlease();
            return true;
        }
    }
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

    public class Logoff : CoreCommand
    {
        public override string Helpfile { get { return "Makes the bot kill itself you monster"; } }
        public override bool CallFunction(string[] args)
        {
            bot.GetClient.Self.Chat("Laters im out", 0, ChatType.Normal);
            bot.KillMePlease();
            return true;
        }
    }
}
