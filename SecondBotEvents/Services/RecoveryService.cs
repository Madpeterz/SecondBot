using OpenMetaverse;
using SecondBotEvents.Config;
using System.Threading;
using Timer = System.Timers.Timer;
namespace SecondBotEvents.Services
{
    public class RecoveryService : BotServices
    {
        protected Timer AutoRestartLoginTimer;
        protected new BasicConfig myConfig;
        public RecoveryService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new BasicConfig(false);
        }

        public override string Status()
        {
            return "";
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if(e.isStart == true)
            {
                LogFormater.Info("RecoveryService [Attached to new client]");
            }
            else if(e.isDC == true)
            {
                TriggerRecovery();
            }
            
        }
        public void TriggerRecovery()
        {
            LogFormater.Info("RecoveryService [Triggered]");
            if(master.BotClient != null)
            {
                if(master.BotClient.client != null)
                {
                    if(master.BotClient.client.Network != null)
                    {
                        master.BotClient.client.Network.BeginLogout(); // attempt to flag logout
                    }
                }
            }
            if (master.BotClient != null)
            {
                master.BotClient.flagLogoutExpected();
            }
            master.StopServices("RecoveryService"); // stop everything but recovery and the core bot service
            Thread.Sleep(2 * 1000); // give it 2 secs
            master.StopService("BotClientService");
            Thread.Sleep(75 * 1000); // wait 75 secs
            master.StartServices();
        }

        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            if (updateEnabled)
            {
                myConfig.setEnabled(setEnabledTo);
            }
            Stop();
            master.BotClientNoticeEvent += BotClientRestart;
            LogFormater.Info("Relay Service [Starting]");
        }

        public override void Stop()
        {
            if (running == true)
            {
                LogFormater.Info("Relay Service [Stopping]");
            }

        }
    }
}

