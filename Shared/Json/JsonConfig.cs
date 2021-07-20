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

    public class JsonCustomRelaysSet
    {
        public JsonCustomRelays[] Entrys {get; set;}
    }

    public class JsonCustomRelays
    {
        public bool encodeJson = false;
        public string sourceType = "";
        public string sourceFilter = "";
        public string targetType = "";
        public string targetConfig = "";
    }

    public class advertConfig
    {
        public string title;
        public string attachment;
        public string content;
        public string notice;
        public string enabled;
        public string[] groups;
        public string days;
        public string hour;
        public string min;
    }

    public class advertsBlob
    {
        public advertConfig[] adverts = new advertConfig[] { };
    }

    public class OnEvent
    {
        public string title;
        public bool Enabled;
        public string On;
        public string Monitor;
        public string[] Where;
        public string[] Actions;
    }

    public class OnEventBlob
    {
        public OnEvent[] listEvents = new OnEvent[] { };
    }


    public class JsonConfig
    {
        // Basics
        public string Basic_BotUserName { get; set; }
        public string Basic_BotPassword { get; set; }
        public string[] Basic_HomeRegions { get; set; }

        public string[] Basic_AvoidRestartRegions { get; set; }

        public string Basic_LoginLocation { get; set; }

        // Security
        public string Security_MasterUsername { get; set; }
        public string Security_SubMasters { get; set; }
        public string Security_SignedCommandkey { get; set; }
        public string Security_WebUIKey { get; set; }


        // Settings
        public bool Setting_AllowFunds { get; set; }
        public string Setting_loginURI { get; set; }

        public string Setting_Tracker { get; set; }

        // Discord FullFat
        public bool DiscordFull_Enable { get; set; }
        public string DiscordFull_Token { get; set; }
        public ulong DiscordFull_ServerID { get; set; }

        public bool DiscordFull_Keep_GroupChat { get; set; }

        // HTTP interface
        public bool Http_Enable { get; set; }
        public string Http_Host { get; set; }

        // Logs
        public bool Log2File_Enable { get; set; }
        public int Log2File_Level { get; set; }
        public bool Setting_LogCommands { get; set; }

        // agent
        public string Agent_Channel { get; set; }
        public string Agent_Version { get; set; }

        // Database

        public bool SQLDB_Enabled { get; set; }
        public string SQLDB_Host { get; set; }
        public int SQLDB_Port { get; set; }
        public string SQLDB_Username { get; set; }
        public string SQLDB_Password { get; set; }
        public int SQLDB_Timeout { get; set; }
        public string SQLDB_Database { get; set; }
    }

}
