using OpenMetaverse;

namespace BetterSecondBot.Commands.CMD_Parcel
{
    public class ParcelFreeze : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar", "True|False" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]", "Status (defaults to True)" }; } }
        public override string Helpfile { get { return "Freezes an avatar [ARG 1]<br/>Set to False to unfreeze"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID target) == true)
                {
                    bool freezethem = true;
                    if (args.Length == 2)
                    {
                        if (args[1] == "False")
                        {
                            freezethem = false;
                        }
                    }
                    bot.GetClient.Parcels.FreezeUser(target, freezethem);
                    return true;
                }
                else
                {
                    return Failed("Arg 0 requires UUID");
                }
            }
            return false;
        }
    }
}
