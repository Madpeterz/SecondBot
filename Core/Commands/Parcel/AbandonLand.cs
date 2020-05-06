namespace BSB.Commands.CMD_Parcel
{
    public class AbandonLand : ParcelCommand_RequirePerms
    {
        public override string Helpfile { get { return "Abandons the parcel the bot is currently on, returning it to Linden's or Estate owner"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                bot.GetClient.Parcels.ReleaseParcel(bot.GetClient.Network.CurrentSim, targetparcel.LocalID);
                return true;
            }
            return false;
        }
    }
}
