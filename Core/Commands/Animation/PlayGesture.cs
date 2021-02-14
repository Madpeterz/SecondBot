using OpenMetaverse;

namespace BetterSecondBot.Commands.Animation
{
    public class PlayGesture : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "The UUID of the gesture to play" }; } }
        public override string Helpfile { get { return "Triggers a gesture [once]"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (UUID.TryParse(args[0], out UUID gesture) == true)
                {
                    InventoryItem itm = bot.GetClient.Inventory.FetchItem(gesture, bot.GetClient.Self.AgentID, (3 * 1000));
                    bot.GetClient.Self.PlayGesture(itm.AssetUUID);
                    return true;
                }
                return Failed("Invaild gesture UUID for arg 1");
            }
            return false;
        }
    }
}
