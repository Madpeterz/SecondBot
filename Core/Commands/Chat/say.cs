using System.Linq;
using OpenMetaverse;

namespace BSB.Commands.Chat
{
    class Say : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Mixed","Text","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Message or channel","Message","Chat level" }; } }
        public override string Helpfile { get { return "Makes the bot talk via chat<br/>Example: say|||Hello<br/>Example: say|||12~#~Goodbye<br/>Example: say|||12~#~Goodbye~#~Shout<br/>Chat levels [Arg 3]:<br/> Whisper<br/> Shout"; } }
        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                string message = args[0];
                int channel = 0;
                ChatType ChatLevel = ChatType.Normal;
                if (args.Count() >= 2)
                {
                    message = args[1];
                    int.TryParse(args[0], out channel);
                    if (args.Count() == 3)
                    {
                        if (args[2] == "Shout")
                        {
                            ChatLevel = ChatType.Shout;
                        }
                        else if (args[2] == "Whisper")
                        {
                            ChatLevel = ChatType.Whisper;
                        }
                    }
                }
                if (channel == 0)
                {
                    bot.AddToLocalChat(bot.GetClient.Self.Name, message);
                }
                bot.GetClient.Self.Chat(message, channel, ChatLevel);
                return true;
            }
            return false;
        }
    }
}
