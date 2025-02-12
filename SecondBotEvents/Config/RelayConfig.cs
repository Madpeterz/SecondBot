using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SecondBotEvents.Config
{
    public class RelayConfig(bool fromENV, string fromFolder = "") : Config(fromENV, fromFolder)
    {
        protected override void MakeSettings()
        {
            filename = "relay";
            settings.Add("count");
            settings.Add("HideStatusOutput");
            settings.Add("UseShortEncoder");
        }

        public int GetRelayCount()
        {
            return ReadSettingAsInt("count", 0);
        }

        public bool GetUseShortEncoder()
        {
            return ReadSettingAsBool("UseShortEncoder", false);
        }

        public bool GetRelayEnabled(int relayID)
        {
            if (GetUseShortEncoder() == false)
            {
                return ReadSettingAsBool(relayID.ToString() + "_enabled", false);
            }
            return true;
        }

        public string RelaySourceType(int relayID)
        {
            if (GetUseShortEncoder() == false)
            {
                return ReadSettingAsString(relayID.ToString() + "_source_type", "none");
            }
            Dictionary<string,string> settings = GetShortEncodedOptions(relayID);
            if (settings.ContainsKey("sType") == false)
            {
                return "none";
            }
            return settings["sType"];
        }

        public string RelaySourceOption(int relayID)
        {
            if (GetUseShortEncoder() == false)
            {
                return ReadSettingAsString(relayID.ToString() + "_source_option", "");
            }
            Dictionary<string, string> settings = GetShortEncodedOptions(relayID);
            if (settings.ContainsKey("sFilter") == false)
            {
                return "";
            }
            return settings["sFilter"];
        }

        public string RelayTargetType(int relayID)
        {
            if (GetUseShortEncoder() == false)
            {
                return ReadSettingAsString(relayID.ToString() + "_target_type", "none");
            }
            Dictionary<string, string> settings = GetShortEncodedOptions(relayID);
            if (settings.ContainsKey("tType") == false)
            {
                return "none";
            }
            return settings["tType"];
        }

        public string RelayTargetOption(int relayID)
        {
            if (GetUseShortEncoder() == false)
            {
                return ReadSettingAsString(relayID.ToString() + "_target_option", "");
            }
            Dictionary<string, string> settings = GetShortEncodedOptions(relayID);
            if (settings.ContainsKey("tConfig") == false)
            {
                return "none";
            }
            return settings["tConfig"];
        }

        protected Dictionary<string,string> GetShortEncodedOptions(int relayID)
        {
            if(UnpackedConfigs.ContainsKey(relayID) == true)
            {
                return UnpackedConfigs[relayID];
            }
            // config not unpacked from string lets unpack it
            string config = ReadSettingAsString(relayID.ToString() + "_RelayConfig", "");
            UnpackedConfigs.Add(relayID, []);
            if (config == "")
            {
                return UnpackedConfigs[relayID];
            }
            string[] bits = config.Split("{}", StringSplitOptions.RemoveEmptyEntries);
            Dictionary<string,string> data = [];
            foreach(string A in bits)
            {
                string[] B = A.Split("::", StringSplitOptions.RemoveEmptyEntries);
                if (B.Count() == 2)
                {
                    if (data.ContainsKey(B[0]) == false)
                    {
                        data.Add(B[0], B[1]);
                    }
                }
            }
            if(data.Count() != 4)
            {
                return UnpackedConfigs[relayID];
            }
            List<string> checks = ["sType", "sFilter", "tType", "tConfig"];
            bool allfound = true;
            foreach (string A in checks)
            {
                if (data.ContainsKey(A) == false)
                {
                    allfound = false;
                    break;
                }
            }
            if(allfound == true)
            {
                UnpackedConfigs[relayID] = data;
            }
            return UnpackedConfigs[relayID];
        }

        protected Dictionary<int, Dictionary<string, string>> UnpackedConfigs = [];

    }

}