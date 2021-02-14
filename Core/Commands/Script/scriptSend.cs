using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BetterSecondBot.Commands.Script
{
    class ScriptSend : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar","Text","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]", "Collection","Script name" }; } }
        public override string Helpfile { get { return "Creats and send a Script to [ARG 1]<br/>" +
                    " using the collection [ARG 2] as the content <br/>" +
                    " Optional: You can name the Script using [ARG 3]"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                // target UUID,Script storage id,["Script title" <unixtime> or defaults to: New Script for <UUID> <unixtime>]
                if (UUID.TryParse(args[0], out UUID target) == true)
                {
                    string content = bot.GetScriptContent(args[1]);
                    if (content != null)
                    {
                        bot.ClearScriptStorage(args[1]);
                        string Script_name = "New Script for " + args[0] + " " + helpers.UnixTimeNow().ToString() + "";
                        if (args.Length == 3)
                        {
                            Script_name = args[2] + " " + helpers.UnixTimeNow().ToString() + "";
                        }
                        return bot.SendScript(Script_name, content, target);
                    }
                    else
                    {
                        return Failed("No content in Script storage " + args[1]);
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
