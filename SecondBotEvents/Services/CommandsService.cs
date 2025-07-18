﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Net.Http;
using OpenMetaverse.ImportExport.Collada14;
using System.ComponentModel.DataAnnotations;

namespace SecondBotEvents.Services
{
    public class BotCommandNotice(string Setcommand, string Setargs, string Setsource, bool Setaccepted, string Setresults)
    {
        public string command = Setcommand;
        public string args = Setargs;
        public string source = Setsource;
        public bool accepted = Setaccepted;
        public string results = Setresults;
    }
    public class CommandsService : BotServices
    {
        private EventHandler<BotCommandNotice> BotclientCommandEventNotices;
        public event EventHandler<BotCommandNotice> BotclientCommandEventNotice
        {
            add { lock (BotCommandNoticeLockable) { BotclientCommandEventNotices += value; } }
            remove { lock (BotCommandNoticeLockable) { BotclientCommandEventNotices -= value; } }
        }
        private readonly object BotCommandNoticeLockable = new();


        public new CommandsConfig myConfig = null;
        public bool acceptNewCommands = false;
        public CommandsService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new CommandsConfig(master.fromEnv, master.fromFolder);
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            LoadCommands();
        }

        public bool GetAllowServiceCommands()
        {
            return myConfig.GetAllowServiceControl();
        }

        protected void LoadCommands()
        {
            Dictionary<string, Type> commandmodules = http_commands_helper.getCommandModules();
            foreach (Type entry in commandmodules.Values)
            {
                LoadCommandEndpoint(entry);
            }
        }

