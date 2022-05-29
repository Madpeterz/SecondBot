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
            settings.Add("Host");
        }

        public string GetHost()
        {
            return ReadSettingAsString("Host", "http://*:80");
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", false);
        }
    }

}