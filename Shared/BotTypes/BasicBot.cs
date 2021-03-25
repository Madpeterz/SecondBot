using System;
using System.Collections.Generic;
using System.Linq;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using Newtonsoft.Json;
using OpenMetaverse;

namespace BetterSecondBotShared.bottypes
{
    public abstract class BasicBot
    {

        protected BasicBot()
        {
            LastStatusMessage = "No status";
        }


        protected Dictionary<UUID, Group> mygroups = new Dictionary<UUID, Group>();

        protected Dictionary<UUID, KeyValuePair<long, List<GroupRole>>> mygrouprolesstorage = new Dictionary<UUID, KeyValuePair<long, List<GroupRole>>>();
        public Dictionary<UUID, Group> MyGroups { get { return mygroups; } }
        public Dictionary<UUID, KeyValuePair<long, List<GroupRole>>> MyGroupRolesStorage { get { return mygrouprolesstorage; } }

        protected GridClient Client;
        public GridClient GetClient { get { return Client; } }
        protected List<string> SubMasters = new List<string>();
        public bool Is_avatar_master(string name)
        {
            if (name == myconfig.Security_MasterUsername)
            {
                return true;
            }
            else
            {
                return SubMasters.Contains(name);
            }
        }

        protected bool running_in_docker = false;
        public void AsDocker()
        {
            running_in_docker = true;
        }

        protected bool killMe;
        protected JsonConfig myconfig;
        public JsonConfig getMyConfig { get { return myconfig; } }
        protected string version = "NotSet V1.0.0.0";

        public bool reconnect;

        public string MyVersion { get { return version; } }
        public string Name { get { return myconfig.Basic_BotUserName; } }
        public string OwnerName { get { return myconfig.Security_MasterUsername; } }
        public bool GetAllowFunds { get { return myconfig.Setting_AllowFunds; } }
        public string LastStatusMessage { get; set; }

        public virtual bool KillMe { get { return killMe; } }

        protected bool teleported;
        public void SetTeleported()
        {
            teleported = true;
        }

        public bool TeleportStatus()
        {
            return teleported;
        }


        protected Dictionary<string, string[]> custom_commands = new Dictionary<string, string[]>();
        public Dictionary<string, string[]> getCustomCommands { get { return custom_commands; } }
        protected bool loaded_custom_commands = false;
        protected void LoadCustomCommands()
        {
            if (running_in_docker == true)
            {
                // search env for cmd_
                foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
                {
                    string name = env.Key.ToString();
                    if (name.StartsWith("cmd_") == true)
                    {
                        string[] steps = env.Value.ToString().Split(new [] { "{-}" }, StringSplitOptions.None);
                        name = name.Replace("cmd_", "");
                        name = name.ToLowerInvariant();
                        custom_commands.Add(name, steps);
                    }
                }
            }
            else
            {
                // search for file called "commands.cmd"
                JsonCommandsfile LoadedCommands = new JsonCommandsfile
                {
                    CustomCommands = new [] { "sayexample!!!say|||Hello{-}delay|||2500{-}say|||Bye" }
                };
                string CommandsFile = "custom_commands.json";
                SimpleIO io = new SimpleIO();
                if (SimpleIO.FileType(CommandsFile, "json") == true)
                {
                    if (io.Exists(CommandsFile) == true)
                    {
                        string json = io.ReadFile(CommandsFile);
                        if (json.Length > 0)
                        {
                            try
                            {
                                LoadedCommands = JsonConvert.DeserializeObject<JsonCommandsfile>(json);
                                foreach (string loaded in LoadedCommands.CustomCommands)
                                {
                                    string[] bits = loaded.Split(new [] { "!!!" }, StringSplitOptions.None);
                                    if (bits.Length == 2)
                                    {
                                        string[] steps = bits[1].Split(new [] { "{-}" }, StringSplitOptions.None);
                                        custom_commands.Add(bits[0].ToLowerInvariant(), steps);
                                    }
                                }
                            }
                            catch
                            {
                                io.makeOld(CommandsFile);
                                io.WriteJsonCommands(LoadedCommands, CommandsFile);
                            }
                        }
                        else
                        {
                            io.WriteJsonCommands(LoadedCommands, CommandsFile);
                        }
                    }
                    else
                    {
                        io.WriteJsonCommands(LoadedCommands, CommandsFile);
                    }
                }
                else
                {
                    io.WriteJsonCommands(LoadedCommands, CommandsFile);
                }
            }
            if (custom_commands.Count > 0)
            {
                Info("Custom commands: " + custom_commands.Count.ToString());
            }
        }