        protected virtual void LoadCommandEndpoint(Type endpointtype)
        {
            CommandsAPI controler = (CommandsAPI)Activator.CreateInstance(endpointtype, args: [master]);
            commandEndpoints.Add(endpointtype.Name, controler);
            foreach (MethodInfo M in endpointtype.GetMethods())
            {
                bool isCallable = false;
                foreach (CustomAttributeData At in M.CustomAttributes)
                {
                    if (At.AttributeType.Name == "About")
                    {
                        isCallable = true;
                        break;
                    }
                }
                if (isCallable == true)
                {
                    if (endpointcommandmap.ContainsKey(M.Name) == true)
                    {
                        LogFormater.Warn("Namespace: " + endpointtype.Name + " / Command: " + M.Name + " already found in " + endpointcommandmap[M.Name]);
                        continue;
                    }
                    if (commandnameLowerToReal.ContainsKey(M.Name.ToLowerInvariant()) == true)
                    {
                        LogFormater.Warn("Namespace: " + endpointtype.Name + " / Command: " + M.Name + " already found in " + endpointcommandmap[M.Name]);
                        continue;
                    }
                    endpointcommandmap.Add(M.Name, endpointtype.Name);
                    commandnameLowerToReal.Add(M.Name.ToLowerInvariant(), M.Name);
                }
            }
        }
        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
        }

        public override void Stop()
        {
            running = false;
            acceptNewCommands = false;
        }

        public override string Status()
        {
            if(myConfig == null)
            {
                return "No Config";
            }
            else if(myConfig.GetHideStatusOutput() == true)
            {
                return "hidden";
            }
            if (acceptNewCommands == false)
            {
                return "Disabled";
            }
            return "Enabled";
        }

        protected void BotImMessage(object o, InstantMessageEventArgs e)
        {
            if(acceptNewCommands == false)
            {
                return;
            }
            if (e.IM.FromAgentName == GetClient().Self.Name)
            {
                return;   
            }
            if (myConfig.GetAllowIMcontrol() == false)
            {
                return;
            }
            bool acceptMessage = false;
            bool requireSigning = true;
            switch (e.IM.Dialog)
            {
                case InstantMessageDialog.MessageFromObject:
                    {
                        acceptMessage = true;
                        if(myConfig.GetObjectMasterOptout() == true)
                        {
                            string ownername = master.DataStoreService.GetAvatarName(e.IM.FromAgentID);
                            requireSigning = !myConfig.GetMastersCSV().Contains(ownername);
                            if ((requireSigning == false) && (myConfig.GetCheckDotNames() == true))
                            {
                                string[] bits = e.IM.FromAgentName.ToLower().Split('.');
                                if (bits.Length == 2)
                                {
                                    requireSigning = myConfig.GetMastersCSV().Contains(bits[0].FirstCharToUpper() + " " + bits[1].FirstCharToUpper());
                                }
                            }
                        }
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        if(e.IM.GroupIM == true) // groups are unable to issue commands
                        {
                            break;
                        }
                        if (myConfig.GetEnableMasterControls() == false)
                        {
                            break;
                        }
                        master.DataStoreService.AddAvatar(e.IM.FromAgentID, e.IM.FromAgentName);
                        requireSigning = false;
                        acceptMessage = myConfig.GetMastersCSV().Contains(e.IM.FromAgentName);
                        if ((acceptMessage == false) && (myConfig.GetCheckDotNames() == true))
                        {
                            string[] bits = e.IM.FromAgentName.ToLower().Split('.');
                            if (bits.Length == 2)
                            {
                                acceptMessage = myConfig.GetMastersCSV().Contains(bits[0].FirstCharToUpper()+" " + bits[1].FirstCharToUpper());
                            }
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
            if(acceptMessage == false)
            {
                return;
            }
            CommandInterfaceCaller(e.IM.Message, requireSigning);
        }

        public bool AvatarUUIDIsMaster(UUID avatar)
        {
            foreach(string name in myConfig.GetMastersCSV())
            {
                string find = master.DataStoreService.GetAvatarUUID(name);
                if (find != "lookup")
                {
                    if(avatar.ToString() == find)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public KeyValuePair<bool, string> CommandInterfaceCaller(string message, bool requireSigning=false, bool viaCustomCommand=false, string forceSetSource= "command interface")
        {
            string source = forceSetSource;
            if(viaCustomCommand == true)
            {
                source = "customcommand";
            }
            SignedCommand C = new(this,source,message,requireSigning,myConfig.GetEnforceTimeWindow(),myConfig.GetTimeWindowSecs(),myConfig.GetSharedSecret());
            if (C.accepted == false)
            {
                return new KeyValuePair<bool, string>(false, "Not accepted via signing");
            }
            KeyValuePair<bool, string> reply = RunCommand(source, C, viaCustomCommand);
            return reply;
        }

        public void SmartCommandReply(string target, string output, string command)
        {
            string mode = "CHAT";
            UUID target_avatar = UUID.Zero;
            int target_channel = 0;
            if (target.StartsWith("http://"))
            {
                mode = "HTTP";
            }
            else if (UUID.TryParse(target, out target_avatar) == true)
            {
                mode = "IM";
            }
            else if (target.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 2)
            {
                string UUIDfetch = master.DataStoreService.GetAvatarUUID(target);
                if (UUIDfetch == "lookup")
                {
                    return;
                }
                mode = "IM";
                UUID.TryParse(UUIDfetch, out target_avatar);
            }
            else if (new EmailAddressAttribute().IsValid(target) == true)
            {
                mode = "EMAIL";
            }
            else if (int.TryParse(target, out target_channel) == false)
            {
                return;
            }
            if (mode == "CHAT")
            {
                if (target_channel < 0)
                {
                    LogFormater.Crit("[SmartReply] output Channel must be zero or higher - using 99");
                    target_channel = 999;
                }
                GetClient().Self.Chat(output, target_channel, ChatType.Normal);
            }
            else if (mode == "IM")
            {
                GetClient().Self.InstantMessage(target_avatar, output);
            }
            else if (mode == "HTTP")
            {
                Dictionary<string, string> values = new()
                {
                        { "reply", output },
                        { "command", command },
                    };

                var content = new FormUrlEncodedContent(values);
                try
                {
                    HTTPclient.PostAsync(target, content);
                }
                catch (Exception e)
                {
                    LogFormater.Crit("[SmartReply] HTTP failed: " + e.Message + "");
                }
            }
            else if(mode == "EMAIL")
            {
                KeyValuePair<bool,string> reply = master.SmtpService.sendReplyTarget(target,command,output);
                if(reply.Key == false)
                {
                    LogFormater.Crit("[SmartReply] EMAIL failed: " + reply.Value);
                }
            }
        }
        protected HttpClient HTTPclient = new();

        protected KeyValuePair<bool, string> RunBaseCommand(SignedCommand C, string source)
        {
            try
            {
                CommandsAPI Endpoint = commandEndpoints[endpointcommandmap[C.command]];
                MethodInfo theMethod = Endpoint.GetType().GetMethod(C.command);
                if (theMethod != null)
                {
                    List<string> argsList = [.. C.args];
                    string reply = "incorrect number of args expected: " + theMethod.GetParameters().Length.ToString() + " but got: " + argsList.Count.ToString();
                    bool status = false;
                    if (argsList.Count == theMethod.GetParameters().Length)
                    {
                        status = true;
                        try
                        {
                            if (Endpoint == null)
                            {
                                return new KeyValuePair<bool, string>(false, "Endpoint is null");
                            }
                            object[] argsWorker = [.. argsList];
                            object processed = theMethod.Invoke(Endpoint, argsWorker);
                            reply = "Error";
                            if (processed != null)
                            {
                                reply = JsonConvert.SerializeObject(processed);
                            }
                            CommandNotice(C.command, source, String.Join("@@@", C.args), true, reply);
                            return new KeyValuePair<bool, string>(status, reply);
                        }
                        catch (Exception e)
                        {
                            CommandNotice(C.command, source+" ~~ Failure: "+e.Message, String.Join("@@@", C.args), false, e.Message);
                            return new KeyValuePair<bool, string>(false, e.Message);
                        }
                    }
                    CommandNotice(C.command, source + " ~~ Failure: incorrect number of args expected: "+ theMethod.GetParameters().Length.ToString()+" but got "+ argsList.Count.ToString()+"", String.Join("@@@", C.args), false, "no results");
                    return new KeyValuePair<bool, string>(status, reply);
                }
                return new KeyValuePair<bool, string>(false, "theMethod is null");
            }
            catch (Exception e)
            {
                return new KeyValuePair<bool, string>(false, e.Message);
            }
        }
        public KeyValuePair<bool, string> RunCommand(string source, SignedCommand C, bool inCustomCommand=false)
        {
            if(acceptNewCommands == false)
            {
                return new KeyValuePair<bool, string>(false, "Not accepting commands");
            }
            try
            {

                string lowerName = C.command.ToLower();
                if(commandnameLowerToReal.ContainsKey(lowerName) == true)
                {
                    C.command = commandnameLowerToReal[lowerName];
                }
                else if(master.CustomCommandsService.HasCommand(C.command) == true)
                {
                    if(inCustomCommand == true)
                    {
                        return new KeyValuePair<bool, string>(false, "Custom command chaining is not allowed");
                    }
                    return master.CustomCommandsService.RunCommand(C, source);
                }
                else
                {
                    return new KeyValuePair<bool, string>(false, "Unknown command");
                }

                KeyValuePair<bool,string> reply = RunBaseCommand(C, source);
                if (C.replyTarget != null)
                {
                    SmartCommandReply(C.replyTarget, reply.Value, C.command);
                }
                return reply;

            }
            catch (Exception e)
            {
                return new KeyValuePair<bool, string>(false, e.Message);
            }
        }

        public string[] GetFullListOfCommands()
        {
            List<string> output = [];
            foreach (string A in endpointcommandmap.Keys)
            {
                output.Add(A.ToLowerInvariant());
            }
            return [.. output];
        }

        protected Dictionary<string, CommandsAPI> commandEndpoints = [];
        protected Dictionary<string, string> endpointcommandmap = []; // command = endpoint
        protected Dictionary<string, string> commandnameLowerToReal = [];

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isStart == false)
            {
                return;
            }
            acceptNewCommands = false;
            LogFormater.Info("Commands service [Attached to new client]");
            if (e.isDC == false)
            {
                GetClient().Network.LoggedOut += BotLoggedOut;
                GetClient().Network.SimConnected += BotLoggedIn;
            }
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            GetClient().Network.SimConnected += BotLoggedIn;
            LogFormater.Info("Commands service [Waiting for connect]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            GetClient().Self.IM += BotImMessage;
            if (myConfig.GetEnableMasterControls() == true)
            {
                foreach(string A in myConfig.GetMastersCSV())
                {
                    master.DataStoreService.KnownAvatar(A);
                }
            }
            acceptNewCommands = true;
            LogFormater.Info("Commands service [accepting IM commands]");
        }

        public void CommandNotice(string command,string source, string args,bool accepted, string results)
        {
            if(myConfig.GetCommandHistoryLogResults() == false)
            {
                results = "";
            }
            BotCommandNotice e = new(command, args, source, accepted, results);
            EventHandler<BotCommandNotice> handler = BotclientCommandEventNotices;
            handler?.Invoke(this, e);
            if(master.BotClient.basicCfg.GetLogCommands() == true)
            {
                LogFormater.Info("Command log:" + JsonConvert.SerializeObject(e));
            }
        }
    }

    public class SignedCommand
    {
        public string command;
        public string signingCode;
        public string[] args;
        public string replyTarget = null;
        public bool accepted = false;
        public long unixtimeOfCommand = 0;
        protected CommandsService master;
        public SignedCommand(CommandsService setMaster, string source, string input, bool requireSigning, bool requireTimewindow, int windowSize, string secret)
        {
            master = setMaster;
            UnpackInput(input);
            if (requireSigning == false)
            {
                accepted = true; // just accept the command
                return;
            }
            Vaildate(requireTimewindow, windowSize, secret);
        }

        public SignedCommand(CommandsService setMaster, string source, string setCommand, string setSigningCode, string[] setArgs, 
            int setUnixtime, string setReplyTarget, bool requireTimewindow, int windowSize, string secret, bool requireSigning = true)
        {
            master = setMaster;
            command = setCommand;
            signingCode = setSigningCode;
            args = setArgs;
            unixtimeOfCommand = setUnixtime;
            replyTarget = setReplyTarget;
            
            if (requireSigning == false)
            {
                accepted = true; // just accept the command
                return;
            }
            Vaildate(requireTimewindow, windowSize, secret);
        }

        protected void Vaildate(bool requireTimewindow, int windowSize, string secret)
        {
            accepted = false;
            long dif = SecondbotHelpers.UnixTimeNow() - unixtimeOfCommand;
            if(requireTimewindow == true)
            {
                if ((unixtimeOfCommand == 0) || (dif > windowSize))
                {
                    return;
                }
            }
            if (signingCode == null) // no signing code
            {
                return;
            }
            string raw = command;
            raw += string.Join("~#~", args);
            if (requireTimewindow == true)
            {
                raw += unixtimeOfCommand.ToString();
            }
            raw += secret;
            string cooked = SecondbotHelpers.GetSHA1(raw);
            if (cooked != signingCode)
            {
                return; // invaild signing code
            }

            accepted = true;
        }

        protected void UnpackInput(string input)
        {
            if (IsValidJson(input) == false)
            {
                UnpackStringToClass(input);
                return;
            }
            UnpackJsonToClass();
        }
        protected JToken obj = null;
        protected void UnpackStringToClass(string input)
        {
            // command|||args~#~args~#~args#|#reply_target@@@sha1 [inc time if required];|;unixtime of command

            string[] bits = input.Split(";|;");
            if(bits.Length == 2)
            {
                unixtimeOfCommand = Convert.ToInt32(bits[1]);
            }
            // command|||args~#~args~#~args#|#reply_target@@@sha1 [inc time if required]
            // unixtime of command
            bits = bits[0].Split("@@@");
            if(bits.Length == 2)
            {
                signingCode = bits[1];
            }
            // command|||args~#~args~#~args#|#reply_target
            // sha1 [inc time if required]
            bits = bits[0].Split("#|#");
            if(bits.Length == 2)
            {
                replyTarget = bits[1];
            }
            // command|||args~#~args~#~args
            // reply_target
            bits = bits[0].Split("|||");
            command = bits[0];
            // command
            // args~#~args~#~args
            args = [];
            if(bits.Length == 2)
            {
                args = bits[1].Split("~#~");
            }
        }
        protected void UnpackJsonToClass()
        {
            command = JsonHelper.GetValue<string>(obj, "cmd", null);
            args = JsonHelper.GetValue<string[]>(obj, "args", null);
            signingCode = JsonHelper.GetValue<string>(obj, "signing", null);
            replyTarget = JsonHelper.GetValue<string>(obj, "reply", null);
            unixtimeOfCommand = JsonHelper.GetValue<long>(obj, "unixtime", 0);
        }

        private bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    obj = JToken.Parse(strInput);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }

    public static class JsonHelper
    {
        public static T GetValue<T>(this JToken jToken, string key, T defaultValue = default)
        {
            dynamic ret = jToken[key];
            if (ret == null) return defaultValue;
            if (ret is JObject) return JsonConvert.DeserializeObject<T>(ret.ToString());
            return (T)ret;
        }
    }
}
