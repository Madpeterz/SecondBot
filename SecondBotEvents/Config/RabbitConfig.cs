using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class RabbitConfig(bool fromENV, string fromFolder = "") : Config(fromENV, fromFolder)
    {
        protected override void MakeSettings()
        {
            filename = "rabbit";
            settings.Add("Enabled");
            settings.Add("HideStatusOutput");
            settings.Add("HostIP");
            settings.Add("HostPort");
            settings.Add("HostUsername");
            settings.Add("HostPassword");
            settings.Add("NotecardQueue");
            settings.Add("CommandQueue");
            settings.Add("ImQueue");
            settings.Add("GroupImQueue");
            settings.Add("LogDebug");
        }

        public bool GetLogDebug()
        {
            return ReadSettingAsBool("LogDebug", true);
        }
        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", false);
        }

        public string GetHostIP()
        {
            return ReadSettingAsString("HostIP", "127.0.0.1");
        }

        public string GetHostUsername()
        {
            return ReadSettingAsString("HostUsername", "guest");
        }

        public string GetHostPassword()
        {
            return ReadSettingAsString("HostPassword", "guest");
        }

        public int GetHostPort()
        {
            return ReadSettingAsInt("HostPort", 5672);
        }
        public string GetNotecardQueue()
        {
            return ReadSettingAsString("NotecardQueue", "notecards");
        }
        public string GetCommandQueue()
        {
            return ReadSettingAsString("CommandQueue", "commands");
        }
        public string GetImQueue()
        {
            return ReadSettingAsString("ImQueue", "ims");
        }
        public string GetGroupImQueue()
        {
            return ReadSettingAsString("GroupImQueue", "groupims");
        }
    }

}