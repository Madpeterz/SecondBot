using OpenMetaverse;
using SecondBotEvents.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class OnEventConfig : Config
    {
        public OnEventConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "onevent";
            settings.Add("Enabled");
            settings.Add("Count");
            settings.Add("HideStatusOutput");
        }

        public void unloadEvents()
        {
            int loop = 1;
            while (loop <= GetCount())
            {
                if (GetEventEnabled(loop) == false)
                {
                    loop++;
                    continue;
                }
                RemoveSetting("Event" + loop.ToString() + "Enabled");
                RemoveSetting("Event" + loop.ToString() + "Source");
                RemoveSetting("Event" + loop.ToString() + "Monitor");
                int loop2 = 1;
                while (loop2 <= GetWhereCount(loop))
                {
                    RemoveSetting("Event" + loop.ToString() + "Where" + loop2.ToString());
                    loop2++;
                }
                RemoveSetting("Event" + loop.ToString() + "WhereCount");
                loop2 = 1;
                while (loop2 <= GetActionCount(loop))
                {
                    settings.Remove("Event" + loop.ToString() + "Action" + loop2.ToString());
                    loop2++;
                }
                RemoveSetting("Event" + loop.ToString() + "ActionCount");
                loop++;
            }
        }

        protected void RemoveSetting(string name)
        {
            mysettings.Remove(name);
            settings.Remove(name);
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", false);
        }

        public int GetCount()
        {
            return ReadSettingAsInt("Count", 0);
        }

        public int GetActionCount(int oneventid)
        {
            return ReadSettingAsInt("Event" + oneventid.ToString() + "ActionCount", 0);
        }

        public string GetActionStep(int oneventid, int stepid)
        {
            return ReadSettingAsString("Event" + oneventid.ToString() + "Action" + stepid.ToString(), "");
        }

        public string GetWhereCheck(int oneventid, int checkid)
        {
            return ReadSettingAsString("Event" + oneventid.ToString() + "Where" + checkid.ToString(), "");
        }

        public int GetWhereCount(int oneventid)
        {
            return ReadSettingAsInt("Event" + oneventid.ToString() + "WhereCount", 0);
        }

        public bool GetEventEnabled(int oneventid)
        {
            return ReadSettingAsBool("Event" + oneventid.ToString() + "Enabled", false);
        }

        public string GetSource(int oneventid)
        {
            return ReadSettingAsString("Event" + oneventid.ToString() + "Source", "");
        }
        public string GetSourceMonitor(int oneventid)
        {
            return ReadSettingAsString("Event" + oneventid.ToString() + "Monitor", "");
        }
    }
}