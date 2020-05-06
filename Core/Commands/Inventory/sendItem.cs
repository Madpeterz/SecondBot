using OpenMetaverse;

namespace BSB.Commands.Inventory
{
    public class SendItem : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Avatar", "UUID", "Number" }; } }
        public override string[] ArgHints { get { return new[] { "Avatar [UUID or Firstname Lastname]", "Item","Time to find item" }; } }
        public override string Helpfile { get { return "Sends a item [ARG 2] to an avatar [ARG 1]. (Optional arg 3: How long to wait to find the item in secs, Defaults to 6)"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                int search_delay_time = 6;
                if(args.Length == 3)
                {
                    int.TryParse(args[2], out search_delay_time);
                }
                if (UUID.TryParse(args[0], out UUID targetavatar) == true)
                {
                    if (UUID.TryParse(args[1], out UUID targetitem) == true)
                    {
                        InventoryItem itm = bot.GetClient.Inventory.FetchItem(targetitem, bot.GetClient.Self.AgentID, (search_delay_time * 1000));
                        if (itm != null)
                        {
                            bot.GetClient.Inventory.GiveItem(itm.AssetUUID, itm.Name, itm.AssetType, targetavatar, false);
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
                else
                {
                    return Failed("Invaild avatar UUID");
                }
            }
            return false;
        }
    }
}
