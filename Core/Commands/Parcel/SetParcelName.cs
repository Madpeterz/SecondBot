namespace BetterSecondBot.Commands.CMD_Parcel
{
    public class SetParcelName : ParcelCommand_RequirePerms_1arg
    {
        public override string[] ArgTypes { get { return new[] { "String" }; } }
        public override string[] ArgHints { get { return new[] { "The new name" }; } }
        public override string Helpfile { get { return "Updates the current parcels name"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                targetparcel.Name = args[0];
                targetparcel.Update(bot.GetClient.Network.CurrentSim, false);
                return true;
            }
            return false;
        }
    }
}
