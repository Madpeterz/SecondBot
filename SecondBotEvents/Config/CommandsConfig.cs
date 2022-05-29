using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SecondBotEvents.Config
{
    public class CommandsConfig : Config
    {
        public CommandsConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "commands";
            settings.Add("AllowFundsCommands");
            settings.Add("AllowIMcontrol");
            settings.Add("SharedSecret");
            settings.Add("OnlyMasterAvs");
            settings.Add("MastersCSV");
            settings.Add("EnforceTimeWindow");
            settings.Add("TimeWindowSecs");
            settings.Add("Enabled");
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", true);
        }

        public string[] GetMastersCSV()
        {
            return ReadSettingAsString("MastersCSV", "Madpeter Zond").Split(",");
        }

        public bool GetEnforceTimeWindow()
        {
            return ReadSettingAsBool("EnforceTimeWindow", false);
        }

        public int GetTimeWindowSecs()
        {
            return ReadSettingAsInt("TimeWindowSecs", 35);
        }

        public bool GetOnlyMasterAvs()
        {
            return ReadSettingAsBool("OnlyMasterAvs", false);
        }


        public bool GetAllowFundsCommands()
        {
            return ReadSettingAsBool("AllowFundsCommands", false);
        }

        public bool GetAllowIMcontrol()
        {
            return ReadSettingAsBool("AllowIMcontrol", false);
        }

        public string GetSharedSecret()
        {
            return ReadSettingAsString("SharedSecret", "ThisIsMySecret");
        }
    }
}
