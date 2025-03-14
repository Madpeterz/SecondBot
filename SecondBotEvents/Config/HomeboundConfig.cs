﻿using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class HomeboundConfig(bool fromENV, string fromFolder = "") : Config(fromENV, fromFolder)
    {
        protected override void MakeSettings()
        {
            filename = "homebound";
            settings.Add("Enabled");
            settings.Add("HomeSimSlUrl");
            settings.Add("BackupSimSLUrl");
            settings.Add("ReturnToHomeSimAfterMins");
            settings.Add("AtHomeSeekLocation");
            settings.Add("AtBackupSeekLocation");
            settings.Add("AtHomeAutoSitUuid");
            settings.Add("HideStatusOutput");
        }

        public string GetAtHomeAutoSitUuid()
        {
            return ReadSettingAsString("AtHomeAutoSitUuid", UUID.Zero.ToString());
        }

        public int GetReturnToHomeSimAfterMins()
        {
            return ReadSettingAsInt("ReturnToHomeSimAfterMins", 1);
        }

        public bool GetAtHomeSeekLocation()
        {
            return ReadSettingAsBool("AtHomeSeekLocation", true);
        }

        public bool GetAtBackupSeekLocation()
        {
            return ReadSettingAsBool("AtBackupSeekLocation", false);
        }

        public string GetBackupSimSLUrl()
        {
            return ReadSettingAsString("BackupSimSLUrl", "Sandbox%20Decorus/128/128/27");
        }

        public string GetHomeSimSlUrl()
        {
            return ReadSettingAsString("HomeSimSlUrl", "Viserion/46/163/23");
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", false);
        }
    }

}