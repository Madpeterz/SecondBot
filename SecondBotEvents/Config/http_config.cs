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
            settings.Add("port");
        }

        public int getPort()
        {
            bool result = int.TryParse(readSetting("port", "8080"), out int port);
            if(result == false)
            {
                return 8080;
            }
            return port;
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