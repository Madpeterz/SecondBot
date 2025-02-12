using Discord.Rest;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Linq;

namespace SecondBotEvents.Services
{
    public class HomeboundService : BotServices
    {
        protected new HomeboundConfig myConfig;
        protected bool botConnected = false;


        protected SimSlURL home = null;
        protected SimSlURL backup = null;
        protected UUID sitTarget = UUID.Zero;
        protected long lastAlertTeleport = 0;
        protected bool evacWanted = false;
        protected bool evacTeleport = false;
        protected bool gotoPosAttemptedTp = false;
        protected bool gotoPosAttemptedWalk = false;
        protected long lastGotoAction = 0;
        protected long lastTeleportHomeAttempt = 0;
        protected bool softDisable = false;
        protected bool closeToHome = false;
        protected bool softDisableSit = false;

        public void MarkTeleport()
        {
            softDisable = true;
        }

        public void MarkStandup()
        {
            softDisableSit = true;
        }

        public HomeboundService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new HomeboundConfig(master.fromEnv, master.fromFolder);
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            home = new SimSlURL(myConfig.GetHomeSimSlUrl());
            if (home.name == "null")
            {
                myConfig.setEnabled(false);
                return;
            }
            backup = new SimSlURL(myConfig.GetBackupSimSLUrl());
            if(backup.name == "null")
            {
                LogFormater.Info("No backup sim was found using a default");
                backup = new SimSlURL("WELCOMEHUB/130/86/25");
            }
            if(UUID.TryParse(myConfig.GetAtHomeAutoSitUuid(), out sitTarget) == false)
            {
                sitTarget = UUID.Zero;
            }
        }

        protected string simNickname = "";
        protected bool attemptedTeleportHome = false;
        protected bool attemptedTeleportBackup = false;
        protected long teleportActionLockout = 0;

        public bool GoHome()
        {
            attemptedTeleportHome = true;
            lastTeleportHomeAttempt = SecondbotHelpers.UnixTimeNow();
            LogFormater.Info("Teleporting to home sim");
            return GetClient().Self.Teleport(home.name, new Vector3(home.x, home.y, home.z), new Vector3(0, 0, 0));
        }
        protected void Tick()
        {
            if (evacWanted == true)
            {
                BotAlertMessage(null, new AlertMessageEventArgs("restart", "999", []));
            }
            if (GetClient().Network.CurrentSim.Name == home.name)
            {
                simNickname = "Home # ";
                evacTeleport = false;
                if (sitTarget != UUID.Zero)
                {
                    if (GetClient().Self.SittingOn == 0)
                    {
                        if (softDisableSit == false)
                        {
                            GetClient().Self.RequestSit(sitTarget, Vector3.Zero);
                        }
                    }
                }
                if (myConfig.GetAtHomeSeekLocation() == true)
                {
                    GetToPos(home);
                }
                return;
            }
            else if (GetClient().Network.CurrentSim.Name == backup.name)
            {
                simNickname = "Backup # ";
                if (myConfig.GetAtBackupSeekLocation() == true)
                {
                    GetToPos(backup);
                }
                TryGoHome();
                return;
            }
            if (simNickname == "")
            {
                if (GetClient().Network.CurrentSim != null)
                {
                    simNickname = ""+ GetClient().Network.CurrentSim.Name+" # ";
                }
                else
                {
                    simNickname = "Someplace # ";
                }
            }
            TryGoHome();
        }

        protected void TryGoHome()
        {
            long dif = SecondbotHelpers.UnixTimeNow() - teleportActionLockout;
            if (dif < 30)
            {
                return;
            }
            teleportActionLockout = SecondbotHelpers.UnixTimeNow();
            if (attemptedTeleportHome == false)
            {
                dif = SecondbotHelpers.UnixTimeNow() - lastTeleportHomeAttempt;
                if ((dif / 60) >= myConfig.GetReturnToHomeSimAfterMins())
                {
                    attemptedTeleportHome = false;
                    evacTeleport = false;
                    LogFormater.Info("trying to return to home sim");
                    if (GoHome() == false)
                    {
                        LogFormater.Warn("Unable to teleport to home sim");
                    }
                }
                return;
            }
            else if (attemptedTeleportBackup == false)
            {
                attemptedTeleportBackup = true;
                LogFormater.Info("Teleporting to backup sim");
                teleportActionLockout = SecondbotHelpers.UnixTimeNow();
                if (GetClient().Self.Teleport(backup.name, new Vector3(backup.x, backup.y, backup.z), new Vector3(0, 0, 0)) == false)
                {
                    LogFormater.Warn("Unable to teleport to backup sim");
                }
                return;
            }
            attemptedTeleportHome = false;
            attemptedTeleportBackup = false;
            LogFormater.Warn("Both home and backup sims failed clearing and trying again");
        }

        public override string Status()
        {
            if (myConfig == null)
            {
                return "No Config";
            }
            else if (myConfig.GetHideStatusOutput() == true)
            {
                return "hidden";
            }
            else if (botConnected == false)
            {
                return simNickname + "No bot";
            }
            else if(softDisable == false)
            {
                Tick();
                if(evacWanted == true)
                {
                    return simNickname + "Waiting to evac";
                }
                else if(GetClient().Self.SittingOn != 0)
                {
                    return simNickname + "Sitting on a thing";
                }
                else if(evacTeleport == true)
                {
                    return simNickname + "Evac teleport";
                }
                else if(closeToHome == true)
                {
                    return simNickname + "In a safe space";
                }
                else if(gotoPosAttemptedWalk == true)
                {
                    return simNickname + "Trying to auto pilot";
                }
                else if(gotoPosAttemptedTp == true)
                {
                    return simNickname + "Trying to TP to location";
                }
                return simNickname + "Waiting";
            }
            return simNickname + "Standby - teleported by master / script";
        }

