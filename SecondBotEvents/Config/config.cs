using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;


namespace SecondBotEvents.Config
{
    public abstract class config
    {
        protected Dictionary<string, string> mysettings = new Dictionary<string, string>();
        protected List<string> settings = new List<string>();
        protected string filename = "config";
        protected bool loaded = false;
        public config(bool fromENV, string fromFolder="")
        {
            makeSettings();
            loadSettings(fromENV, fromFolder);
        }

        public bool isLoaded()
        {
            return loaded;
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

        protected void makeSettingsFile(string fromFolder)
        {
            Dictionary<string, string> saveMe = new Dictionary<string, string>();
            Type thisType = this.GetType();
            foreach (string setting in settings)
            {
                string reader = "get"+ StringExtensions.FirstCharToUpper(setting);
                MethodInfo theMethod = thisType.GetMethod(reader);
                string value = theMethod.Invoke(this, null).ToString();
                saveMe.Add(setting, value);
            }
            SimpleIO IO = new SimpleIO();
            IO.ChangeRoot(fromFolder);
            string writeFile = filename + ".json";
            IO.WriteFile(writeFile, JsonConvert.SerializeObject(saveMe));

        }
        protected void loadSettingsFromFile(string fromFolder)
        {
            SimpleIO IO = new SimpleIO();
            IO.ChangeRoot(fromFolder);
            string readfile = filename + ".json";
            if (IO.Exists(readfile) == false)
            {
                makeSettingsFile(fromFolder);
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
            loaded = true;
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
