using OpenMetaverse;

namespace BSB.Commands.Movement
{
    public class Teleport : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Mixed" }; } }
        public override string[] ArgHints { get { return new[] { "SLurl<br/>Region<br/>X/Y/Z<br/>Region/X/Y/Z" }; } }
        public override string Helpfile { get { return "Teleports the avatar<br/> " +
                    "SLurl: http://maps.secondlife.com/secondlife/Bombalai/216/51/33 <br/> " +
                    "Region: Bombalai [Defaults to 128,128,0]<br>" +
                    "X/Y/Z: Stays in the sim just changes location <br/> " +
                    "Region/X/Y/Z: Teleports to the given region and trys to goto X/Y/Z if no teleport hub (or has access)"; 
            } }

        public override bool CallFunction(string[] args)
        {
            if(base.CallFunction(args) == true)
            {
                bot.GetClient.Self.AutoPilotCancel();
                if (args[0].Contains("http://maps.secondlife.com/secondlife/") == true)
                {
                    bot.TeleportWithSLurl(args[0]);
                    return true;
                }
                else
                {
                    float posX = 128;
                    float posY = 128;
                    float posZ = 0;
                    string regionName = bot.GetClient.Network.CurrentSim.Name;
                    bool ok = true;
                    int offset = 0;
                    string[] tp_args = args[0].Split('/');
                    if ((tp_args.Length == 4) || (tp_args.Length == 1))
                    {
                        regionName = tp_args[0];
                        offset = 1;
                    }
                    if (tp_args.Length >= 3)
                    {
                        float.TryParse(tp_args[offset + 0], out posX);
                        float.TryParse(tp_args[offset + 1], out posY);
                        float.TryParse(tp_args[offset + 2], out posZ);
                    }
                    else if (tp_args.Length == 2)
                    {
                        ok = false;
                    }
                    if (ok == true)
                    {
                        bot.SetTeleported();
                        bool status = bot.GetClient.Self.Teleport(regionName, new Vector3(posX, posY, posZ));
                        bot.ResetAnimations();
                        return status;
                    }
                    else
                    {
                        return Failed("Required args [Region or x/y/z or region/x/y/z not sent]");
                    }
                }
            }
            return false;
        }
    }
}
