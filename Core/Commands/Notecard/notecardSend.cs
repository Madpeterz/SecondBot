using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.Notecard
{
    class NotecardSend : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar","Text","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]", "Collection","Notecard name" }; } }
        public override string Helpfile { get { return "Creats and send a notecard to [ARG 1]<br/>" +
                    " using the collection [ARG 2] as the content <br/>" +
                    " Optional: You can name the notecard using [ARG 3]"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                // target UUID,notecard storage id,["notecard title" <unixtime> or defaults to: New notecard for <UUID> <unixtime>]
                if (UUID.TryParse(args[0], out UUID target) == true)
                {
                    string content = bot.GetNotecardContent(args[1]);
                    if (content != null)
                    {
                        bot.ClearNotecardStorage(args[1]);
                        string notecard_name = "New notecard for " + args[0] + " " + helpers.UnixTimeNow().ToString() + "";
                        if (args.Length == 3)
                        {
                            notecard_name = args[2] + " " + helpers.UnixTimeNow().ToString() + "";
                        }
                        return bot.SendNotecard(notecard_name, content, target);
                    }
                    else
                    {
                        return Failed("No content in notecard storage " + args[1]);
                    }
                }
                else
                {
                    return Failed("Arg 0 not a UUID");
                }
            }
            return false;
        }
    }
}
