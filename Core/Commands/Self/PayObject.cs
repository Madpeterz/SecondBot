using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Self
{
    public class PayObject : CoreCommand
    {
        public override string[] ArgTypes { get { return new[] { "UUID","Number","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Object UUID","Amount to pay","Object Name" }; } }
        public override string Helpfile { get { return "Makes the bot pay a object"; } }
        public override int MinArgs { get { return 3; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID objectuuid) == true)
                {
                    if (int.TryParse(args[1], out int amount) == true)
                    {
                        if (amount > 0)
                        {
                            if (args[2].Length > 0)
                            {
                                bot.GetClient.Self.GiveObjectMoney(objectuuid, amount, args[2]);
                                return true;
                            }
                            return Failed("Invaild object name");
                        }
                    }
                    return Failed("Invaild amount");
                }
                return Failed("Invaild object UUID");
            }
            return false;
        }
    }
}
