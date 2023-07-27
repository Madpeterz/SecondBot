using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace SecondBotEvents.Config
{
    public abstract class Config
    {
        protected Dictionary<string, string> mysettings = new Dictionary<string, string>();
        protected List<string> settings = new List<string>();
        protected string filename = "config";
        protected bool loaded = false;
        protected bool tryLoadfromEnv = false;

        public Config(bool fromENV, string fromFolder="")
        {
            MakeSettings();
            LoadSettings(fromENV, fromFolder);
        }
        public bool IsLoaded()
        {
            return loaded;
        }

        protected void LoadSettings(bool fromENV, string fromFolder = "")
        {
            if(fromENV == true)
            {
                tryLoadfromEnv = true;
                LoadSettingsFromEnv();
                return;
            }
            LoadSettingsFromFile(fromFolder);
        }

        protected void MakeSettingsFile(string fromFolder)
        {
            Dictionary<string, string> saveMe = new Dictionary<string, string>();
            Type thisType = this.GetType();
            foreach (string setting in settings)
            {
                string reader = "Get"+ StringExtensions.FirstCharToUpper(setting);
                MethodInfo theMethod = thisType.GetMethod(reader);
                if (theMethod == null)
                {
                    continue;
                }
                string value = theMethod.Invoke(this, null).ToString();
                saveMe.Add(setting, value);
            }
            SimpleIO IO = new SimpleIO();
            IO.ChangeRoot(fromFolder);
            string writeFile = filename + ".json";
            IO.WriteFile(writeFile, JsonConvert.SerializeObject(saveMe));

        }
        protected void LoadSettingsFromFile(string fromFolder)
        {
            SimpleIO IO = new SimpleIO();
            IO.ChangeRoot(fromFolder);
            string readfile = filename + ".json";
            if (IO.Exists(readfile) == false)
            {
                MakeSettingsFile(fromFolder);
                return;
            }
            JObject result = JObject.Parse(IO.ReadFile(readfile));
            foreach (string setting in settings)
            {
                if(result.ContainsKey(setting) == false)
                {
                    continue;
                }
                string value = result[setting].ToString();
                if (SecondbotHelpers.notempty(value) == false)
                {
                    mysettings.Add(setting, "");
                    continue;
                }
                mysettings.Add(setting, value);
            }
            loaded = true;
        }

        protected void LoadSettingsFromEnv()
        {
            foreach (string setting in settings)
            {
                TryLoadSetting(setting);
            }
            loaded = true;
        }

        protected virtual void MakeSettings()
        {

        }

        protected bool TryLoadSetting(string setting)
        {
            if(tryLoadfromEnv == false)
            {
                return false;
            }
            string key = filename + "_" + setting;
            string value = Environment.GetEnvironmentVariable(key);
            if(value == null)
            {
                return false;
            }
            mysettings.Add(setting, value);
            return true;

        }

        protected string ReadSettingAsString(string key, string defaultValue)
        {
            if (mysettings.ContainsKey(key) == false)
            {
                if(TryLoadSetting(key) == false)
                {
                    return defaultValue;
                }
            }
            return mysettings[key];
        }

        protected bool ReadSettingAsBool(string key,bool defaultValue=false)
        {
            bool result = bool.TryParse(ReadSettingAsString(key, defaultValue.ToString()), out bool enabled);
            if (result == false)
            {
                return defaultValue;
            }
            return enabled;
        }

        protected int ReadSettingAsInt(string key, int defaultValue = 30)
        {
            bool result = int.TryParse(ReadSettingAsString(key, defaultValue.ToString()), out int value);
            if (result == false)
            {
                return defaultValue;
            }
            return value;
        }

        protected ulong ReadSettingAsUlong(string key, ulong defaultValue = 0)
        {
            bool result = ulong.TryParse(ReadSettingAsString(key, defaultValue.ToString()), out ulong value);
            if (result == false)
            {
                return defaultValue;
            }
            return value;
        }
    }
}
