using BetterSecondBot;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using BetterSecondBot.bottypes;
using OpenMetaverse;
using System.Collections.Generic;

namespace BetterSecondbot.BetterAtHome
{
    public class BetterAtHome
    {
        protected CliExitOnLogout controler = null;
        protected JsonConfig configfile;

        protected bool LoginFailed = false;
        protected bool LoggingIn = false;
        protected bool LoggedIn = false;
        protected bool StartingLogin = false;
        protected long LastLoginEvent = 0;
        protected int homeregionIndexer = 0;
        protected long LastTeleportEvent = 0;
        protected List<string> homeRegions = new List<string>();

        public BetterAtHome(CliExitOnLogout setcontroler, JsonConfig configfile)
        {
            controler = setcontroler;
            this.configfile = configfile;
            attach_events();
            controler.Bot.SetBetterAtHomeAction("Standby");
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
            controler.Bot.SetBetterAtHomeAction("Event cooldown");
        }

        protected void NextHomeRegionRequest(object o, NextHomeRegionArgs e)
        {
            gotoNextHomeRegion(true);
        }

        protected void LoginProcess(object o, LoginProgressEventArgs e)
        {
            resetSwitchs();
            if(e.Status == LoginStatus.Failed)
            {
                LoginFailed = true;
            }
            else if(e.Status == LoginStatus.ConnectingToLogin)
            {
                LoggingIn = true;
                controler.Bot.SetBetterAtHomeAction("Waiting for login to finish");
            }
            else if(e.Status == LoginStatus.Success)
            {
                LoggedIn = true;
                controler.Bot.AfterBotLoginHandler();
            }
        }

        protected void StatusPing(object o, StatusMessageEvent e)
        {
            if(LoggedIn == true)
            {
                LoggedInAction();
            }
            else if(LoggingIn == false)
            {
                if(LoginFailed == false)
                {
                    controler.Bot.SetBetterAtHomeAction("Standby");
                }
                else
                {
                    LoggedOutActions();
                }
            }
        }

        protected string simname = "";
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
            controler.Bot.SetBetterAtHomeAction("LoggedOutActions");
            if (controler.Bot.KillMe == true)
            {
                controler.Bot.SetBetterAtHomeAction("Marked for death");
                return;
            }
            controler.Bot.SetBetterAtHomeAction("Attempt to recover");
            if (StartingLogin == true)
            {
                controler.Bot.SetBetterAtHomeAction("Starting Login");
                return;
            }
            long dif = helpers.UnixTimeNow() - LastLoginEvent;
            if (dif < 25)
            {
                controler.Bot.SetBetterAtHomeAction("Waiting for login lockout");
                return;
            }
            controler.Bot.SetBetterAtHomeAction("Attempting login");
            StartingLogin = true;
            controler.Bot.Start();
        }

        protected void LoggedInAction()
        {
            controler.Bot.SetBetterAtHomeAction("LoggedInAction");
            if (controler.Bot.KillMe == true)
            {
                controler.Bot.SetBetterAtHomeAction("Marked for death");
                return;
            }
            long dif = helpers.UnixTimeNow() - LastLoginEvent;
            if (dif < 25)
            {
                controler.Bot.SetBetterAtHomeAction("Waiting for Login cooldown");
                return;
            }

            dif = helpers.UnixTimeNow() - LastTeleportEvent;
            if (dif < 120)
            {
                controler.Bot.SetBetterAtHomeAction("Waiting for Teleport lockout");
                return;
            }
            if(homeRegions.Count == 0)
            {
                controler.Bot.SetBetterAtHomeAction("No home regions");
                return;
            }
            if (IsAtHome() == true)
            {
                controler.Bot.SetBetterAtHomeAction("AtHome");
                return;
            }
            gotoNextHomeRegion(false);
            return;
           
        }

        protected void gotoNextHomeRegion(bool force)
        {
            if (homeRegions.Count == 0)
            {
                controler.Bot.SetBetterAtHomeAction("No home regions");
                return;
            }
            homeregionIndexer++;
            if (homeregionIndexer >= homeRegions.Count)
            {
                controler.Bot.SetBetterAtHomeAction("Attempted all home regions restarting list");
                homeregionIndexer = -1;
                LastTeleportEvent = helpers.UnixTimeNow();
                if(force == true)
                {
                    gotoNextHomeRegion(false);
                }
                return;
            }
            controler.Bot.SetBetterAtHomeAction("Teleporting to home region: " + homeRegions[homeregionIndexer] + "");
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
