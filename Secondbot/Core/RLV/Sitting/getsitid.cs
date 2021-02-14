using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.RLV.Sitting
{
    class GetSitId : RLV_command_1arg
    {
        public override bool CallFunction(string[] args)
        {
            _ = int.TryParse(args[0], out int channel);
            if (channel > -1)
            {
                if(bot.GetClient.Self.SittingOn > 0)
                {
                    if(bot.GetClient.Network.CurrentSim.ObjectsPrimitives.ContainsKey(bot.GetClient.Self.SittingOn))
                    {
                        Primitive obj = bot.GetClient.Network.CurrentSim.ObjectsPrimitives[bot.GetClient.Self.SittingOn];
                        
                        bot.GetClient.Self.Chat(obj.ID.ToString(), channel, OpenMetaverse.ChatType.Normal);
                        return true;
                    }                    
                }
                bot.GetClient.Self.Chat(UUID.Zero.ToString(), channel, OpenMetaverse.ChatType.Normal);
                return true;
            }
            return Failed("Invaild channel given");
        }
    }
}
