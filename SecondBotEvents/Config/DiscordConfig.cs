using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class DiscordConfig : Config
    {
        public DiscordConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "discord";
            settings.Add("enabled");
            settings.Add("serverID");
            settings.Add("clientToken");
            settings.Add("allowDiscordCommands");
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("enabled");
        }

        public ulong GetServerID()
        {
            return ReadSettingAsUlong("serverID", 0);
        }

        public string GetClientToken()
        {
            return ReadSettingAsString("clientToken","tokenPlzKThanks");
        }

        public bool GetAllowDiscordCommands()
        {
            return ReadSettingAsBool("allowDiscordCommands");
        }
    }
}