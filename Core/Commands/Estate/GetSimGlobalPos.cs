using OpenMetaverse;

namespace BetterSecondBot.Commands.Estate
{
    public class GetSimGlobalPos : CoreCommand_SmartReply_2arg
    {
        public override string Helpfile { get { return "Fetchs a region and returns its global X,Y"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.GetClient.Grid.GetGridRegion(args[1], GridLayerType.Objects, out GridRegion region))
                {
                    collection.Add("X", region.X.ToString());
                    collection.Add("Y", region.Y.ToString());
                    return bot.GetCommandsInterface.SmartCommandReply(true, args[0], "ok", CommandName,collection);
                }
                else
                {
                    return Failed("Unable to find region");
                }
            }
            return false;
        }
    }
}
