﻿using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot.RLV.Camera
{
    public class GetCam_FovMax : RLV_UUID_flag_get
    {
        public override string LookupFlag { get { return "camfovmax"; } }

    }
}
