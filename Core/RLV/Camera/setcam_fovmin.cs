using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.RLV.Camera
{
    public class SetCam_FovMin : RLV_UUID_flag_arg_yn
    {
        public override string SetFlagName { get { return "camfovmin"; } }
    }
}
