using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBotShared.Json
{
    public class JsonConfig
    {
        // Basics
        public string Basic_BotUserName { get; set; }
        public string Basic_BotPassword { get; set; }
        public string[] Basic_HomeRegions { get; set; }

        // Security
        public string Security_MasterUsername { get; set; }
        public string Security_SubMasters { get; set; }
        public string Security_SignedCommandkey { get; set; }
        public string Security_WebUIKey { get; set; }


        // Settings
        public bool Setting_AllowRLV { get; set; }
        public bool Setting_AllowFunds { get; set; }
        public bool Setting_LogCommands { get; set; }
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

        // HTTP interface
        public bool Http_Enable { get; set; }
        public int Http_Port { get; set; }
        public string Http_Host { get; set; }
        public string Http_PublicUrl { get; set; }

    }

}
