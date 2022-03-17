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
            settings.Add("allowFundsCommands");
            settings.Add("allowIMcontrol");
            settings.Add("sharedSecret");
            settings.Add("onlySelectedAvs");
            settings.Add("avsCSV");
            settings.Add("enforceTimeWindow");
            settings.Add("timeWindowSecs");
        }

        public bool GetEnforceTimeWindow()
        {
            return ReadSettingAsBool("enforceTimeWindow", false);
        }

        public int GetTimeWindowSecs()
        {
            return ReadSettingAsInt("timeWindowSecs", 35);
        }

        public bool GetOnlySelectedAvs()
        {
            return ReadSettingAsBool("onlySelectedAvs", false);
        }

        public List<string> GetAvsCSV()
        {
            return ReadSettingAsString("avsCSV", "Madpeter Zond,Madpeter Zond,Madpeter Zond").Split(",").ToList();
        }

        public bool GetAllowFundsCommands()
        {
            return ReadSettingAsBool("allowFundsCommands", false);
        }

        public bool GetAllowIMcontrol()
        {
            return ReadSettingAsBool("allowIMcontrol", false);
        }

        public string GetSharedSecret()
        {
            return ReadSettingAsString("sharedSecret", "ThisIsMySecret");
        }
    }
}
