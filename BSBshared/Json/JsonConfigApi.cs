using BetterSecondBotShared.API;
using BetterSecondBotShared.logs;
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
                case "homeregion": { return "AtHome"; }
                case "defaultsituuid": { return "AtHome"; }
                case "discordgrouptarget": { return "DiscordRelay"; }
                case "discord": { return "DiscordRelay"; }
                case "allowrlv": { return "RLV"; }
                case "code": { return "Required"; }
                case "enablehttp": { return "HTTP"; }
                case "httpport": { return "HTTP"; }
                case "httpkey": { return "HTTP"; }
                case "httprequiresigned": { return "HTTP"; }
                case "httphost": { return "HTTP"; }
                case "httpascnc": { return "HTTP"; }
                case "discordfullserver": { return "DiscordFull"; }
                case "discordclienttoken": { return "DiscordFull"; }
                case "discordserverid": { return "DiscordFull"; }
                case "discordserverimhistoryhours": { return "DiscordFull"; }
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
                case "discord": { return "A vaild discord hook URL to relay group chat onto<br/>leave as \"\" to disable"; }
                case "allowrlv": { return "Enable RLV commands interface for the bot"; }
                case "code": { return "A known secret between your secondlife scripts and the bot<br/>Used to support IM commands"; }
                case "enablehttp": { return "Enable the HTTP interface (using the HTTP settings)"; }
                case "httpport": { return "What port the HTTP interface should be on<br/> Example: 8080"; }
                case "httpkey": { return "A known secret between used for HTTP calls<br/>Example: \"DontUseThis\""; }
                case "httprequiresigned": { return "Force HTTP calls to be Signed with a hash (Not yet KnownCommand)"; }
                case "httphost": { return "The URL to listen for HTTP connections on"; }
                case "httpascnc": { return "Switchs the software into command and control mode"; }
                case "discordfullserver": { return "Should we switch to a full server control discord link"; }
                case "discordclienttoken": { return "What is the bots client token"; }
                case "discordserverid": { return "What is the Discord server id we should be using"; }
                case "discordserverimhistoryhours": { return "How long to keep history messages in Group/IM chats in hours"; }
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
                case "discord": { return new[] { "\"https://discordapp.com/api/webhooks/XXXX/YYYY\"" }; }
                case "allowrlv": { return new[] { "False" }; }
                case "code": { return new[] { "\"DontTellAnyoneThis\"" }; }
                case "enablehttp": { return new[] { "False" }; }
                case "httpport": { return new[] { "8080" }; }
                case "httpkey": { return new[] { "\"ThisIsAKeyYo\"" }; }
                case "httprequiresigned": { return new[] { "False" }; }
                case "httphost": { return new[] { "\"http://localhost\"" }; }
                case "httpascnc": { return new[] { "False" }; }
                case "discordfullserver": { return new[] { "False" }; }
                case "discordclienttoken": { return new[] { "https://discordapp.com/developers/ Bot" }; }
                case "discordserverid": { return new[] { "1234567890" }; }
                case "discordserverimhistoryhours": { return new[] { "24" }; }
                case "defaultsituuid": { return new[] { "\"" + OpenMetaverse.UUID.Zero + "\"" }; }
                default: { return new string[] { }; }
            }
        }
        public override int GetCommandArgs(string cmd)
        {
            return 1;
        }

        public string GetProp(JsonConfig reply, string arg)
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
            if ((jsonConfig.Httpkey != null) && (jsonConfig.HttpHost != null))
            {
                if ((jsonConfig.Httpport < 80) || (jsonConfig.Httpkey.Length < 12) || (jsonConfig.HttpHost.StartsWith("http") == false))
                {
                    jsonConfig.EnableHttp = false;
                }
            }
            else
            {
                jsonConfig.EnableHttp = false;
            }
            if (jsonConfig.EnableHttp == false)
            {
                jsonConfig.HttpAsCnC = false;
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
