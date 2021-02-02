using BSB.Static;
using OpenMetaverse;

namespace BSB.Commands.Inventory
{
    public class RenameInventory : CoreCommand_3arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "UUID", "Text" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply target: Channel,Avatar,HTTP", "Inventory UUID", "New name" }; } }
        public override string Helpfile
        {
            get
            {
                return "Renames an inventory item to something else.";
            }
        }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[1], out UUID target) == true)
                {
                    if (args[2].Length >= 1)
                    {
                        InventoryItem item = HelperInventory.getItemByInventoryUUID(bot, target);
                        if (item != null)
                        {
                            item.Name = args[2];
                            bot.GetClient.Inventory.RequestUpdateItem(item);
                            return true;
                        }
                        return Failed("Unable to get base item");
                    }
                    return Failed("Item name is to short");
                }
                return Failed("Unable to get UUID from target");
            }
            return false;
        }
    }
}
