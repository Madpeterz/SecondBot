using BetterSecondBotShared.API;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BetterSecondBotShared.Json
{
    public class MakeJsonConfig : API_supported_interface
    {
        public override string GetCommandWorkspace(string cmd)
        {
            cmd = cmd.ToLowerInvariant();
            switch (cmd)
            {
                case "username": { return "Required"; }
                case "password": { return "Required"; }
                case "master": { return "Required"; }
                case "discordgrouptarget": { return "DiscordRelay"; }
                case "discordwebhookurl": { return "DiscordRelay"; }
                case "allowrlv": { return "RLV"; }
                case "code": { return "Required"; }
                case "enablehttp": { return "HTTP"; }
                case "httppublicurlbase": { return "HTTP"; }
                case "httpport": { return "HTTP"; }
                case "httpkey": { return "HTTP"; }
                case "httphost": { return "HTTP"; }
                case "discordfullserver": { return "DiscordFull"; }
                case "discordclienttoken": { return "DiscordFull"; }
                case "discordserverid": { return "DiscordFull"; }
                case "discordserverimhistoryhours": { return "DiscordFull"; }
                case "commandstoconsole": { return "Options"; }
                case "maxcommandhistory": { return "Options"; }
                case "relayimtoavatar": { return "Options"; }
                case "homeregion": { return "AtHome"; }
                case "defaultsituuid": { return "AtHome"; }
                case "athomelocktopos": { return "AtHome"; }
                case "athomelocktoposmaxrange": { return "AtHome"; }
                case "allowfunds": { return "security"; }
                default: { return "Unknown"; }
            }
    }
        public override string GetCommandHelp(string cmd)
        {
            cmd = cmd.ToLowerInvariant();
            switch (cmd)
            {
                case "username": { return "The full username of the bot"; }
                case "password": { return "Raw password OR  $1$ followed by the md5 of your password"; }
                case "master": { return "A vaild avatar name, you can skip Resident if thats the last name"; }
                case "defaultsituuid": { return "The UUID to sit on when at home region [Any invaild UUID to disable like \"none\"]"; }
                case "homeregion": { return "An array of SLurl strings to teleport to to avoid a sim shutdown"; }
                case "discordgrouptarget": { return "The target group UUID to relay group chat from, leave empty for all groups"; }
                case "discordwebhookurl": { return "A vaild discord hook URL to relay group chat onto<br/>leave as \"\" to disable"; }
                case "allowrlv": { return "Enable RLV commands interface for the bot"; }
                case "code": { return "A known secret between your secondlife scripts and the bot<br/>Used to support IM commands"; }
                case "enablehttp": { return "Enable the HTTP interface (using the HTTP settings)"; }
                case "httppublicurlbase": { return "what is the public url base for the http interface"; }
                case "httpport": { return "What port the HTTP interface should be on<br/> Example: 8080"; }
                case "httpkey": { return "A known secret between used for HTTP calls<br/>Example: \"DontUseThis\""; }
                case "httphost": { return "The URL to listen for HTTP connections on"; }
                case "discordfullserver": { return "Should we switch to a full server control discord link"; }
                case "discordclienttoken": { return "What is the bots client token"; }
                case "discordserverid": { return "What is the Discord server id we should be using"; }
                case "discordserverimhistoryhours": { return "How long to keep history messages in Group/IM chats in hours"; }
                case "commandstoconsole": { return "Should the bot log commands to the console window"; }
                case "maxcommandhistory": { return "How many commands should the bots command history be"; }
                case "relayimtoavatar": { return "The UUID to relay avatar IMs to [Any invaild UUID to disable like \"none\"]"; }
                case "athomelocktopos": { return "When set to true the @home system will attempt to teleport the bot back to the home pos if it leaves athomelocktopos maxrange"; }
                case "athomelocktoposmaxrange": { return "How far away from the location in homeregion can the bot be before teleporting"; }
                case "allowfunds": { return "Allow the bot to transfer L$ to avatars / objects and display its L$ balance via command/web gui"; }
                default: { return "None given"; }
            }
        }

        public override int ApiCommandsCount { get { return GetCommandsList().Length; } }

        public override string[] GetCommandsList()
        {
            JsonConfig Obj = new JsonConfig();
            return Obj.GetType().GetProperties().Select(X => X.Name).ToArray();
        }
        public override string[] GetCommandArgTypes(string cmd)
        {
            JsonConfig Obj = new JsonConfig();
            Type valuetype = Obj.GetType().GetProperties().Where(X => X.Name == cmd).Select(X => X.PropertyType).FirstOrDefault();
            if (valuetype.Name == "Int32") { return new[] { "Number" }; }
            else if (valuetype.Name == "String") { return new[] { "Text" }; }
            else if (valuetype.Name == "Boolean") { return new[] { "True|False" }; }
            else if (valuetype.Name == "String[]") { return new[] { "Collection" }; }
            else if (valuetype.Name == "UInt64") { return new[] { "BigNumber" }; }
            else if (valuetype.Name == "Single") { return new[] { "Float" }; }
            return new[] { "Unknown" };
        }
        public override string[] GetCommandArgHints(string cmd)
        {
            cmd = cmd.ToLowerInvariant();
            switch (cmd)
            {
                case "username": { return new[] { "\"Firstname LastName\"" }; }
                case "password": { return new[] { "\"PassWord\"" }; }
                case "master": { return new[] { "\"Madpeter Zond\"" }; }
                case "homeregion": { return new[] { "[\"SLurl1\",\"SLurl2\"]" }; }
                case "discordgrouptarget": { return new[] { "\"" + OpenMetaverse.UUID.Zero + "\"" }; }
                case "discordwebhookurl": { return new[] { "\"https://discordapp.com/api/webhooks/XXXX/YYYY\"" }; }
                case "allowrlv": { return new[] { "False" }; }
                case "code": { return new[] { "\"DontTellAnyoneThis\"" }; }
                case "enablehttp": { return new[] { "false" }; }
                case "httppublicurlbase": { return new[] { "\"http://bot.domain.com\"" }; }
                case "httpport": { return new[] { "8080" }; }
                case "httpkey": { return new[] { "\"ThisIsAKeyYo\"" }; }
                case "httphost": { return new[] { "\"http://localhost\"" }; }
                case "discordfullserver": { return new[] { "false" }; }
                case "discordclienttoken": { return new[] { "https://discordapp.com/developers/ Bot" }; }
                case "discordserverid": { return new[] { "1234567890" }; }
                case "discordserverimhistoryhours": { return new[] { "24" }; }
                case "defaultsituuid": { return new[] { "\"" + OpenMetaverse.UUID.Zero + "\"" }; }
                case "commandstoconsole": { return new[] { "false" }; }
                case "maxcommandhistory": { return new[] { "250" }; }
                case "relayimtoavatar": { return new[] { "\"" + OpenMetaverse.UUID.Zero + "\"" }; }
                case "athomelocktopos": { return new[] { "false" }; }
                case "athomelocktoposmaxrange": { return new[] { "10.0" }; }
                case "allowfunds": { return new[] { "true" }; }
                default: { return new string[] { }; }
            }
        }
        public override int GetCommandArgs(string cmd)
        {
            return 1;
        }

        public static string GetProp(JsonConfig reply, string arg)
        {
            return reply.GetType().GetProperty(arg, BindingFlags.Public | BindingFlags.Instance).GetValue(reply).ToString();
        }

        protected JsonConfig process_prop(JsonConfig reply,string arg,string arg_value_default)
        {
            string arg_type = GetCommandArgTypes(arg).First();
            if (arg_type != "")
            {
                if (arg_value_default != null)
                {
                    Type Dtype = reply.GetType();
                    PropertyInfo prop = Dtype.GetProperty(arg, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        if (prop.CanWrite == true)
                        {
                            if (arg_type == "Number")
                            {
                                if (int.TryParse(arg_value_default, out int result) == true)
                                {
                                    prop.SetValue(reply, result, null);
                                }
                            }
                            else if (arg_type == "Text")
                            {
                                prop.SetValue(reply, arg_value_default, null);
                            }
                            else if (arg_type == "True|False")
                            {
                                if (bool.TryParse(arg_value_default, out bool result) == true)
                                {
                                    prop.SetValue(reply, result, null);
                                }
                            }
                            else if (arg_type == "Collection")
                            {
                                prop.SetValue(reply, arg_value_default.Split(','), null);
                            }
                            else if (arg_type == "BigNumber")
                            {
                                if (ulong.TryParse(arg_value_default, out ulong result) == true)
                                {
                                    prop.SetValue(reply, result, null);
                                }
                            }
                            else if(arg_type == "Float")
                            {
                                if (float.TryParse(arg_value_default, out float result) == true)
                                {
                                    prop.SetValue(reply, result, null);
                                }
                            }
                            else
                            {
                                ConsoleLog.Debug("unsupported arg_type: " + arg_type + " for " + arg + "");
                            }
                        }
                        else
                        {
                            ConsoleLog.Warn("Unable to write to " + arg + "");
                        }
                    }
                    else
                    {
                        ConsoleLog.Crit("unknown prop " + arg + "");
                    }
                }
            }
            else
            {
                ConsoleLog.Debug("unknown arg_type for " + arg + "");
            }
            return reply;
        }

        public static JsonConfig http_config_check(JsonConfig jsonConfig)
        {
            if (helpers.notempty(jsonConfig.Httpkey) == true)
            {
                if (helpers.notempty(jsonConfig.HttpHost) == true)
                {
                    jsonConfig.EnableHttp = false;
                    if (jsonConfig.Httpkey.Length < 12)
                    {
                        ConsoleLog.Warn("Http disabled: Httpkey length must be 12 or more");
                    }
                    else if (jsonConfig.Httpport < 81)
                    {
                        ConsoleLog.Warn("Http disabled: Httpport range below protected range - Given: (" + jsonConfig.Httpport + ")");
                    }
                    else if ((jsonConfig.HttpHost != "docker") && (jsonConfig.HttpHost.StartsWith("http") == false))
                    {
                        ConsoleLog.Warn("Http disabled: HttpHost must be vaild: http://XXXXXX or \"docker\" - Given: ("+jsonConfig.HttpHost+")");
                    }
                    else
                    {
                        jsonConfig.EnableHttp = true;
                    }
                }
                else
                {
                    ConsoleLog.Warn("Http disabled: HttpHost is null");
                    jsonConfig.EnableHttp = false;
                }
            }
            else
            {
                ConsoleLog.Warn("Http disabled: Httpkey is null");
                jsonConfig.EnableHttp = false;
            }
            return jsonConfig;
        }

        protected JsonConfig FromDockerENV()
        {
            string[] args = this.GetCommandsList();
            JsonConfig reply = new JsonConfig();
            foreach (string arg in args)
            {
                
                string arg_value_default = Environment.GetEnvironmentVariable(arg);
                if (arg_value_default != null)
                {
                    arg_value_default = arg_value_default.Replace("[", "");
                    arg_value_default = arg_value_default.Replace("]", "");
                    arg_value_default = arg_value_default.Replace("\"", "");
                }
                reply = process_prop(reply, arg, arg_value_default);
            }
            return reply;
        }

        public static JsonConfig FromENV()
        {
            return new MakeJsonConfig().FromDockerENV();
        }

        protected JsonConfig MakeDefault()
        {
            string[] args = this.GetCommandsList();
            JsonConfig reply = new JsonConfig();
            foreach (string arg in args)
            {
                string arg_type = GetCommandArgTypes(arg).First();
                string arg_value_default = null;
                if (arg_type != "")
                {
                    string[] ArgHints = GetCommandArgHints(arg);
                    if (ArgHints.Length > 0)
                    {
                        arg_value_default = ArgHints[0];
                        arg_value_default = arg_value_default.Replace("[", "");
                        arg_value_default = arg_value_default.Replace("]", "");
                        arg_value_default = arg_value_default.Replace("\"", "");
                    }
                    else
                    {
                        ConsoleLog.Debug("[Notice] No args hint found for " + arg + "");
                    }
                }
                if (arg_value_default != null)
                {
                    Type Dtype = reply.GetType();
                    PropertyInfo prop = Dtype.GetProperty(arg, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        if (prop.CanWrite == true)
                        {
                            if (arg_type == "Number")
                            {
                                if (int.TryParse(arg_value_default, out int result) == true)
                                {
                                    prop.SetValue(reply, result, null);
                                }
                            }
                            else if (arg_type == "Text")
                            {
                                prop.SetValue(reply, arg_value_default, null);
                            }
                            else if (arg_type == "True|False")
                            {
                                if (bool.TryParse(arg_value_default, out bool result) == true)
                                {
                                    prop.SetValue(reply, result, null);
                                }
                            }
                            else if (arg_type == "Collection")
                            {
                                prop.SetValue(reply, arg_value_default.Split(','), null);
                            }
                            else if (arg_type == "BigNumber")
                            {
                                if (ulong.TryParse(arg_value_default, out ulong result) == true)
                                {
                                    prop.SetValue(reply, result, null);
                                }
                            }

                            else if (arg_type == "Float")
                            {
                                if (float.TryParse(arg_value_default, out float result) == true)
                                {
                                    prop.SetValue(reply, result, null);
                                }
                            }
                            else
                            {
                                ConsoleLog.Debug("unknown arg_type: " + arg_type + " for " + arg + "");
                            }
                        }
                        else
                        {
                            ConsoleLog.Warn("Unable to write to " + arg + "");
                        }
                    }
                    else
                    {
                        ConsoleLog.Crit("unknown prop " + arg + "");
                    }
                }
            }
            return reply;
        }
        public static JsonConfig GetDefault()
        {
            return new MakeJsonConfig().MakeDefault();
        }
    }
}
