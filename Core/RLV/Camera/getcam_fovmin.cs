﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.RLV.Camera
{
    public class GetCam_FovMin : RLV_UUID_flag_get
    {
        public override string LookupFlag { get { return "camfovmin"; } }
    }
}
