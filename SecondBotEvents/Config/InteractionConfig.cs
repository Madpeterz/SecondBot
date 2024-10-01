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
            settings.Add("FriendRequestLevel");
            settings.Add("InventoryTransferLevel");
            settings.Add("GroupInviteLevel");
            settings.Add("TeleportRequestLevel");
            settings.Add("Enabled");
            settings.Add("HideStatusOutput");
        }

        public string GetFriendRequestLevel()
        {
            return ReadSettingAsString("FriendRequestLevel", "Owner");
        }
        public string GetInventoryTransferLevel()
        {
            return ReadSettingAsString("InventoryTransferLevel", "Owner");
        }
        public string GetGroupInviteLevel()
        {
            return ReadSettingAsString("GroupInviteLevel", "Owner");
        }
        public string GetTeleportRequestLevel()
        {
            return ReadSettingAsString("TeleportRequestLevel", "Owner");
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

        public bool GetAcceptFriendRequests()
        {
            return ReadSettingAsBool("AcceptFriendRequests", true);
        }

        public string GetJsonOutputEventsTarget()
        {
            return ReadSettingAsString("JsonOutputEventsTarget", "none");
        }

        public bool GetEnableJsonOutputEvents()
        {
            return ReadSettingAsBool("EnableJsonOutputEvents", false);
        }
    }

}