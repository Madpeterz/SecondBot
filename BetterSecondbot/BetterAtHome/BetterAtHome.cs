using BetterSecondBot;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using BetterSecondBot.bottypes;
using OpenMetaverse;
using System.Collections.Generic;
using BetterSecondBotShared.logs;

namespace BetterSecondbot.BetterAtHome
{
    public class BetterAtHome
    {
        protected Cli controler = null;
        protected JsonConfig configfile;

        protected bool LoginFailed = false;
        protected bool LoggingIn = false;
        protected bool LoggedIn = false;
        protected bool StartingLogin = false;
        protected long LastLoginEvent = 0;
        protected int homeregionIndexer = 0;
        protected long LastTeleportEvent = 0;
        protected List<string> homeRegions = new List<string>();
        protected string LastLoginStatus = "None";

        public BetterAtHome(Cli setcontroler, JsonConfig configfile)
        {
            controler = setcontroler;
            this.configfile = configfile;
            attach_events();
            SetBetterAtHomeAction("Standby");
            foreach (string a in this.configfile.Basic_HomeRegions)
            {
                string[] bits = helpers.ParseSLurl(a);
                homeRegions.Add(bits[0]);
            }
        }

        protected void attach_events()
        {
            controler.Bot.ChangeSimEvent += ChangedSim;
            controler.Bot.AlertMessage += AlertMessage;
            controler.Bot.StatusMessageEvent += StatusPing;
            controler.Bot.LoginProgess += LoginProcess;
            controler.Bot.NextHomeRegion += NextHomeRegionRequest;
        }

        protected void resetSwitchs()
        {
            StartingLogin = false;
            LoginFailed = false;
            LoggingIn = false;
            LoggedIn = false;
            homeregionIndexer = -1;
            LastTeleportEvent = 0;
            LastLoginEvent = helpers.UnixTimeNow();
            SetBetterAtHomeAction("Event cooldown");
        }

        protected void NextHomeRegionRequest(object o, NextHomeRegionArgs e)
        {
            gotoNextHomeRegion(true);
        }

        protected void LoginProcess(object o, LoginProgressEventArgs e)
        {
            LastLoginStatus = e.Status.ToString();
            if (e.Status == LoginStatus.Failed)
            {
                resetSwitchs();
                LoginFailed = true;
            }
            else if(e.Status == LoginStatus.ConnectingToLogin)
            {
                resetSwitchs();
                LoggingIn = true;
                SetBetterAtHomeAction("Waiting for login to finish");
            }
            else if(e.Status == LoginStatus.Success)
            {
                resetSwitchs();
                controler.Bot.AfterBotLoginHandler();
                LoggedIn = true;
            }
        }

        protected bool hasBasicBot()
        {
            if (controler == null)
            {
                return false;
            }
            if (controler.Bot == null)
            {
                return false;
            }
            return true;
        }

        protected bool hasBot()
        {
            if(hasBasicBot() == false)
            {
                return false;
            }
            if (controler.Bot.GetClient == null)
            {
                return false;
            }
            if (controler.Bot.GetClient.Network == null)
            {
                return false;
            }
            return true;
        }

        protected bool connected()
        {
            if(hasBot() == false)
            {
                return false;
            }
            if(hasBasicBot() == false)
            {
                return false;
            }
            if(LoggingIn == true)
            {
                return false;
            }
            if (LoggedIn == false)
            {
                return false;
            }
            return controler.Bot.GetClient.Network.Connected;
        }

        protected void SetBetterAtHomeAction(string message)
        {
            if(hasBasicBot() == true)
            {
                controler.Bot.SetBetterAtHomeAction(message);
            }
        }

        protected void StatusPing(object o, StatusMessageEvent e)
        {
            if (hasBasicBot() == false)
            {
                return; // no bot (cant do anything)
            }
            if (connected() == true)
            {
                LoggedInAction();
                return;
            }
            if (LoggingIn == true)
            {
                SetBetterAtHomeAction("Logging in");
                return;
            }
            LoggedOutActions();
        }

