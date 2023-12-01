using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class HttpConfig: Config
    {
        public HttpConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "http";
            settings.Add("Enabled");
            settings.Add("Port");
            settings.Add("HideStatusOutput");
        }

        public int GetPort()
        {
            return ReadSettingAsInt("Port", 80);
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", false);
        }
    }

}