using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class InteractionConfig : Config
    {
        public InteractionConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "interaction";
            settings.Add("AcceptTeleports");
            settings.Add("AcceptGroupInvites");
            settings.Add("AcceptInventory");
            settings.Add("EnableJsonOutputEvents");
            settings.Add("JsonOutputEventsTarget");
            settings.Add("AcceptFriendRequests");
            settings.Add("Enabled");
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", true);
        }

        public bool GetAcceptTeleports()
        {
            return ReadSettingAsBool("AcceptTeleports", true);
        }

        public bool GetAcceptGroupInvites()
        {
            return ReadSettingAsBool("AcceptGroupInvites", true);
        }

        public bool GetAcceptInventory()
        {
            return ReadSettingAsBool("AcceptInventory", true);
        }

        public string GetJsonOutputEventsTarget()
        {
            return ReadSettingAsString("JsonOutputEventsTarget", "none");
        }

        public bool GetEnableJsonOutputEvents()
        {
            return ReadSettingAsBool("EnableJsonOutputEvents", true);
        }

        public bool GetAcceptFriendRequests()
        {
            return ReadSettingAsBool("AcceptFriendRequests", true);
        }
    }

}