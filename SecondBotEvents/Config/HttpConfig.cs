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
            settings.Add("enabled");
            settings.Add("host");
        }

        public string GetHost()
        {
            return ReadSettingAsString("host", "http://*:80");
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("enabled", false);
        }
    }

}