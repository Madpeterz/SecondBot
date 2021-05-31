using BetterSecondBot;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using BetterSecondBot.bottypes;
using OpenMetaverse;
using System.Collections.Generic;
using BetterSecondBotShared.logs;
using System;

namespace BetterSecondbot.BetterAtHome
{
    public class BetterAtHome
    {
        protected Cli controler = null;
        protected JsonConfig configfile;


        protected bool LoginFailed ;
        protected string whyfailed = "";
        protected string whyloggedout = "";
        protected bool LoggingIn = true;
        protected bool LoggedIn;
        protected bool StartingLogin;
        protected long LastLoginEvent = 0;
        protected bool FiredAfterLogin;
        protected int homeregionIndexer = 0;
        protected long LastTeleportEvent = 0;
        protected List<string> homeRegions = new List<string>();
        protected List<string> avoidRestartRegions = new List<string>();
        protected string LastLoginStatus = "None";

        protected bool returnHomeDisabled = false;
        protected long returnHomeDisabledUntill = 0;



        Dictionary<string, long> AvoidSimList = new Dictionary<string, long>();
        public BetterAtHome(Cli setcontroler, JsonConfig configfile)
        {
            controler = setcontroler;
            this.configfile = configfile;
            attach_events();
            SetBetterAtHomeAction("Standby");
            foreach (string a in configfile.Basic_HomeRegions)
            {
                string[] bits = helpers.ParseSLurl(a);
                homeRegions.Add(bits[0]);
                LogFormater.Info("Added home region: " + bits[0]);
            }
            foreach (string a in configfile.Basic_AvoidRestartRegions)
            {
                string[] bits = helpers.ParseSLurl(a);
                avoidRestartRegions.Add(bits[0]);
                LogFormater.Info("Added restart region: " + bits[0]);
            }
            
        }

        protected void attach_events()
        {
            controler.getBot().ChangeSimEvent += ChangedSim;
            controler.getBot().AlertMessage += AlertMessage;
            controler.getBot().StatusMessageEvent += StatusPing;
            controler.getBot().LoginProgess += LoginProcess;
            controler.getBot().NextHomeRegion += NextHomeRegionRequest;
        }

        protected void resetSwitchs()
        {
            whyloggedout = "";
            StartingLogin = false;
            LoginFailed = false;
            LoggingIn = false;
            LoggedIn = false;
            FiredAfterLogin = false;
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
                whyloggedout = "Login failed";
                LoginFailed = true;
                whyfailed = e.FailReason + " | "+e.Message;
            }
            if (LoginFailed == false)
            {
                if (e.Status == LoginStatus.ConnectingToLogin)
                {
                    whyloggedout = "";
                    LoginFailed = false;
                    LoggingIn = true;
                    SetBetterAtHomeAction("Waiting for login to finish");
                }
                else if (e.Status == LoginStatus.ConnectingToSim)
                {
                    whyloggedout = "";
                    LoginFailed = false;
                    if (LoggedIn == false)
                    {
                        resetSwitchs();
                        LoggedIn = true;
                    }
                }
            }
        }

