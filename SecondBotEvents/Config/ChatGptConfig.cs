using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
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
            settings.Add("ShowDebug");
            settings.Add("Provider");
            settings.Add("CustomName");
            settings.Add("FakeTypeDelay");
            settings.Add("UseRedis");
            settings.Add("RedisSource");
            settings.Add("RedisPrefix");
            settings.Add("RedisLocalchat");
            settings.Add("RedisGroupchat");
            settings.Add("RedisImchat");
            settings.Add("RedisMaxageMins");
            settings.Add("RedisCountLocal");
            settings.Add("RedisCountGroup");
            settings.Add("RedisCountIm");
        }

        public bool GetFakeTypeDelay()
        {
            return ReadSettingAsBool("FakeTypeDelay", true);
        }

        public int GetRedisCountIm()
        {
            return ReadSettingAsInt("RedisCountIm", 60);
        }

        public int GetRedisCountLocal()
        {
            return ReadSettingAsInt("RedisCountLocal", 60);
        }

        public int GetRedisCountGroup()
        {
            return ReadSettingAsInt("RedisCountGroup", 60);
        }

        public bool GetUseRedis()
        {
            return ReadSettingAsBool("UseRedis", false);
        }

        public int GetRedisMaxageMins()
        {
            return ReadSettingAsInt("RedisMaxageMins", 120);
        }

        public string GetRedisSource()
        {
            return ReadSettingAsString("RedisSource", "127.0.0.1:6379");
        }

        public string GetRedisPrefix()
        {
            return ReadSettingAsString("RedisPrefix", "sbot");
        }

        public bool GetRedisLocalchat()
        {
            return ReadSettingAsBool("RedisLocalchat", false);
        }

        public bool GetRedisImchat()
        {
            return ReadSettingAsBool("RedisImchat", true);
        }

        public bool GetRedisGroupchat()
        {
            return ReadSettingAsBool("RedisGroupchat", false);
        }

        public string GetCustomName()
        {
            return ReadSettingAsString("CustomName", "<!FIRSTNAME!>");
        }

        public bool GetShowDebug()
        {
            return ReadSettingAsBool("ShowDebug", false);
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