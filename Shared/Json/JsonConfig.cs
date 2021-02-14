using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBotShared.Json
{

    public class JsonScopedTokens
    {
        public string[] ScopedTokens { get; set; }
    }

    public class JsonCommandsfile
    {
        public string[] CustomCommands { get; set; }
    }

    public class JsonCustomRelays
    {
        public string[] CustomRelays { get; set; }
    }


    public class JsonConfig
    {
        // Basics
        public string Basic_BotUserName { get; set; }
        public string Basic_BotPassword { get; set; }
        public string[] Basic_HomeRegions { get; set; }

        public string Basic_LoginLocation { get; set; }

        // Security
        public string Security_MasterUsername { get; set; }
        public string Security_SubMasters { get; set; }
        public string Security_SignedCommandkey { get; set; }
        public string Security_WebUIKey { get; set; }


        // Settings
        public bool Setting_AllowRLV { get; set; }
        public bool Setting_AllowFunds { get; set; }
        public string Setting_RelayImToAvatarUUID { get; set; }
        public string Setting_DefaultSit_UUID { get; set; }
        public string Setting_loginURI { get; set; }

        // Discord Relay
        public string DiscordRelay_URL { get; set; }
        public string DiscordRelay_GroupUUID { get; set; }

        // Discord FullFat
        public bool DiscordFull_Enable { get; set; }
        public string DiscordFull_Token { get; set; }
        public ulong DiscordFull_ServerID { get; set; }

        public bool DiscordFull_Keep_GroupChat { get; set; }

        // HTTP interface
        public bool Http_Enable { get; set; }
        public string Http_Host { get; set; }

        // Name2Key DB
        public bool Name2Key_Enable { get; set; }
        public string Name2Key_Url { get; set; }
        public string Name2Key_Key { get; set; }


        // TTS
        public bool DiscordTTS_Enable { get; set; }
        public ulong DiscordTTS_server_id { get; set; }
        public string DiscordTTS_channel_name { get; set; }
        public string DiscordTTS_avatar_uuid { get; set; }
        public string DiscordTTS_Nickname { get; set; }

        // Logs
        public bool Log2File_Enable { get; set; }
        public int Log2File_Level { get; set; }
        public bool Setting_LogCommands { get; set; }
    }

}
