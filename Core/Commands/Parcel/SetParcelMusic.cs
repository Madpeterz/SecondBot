namespace BSB.Commands.CMD_Parcel
{
    class SetParcelMusic : ParcelCommand_RequirePerms
    {
        public override string[] ArgTypes { get { return new[] { "String" }; } }
        public override string[] ArgHints { get { return new[] { "A vaild url" }; } }
        public override string Helpfile { get { return "Updates the current parcels music url"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (args.Length == 0)
                {
                    return parcel_static.set_parcel_music(bot,targetparcel,"");
                }
                else
                {
                    if (args[0].StartsWith("http") == true)
                    {
                        return parcel_static.set_parcel_music(bot, targetparcel, args[0]);
                    }
                    else
                    {
                        return Failed("Invaild url");
                    }
                }
            }
            return false;
        }
    }
}