        protected void GetToPos(SimSlURL location)
        {
            if (GetClient().Self.SittingOn != 0)
            {
                return;
            }
            long dif = SecondbotHelpers.UnixTimeNow() - lastGotoAction;
            if (dif < 30)
            {
                return;
            }
            lastGotoAction = SecondbotHelpers.UnixTimeNow();
            float dist = Vector3.Distance(new Vector3(location.x, location.y, location.z), GetClient().Self.SimPosition);
            closeToHome = false;
            if (dist < 1)
            {
                closeToHome = true;
                if (gotoPosAttemptedWalk == true)
                {
                    GetClient().Self.AutoPilotCancel();
                }
                gotoPosAttemptedWalk = false;
                gotoPosAttemptedTp = false;
                return;
            }
            if ((gotoPosAttemptedWalk == false) && (dist < 16))
            {
                gotoPosAttemptedWalk = true;
                GetClient().Self.AutoPilotLocal(location.x, location.y, GetClient().Self.SimPosition.Z);
                return;
            }
            if (gotoPosAttemptedTp == false)
            {
                gotoPosAttemptedTp = true;
                GetClient().Self.Teleport(location.name, new Vector3(location.x, location.y, location.z), new Vector3(0, 0, 0));
                return;
            }
            GetClient().Self.AutoPilotCancel();
            gotoPosAttemptedWalk = false;
        }

        protected void BotChangedSim(object o, SimChangedEventArgs e)
        {
            gotoPosAttemptedTp = false;
            evacWanted = false;
            gotoPosAttemptedWalk = false;
            closeToHome = false;
            simNickname = "";
            attemptedTeleportHome = false;
            attemptedTeleportBackup = false;
            lastGotoAction = SecondbotHelpers.UnixTimeNow();
            teleportActionLockout = SecondbotHelpers.UnixTimeNow() + 30;
        }

        protected void BotAlertMessage(object o, AlertMessageEventArgs e)
        {
            if (e.Message.ToLower().Contains("restart") == false)
            {
                return;
            }
            long dif = SecondbotHelpers.UnixTimeNow() - lastAlertTeleport;
            if (dif < 60)
            {
                if (evacWanted == false)
                {
                    LogFormater.Warn("Evac wanted due to restart");
                    evacWanted = true;
                }
                return;
            }
            evacWanted = false;
            evacTeleport = true;
            if (GetClient().Network.CurrentSim.Name == home.name)
            {
                LogFormater.Info("Teleporting to backup sim to avoid restart");
                GetClient().Self.Teleport(backup.name, new Vector3(backup.x, backup.y, backup.z), new Vector3(0, 0, 0));
                return;
            }
            LogFormater.Info("Teleporting to home sim to avoid restart");
            GetClient().Self.Teleport(home.name, new Vector3(home.x, home.y, home.z), new Vector3(0, 0, 0));
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isStart == false)
            {
                return;
            }
            botConnected = false;
            LogFormater.Info("Homebound Service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
            GetClient().Self.AlertMessage += BotAlertMessage;
            GetClient().Network.SimChanged += BotChangedSim;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            lastAlertTeleport = 0;
            evacWanted = false;
            botConnected = false;
            gotoPosAttemptedTp = false;
            evacWanted = false;
            gotoPosAttemptedWalk = false;
            softDisable = false;
            closeToHome = false;
            lastGotoAction = SecondbotHelpers.UnixTimeNow();
            LogFormater.Info("Homebound Service [Standby]");
        }
        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            botConnected = true;
            LogFormater.Info("Homebound Service [Active]");
        }

        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            if (updateEnabled)
            {
                myConfig.setEnabled(setEnabledTo);
            }
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            Stop();
            if(master.RLV.Enabled == true)
            {
                myConfig.setEnabled(false);
                LogFormater.Info("Homebound Service [Disabled RLV enabled]");
                return;
            }
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
            LogFormater.Info("Homebound Service [Starting]");
        }

        public override void Stop()
        {
            if(running == true)
            {
                LogFormater.Info("Homebound Service [Stopping]");
            }
            running = false;
            master.BotClientNoticeEvent -= BotClientRestart;
            if (master.BotClient != null)
            {
                if (GetClient() != null)
                {
                    GetClient().Network.SimConnected -= BotLoggedIn;
                    GetClient().Self.AlertMessage -= BotAlertMessage;
                    GetClient().Network.SimChanged -= BotChangedSim;
                }
            }
            
        }
    }

    public class SimSlURL
    {
        public string name;
        public int x = 0;
        public int y = 0;
        public int z = 0;
        public SimSlURL(string? url)
        {
            if(url == null)
            {
                name = "null";
                x = 0;
                y = 0;
                z = 0;
                return;
            }
            // http://maps.secondlife.com/secondlife/Viserion/66/166/23
            url = url.Replace("maps.secondlife.com/secondlife/", "");
            url = url.Replace("http://", "");
            url = url.Replace("https://", "");
            // Viserion/66/166/23
            string[] bits = url.Split("/");
            if(bits.Count() != 4)
            {
                name = "null";
                x = 0;
                y = 0;
                z = 0;
                return;
            }
            name = System.Net.WebUtility.UrlDecode(bits[0]);
            x = int.Parse(bits[1]);
            y = int.Parse(bits[2]);
            z = int.Parse(bits[3]);
        }
    }
}



