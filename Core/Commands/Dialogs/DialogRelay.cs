using OpenMetaverse;

namespace BetterSecondBot.Commands.Dialogs
{
    class DialogRelay : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart" }; } }
        public override string[] ArgHints { get { return new[] { "Channel (Any number),Avatar UUID,Avatar name,HTTPurl<br/>Clear" }; } }
        public override string Helpfile { get { return "Updates the relay target (you can have 1 of each type)<br/>Clear will disable them all"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if(args[0] != "Clear")
                {
                    if (UUID.TryParse(args[0], out UUID avatar) == true)
                    {
                        bot.SetRelayDialogsAvatar(avatar);
                    }
                    else if (args[0].StartsWith("http") == true)
                    {
                        bot.SetRelayDialogsHTTP(args[0]);
                    }
                    else if (int.TryParse(args[0], out int channel) == true)
                    {
                        bot.SetRelayDialogsChannel(channel);
                    }
                    else
                    {
                        return Failed("Not a vaild option");
                    }
                    return true;
                }
                else
                {
                    bot.SetRelayDialogsAvatar(UUID.Zero);
                    bot.SetRelayDialogsChannel(0);
                    bot.SetRelayDialogsHTTP("");
                    return true;
                }
            }
            return false;
        }
    }

}
