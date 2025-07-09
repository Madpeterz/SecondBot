using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class DataStoreConfig(bool fromENV, string fromFolder = "") : Config(fromENV, fromFolder)
    {
        protected override void MakeSettings()
        {
            filename = "datastore";
            settings.Add("AutoCleanAvatars");
            settings.Add("AvatarsCleanAfterMins");
            settings.Add("LocalChatHistoryLimit");
            settings.Add("GroupChatHistoryLimitPerGroup");
            settings.Add("ImChatHistoryLimit");
            settings.Add("PrefetchGroupMembers");
            settings.Add("PrefetchGroupRoles");
            settings.Add("PrefetchEstateBanlist");
            settings.Add("PrefetchAvatarDisplaynames");
            settings.Add("AutoCleanKeyValueStore");
            settings.Add("CleanKeyValueStoreAfterMins");
            settings.Add("CommandHistoryLimit");
            settings.Add("HideStatusOutput");
        }

        public bool GetPrefetchAvatarDisplaynames()
        {
            return ReadSettingAsBool("PrefetchAvatarDisplaynames", true);
        }

        public int GetCommandHistoryLimit()
        {
            return ReadSettingAsInt("CommandHistoryLimit", 30);
        }


        public bool GetAutoCleanKeyValueStore()
        {
            return ReadSettingAsBool("AutoCleanKeyValueStore", true);
        }

        public int GetCleanKeyValueStoreAfterMins()
        {
            return ReadSettingAsInt("CleanKeyValueStoreAfterMins", 10);
        }

        public bool GetPrefetchEstateBanlist()
        {
            return ReadSettingAsBool("PrefetchEstateBanlist", true);
        }

        public bool GetPrefetchGroupRoles()
        {
            return ReadSettingAsBool("PrefetchGroupRoles", false);
        }

        public bool GetPrefetchGroupMembers()
        {
            return ReadSettingAsBool("PrefetchGroupMembers", true);
        }

        public bool GetAutoCleanAvatars()
        {
            return ReadSettingAsBool("AutoCleanAvatars", true);
        }

        public int GetAvatarsCleanAfterMins()
        {
            return ReadSettingAsInt("AvatarsCleanAfterMins", 10);
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
    }
}