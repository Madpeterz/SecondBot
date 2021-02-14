using OpenMetaverse;

namespace BSB.Commands.Estate
{
    public class GetSimTexture : CoreCommand_SmartReply_2arg
    {
        public override string Helpfile { get { return "Fetchs a region and returns its tile texture UUID - Expect this to die at some point!"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.GetClient.Grid.GetGridRegion(args[1], GridLayerType.Objects, out GridRegion region))
                {
                    return bot.GetCommandsInterface.SmartCommandReply(true, args[0], region.MapImageID.ToString(),CommandName);
                }
                else
                {
                    return bot.GetCommandsInterface.SmartCommandReply(false, args[0], "Unable to find region", CommandName);
                }
            }
            return false;
        }
    }
}
