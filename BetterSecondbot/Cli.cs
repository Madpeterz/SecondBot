using BetterSecondBot.HttpService;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using BetterSecondBot.bottypes;
using System.Threading;
using static BetterSecondBot.Program;
using BetterSecondbot.BetterAtHome;
using BetterSecondbot.DataStorage;
using BetterSecondbot.Tracking;
using BetterSecondbot.Adverts;
using BetterSecondbot.OnEvents;

namespace BetterSecondBot
{
    public class Cli
    {
        public bool Exited { get { return exitBot; } }
        protected bool exitBot = false;
        protected string use_folder = "";
        protected SecondBot Bot = null;
        protected BetterAtHome betterAtHomeService;
        protected BetterTracking betterTracking;
        protected advertsService Adverts;
        protected onevent Events;
        protected Datastorage datastorage;

        public SecondBot getBot()
        {
            return Bot;
        }

        public bool BotReady()
        {
            if (getBot() == null)
            {
                return false;
            }
            else if (getBot().GetClient == null)
            {
                return false;
            }
            else if (getBot().GetClient.Network == null)
            {
                return false;
            }
            else if (getBot().GetClient.Network.Connected == false)
            {
                return false;
            }
            else if (getBot().GetClient.Network.CurrentSim == null)
            {
                return false;
            }
            return getBot()._AllowActions;
        }

        public string getFolderUsed()
        {
            return use_folder;
        }

        protected void keep_alive()
        {
            while (Bot.KillMe == false)
            {
                string NewStatusMessage = Bot.GetStatus();
                NewStatusMessage = NewStatusMessage.Trim();
                if (NewStatusMessage != Bot.LastStatusMessage)
                {
                    if (NewStatusMessage.Replace(" ", "") != "")
                    {
                        Bot.LastStatusMessage = NewStatusMessage;
                        Bot.Log2File(LogFormater.Status(Bot.LastStatusMessage, false), ConsoleLogLogLevel.Status);
                    }
                }
                Thread.Sleep(1500);
            }
            Adverts = null;
            Events = null;
            betterAtHomeService = null;
            betterTracking = null;
            datastorage = null;
            Bot = null;
            exitBot = true;
        }

        public void attachEventListenerObjects(JsonConfig Config, bool as_docker)
        {
            LogFormater.Info("HTTP service requested:" + Config.Http_Enable.ToString());
            if (Config.Http_Enable == true)
            {
                new Thread(() =>
                {
                    new HttpAsService(Bot, Config, as_docker);
                }).Start();
            }
            Adverts = new advertsService(this, as_docker);
            Events = new onevent(this, as_docker);
            betterAtHomeService = new BetterAtHome(this, Config);
            betterTracking = new BetterTracking(this);
            datastorage = new Datastorage(this);
        }
        public Cli(JsonConfig Config, bool as_docker, bool use_self_keep_alive, string loadingFromFolder)
        {
            exitBot = false;
            use_folder = loadingFromFolder;
            LogFormater.Info("Starting cli");
            if (helpers.botRequired(Config) == true)
            {
                Bot = new SecondBot();
                Bot.Setup(Config, AssemblyInfo.GetGitHash());
                
                if (as_docker == true)
                {
                    Bot.AsDocker();
                }

                Bot.Start(false);
                
                attachEventListenerObjects(Config, as_docker);
                if(use_self_keep_alive == true)
                {
                    keep_alive();
                }
            }
            else
            {
                LogFormater.Warn("Required settings missing");
            }
            if (use_self_keep_alive == true)
            {
                exitBot = true;
            }
        }
    }
}
