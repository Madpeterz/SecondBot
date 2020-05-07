using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBotShared.Json
{
    public class JsonConfig
    {
        // Required
        public string userName { get; set; }
        public string password { get; set; }
        public string master { get; set; }
        public string code { get; set; }

        // Security
        public bool AllowFunds { get; set; }

        // Settings
        public bool allowRLV { get; set; }
        public bool OnStartLinkupWithMaster { get; set; }

        // Mixed 
        public bool CommandsToConsole { get; set; }
        public int MaxCommandHistory { get; set; }
        public string RelayImToAvatar { get; set; }
        
        // Discord
        public string discordWebhookURL { get; set; }
        public string discordGroupTarget { get; set; }
        public bool DiscordFullServer { get; set; }
        public string DiscordClientToken { get; set; }
        public ulong DiscordServerID { get; set; }
        public int DiscordServerImHistoryHours { get; set; }

        // @home
        public string[] homeRegion { get; set; }
        public bool AtHomeSimOnly { get; set; }
        public float AtHomeSimPosMaxRange { get; set; }
        public string DefaultSitUUID { get; set; }

        // HTTP interface
        public bool EnableHttp { get; set; }
        public int Httpport { get; set; }
        public string Httpkey { get; set; }
        public string HttpHost { get; set; }
        public bool HttpAsCnC { get; set; }
        public string HttpPublicUrlBase { get; set; }
    }

}
