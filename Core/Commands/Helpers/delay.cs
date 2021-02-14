using System.Text;
using System.Threading;
using OpenMetaverse;

namespace BetterSecondBot.Commands.Helpers
{
    public class Delay : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Number" }; } }
        public override string[] ArgHints { get { return new[] { "Delay in MS" }; } }
        public override string Helpfile { get { return "Delays a thead by X ms<br/>Mostly pointless but good if your doing custom commands"; } }
        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (int.TryParse(args[0], out int delayms) == true)
                {
                    Thread.Sleep(delayms);
                    return true;
                }
                else
                {
                    return Failed("Delay is not vaild");
                }
            }
            return false;
        }
    }
}