        protected string simname = "";
        protected int void_counter = 0;
        protected void ChangedSim(object o, SimChangedEventArgs e)
        {
            simname = "";
            if (controler.Bot.GetClient.Network.CurrentSim != null)
            {
                simname = controler.Bot.GetClient.Network.CurrentSim.Name;
            }
        }

        protected void AlertMessage(object o, AlertMessageEventArgs e)
        {
            if(e.Message.Contains("Restart") == true)
            {
                gotoNextHomeRegion(true);
            }
        }

        protected void LoggedOutActions()
        {
            SetBetterAtHomeAction("Logged Out Actions");
            if (controler.Bot.KillMe == true)
            {
                SetBetterAtHomeAction("Marked for death");
                return;
            }
            long dif = helpers.UnixTimeNow() - LastLoginEvent;
            if (dif < 15)
            {
                SetBetterAtHomeAction("[DC] Waiting for login lockout");
                return;
            }
            dif = helpers.UnixTimeNow() - LastTeleportEvent;
            if (dif < 15)
            {
                SetBetterAtHomeAction("[DC] Waiting for Teleport lockout");
                return;
            }
            if (controler.Bot.KillMe == true)
            {
                SetBetterAtHomeAction("[DC] Marked for death");
                return;
            }
            SetBetterAtHomeAction("[DC] Attempt to recover");
            if (StartingLogin == true)
            {
                SetBetterAtHomeAction("[DC] Starting Login");
                return;
            }
            SetBetterAtHomeAction("[DC] Attempting login");
            StartingLogin = true;
            controler.Bot.Start();
        }

        protected void LoggedInAction()
        {
            if((simname == "") || (controler.Bot.GetClient.Self.SimPosition.Z == 0))
            {
                ChangedSim(null, null);
                void_counter++;
                SetBetterAtHomeAction("Void counter: "+ void_counter.ToString());
                if (void_counter == 10)
                {
                    resetSwitchs();
                    SetBetterAtHomeAction("Void detected - Resetting switchs");
                }
                return;
            }
            void_counter = 0;
            SetBetterAtHomeAction("Logged In Actions");
            if (controler.Bot.KillMe == true)
            {
                SetBetterAtHomeAction("Marked for death");
                return;
            }
            long dif = helpers.UnixTimeNow() - LastLoginEvent;
            if (dif < 25)
            {
                SetBetterAtHomeAction("Waiting for Login cooldown");
                return;
            }
            dif = helpers.UnixTimeNow() - LastTeleportEvent;
            if (dif < 120)
            {
                SetBetterAtHomeAction("Waiting for Teleport lockout");
                return;
            }
            if (homeRegions.Count == 0)
            {
                SetBetterAtHomeAction("No home regions");
                return;
            }
            if (IsAtHome() == true)
            {
                SetBetterAtHomeAction("AtHome");
                return;
            }
            gotoNextHomeRegion(false);
            return;
           
        }

        protected void gotoNextHomeRegion(bool force)
        {
            if (homeRegions.Count == 0)
            {
                SetBetterAtHomeAction("No home regions");
                return;
            }
            homeregionIndexer++;
            if (homeregionIndexer >= homeRegions.Count)
            {
                SetBetterAtHomeAction("Attempted all home regions restarting list");
                homeregionIndexer = -1;
                LastTeleportEvent = helpers.UnixTimeNow();
                if(force == true)
                {
                    gotoNextHomeRegion(false);
                }
                return;
            }
            SetBetterAtHomeAction("Teleporting to home region: " + homeRegions[homeregionIndexer] + "");
            LastTeleportEvent = helpers.UnixTimeNow();
            controler.Bot.TeleportWithSLurl(configfile.Basic_HomeRegions[homeregionIndexer]);
        }

        protected bool IsAtHome()
        {
            if(simname == "")
            {
                if (controler.Bot.GetClient.Network.CurrentSim == null)
                {
                    return true;
                }
                simname = controler.Bot.GetClient.Network.CurrentSim.Name;
            }
            return homeRegions.Contains(simname);
        }


    }
}
