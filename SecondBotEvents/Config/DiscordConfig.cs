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
            settings.Add("Enabled");
            settings.Add("ServerID");
            settings.Add("ClientToken");
            settings.Add("AllowDiscordCommands");
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled");
        }

        public ulong GetServerID()
        {
            return ReadSettingAsUlong("ServerID", 0);
        }

        public string GetClientToken()
        {
            return ReadSettingAsString("ClientToken","tokenPlzKThanks");
        }

        public bool GetAllowDiscordCommands()
        {
            return ReadSettingAsBool("AllowDiscordCommands");
        }
    }
}