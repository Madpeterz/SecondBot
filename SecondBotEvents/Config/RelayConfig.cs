﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class RelayConfig : Config
    {
        public RelayConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }

        protected override void MakeSettings()
        {
            filename = "relay";
            settings.Add("count");
            settings.Add("HideStatusOutput");
        }

        public int GetRelayCount()
        {
            return ReadSettingAsInt("count", 0);
        }

        public bool GetRelayEnabled(int relayID)
        {
            return ReadSettingAsBool(relayID.ToString() + "_enabled", false);
        }

        public string RelaySourceType(int relayID)
        {
            return ReadSettingAsString(relayID.ToString() + "_source_type", "none");
        }

        public string RelaySourceOption(int relayID)
        {
            return ReadSettingAsString(relayID.ToString() + "_source_option", "");
        }

        public string RelayTargetType(int relayID)
        {
            return ReadSettingAsString(relayID.ToString() + "_target_type", "none");
        }

        public string RelayTargetOption(int relayID)
        {
            return ReadSettingAsString(relayID.ToString() + "_target_option", "");
        }


    }

}