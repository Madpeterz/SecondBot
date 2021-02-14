using BSB.Static;
using OpenMetaverse;

namespace BSB.Commands.Inventory
{
    public class getRealUUID : CoreCommand_2arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart", "UUID" }; } }
        public override string[] ArgHints { get { return new[] { "Smart reply target: Channel,Avatar,HTTP", "Inventory UUID" }; } }
        public override string Helpfile
        {
            get
            {
                return "Converts a Inventory UUID into an asset UUID used to send? im not sure yet";
            }
        }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if(UUID.TryParse(args[1],out UUID target) == true)
                {
                    UUID reply = HelperInventory.GetAssetUUID(bot, target);
                    if(reply != UUID.Zero)
                    {
                        return bot.GetCommandsInterface.SmartCommandReply(true, args[0], reply.ToString(), CommandName);
                    }
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "Unable to fetch asset UUID", CommandName);
                }
                return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "Unable to process target UUID", CommandName);
            }
            return false;
        }
    }
}
