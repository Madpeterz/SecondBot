using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Net.Http;

namespace SecondBotEvents.Services
{
    public class CommandsService : Services
    {
        public CommandsConfig myConfig = null;
        public bool acceptNewCommands = false;
        public CommandsService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new CommandsConfig(master.fromEnv, master.fromFolder);
            loadCommands();
        }

        protected void loadCommands()
        {
            Dictionary<string, Type> commandmodules = http_commands_helper.getCommandModules();
            foreach (Type entry in commandmodules.Values)
            {
                LoadCommandEndpoint(entry);
            }
        }

        protected virtual void LoadCommandEndpoint(Type endpointtype)
        {
            CommandsAPI controler = (CommandsAPI)Activator.CreateInstance(endpointtype, args: new object[] { master });
            controler.disableIPlockout();
            commandEndpoints.Add(endpointtype.Name, controler);
            foreach (MethodInfo M in endpointtype.GetMethods())
            {
                bool isCallable = false;
                foreach (CustomAttributeData At in M.CustomAttributes)
                {
                    if (At.AttributeType.Name == "RouteAttribute")
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

        Dictionary<string,int> acceptTokens = new Dictionary<string,int>();
        public bool AllowToken(string token)
        {
            if(acceptTokens.ContainsKey(token) == false)
            {
                return false;
            }
            if (acceptTokens[token] != -1)
            {
                acceptTokens[token] = acceptTokens[token] - 1;
                if(acceptTokens[token] == 0)
                {
                    acceptTokens.Remove(token);
                }
            }
            return true;
        }

        protected string SingleUseToken()
        {
            bool found = false;
            string token = "";
            while(found == false)
            {
                token = SecondbotHelpers.GetSHA1(token + SecondbotHelpers.UnixTimeNow().ToString());
                if(acceptTokens.ContainsKey(token) == false)
                {
                    addNewToken(token, 1);
                    found = true;
                }
            }
            return token;
        }

        protected void addNewToken(string token,int uses)
        {
            if (acceptTokens.ContainsKey(token) == false)
            {
                acceptTokens[token] = 0;
            }
            acceptTokens[token] = uses;
        }

        public override void Start()
        {
            master.BotClientNoticeEvent += BotClientRestart;
        }

        public override void Stop()
        {
            acceptNewCommands = false;
        }

        protected void BotImMessage(object o, InstantMessageEventArgs e)
        {
            if(acceptNewCommands == false)
            {
                return;
            }
            if (e.IM.FromAgentName == master.botClient.client.Self.Name)
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
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        if(e.IM.GroupIM == true) // groups are unable to issue commands
                        {
                            break;
                        }
                        if (myConfig.GetOnlySelectedAvs() == false)
                        {
                            break;
                        }
                        requireSigning = false;
                        acceptMessage = myConfig.GetAvsCSV().Contains(e.IM.FromAgentName);
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
            SignedCommand C = new SignedCommand(e.IM.Message,
                requireSigning, 
                myConfig.GetEnforceTimeWindow(), 
                myConfig.GetTimeWindowSecs(),
                myConfig.GetSharedSecret()
            );
            if (C.accepted == false)
            {
                return;
            }
            KeyValuePair<bool, string> reply = runCommand(C);
            if(C.replyTarget != null)
            {
                SmartCommandReply(reply.Key, C.replyTarget, reply.Value, C.command);
            }
        }

        protected void SmartCommandReply(bool run_status, string target, string output, string command)
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
            else
            {
                if (int.TryParse(target, out target_channel) == false)
                {
                    mode = "None";
                }
            }
            if (mode == "CHAT")
            {
                if (target_channel <= 0)
                {
                    LogFormater.Crit("[SmartReply] output Channel must be zero or higher - using 99");
                    target_channel = 999;
                }
                master.botClient.client.Self.Chat(output, target_channel, ChatType.Normal);
            }
            else if (mode == "IM")
            {
                master.botClient.client.Self.InstantMessage(target_avatar, output);
            }
            else if (mode == "HTTP")
            {
                Dictionary<string, string> values = new Dictionary<string, string>
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
        }
        protected HttpClient HTTPclient = new HttpClient();

        public KeyValuePair<bool, string> runCommand(SignedCommand C)
        {
            C.command = commandnameLowerToReal[C.command.ToLowerInvariant()];
            try
            {
                string ott = SingleUseToken();
                CommandsAPI Endpoint = commandEndpoints[endpointcommandmap[C.command]];
                MethodInfo theMethod = Endpoint.GetType().GetMethod(C.command);
                if (theMethod != null)
                {
                    List<string> argsList = C.args.ToList();
                    argsList.Add(ott);
                    string reply = "Inconnect number of args expected: " + theMethod.GetParameters().Count().ToString() + " but got: " + argsList.Count.ToString();
                    bool status = false;
                    if (argsList.Count == theMethod.GetParameters().Count())
                    {
                        status = true;
                        try
                        {
                            if (Endpoint == null)
                            {
                                return new KeyValuePair<bool, string>(false, "Endpoint is null");
                            }
                            if (argsList.Count == 0)
                            {
                                return new KeyValuePair<bool, string>(false, "Zero args at final check require at min 1");
                            }
                            object[] argsWorker = argsList.ToArray<object>();
                            object processed = theMethod.Invoke(Endpoint, argsWorker);
                            reply = "Error";
                            if (processed != null)
                            {
                                reply = JsonConvert.SerializeObject(processed);
                            }
                            return new KeyValuePair<bool, string>(status, reply);
                        }
                        catch (Exception e)
                        {
                            return new KeyValuePair<bool, string>(false, e.Message);
                        }
                    }
                    return new KeyValuePair<bool, string>(status, reply);
                }
                return new KeyValuePair<bool, string>(false, "theMethod is null");
            }
            catch (Exception e)
            {
                return new KeyValuePair<bool, string>(false, e.Message);
            }
        }

        public string[] getFullListOfCommands()
        {
            List<string> output = new List<string>();
            foreach (string A in endpointcommandmap.Keys)
            {
                output.Add(A.ToLowerInvariant());
            }
            return output.ToArray();
        }

        protected Dictionary<string, CommandsAPI> commandEndpoints = new Dictionary<string, CommandsAPI>();
        protected Dictionary<string, string> endpointcommandmap = new Dictionary<string, string>(); // command = endpoint
        protected Dictionary<string, string> commandnameLowerToReal = new Dictionary<string, string>();

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            acceptNewCommands = false;
            Console.WriteLine("Commands service [Attached to new client]");
            master.botClient.client.Network.LoggedOut += BotLoggedOut;
            master.botClient.client.Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            master.botClient.client.Network.SimConnected += BotLoggedIn;
            Console.WriteLine("Commands service [Waiting for connect]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            master.botClient.client.Network.SimConnected -= BotLoggedIn;
            master.botClient.client.Self.IM += BotImMessage;
            acceptNewCommands = true;
            Console.WriteLine("Commands service [accepting IM commands]");
        }
    }

    public class SignedCommand
    {
        public string command;
        public string signingCode;
        public string[] args;
        public string replyTarget;
        public bool accepted = false;
        public long unixtimeOfCommand = 0;
        public SignedCommand(string input, bool requireSigning, bool requireTimewindow, int windowSize, string secret)
        {
            unpackInput(input);
            if (requireSigning == false)
            {
                accepted = true; // just accept the command
                return;
            }
            long dif = SecondbotHelpers.UnixTimeNow() - unixtimeOfCommand;
            if ((requireTimewindow == true) && ((unixtimeOfCommand == 0) || (dif > windowSize))) // bad time window
            {
                return;
            }
            if ((signingCode == null) ) // no signing code
            {
                return;
            }
            string raw = command;
            raw = raw + string.Join("~#~", args);
            if (requireTimewindow == true)
            {
                raw = raw + unixtimeOfCommand.ToString();
            }
            raw = raw + secret;
            string cooked = SecondbotHelpers.GetSHA1(raw);
            if (cooked != signingCode)
            {
                return; // invaild signing code
            }
            accepted = true;


        }
        protected void unpackInput(string input)
        {
            if (IsValidJson(input) == false)
            {
                unpackStringToClass(input);
                return;
            }
            unpackJsonToClass();
        }
        protected JToken obj = null;
        protected void unpackStringToClass(string input)
        {
            // command|||args~#~args~#~args#|#reply_target@@@sha1 [inc time if required];|;unixtime of command

            string[] bits = input.Split(";|;");
            if(bits.Length == 2)
            {
                unixtimeOfCommand = Convert.ToInt32(bits[1]);
            }
            bits = bits[0].Split("#|#");
            if(bits.Length == 2)
            {
                replyTarget = bits[1];
            }
            bits = bits[0].Split("|||");
            command = bits[0];
            args = new string[] { };
            if(bits.Length == 2)
            {
                args = bits[1].Split("~#~");
            }
        }
        protected void unpackJsonToClass()
        {
            command = jsonHelper.GetValue<string>(obj, "cmd", null);
            args = jsonHelper.GetValue<string[]>(obj, "args", null);
            signingCode = jsonHelper.GetValue<string>(obj, "signing", null);
            replyTarget = jsonHelper.GetValue<string>(obj, "reply", null);
            unixtimeOfCommand = jsonHelper.GetValue<long>(obj, "unixtime", 0);
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

    public static class jsonHelper
    {
        public static T GetValue<T>(this JToken jToken, string key, T defaultValue = default(T))
        {
            dynamic ret = jToken[key];
            if (ret == null) return defaultValue;
            if (ret is JObject) return JsonConvert.DeserializeObject<T>(ret.ToString());
            return (T)ret;
        }
    }
}
