using Newtonsoft.Json.Linq;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Text.Json;


namespace SecondBotEvents.Config
{
    public class ConfigDescriptor
    {
        public string argname = "";
        public string argtype = "";
        public string argvalue = "";

        public ConfigDescriptor(string argname, string argtype, string argvalue)
        {
            this.argname = argname;
            this.argtype = argtype;
            this.argvalue = argvalue;
        }
    }
    public abstract class Config
    {
        protected Dictionary<string, string> mysettings = [];
        protected List<string> settings = [];
        protected string filename = "config";
        protected bool loaded = false;
        protected bool tryLoadfromEnv = false;

        public List<ConfigDescriptor> DescribeConfig()
        {
            List<ConfigDescriptor> settingsDescribed = [];
            // Reorder settings: "Enabled" first, "HideStatusOutput" last if present
            if (settings.Contains("Enabled"))
            {
                settings.Remove("Enabled");
                settings.Insert(0, "Enabled");
            }
            if (settings.Contains("HideStatusOutput"))
            {
                settings.Remove("HideStatusOutput");
                settings.Add("HideStatusOutput");
            }
            foreach (var setting in settings)
            {
                Type configType = this.GetType();
                string accessor = "Get" + StringExtensions.FirstCharToUpper(setting);
                MethodInfo method = configType.GetMethod(accessor);

                if (method != null)
                {
                    // Get the return type
                    Type returnType = method.ReturnType;
                    object value = method.Invoke(this, null);
                    Dictionary<string, string> typeMapping = new()
                    {
                        { "System.String", "Text" },
                        { "System.Int32", "Number" },
                        { "System.UInt64", "Number" }, // everything is a int if you dont check the size 
                        { "System.Boolean", "True|False" },
                        { "System.Double", "Float" }, // lies :P but SL does not have the concept of double
                        { "System.String[]", "CSV" } // more lies but we convert them anyway
                    };
                    string typeName = returnType.FullName != null && typeMapping.ContainsKey(returnType.FullName) ? typeMapping[returnType.FullName] : returnType.Name;
                    if(typeName == "CSV")
                    {
                        value = string.Join(",", (string[])value);
                    }
                    settingsDescribed.Add(new ConfigDescriptor(setting, typeName, value.ToString()));
                }
            }
            return settingsDescribed;
        }

        public Config(bool fromENV, string fromFolder="")
        {
            MakeSettings();
            LoadSettings(fromENV, fromFolder);
        }

        public void updateKey(string key, string value)
        {
            if(mysettings.ContainsKey(key))
            {
                mysettings[key] = value;
                return;
            }
            mysettings.Add(key, value);
        }
        public void setEnabled(bool enabled)
        {
            mysettings["Enabled"] = enabled.ToString();
        }

        public bool GetHideStatusOutput()
        {
            return ReadSettingAsBool("HideStatusOutput", false);
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
            if (fromFolder != "")
            {
                LoadSettingsFromFile(fromFolder);
            }
            
        }

        protected void MakeSettingsFile(string fromFolder)
        {
            List<ConfigDescriptor> myconfigread = DescribeConfig();

            Dictionary<string, string> saveMe = [];
            foreach (ConfigDescriptor entry in myconfigread)
            {
                saveMe.Add(entry.argname, entry.argvalue);
            }
            SimpleIO IO = new();
            IO.ChangeRoot(fromFolder);
            string writeFile = filename + ".json";
            IO.WriteFile(writeFile, JsonSerializer.Serialize(saveMe, JsonOptions.UnsafeRelaxed));

        }
        protected void LoadSettingsFromFile(string fromFolder)
        {
            SimpleIO IO = new();
            IO.ChangeRoot(fromFolder);
            string readfile = filename + ".json";
            if (IO.Exists(readfile) == false)
            {
                MakeSettingsFile(fromFolder);
                return;
            }
            try
            {
                JObject result = JObject.Parse(IO.ReadFile(readfile));
                foreach (var x in result)
                {
                    settings.Add(x.Key);
                    string value = x.Value.ToString();
                    if (SecondbotHelpers.NotEmpty(value) == false)
                    {
                        mysettings.Add(x.Key, "");
                        continue;
                    }
                    mysettings.Add(x.Key, value);
                }
                loaded = true;
            }
            catch (Exception ex)
            {
                LogFormater.Warn("Json format for " + readfile + " is broken: " + ex.Message);
            }
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
            string read = ReadSettingAsString(key, defaultValue.ToString());
            bool result = int.TryParse(read, out int value);
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

        protected double ReadSettingAsDouble(string key, double defaultValue = 0)
        {
            bool result = double.TryParse(ReadSettingAsString(key, defaultValue.ToString()), out double value);
            if (result == false)
            {
                return defaultValue;
            }
            return value;
        }
    }
}
