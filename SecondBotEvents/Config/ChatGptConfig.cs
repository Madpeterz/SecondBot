using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class ChatGptConfig : Config
    {
        public ChatGptConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "chatgpt";
            settings.Add("Enabled");
            settings.Add("ApiKey");
            settings.Add("OrganizationId");
            settings.Add("AllowImReplys");
            settings.Add("ImReplyFriendsOnly");
            settings.Add("ImReplyRateLimiter");
            settings.Add("AllowGroupReplys");
            settings.Add("GroupReplyForGroup");
            settings.Add("GroupReplyRateLimiter");
            settings.Add("LocalchatReply");
            settings.Add("LocalchatRateLimiter");
            settings.Add("UseModel");
            settings.Add("ChatHistoryMessages");
            settings.Add("ChatPrompt");
            settings.Add("ChatHistoryTimeout");
            settings.Add("Provider");
        }

        public int GetChatHistoryTimeout()
        {
            return ReadSettingAsInt("ChatHistoryTimeout", 15);
        }

        public string GetProvider()
        {
            return ReadSettingAsString("Provider", "openai");
        }

        public string GetChatPrompt()
        {
            return ReadSettingAsString("ChatPrompt", "respond as if you are a horse that knows its going to the glue factory and you are upset about this fact");
        }

        public int GetImReplyRateLimiter()
        {
            return ReadSettingAsInt("ImReplyRateLimiter", 3);
        }
        public int GetGroupReplyRateLimiter()
        {
            return ReadSettingAsInt("GroupReplyRateLimiter", 3);
        }
        public int GetLocalchatRateLimiter()
        {
            return ReadSettingAsInt("LocalchatRateLimiter", 3);
        }
        public int GetChatHistoryMessages()
        {
            return ReadSettingAsInt("ChatHistoryMessages", 5);
        }
        public string GetUseModel()
        {
            return ReadSettingAsString("UseModel", "gpt-3.5-turbo");
        }
        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", false);
        }
        public string GetApiKey()
        {
            return ReadSettingAsString("ApiKey","none");
        }
        public string GetOrganizationId()
        {
            return ReadSettingAsString("OrganizationId","none");
        }
        public bool GetAllowImReplys()
        {
            return ReadSettingAsBool("AllowImReplys");
        }
        public bool GetImReplyFriendsOnly()
        {
            return ReadSettingAsBool("ImReplyFriendsOnly");
        }
        public bool GetAllowGroupReplys()
        {
            return ReadSettingAsBool("AllowGroupReplys");
        }
        public string GetGroupReplyForGroup()
        {
            return ReadSettingAsString("GroupReplyForGroup","none");
        }
        public bool GetLocalchatReply()
        {
            return ReadSettingAsBool("LocalchatReply");
        }
    }
}