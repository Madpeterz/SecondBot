using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BetterSecondBotShared.bottypes
{
    public abstract class BasicBot
    {
        protected BasicBot()
        {
            LastStatusMessage = "No status";
        }
        protected List<string> SubMasters = new List<string>();
        public bool is_avatar_master(string name)
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

        protected bool killMe;
        protected JsonConfig myconfig;
        protected string version = "NotSet V1.0.0.0";

        protected bool reconnect;
        public string MyVersion { get { return version; } }
        public string Name { get { return myconfig.Basic_BotUserName; } }
        public string OwnerName { get { return myconfig.Security_MasterUsername; } }
        public bool GetAllowFunds { get { return myconfig.Setting_AllowFunds; } }
        public string LastStatusMessage { get; set; }

        public bool KillMe { get { return killMe; } }

        protected bool teleported;
        public void SetTeleported()
        {
            teleported = true;
        }

        protected Dictionary<UUID, Group> mygroups = new Dictionary<UUID, Group>();

        public Dictionary<UUID, Group> MyGroups { get { return mygroups; } }

        protected GridClient Client;
        public GridClient GetClient { get { return Client; } }
        public virtual void KillMePlease()
        {
            killMe = true;
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
                        SubMasters.Add(namesubmaster);
                        Info("Sub-Master: " + namesubmaster);
                    }
                }
            }
            Info("Build: " + version);
        }

        public virtual void Start()
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

        public virtual string GetStatus()
        {
            if (Client != null)
            {
                if (Client.Network != null)
                {
                    if (Client.Network.Connected)
                    {
                        if (Client.Network.CurrentSim != null)
                        {
                            return "Sim: " + Client.Network.CurrentSim.Name + " " + GetSimPositionAsString() + "";
                        }
                        return "Sim: Not on sim";
                    }
                    return "Network: Not connected";
                }
                return "Network: Not ready";
            }
            return "Info: No client";
        }

        protected virtual void LoginHandler(object o, LoginProgressEventArgs e)
        {
            Debug("LoginHandler proc not overridden");
        }

        protected virtual void AfterBotLoginHandler()
        {
        }


        protected virtual void GroupInvite(InstantMessageEventArgs e)
        {
            string[] stage1 = e.IM.FromAgentName.ToLowerInvariant().Split('.');
            string name = "" + stage1[0].FirstCharToUpper() + "";
            if (stage1.Length == 1)
            {
                name = " Resident";
            }
            else
            {
                name = "" + name + " " + stage1[1].FirstCharToUpper() + "";
            }
            if(is_avatar_master(name) == true)
            {
                GroupInvitationEventArgs G = new GroupInvitationEventArgs(e.Simulator, e.IM.FromAgentID, e.IM.FromAgentName, e.IM.Message);
                Client.Self.GroupInviteRespond(G.AgentID, e.IM.IMSessionID, true);
                Client.Groups.RequestCurrentGroups();
            }
        }
        protected virtual void FriendshipOffer(UUID IMSessionID, string FromAgentName, UUID FromAgentID)
        {
            if (is_avatar_master(FromAgentName) == true)
            {
                Client.Friends.AcceptFriendship(FromAgentID, IMSessionID);
            }
        }

        protected virtual void RequestLure(UUID IMSessionID, string FromAgentName, UUID FromAgentID)
        {
            if (is_avatar_master(FromAgentName) == true)
            {
                Client.Self.SendTeleportLure(FromAgentID);
            }
        }

        protected List<UUID> accept_next_teleport_from = new List<UUID>();
        public void add_uuid_to_teleport_list(UUID avatar)
        {
            if (accept_next_teleport_from.Contains(avatar) == false)
            {
                accept_next_teleport_from.Add(avatar);
            }
        }

        protected virtual void RequestTeleport(UUID IMSessionID, string FromAgentName, UUID FromAgentID)
        {
            bool allow = false;
            if (is_avatar_master(FromAgentName) == true)
            {
                allow = true;
            }
            else if(accept_next_teleport_from.Contains(FromAgentID) == true)
            {
                allow = true;
                accept_next_teleport_from.Remove(FromAgentID);
            }
            if(allow == true)
            {
                ResetAnimations();
                SetTeleported();
                Client.Self.TeleportLureRespond(FromAgentID, IMSessionID, true);
            }
        }

        protected virtual void ChatInputHandler(object sender, ChatEventArgs e)
        {
            Debug("ChatInputHandler proc not overridden");
        }

        protected virtual void CoreCommandLib(UUID fromUUID, bool from_master, string command, string arg)
        {
            CoreCommandLib(fromUUID, from_master, command, arg, "");
        }

        protected virtual void CoreCommandLib(UUID fromUUID, bool from_master, string command, string arg, string signing_code)
        {
            CoreCommandLib(fromUUID, from_master, command, arg, signing_code, "~#~");
        }

        protected virtual void CoreCommandLib(UUID fromUUID,bool from_master,string command,string arg,string signing_code,string signed_with)
        {
            Debug("CoreCommandLib proc not overridden");
        }

        public virtual void ResetAnimations()
        {
           Debug("reset_animations not enabled at this level");
        }

        public virtual void Warn(string message)
        {
            Log2File(LogFormater.Warn(message), ConsoleLogLogLevel.Warn);
        }
        public virtual void Crit(string message)
        {
            Log2File(LogFormater.Crit(message), ConsoleLogLogLevel.Crit);
        }
        public virtual void Info(string message)
        {
            Log2File(LogFormater.Info(message), ConsoleLogLogLevel.Info);
        }
        public virtual void Status(string message)
        {
            Log2File(LogFormater.Status(message), ConsoleLogLogLevel.Status);
        }
        public virtual void Debug(string message)
        {
            Log2File(LogFormater.Debug(message), ConsoleLogLogLevel.Debug);
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