        public virtual void KillMePlease()
        {
            killMe = true;
            Client.Network.BeginLogout();
        }
        public virtual void Setup(JsonConfig config, string Version)
        {
            version = Version;
            myconfig = config;
            if (myconfig.Security_SignedCommandkey.Length < 8)
            {
                myconfig.Security_SignedCommandkey = helpers.GetSHA1("" + myconfig.Basic_BotUserName + "" + myconfig.Basic_BotPassword + "" + helpers.UnixTimeNow().ToString() + "").Substring(0, 8);
                Warn("Given code is not acceptable (min length 8)");
            }
            List<string> bits = myconfig.Basic_BotUserName.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (bits.Count == 1)
            {
                bits.Add("Resident");
                myconfig.Basic_BotUserName = String.Join(' ', bits);
            }
            bits = myconfig.Security_MasterUsername.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            if (bits.Count == 1)
            {
                bits.Add("Resident");
                myconfig.Security_MasterUsername = String.Join(" ", bits);

            }
            if (helpers.notempty(myconfig.Security_SubMasters) == true)
            {
                SubMasters.Clear();
                string[] submaster_names = myconfig.Security_SubMasters.Split(",", StringSplitOptions.RemoveEmptyEntries);
                foreach (string a in submaster_names)
                {
                    if (a.Length > 3)
                    {
                        bits = a.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
                        if (bits.Count == 1)
                        {
                            bits.Add("Resident");
                        }
                        string namesubmaster = String.Join(" ", bits);
                        if (namesubmaster != myconfig.Security_MasterUsername)
                        {
                            SubMasters.Add(namesubmaster);
                            Info("Sub-Master: " + namesubmaster);
                        }
                    }
                }
            }
            Info("Build: " + version);
        }
        protected virtual string[] getNextHomeArgs()
        {
            return new [] { };
        }
        public virtual void Start()
        {
            Start(false);
        }
        protected virtual void AvoidSim(string simname)
        {
        }

        public virtual void Start(bool use_home_region_redirect)
        {
            Client = new GridClient();
            Client.Settings.LOG_RESENDS = false;
            Client.Settings.ALWAYS_REQUEST_OBJECTS = true;
            Client.Settings.AVATAR_TRACKING = true;
            List<string> bits = myconfig.Basic_BotUserName.Split(' ').ToList();
            LoginParams Lp;
            if (myconfig.Setting_loginURI != "secondlife")
            {
                if (myconfig.Setting_loginURI != null)
                {
                    if (myconfig.Setting_loginURI.Length > 5)
                    {
                        Info("Using custom login server");
                        Lp = new LoginParams(Client, bits[0], bits[1], myconfig.Basic_BotPassword, "BetterSecondBot", version, myconfig.Setting_loginURI);
                    }
                    else
                    {
                        Warn("loginURI invaild: using secondlife");
                        Lp = new LoginParams(Client, bits[0], bits[1], myconfig.Basic_BotPassword, "BetterSecondBot", version);
                    }
                }
                else
                {
                    Warn("loginURI invaild: using secondlife");
                    Lp = new LoginParams(Client, bits[0], bits[1], myconfig.Basic_BotPassword, "BetterSecondBot", version);
                }
            }
            else
            {
                Lp = new LoginParams(Client, bits[0], bits[1], myconfig.Basic_BotPassword, "BetterSecondBot", version);
            }
            if (helpers.notempty(myconfig.Basic_LoginLocation) == false)
            {
                myconfig.Basic_LoginLocation = "home";
                Info("Basic_LoginLocation: is empty using home!");
            }
            if (use_home_region_redirect == false)
            {
                if ((myconfig.Basic_LoginLocation != "home") && (myconfig.Basic_LoginLocation != "last"))
                {
                    Lp.Start = "home";
                    Lp.LoginLocation = myconfig.Basic_LoginLocation;
                    Info("Basic_LoginLocation: First login using->" + Lp.LoginLocation);
                }
            }
            else
            {
                if ((myconfig.Basic_LoginLocation != "home") && (myconfig.Basic_LoginLocation != "last"))
                {
                    Info("Recovery login: using home not custom location!");
                    Lp.Start = "home";
                    myconfig.Basic_LoginLocation = "home";
                }
                else if (Lp.Start == "home")
                {
                    Info("Recovery login: using last location");
                    Lp.Start = "last";
                    myconfig.Basic_LoginLocation = "last";
                }
                else
                {
                    Info("Recovery login: using home location");
                    Lp.Start = "home";

                }

                Lp.Start = myconfig.Basic_LoginLocation;

            }
            if (reconnect == false)
            {
                BotStartHandler();
            }
            Client.Network.BeginLogin(Lp);
        }
        protected virtual void BotStartHandler()
        {
            Debug("BotStartHandler proc not overridden");
        }

