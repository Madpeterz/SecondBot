using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class http_config: config
    {
        public http_config(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void makeSettings()
        {
            filename = "http";
            settings.Add("enabled");
            settings.Add("host");
        }

        public string getHost()
        {
            return readSetting("host", "http://*:80");
        }

        public bool getEnabled()
        {
            bool result = bool.TryParse(readSetting("enabled", "false"), out bool enabled);
            if (result == false)
            {
                return false;
            }
            return enabled;
        }
    }

}