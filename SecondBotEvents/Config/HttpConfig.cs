using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class HttpConfig(bool fromENV, string fromFolder = "") : Config(fromENV, fromFolder)
    {
        protected override void MakeSettings()
        {
            filename = "http";
            settings.Add("Enabled");
            settings.Add("HideStatusOutput");
            settings.Add("DisableCommandValidation");
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", true);
        }

        public bool GetDisableCommandValidation()
        {
            return ReadSettingAsBool("DisableCommandValidation", false);
        }
    }

}