        protected string GetSimPositionAsString()
        {
            if (Client.Network.Connected == true)
            {
                if (Client.Network.CurrentSim != null)
                {
                    int X = Convert.ToInt32(Math.Floor(Client.Self.SimPosition.X));
                    int Y = Convert.ToInt32(Math.Floor(Client.Self.SimPosition.Y));
                    int Z = Convert.ToInt32(Math.Floor(Client.Self.SimPosition.Z));
                    return "" + X.ToString() + "," + Y.ToString() + "," + Z.ToString() + "";
                }
            }
            return "0,0,0";
        }

        protected string BasicBot_laststatus = "";
        public virtual string GetStatus()
        {
            string reply = "Info: No client";
            if (Client != null)
            {
                reply = "Network: Not ready";
                if (Client.Network != null)
                {
                    reply = "Network: Not connected";
                    if (Client.Network.Connected)
                    {
                        reply = "Sim: Not on sim";
                        if (Client.Network.CurrentSim != null)
                        {
                            reply = "Sim: " + Client.Network.CurrentSim.Name + " " + GetSimPositionAsString() + "";
                        }
                    }
                }
            }
            BasicBot_laststatus = reply;
            return reply;
        }

        protected virtual void LoginHandler(object o, LoginProgressEventArgs e)
        {
            Debug("LoginHandler proc not overridden");
        }

        public virtual void AfterBotLoginHandler()
        {
            Debug("AfterBotLoginHandler proc not overridden");
        }


        protected virtual void GroupInvite(InstantMessageEventArgs e)
        {
            Debug("GroupInvite proc not overridden");
        }
        protected virtual void FriendshipOffer(UUID IMSessionID, string FromAgentName, UUID FromAgentID)
        {
            Debug("FriendshipOffer proc not overridden");
        }

        protected virtual void RequestLure(UUID IMSessionID, string FromAgentName, UUID FromAgentID)
        {
            if (Is_avatar_master(FromAgentName) == true)
            {
                Client.Self.SendTeleportLure(FromAgentID);
            }
        }

        protected List<UUID> accept_next_teleport_from = new List<UUID>();
        public void Add_uuid_to_teleport_list(UUID avatar)
        {
            if (accept_next_teleport_from.Contains(avatar) == false)
            {
                accept_next_teleport_from.Add(avatar);
            }
        }

        protected virtual void RequestTeleport(UUID IMSessionID, string FromAgentName, UUID FromAgentID)
        {
            Debug("RequestTeleport proc not overridden");
        }

        protected virtual void ChatInputHandler(object sender, ChatEventArgs e)
        {
            Debug("ChatInputHandler proc not overridden");
        }

        public virtual void ResetAnimations()
        {
            Debug("reset_animations not enabled at this level");
        }

        public virtual void Warn(string message)
        {
            Log2File(LogFormater.Warn(message, false), ConsoleLogLogLevel.Warn);
        }
        public virtual void Crit(string message)
        {
            Log2File(LogFormater.Crit(message, false), ConsoleLogLogLevel.Crit);
        }
        public virtual void Info(string message)
        {
            Log2File(LogFormater.Info(message, false), ConsoleLogLogLevel.Info);
        }
        public virtual void Status(string message)
        {
            Log2File(LogFormater.Status(message, false), ConsoleLogLogLevel.Status);
        }
        public virtual void Debug(string message)
        {
            Log2File(LogFormater.Debug(message, false), ConsoleLogLogLevel.Debug);
        }

        public virtual void Log2File(string message, ConsoleLogLogLevel Level)
        {
            if (myconfig == null)
            {
                Console.WriteLine(message);
            }
            else
            {
                if (myconfig.Log2File_Level >= (int)Level)
                {
                    Console.WriteLine(message);
                }
            }
        }
    }
}
