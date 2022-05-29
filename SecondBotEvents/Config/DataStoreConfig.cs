using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class DataStoreConfig : Config
    {
        public DataStoreConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "datastore";
            settings.Add("AvatarsPurgeAfterMins");
            settings.Add("LocalChatHistoryLimit");
            settings.Add("GroupChatHistoryLimitPerGroup");
            settings.Add("ImChatHistoryLimit");
            settings.Add("NotecardStoragePurgeAfterMins");
            settings.Add("PrefetchGroupMembers");
            settings.Add("PrefetchGroupRoles");
        }

        public bool GetPrefetchGroupRoles()
        {
            return ReadSettingAsBool("PrefetchGroupRoles", false);
        }

        public bool GetPrefetchGroupMembers()
        {
            return ReadSettingAsBool("PrefetchGroupMembers", true);
        }

        public int GetAvatarsPurgeAfterMins()
        {
            return ReadSettingAsInt("AvatarsPurgeAfterMins", 25);
        }

        public int GetLocalChatHistoryLimit()
        {
            return ReadSettingAsInt("LocalChatHistoryLimit", 150);
        }

        public int GetGroupChatHistoryLimitPerGroup()
        {
            return ReadSettingAsInt("GroupChatHistoryLimitPerGroup", 50);
        }

        public int GetImChatHistoryLimit()
        {
            return ReadSettingAsInt("ImChatHistoryLimit", 50);
        }

        public int GetNotecardStoragePurgeAfterMins()
        {
            return ReadSettingAsInt("NotecardStoragePurgeAfterMins", 10);
        }
    }
}