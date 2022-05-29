using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using OpenMetaverse;
using SecondBotEvents.Services;
using System.Threading;

namespace SecondBotEvents.Commands
{
    public class Core : CommandsAPI
    {
        public Core(EventsSecondBot setmaster) : base(setmaster)
        {
        }
        [About("Used to check HTTP connections")]
        [ReturnHints("world")]
        [NeedsToken(false)]
        [Route(HttpVerbs.Get, "/Hello")]
        public object Hello()
        {
            return BasicReply("world", "Hello");
        }

        [About("Removes the given token from the accepted token pool")]
        [ReturnHints("ok")]
        [Route(HttpVerbs.Get, "/LogoutUI/{token}")]
        public object LogoutUI(string token)
        {
            if (AllowToken(token) == false)
            {
                return Failure("Token not accepted");
            }
            SuccessNoReturn("LogoutUI");
            // @todo remove token from stoage
            return BasicReply("ok");
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
