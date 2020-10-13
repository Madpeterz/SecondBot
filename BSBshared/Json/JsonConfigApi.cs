using BetterSecondBotShared.API;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;

namespace BetterSecondBotShared.Json
{
    public class MakeJsonConfig : API_supported_interface
    {
        public override string GetCommandWorkspace(string cmd)
        {
            string[] bits = cmd.Split("_");
            return bits.First();
        }

        public override string GetCommandHelp(string cmd)
        {
            // Basic
            if (cmd == "Basic_BotUserName") return "Username of the bot *you can leave out Resident if you so wish*";
            else if (cmd == "Basic_BotPassword") return "Password of the bot *add $1$ followed by the md5 hash if you dont want to expose the raw password example: $1$A3DCB4D229DE6FDE0DB5686DEE47145D";
            else if (cmd == "Basic_HomeRegions") return "a CSV of SLURLs that the bot will attempt to return to after avoiding a sim shutdown (In testing)";
            // Security
            else if (cmd == "Security_MasterUsername") return "The username of the bots owner *you can leave out Resident if you so wish*";
            else if (cmd == "Security_SubMasters") return "A CSV of avatar names who can issue commands to the bot";

            else if (cmd == "Security_SignedCommandkey") return "Used when sending the bot a HTTP or IM command that has a hash validation";
            else if (cmd == "Security_WebUIKey") return "Used with the webUI to login, note: 2FA support is planned soon for this system that will replace this UIkey";
            // Settings
            else if (cmd == "Setting_AllowRLV") return "Enable the RLV api interface";
            else if (cmd == "Setting_AllowFunds") return "Allow the bot to transfer L$ and get a non zero balance";
            else if (cmd == "Setting_RelayImToAvatarUUID") return "UUID of who the bot should relay IMs to in secondlife";
            else if (cmd == "Setting_DefaultSit_UUID") return "UUID of a object the bot should attempt to sit on after logging in";
            else if (cmd == "Setting_loginURI") return "the URI to login to (leave as \"secondlife\" unless you know what your doing!)";
            // Discord
            else if (cmd == "DiscordRelay_URL") return "The webhook URL to send group chat to";
            else if (cmd == "DiscordRelay_GroupUUID") return "The group UUID to relay group chat for";
            else if (cmd == "DiscordFull_Enable") return "Allow the bot to fully connect to discord (Disables relay)";
            else if (cmd == "DiscordFull_Token") return "The discord client token (see: https://discord.com/developers/applications)";
            else if (cmd == "DiscordFull_ServerID") return "UUID of a object the bot should attempt to sit on after logging in";
            // HTTP
            else if (cmd == "Http_Enable") return "Allow the bot to start a HTTP interface (Required for web UI and Post commands)";
            else if (cmd == "Http_Port") return "The port the HTTP interface should be running on";
            else if (cmd == "Http_Host") return "The URL the interface is on example use \"docker\" for auto or \"http://localhost:portnum\"";
            else if (cmd == "Http_PublicUrl") return "The public url used to access the web ui";
            // Discord TTS
            else if (cmd == "DiscordTTS_Enable") return "Enable discord TTS helper (Requires DiscordFull_Enable set to true) ";
            else if (cmd == "DiscordTTS_server_id") return "The server ID to use for TTS";
            else if (cmd == "DiscordTTS_channel_name") return "The channel name to use (found on the server set by DiscordTTS_server_id)";
            else if (cmd == "DiscordTTS_avatar_uuid") return "The avatar UUID whos IMs will be turned into TTS";
            else if (cmd == "DiscordTTS_Nickname") return "The nickname to use on the TTS server";
            // Logs
            else if (cmd == "Log2File_Enable") return "Enable writing to logs to files [Logs are sent to the Logs folder]"
                    + "\n If you dont attach a volume called logs to the bot it will not be kept when the docker instance is restarted";
            else if (cmd == "Log2File_Level") return "Enabled logging levels\n"
                                                    + "- 1: Nothing\n"
                                                    + "0: Status only\n"
                                                    + "1: +Info\n"
                                                    + "2: +Crititcal\n"
                                                    + "3: +Warnings\n"
                                                    + "4: +Debug\n";
            else if (cmd == "Setting_LogCommands") return "Allow the bot to send command to console and discord full if enabled";
            // Give up
            return "Unknown value:" + cmd;
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
            return new string[] { JsonGetCommandArgHints(cmd) };
        }
        public string JsonGetCommandArgHints(string cmd)
        {
            // Basic
            if (cmd == "Basic_BotUserName") return "Example Resident";
            else if (cmd == "Basic_BotPassword") return "Pass";
            else if (cmd == "Basic_HomeRegions") return "http://maps.secondlife.com/secondlife/Viserion/50/140/23";
            // Security
            else if (cmd == "Security_MasterUsername") return "Madpeter Zond";
            else if (cmd == "Security_SubMasters") return "Master Zond,Madpeter Zond,Madpeter Zond";
            else if (cmd == "Security_SignedCommandkey") return "asdt234t34d3f34f";
            else if (cmd == "Security_WebUIKey") return "2135r3y4vw232";
            // Settings
            else if (cmd == "Setting_AllowRLV") return "False";
            else if (cmd == "Setting_AllowFunds") return "True";
            else if (cmd == "Setting_RelayImToAvatarUUID") return "";
            else if (cmd == "Setting_DefaultSit_UUID") return "";
            else if (cmd == "Setting_loginURI") return "secondlife";
            // Discord
            else if (cmd == "DiscordRelay_URL") return "";
            else if (cmd == "DiscordRelay_GroupUUID") return "";
            else if (cmd == "DiscordFull_Enable") return "False";
            else if (cmd == "DiscordFull_Token") return "";
            else if (cmd == "DiscordFull_ServerID") return "";
            // HTTP
            else if (cmd == "Http_Enable") return "False";
            else if (cmd == "Http_Port") return "80";
            else if (cmd == "Http_Host") return "http://localhost:80";
            else if (cmd == "Http_PublicUrl") return "http://localhost/";
            // Discord TTS
            else if (cmd == "DiscordTTS_Enable") return "False";
            else if (cmd == "DiscordTTS_server_id") return "";
            else if (cmd == "DiscordTTS_channel_name") return "";
            else if (cmd == "DiscordTTS_avatar_uuid") return "";
            else if (cmd == "DiscordTTS_Nickname") return "";
            // Logs
            else if (cmd == "Log2File_Enable") return "False";
            else if (cmd == "Log2File_Level") return "1";
            else if (cmd == "Setting_LogCommands") return "True";
            // Give up
            return "Unknown value:" + cmd;
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
                                LogFormater.Debug("unsupported arg_type: " + arg_type + " for " + arg + "");
                            }
                        }
                        else
                        {
                            LogFormater.Warn("Unable to write to " + arg + "");
                        }
                    }
                    else
                    {
                        LogFormater.Crit("unknown prop " + arg + "");
                    }
                }
            }
            else
            {
                LogFormater.Debug("unknown arg_type for " + arg + "");
            }
            return reply;
        }

        public static JsonConfig http_config_check(JsonConfig jsonConfig)
        {
            if (helpers.notempty(jsonConfig.Security_WebUIKey) == true)
            {
                if (helpers.notempty(jsonConfig.Http_Host) == true)
                {
                    if (jsonConfig.Http_Enable == true)
                    {
                        jsonConfig.Http_Enable = false;
                        if (jsonConfig.Security_WebUIKey.Length < 12)
                        {
                            LogFormater.Warn("Http disabled: Security_WebUIKey length must be 12 or more");
                        }
                        else if (jsonConfig.Http_Port < 80)
                        {
                            LogFormater.Warn("Http disabled: Httpport range below protected range - Given: (" + jsonConfig.Http_Port + ")");
                        }
                        else if ((jsonConfig.Http_Host != "docker") && (jsonConfig.Http_Host.StartsWith("http") == false))
                        {
                            LogFormater.Warn("Http disabled: Http_Host must be vaild: http://XXXXXX or \"docker\" - Given: (" + jsonConfig.Http_Host + ")");
                        }
                        else
                        {
                            jsonConfig.Http_Enable = true;
                        }
                    }
                    else
                    {
                        LogFormater.Info("Http interface disabled by config");
                    }
                }
                else
                {
                    LogFormater.Warn("Http disabled: Http_Host is null");
                    jsonConfig.Http_Enable = false;
                }
            }
            else
            {
                LogFormater.Warn("Http disabled: Security_WebUIKey is null");
                jsonConfig.Http_Enable = false;
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
                        LogFormater.Debug("[Notice] No args hint found for " + arg + "");
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
                                LogFormater.Debug("unknown arg_type: " + arg_type + " for " + arg + "");
                            }
                        }
                        else
                        {
                            LogFormater.Warn("Unable to write to " + arg + "");
                        }
                    }
                    else
                    {
                        LogFormater.Crit("unknown prop " + arg + "");
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
