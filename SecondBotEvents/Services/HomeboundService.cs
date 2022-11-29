using OpenMetaverse;
using SecondBotEvents.Config;
using System;

namespace SecondBotEvents.Services
{
    public class HomeboundService : BotServices
    {
        protected HomeboundConfig myConfig;
        protected bool botConnected = false;


        protected SimSlURL home = null;
        protected SimSlURL backup = null;
        protected UUID sitTarget = UUID.Zero;
        protected long lastAlertTeleport = 0;
        protected bool evacWanted = false;
        protected bool gotoPosAttemptedTp = false;
        protected bool gotoPosAttemptedWalk = false;
        protected long lastGotoAction = 0;
        protected bool softDisable = false;
        protected bool closeToHome = false;

        public void MarkTeleport()
        {
            softDisable = true;
        }

        public HomeboundService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new HomeboundConfig(master.fromEnv, master.fromFolder);
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            home = new SimSlURL(myConfig.GetHomeSimSlUrl());
            backup = new SimSlURL(myConfig.GetBackupSimSLUrl());
            if(UUID.TryParse(myConfig.GetAtHomeAutoSitUuid(), out sitTarget) == false)
            {
                sitTarget = UUID.Zero;
            }
        }

        protected string simNickname = "";
        protected bool attemptedTeleportHome = false;
        protected bool attemptedTeleportBackup = false;
        protected long teleportActionLockout = 0;

        public void GoHome()
        {
            attemptedTeleportHome = true;
            LogFormater.Info("Teleporting to home sim");
            GetClient().Self.Teleport(home.name, new Vector3(home.x, home.y, home.z), new Vector3(0, 0, 0));
        }
        protected void Tick()
        {
            if (evacWanted == true)
            {
                BotAlertMessage(null, new AlertMessageEventArgs("restart", "999", new OpenMetaverse.StructuredData.OSDMap()));
            }
            if (GetClient().Network.CurrentSim.Name == home.name)
            {
                simNickname = "Home # ";
                if (sitTarget != UUID.Zero)
                {
                    if (GetClient().Self.SittingOn == 0)
                    {
                        GetClient().Self.RequestSit(sitTarget, Vector3.Zero);
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
                return;
            }
            if (simNickname == "")
            {
                simNickname = "Someplace # ";
                return;
            }
            long dif = SecondbotHelpers.UnixTimeNow() - teleportActionLockout;
            if(dif < 30)
            {
                return;
            }
            teleportActionLockout = SecondbotHelpers.UnixTimeNow();
            if (attemptedTeleportHome == false)
            {
                GoHome();
                return;
            }
            else if(attemptedTeleportBackup == false)
            {
                attemptedTeleportBackup = true;
                LogFormater.Info("Teleporting to backup sim");
                teleportActionLockout = SecondbotHelpers.UnixTimeNow();
                GetClient().Self.Teleport(backup.name, new Vector3(backup.x, backup.y, backup.z), new Vector3(0, 0, 0));
                return;
            }
            attemptedTeleportHome = false;
            attemptedTeleportBackup = false;
            LogFormater.Warn("Both home and backup sims failed clearing and trying again");
        }

        public override string Status()
        {
            if(myConfig == null)
            {
                return simNickname + "Config broken";
            }
            if(botConnected == false)
            {
                return simNickname + "No bot";
            }
            if(softDisable == false)
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
            botConnected = false;
            Console.WriteLine("Homebound Service [Attached to new client]");
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
            Console.WriteLine("Homebound Service [Standby]");
        }
        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            botConnected = true;
            Console.WriteLine("Homebound Service [Active]");
        }

        public override void Start()
        {
            if(myConfig.GetEnabled() == false)
            {
                return;
            }
            Stop();
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
            Console.WriteLine("Homebound Service [Starting]");
        }

        public override void Stop()
        {
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
            Console.WriteLine("Homebound Service [Stopping]");
        }
    }

    public class SimSlURL
    {
        public string name;
        public int x = 0;
        public int y = 0;
        public int z = 0;
        public SimSlURL(string url)
        {
            // http://maps.secondlife.com/secondlife/Viserion/66/166/23
            url = url.Replace("maps.secondlife.com/secondlife/", "");
            url = url.Replace("http://", "");
            url = url.Replace("https://", "");
            // Viserion/66/166/23
            string[] bits = url.Split("/");
            name = System.Net.WebUtility.UrlDecode(bits[0]);
            x = int.Parse(bits[1]);
            y = int.Parse(bits[2]);
            z = int.Parse(bits[3]);
        }
    }
}



