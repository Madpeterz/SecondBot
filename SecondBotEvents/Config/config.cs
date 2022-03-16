using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SecondBotEvents.Config
{
    public abstract class config
    {
        protected Dictionary<string, string> mysettings = new Dictionary<string, string>();
        protected List<string> settings = new List<string>();
        protected string filename = "config";
        public config(bool fromENV, string fromFolder="")
        {
            makeSettings();
            loadSettings(fromENV, fromFolder);
        }

        protected void loadSettings(bool fromENV, string fromFolder = "")
        {
            if(fromENV == true)
            {
                loadSettingsFromEnv();
                return;
            }
            loadSettingsFromFile(fromFolder);
        }

        protected void loadSettingsFromFile(string fromFolder)
        {
            SimpleIO IO = new SimpleIO();
            IO.ChangeRoot(fromFolder);
            string readfile = filename + ".json";
            if (IO.Exists(readfile) == false)
            {
                return;
            }
            JToken result = JsonConvert.DeserializeObject<JToken>(IO.ReadFile(readfile));
            foreach (string setting in settings)
            {
                string value = Environment.GetEnvironmentVariable(key);
                if (SecondbotHelpers.notempty(value) == false)
                {
                    mysettings.Add(setting, "");
                    continue;
                }
                mysettings.Add(setting, value);
            }
        }

        protected void loadSettingsFromEnv()
        {
            foreach (string setting in settings)
            {
                string key = filename + "_" + setting;
                string value = Environment.GetEnvironmentVariable(key);
                if (SecondbotHelpers.notempty(value) == false)
                {
                    mysettings.Add(setting, "");
                    continue;
                }
                mysettings.Add(setting, value);
            }
        }

        protected virtual void makeSettings()
        {

        }

        protected string readSetting(string key, string defaultValue)
        {
            if (mysettings.ContainsKey(key) == false)
            {
                return defaultValue;
            }
            return mysettings[key];
        }
    }
}
