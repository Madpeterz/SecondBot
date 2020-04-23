using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.RLV.Movement
{
    public class AdjustHeight : RLV_command
    {
        public override int MinArgs { get { return 3; } }
        public override bool CallFunction(string[] args)
        {
            int distance_pelvis_to_foot_in_meters = 0;
            double factor = 0;
            double offset = 0;
            if (args[^1] == "force")
            {
                if(args.Length == 3)
                {
                    int.TryParse(args[0], out distance_pelvis_to_foot_in_meters);
                    double.TryParse(args[1], out factor);
                }
                else if (args.Length == 4)
                {
                    int.TryParse(args[0], out distance_pelvis_to_foot_in_meters);
                    double.TryParse(args[1], out factor);
                    double.TryParse(args[2], out offset);
                }
                Double hh = distance_pelvis_to_foot_in_meters * factor;
                hh += offset;
                if (hh > 2.0)
                {
                    hh = 2.0;
                }
                else if (hh < -2.0)
                {
                    hh = -2.0;
                }
                bot.GetClient.Self.SetHoverHeight(hh);
                return true;
            }
            return Failed("Required magic word force missing");
        }
        
    }
}
