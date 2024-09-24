using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class EventsConfig : Config
    {
        public EventsConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "events";
            settings.Add("Enabled");
            settings.Add("GroupMemberJoins");
            settings.Add("GroupMemberLeaves");
            settings.Add("GroupMemberEventsGroupUUID");

            settings.Add("GuestEntersArea");
            settings.Add("GuestLeavesArea");
            settings.Add("GuestTrackingSimname");
            settings.Add("GuestTrackingParcelname");

            
            settings.Add("SimAlertMessage");
            settings.Add("StatusMessage");
            settings.Add("MoneyEvent");


            settings.Add("ChangeSim");
            settings.Add("ChangeParcel");

            settings.Add("OutputChannel");
            settings.Add("OutputIMuuid");
            settings.Add("OutputHttpURL");
            settings.Add("OutputSecret");
            settings.Add("HideStatusOutput");
        }

        public string GetOutputSecret()
        {
            return ReadSettingAsString("OutputSecret", "donttellanyone");
        }

        public string GetGuestTrackingSimname()
        {
            return ReadSettingAsString("GuestTrackingSimname", "example");
        }

        public string GetGuestTrackingParcelname()
        {
            return ReadSettingAsString("GuestTrackingParcelname", "example");
        }

        public string GetGroupMemberEventsGroupUUID()
        {
            return ReadSettingAsString("GroupMemberEventsGroupUUID", UUID.Zero.ToString());
        }

        public string GetOutputHttpURL()
        {
            return ReadSettingAsString("OutputHttpURL", "n/a");
        }

        public string GetOutputIMuuid()
        {
            return ReadSettingAsString("OutputIMuuid", UUID.Zero.ToString());
        }

        public int GetOutputChannel()
        {
            return ReadSettingAsInt("OutputChannel", -1);
        }

        public bool GetGroupMemberJoins()
        {
            return ReadSettingAsBool("GroupMemberJoins", true);
        }

        public bool GetGroupMemberLeaves()
        {
            return ReadSettingAsBool("GroupMemberLeaves", true);
        }

        public bool GetGuestEntersArea()
        {
            return ReadSettingAsBool("GuestEntersArea", true);
        }

        public bool GetGuestLeavesArea()
        {
            return ReadSettingAsBool("GuestLeavesArea", true);
        }

        public bool GetChangeSim()
        {
            return ReadSettingAsBool("ChangeSim", true);
        }

        public bool GetChangeParcel()
        {
            return ReadSettingAsBool("ChangeParcel", true);
        }

        public bool GetSimAlertMessage()
        {
            return ReadSettingAsBool("SimAlertMessage", true);
        }

        public bool GetStatusMessage()
        {
            return ReadSettingAsBool("StatusMessage", true);
        }

        public bool GetMoneyEvent()
        {
            return ReadSettingAsBool("MoneyEvent", true);
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", false);
        }
    }

}