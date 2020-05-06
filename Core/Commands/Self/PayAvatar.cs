using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;


namespace BSB.Commands.Self
{
    public class PayAvatar : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar", "Number" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]", "Amount to pay"}; } }
        public override string Helpfile { get { return "Makes the bot pay a avatar"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.GetAllowFunds == true)
                {
                    if (UUID.TryParse(args[0], out UUID avataruuid) == true)
                    {
                        if (int.TryParse(args[1], out int amount) == true)
                        {
                            if (amount > 0)
                            {
                                bot.GetClient.Self.GiveAvatarMoney(avataruuid, amount);
                                return true;
                            }
                        }
                        return Failed("Invaild amount");
                    }
                    return Failed("Invaild avatar UUID");
                }
                return Failed("Transfer funds to avatars disabled");
            }
            return false;
        }
    }
}