        protected bool hasBasicBot()
        {
            if (controler == null)
            {
                whyloggedout = "No control";
                return false;
            }
            if (controler.getBot() == null)
            {
                whyloggedout = "No Bot";
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
            if (controler.getBot().GetClient == null)
            {
                whyloggedout = "No client";
                return false;
            }
            if (controler.getBot().GetClient.Network == null)
            {
                whyloggedout = "No network";
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
            if(LoginFailed == true)
            {
                whyloggedout = "Flag: LoginFailed set";
                return false;
            }
            if(LoggingIn == true)
            {
                whyloggedout = "Flag: LoggingIn set";
                return false;
            }
            if (LoggedIn == false)
            {
                whyloggedout = "Flag: LoggedIn not set";
                return false;
            }
            whyloggedout = "";
            return controler.getBot().GetClient.Network.Connected;
        }

        protected string lastwarn = "";

        protected void SetBetterAtHomeAction(string message)
        {
            if(hasBasicBot() == true)
            {
                //controler.getBot().SetBetterAtHomeAction(whyloggedout+" "+message);
                controler.getBot().SetBetterAtHomeAction(message);
            }
            /*
             * extra debug stuff
            else
            {
                if (lastwarn != whyloggedout)
                {
                    lastwarn = whyloggedout;
                    if (whyloggedout != "")
                    {
                        LogFormater.Warn(whyloggedout);
                    }
                }
            }
            */
        }

        protected void StatusPing(object o, StatusMessageEvent e)
        {
            CleanAvoidSimList();
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
            if (controler.getBot().GetClient.Network.CurrentSim != null)
            {
                simname = controler.getBot().GetClient.Network.CurrentSim.Name;
            }
        }

        protected void AlertMessage(object o, AlertMessageEventArgs e)
        {
            if(e.Message.ToLower().Contains("restart") == true)
            {
                returnHomeDisabled = true;
                returnHomeDisabledUntill = helpers.UnixTimeNow() + (60 * 10);
                AvoidSim();
            }
        }

        protected void CleanAvoidSimList()
        {
            List<string> clear = new List<string>();
            foreach(string A in AvoidSimList.Keys)
            {
                long dif = helpers.UnixTimeNow() - AvoidSimList[A];
                if(dif > 240)
                {
                    clear.Add(A);
                }
            }
            foreach(string A in clear)
            {
                AvoidSimList.Remove(A);
            }
        }

        protected bool SimInAvoid()
        {
            return AvoidSimList.ContainsKey(simname);
        }

        protected bool IsInAvoidList(string slurl)
        {
            string[] bits = helpers.ParseSLurl(slurl);
            return AvoidSimList.ContainsKey(bits[0]);
        }

        
        protected void AvoidSim()
        {
            if(AvoidSimList.ContainsKey(simname) == false)
            {
                AvoidSimList.Add(simname, 1);
            }
            AvoidSimList[simname] = helpers.UnixTimeNow();
        }

        protected void LoggedOutActions()
        {
            SetBetterAtHomeAction("Logged Out Actions");
            if (controler.getBot().KillMe == true)
            {
                SetBetterAtHomeAction("Marked for death");
                return;
            }
            long dif = helpers.UnixTimeNow() - LastLoginEvent;
            if (LoginFailed == true)
            {
                if (whyfailed.Contains("presence") == false)
                {
                    SetBetterAtHomeAction("[DC] Unable to continue: " + whyfailed);
                    return;
                }
                else
                {
                    if (dif < 25)
                    {
                        SetBetterAtHomeAction("[DC] Clearing logged in avatar 25 secs");
                        return;
                    }
                }
            }
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
            SetBetterAtHomeAction("[DC] Attempt to recover");
            if (StartingLogin == true)
            {
                SetBetterAtHomeAction("[DC] Starting Login");
                return;
            }
            SetBetterAtHomeAction("[DC] Attempting login");
            resetSwitchs();
            StartingLogin = true;
            controler.getBot().Start();
        }

        protected void LoggedInAction()
        {
            SetBetterAtHomeAction("Logged In Actions");
            if (controler.getBot().KillMe == true)
            {
                SetBetterAtHomeAction("Marked for death");
                return;
            }
            if (SimInAvoid() == true)
            {
                SetBetterAtHomeAction("Attempting to avoid this sim");
                if (gotoAvoidRestartRegion(true) == false)
                {
                    gotoNextHomeRegion(true);
                }
                return;
            }
            long remaining = returnHomeDisabledUntill - helpers.UnixTimeNow();
            if ((returnHomeDisabled == true) && (remaining > 0))
            {
                remaining = remaining / 60;
                remaining++;
                SetBetterAtHomeAction("Avoid restart lockout - about " + remaining.ToString() + " min('s) remaining");
                return;
            }
            long dif = helpers.UnixTimeNow() - LastLoginEvent;
            if (dif < 15)
            {
                SetBetterAtHomeAction("Waiting for Login cooldown");
                return;
            }
            dif = helpers.UnixTimeNow() - LastTeleportEvent;
            if (dif < 45)
            {
                SetBetterAtHomeAction("Waiting for Teleport lockout");
                return;
            }
            if ((simname == "") || (controler.getBot().GetClient.Self.SimPosition.Z == 0))
            {
                ChangedSim(null, null);
            }
            if ((simname == "") || (controler.getBot().GetClient.Self.SimPosition.Z == 0))
            {
                void_counter++;
                SetBetterAtHomeAction("Void counter: " + void_counter.ToString());
                if (void_counter == 10)
                {
                    void_counter = 0;
                    resetSwitchs();
                    SetBetterAtHomeAction("Void detected - Resetting switchs");
                }
                return;
            }
            if (FiredAfterLogin == false)
            {
                FiredAfterLogin = true;
                controler.getBot().AfterBotLoginHandler();
                controler.getBot().reconnect = true;
                SetBetterAtHomeAction("Starting after login events");
                return;
            }
            void_counter = 0;
            if (controler.getBot().TeleportStatus() == true)
            {
                SetBetterAtHomeAction("Teleported by script/master");
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
        protected int avoidregionIndexer = -1;
        protected bool gotoAvoidRestartRegion(bool force)
        {
            if (avoidRestartRegions.Count == 0)
            {
                SetBetterAtHomeAction("No avoidRestart regions");
                return false;
            }
            avoidregionIndexer++;
            if (avoidregionIndexer >= avoidRestartRegions.Count)
            {
                SetBetterAtHomeAction("Attempted all avoidRestart regions giving up");
                avoidregionIndexer = -1;
                return false;
            }
            if (IsInAvoidList(configfile.Basic_AvoidRestartRegions[avoidregionIndexer]) == true)
            {
                if (force == true)
                {
                    return gotoAvoidRestartRegion(true);
                }
                SetBetterAtHomeAction("next avoidRegion is alreay in the avoid list");
                return false;
            }
            SetBetterAtHomeAction("Teleporting to avoid restart region: " + avoidRestartRegions[avoidregionIndexer]);
            LastTeleportEvent = helpers.UnixTimeNow();
            controler.getBot().TeleportWithSLurl(configfile.Basic_AvoidRestartRegions[avoidregionIndexer]);
            return true;
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
                if (force == true)
                {
                    gotoNextHomeRegion(false);
                }
                return;
            }
            if(IsInAvoidList(configfile.Basic_HomeRegions[homeregionIndexer]) == true)
            {
                if (force == true)
                {
                    gotoNextHomeRegion(false);
                }
                return;
            }

            SetBetterAtHomeAction("Teleporting to home region: " + homeRegions[homeregionIndexer] + "");
            LastTeleportEvent = helpers.UnixTimeNow();
            controler.getBot().TeleportWithSLurl(configfile.Basic_HomeRegions[homeregionIndexer]);
        }

        protected bool IsAtHome()
        {
            if ((simname == "") || (FiredAfterLogin == false))
            {
                if (controler.getBot().GetClient.Network.CurrentSim == null)
                {
                    return true;
                }
                simname = controler.getBot().GetClient.Network.CurrentSim.Name;
            }
            return homeRegions.Contains(simname);
        }


    }
}
