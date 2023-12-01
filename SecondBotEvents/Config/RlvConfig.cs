using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class RlvConfig : Config
    {
        public RlvConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "rlv";
            settings.Add("Enabled");
            settings.Add("AllowRlvViaIm");
            settings.Add("AllowRlvViaObjectOwnerSay");
            settings.Add("HideStatusOutput");
        }
        public bool GetAllowRlvViaIm()
        {
            return ReadSettingAsBool("AllowRlvViaIm", false);
        }
        public bool GetAllowRlvViaObjectOwnerSay()
        {
            return ReadSettingAsBool("AllowRlvViaObjectOwnerSay", false);
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", false);
        }
    }
}