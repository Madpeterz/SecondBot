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

        protected bool killMe;
        protected JsonConfig myconfig;
        protected string version = "NotSet V1.0.0.0";
        protected bool reconnect;
        public string MyVersion { get { return version; } }
        public string Name { get { return myconfig.userName; } }
        public string OwnerName { get { return myconfig.master; } }
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
        public virtual void Setup(JsonConfig config,string Version)
        {
            version = Version;
            myconfig = config;
            if(myconfig.code.Length < 8)
            {
                myconfig.code = helpers.GetSHA1("" + myconfig.userName + "" + myconfig.password + ""+ helpers.UnixTimeNow().ToString()+"").Substring(0, 8);
                ConsoleLog.Warn("Given code is not acceptable (min length 8)");
            }
            List<string> bits = myconfig.userName.Split(' ').ToList();
            if (bits.Count == 1)
            {
                bits.Add("Resident");
                myconfig.userName = String.Join(' ', bits);
            }
            bits = myconfig.master.Split(new[] { " " }, StringSplitOptions.None).ToList();
            if (bits.Count == 1)
            {
                bits.Add("Resident");
            }
            myconfig.master = String.Join(" ", bits);
            ConsoleLog.Info("Build: " + version);
        }

        public virtual void Start()
        {
            Client = new GridClient();
            Client.Settings.LOG_RESENDS = false;
            Client.Settings.ALWAYS_REQUEST_OBJECTS = true;
            Client.Settings.AVATAR_TRACKING = true;
            List<string> bits = myconfig.userName.Split(' ').ToList();
            LoginParams Lp = new LoginParams(Client, bits[0], bits[1], myconfig.password, "BetterSecondBot", "BetterSecondBot "+version);
            if (reconnect == false)
            {
                BotStartHandler();
            }
            Client.Network.BeginLogin(Lp);
        }
        protected virtual void BotStartHandler()
        {
            ConsoleLog.Debug("BotStartHandler proc not overridden");
        }

        protected string GetSimPositionAsString()
        {
            if(Client.Network.Connected == true)
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
                            return "Sim: " + Client.Network.CurrentSim.Name + " "+GetSimPositionAsString()+"";
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
            ConsoleLog.Debug("LoginHandler proc not overridden");
        }

        protected virtual void AfterBotLoginHandler()
        {
        }


        protected virtual void GroupInvite(InstantMessageEventArgs e)
        {
            string[] stage1 = myconfig.master.ToLowerInvariant().Split(' ');
            string bad_master_name = String.Join('.', stage1);
            if (e.IM.FromAgentName == bad_master_name)
            {
                GroupInvitationEventArgs G = new GroupInvitationEventArgs(e.Simulator, e.IM.FromAgentID, e.IM.FromAgentName, e.IM.Message);
                Client.Self.GroupInviteRespond(G.AgentID, e.IM.IMSessionID, true);
                Client.Groups.RequestCurrentGroups(); // reload my groups to include the new group
            }
        }
        protected virtual void FriendshipOffer(UUID IMSessionID,string FromAgentName,UUID FromAgentID)
        {
            if(FromAgentName == myconfig.master)
            {
                Client.Friends.AcceptFriendship(FromAgentID, IMSessionID);
            }
        }

        protected virtual void RequestLure(UUID IMSessionID, string FromAgentName, UUID FromAgentID)
        {
            if (FromAgentName == myconfig.master)
            {
                Client.Self.SendTeleportLure(FromAgentID);
            }
        }

        protected virtual void RequestTeleport(UUID IMSessionID, string FromAgentName, UUID FromAgentID)
        {
            if (FromAgentName == myconfig.master)
            {
                ResetAnimations();
                SetTeleported();
                Client.Self.TeleportLureRespond(FromAgentID, IMSessionID, true);
            }
        }

        protected virtual void ChatInputHandler(object sender, ChatEventArgs e)
        {
            ConsoleLog.Debug("ChatInputHandler proc not overridden");
        }

        protected virtual void CoreCommandLib(UUID fromUUID, bool from_master, string command, string arg)
        {
            CoreCommandLib(fromUUID, from_master, command, arg, "");
        }

        protected virtual void CoreCommandLib(UUID fromUUID,bool from_master,string command,string arg,string signing_code)
        {
            ConsoleLog.Debug("CoreCommandLib proc not overridden");
        }

        public virtual void ResetAnimations()
        {
           ConsoleLog.Debug("reset_animations not enabled at this level");
        }
    }
}
