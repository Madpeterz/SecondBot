using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BetterSecondBot.Commands.Self
{
    public class PayObject : CoreCommand_3arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID","Number","Text" }; } }
        public override string[] ArgHints { get { return new[] { "Object UUID","Amount to pay","Object Name" }; } }
        public override string Helpfile { get { return "Makes the bot pay a object"; } }
        public override bool CallFunction(string[] args)
        {

            if (base.CallFunction(args) == true)
            {
                if (bot.GetAllowFunds == true)
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
                return Failed("Transfer funds to objects disabled");
            }
            return false;
        }
    }
}
