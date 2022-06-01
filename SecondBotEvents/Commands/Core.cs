using SecondBotEvents.Services;

namespace SecondBotEvents.Commands
{
    public class Core : CommandsAPI
    {
        public Core(EventsSecondBot setmaster) : base(setmaster)
        {
        }
        [About("Used to check connections")]
        [ReturnHints("world")]
        public object Hello()
        {
            return BasicReply("world");
        }
    }

    public class CommandLibCall
    {
        public string Command { get; set; }
        public string[] Args { get; set; }
        public string AuthCode { get; set; }
    }

    public class NearMeDetails
    {
        public string id { get; set; }
        public string name { get; set; }
        public int x { get; set; }
        public int y { get; set; }
        public int z { get; set; }
        public int range { get; set; }

    }


}
