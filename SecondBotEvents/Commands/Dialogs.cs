using SecondBotEvents.Services;


namespace SecondBotEvents.Commands
{
    public class Dialogs : CommandsAPI
    {
        public Dialogs(EventsSecondBot setmaster) : base(setmaster)
        {
        }
        [About("Updates the relay target (you can have 1 of each type)<br/>Clear will disable them all")]
        [ReturnHints("cleared")]
        [ReturnHints("set/avatar [ok]")]
        [ReturnHints("set/http [ok]")]
        [ReturnHints("set/channel [ok]")]
        [ReturnHintsFailure("Not a vaild option")]
        [ArgHints("target", "Options: Channel (Any number),Avatar UUID,HTTPurl<br/>Clear")]
        public object DialogRelay(string target)
        {
            return Failure("@todo dialog relay");
        }

        [About("Makes the bot interact with the dialog [dialogid] with the button [buttontext]")]
        [ReturnHints("true")]
        [ReturnHints("false")]
        [ReturnHintsFailure("bad dialog id")]
        [ArgHints("dialogid", "The ID for the dialog")]
        [ArgHints("buttontext", "The button text to push")]
        public object DialogResponce(string dialogid, string buttontext)
        {
            if (int.TryParse(dialogid, out int dialogidnum) == false)
            {
                return Failure("bad dialog id", new [] { dialogid, buttontext });
            }
            return Failure("@todo dialog relay");
        }

        [About("Should the bot track dialogs and send them to the relays setup?")]
        [ReturnHints("updated")]
        [ReturnHintsFailure("bad status")]
        [ArgHints("status", "true or false")]
        public object DialogTrack(string status)
        {
            if (bool.TryParse(status, out bool statuscode) == false)
            {
                return Failure("bad status", new [] { status });
            }
            return Failure("@todo dialog relay");
        }

    }
}
