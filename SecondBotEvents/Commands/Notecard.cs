using OpenMetaverse;
using SecondBotEvents.Services;


namespace SecondBotEvents.Commands
{
    public class Notecard : CommandsAPI
    {
        public Notecard(EventsSecondBot setmaster) : base(setmaster)
        {
        }

        [About("Adds content to the Collection<br/> Also creates the Collection if it does not exist")]
        [ReturnHintsFailure("Collection value is empty")]
        [ReturnHintsFailure("Content value is empty")]
        [ReturnHints("ok")]
        [ArgHints("collection", "The name of the collection")]
        [ArgHints("content", "The text to add to the collection")]
        public object NotecardAdd(string collection, string content)
        {
            if (SecondbotHelpers.notempty(collection) == false)
            {
                return Failure("Collection value is empty", new [] { collection, content });
            }
            if (SecondbotHelpers.notempty(content) == false)
            {
                return Failure("Content value is empty", new [] { collection, content });
            }
            return Failure("@todo notecard temp stroage");
            //return BasicReply("ok", "NotecardAdd", new [] { collection, content });
        }

        [About("Clears the contents of a collection")]
        [ReturnHintsFailure("Collection value is empty")]
        [ReturnHints("ok")]
        [ArgHints("collection", "The name of the collection")]
        public object NotecardClear(string collection)
        {
            if (SecondbotHelpers.notempty(collection) == false)
            {
                return Failure("Collection value is empty", new [] { collection });
            }
            return Failure("@todo notecard storage");
        }

        [About("Sends a notecard to a avatar using the text in the prebuilt collection [see NotecardAdd] and also clears the collection just before sending [see NotecardClear]")]
        [ReturnHintsFailure("Collection value is empty")]
        [ReturnHintsFailure("Notecardname value is empty")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHintsFailure("No content in notecard storage ?")]
        [ReturnHints("ok")]
        [ArgHints("avatar", "The UUID or Name of an avatar")]
        [ArgHints("collection", "The name of the collection")]
        [ArgHints("notecardname", "What to call the created notecard")]
        public object NotecardSend(string avatar, string collection, string notecardname)
        {
            if (SecondbotHelpers.notempty(collection) == false)
            {
                return Failure("Collection value is empty", new [] { avatar, collection, notecardname });
            }
            if (SecondbotHelpers.notempty(notecardname) == false)
            {
                return Failure("Notecardname value is empty", new [] { avatar, collection, notecardname });
            }
            ProcessAvatar(avatar);
            if(avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", new [] { avatar, collection, notecardname });
            }
            return Failure("@todo notecard storage");
        }

        [About("Creates and sends a notecard in one command good if you are using HTTP otherwise see [NotecardSend]")]
        [ReturnHintsFailure("notecardname value is empty")]
        [ReturnHintsFailure("Content value is empty")]
        [ReturnHintsFailure("Invaild avatar uuid")]
        [ReturnHints("ok")]
        [ArgHints("avatar", "The UUID or Name of an avatar")]
        [ArgHints("content", "The text to add to the collection")]
        [ArgHints("notecardname", "What to call the created notecard")]
        public object NotecardDirectSend(string avatar, string content, string notecardname)
        {
            if (SecondbotHelpers.notempty(notecardname) == false)
            {
                return Failure("notecardname value is empty", new[] { avatar, content, notecardname });
            }
            if (SecondbotHelpers.notempty(content) == false)
            {
                return Failure("content value is empty", new[] { avatar, content, notecardname });
            }
            ProcessAvatar(avatar);
            if (avataruuid == UUID.Zero)
            {
                return Failure("Invaild avatar uuid", new[] { avatar, content, notecardname });
            }
            return Failure("@todo notecard sending");
        }
    }
}
