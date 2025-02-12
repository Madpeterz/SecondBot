using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace SecondBotEvents.Config
{
    public class CommandsConfig(bool fromENV, string fromFolder = "") : Config(fromENV, fromFolder)
    {
        protected override void MakeSettings()
        {
            filename = "commands";
            settings.Add("AllowFundsCommands");
            settings.Add("AllowIMcontrol");
            settings.Add("SharedSecret");
            settings.Add("EnableMasterControls");
            settings.Add("MastersCSV");
            settings.Add("CheckDotNames");
            settings.Add("EnforceTimeWindow");
            settings.Add("TimeWindowSecs");
            settings.Add("Enabled");
            settings.Add("ObjectMasterOptout");
            settings.Add("AllowServiceControl");
            settings.Add("CommandHistoryLogResults");
            settings.Add("HideStatusOutput");
        }


        public bool GetCommandHistoryLogResults()
        {
            return ReadSettingAsBool("CommandHistoryLogResults", false);
        }
        public bool GetAllowServiceControl()
        {
            return ReadSettingAsBool("AllowServiceControl", false);
        }
        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", true);
        }

        public bool GetCheckDotNames()
        {
            return ReadSettingAsBool("CheckDotNames", false);
        }

        public bool GetObjectMasterOptout()
        {
            return ReadSettingAsBool("ObjectMasterOptout", false);
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

        public bool GetEnableMasterControls()
        {
            return ReadSettingAsBool("EnableMasterControls", true);
        }


        public bool GetAllowFundsCommands()
        {
            return ReadSettingAsBool("AllowFundsCommands", false);
        }

        public bool GetAllowIMcontrol()
        {
            return ReadSettingAsBool("AllowIMcontrol", true);
        }

        public string GetSharedSecret()
        {
            return ReadSettingAsString("SharedSecret", "ThisIsMySecret");
        }
    }
}
