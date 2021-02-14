using System;
using System.Collections.Generic;
using System.Linq;
using OpenMetaverse;

namespace BetterSecondBot.Commands.Inventory
{
    public class DeleteInventoryItem : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "inventory UUID" }; } }
        public override string Helpfile { get { return "Attempts to Remove the given inventory item"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID removeme) == true)
                {
                    bot.GetClient.Inventory.Remove(new List<UUID>() { removeme }, new List<UUID>());
                    return true;
                }
                return Failed("Invaild inventory UUID");
            }
            return false;
        }
    }

    public class DeleteInventoryFolder : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] {  "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "inventory UUID" }; } }
        public override string Helpfile { get { return "Attempts to Remove the given inventory folder"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID removeme) == true)
                {
                    bot.GetClient.Inventory.Remove(new List<UUID>(), new List<UUID>() { removeme });
                    return true;
                }
                return Failed("Invaild inventory UUID");
            }
            return false;
        }
    }
}
