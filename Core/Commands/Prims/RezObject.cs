using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.Commands.Prims
{
    class RezObject : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID", "Vector" }; } }
        public override string[] ArgHints { get { return new[] { "The object to rez", "The location to rez at: Example \"<123,32,18>\"" }; } }
        public override string Helpfile { get { return "Rez a object [ARG 1] from inventory at [ARG 2] (Or current pos if not given)"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID targetitem) == true)
                {
                    InventoryItem itm = bot.GetClient.Inventory.FetchItem(targetitem, bot.GetClient.Self.AgentID, (5 * 1000));
                    if (itm != null)
                    {
                        Vector3 pos = bot.GetClient.Self.RelativePosition;
                        if (args.Length == 2)
                        {
                            if(Vector3.TryParse(args[1], out pos) == false)
                            {
                                return Failed("Unable to process pos");
                            }
                        }
                        bot.GetClient.Inventory.RequestRezFromInventory(bot.GetClient.Network.CurrentSim, bot.GetClient.Self.SimRotation, pos, itm);
                        return true;
                    }
                    else
                    {
                        return Failed("Unable to find item");
                    }
                }
                else
                {
                    return Failed("Invaild item UUID");
                }
            }
            return false;
        }
    }
}
