using BetterSecondBot.HttpService;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using BetterSecondBot.bottypes;
using System.Threading;
using static BetterSecondBot.Program;
using BetterSecondbot.BetterAtHome;
using BetterSecondbot.DataStorage;

namespace BetterSecondBot
{
    public class Cli
    {
        public bool Exited { get { return exitBot; } }
        protected bool exitBot = false;
        public SecondBot Bot;
        protected BetterAtHome betterAtHomeService;
        protected Datastorage datastorage;

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
            betterAtHomeService = null;
        }

        public void attachEventListenerObjects(JsonConfig Config, bool as_docker)
        {
            if (Config.Http_Enable == true)
            {
                new Thread(() =>
                {
                    new HttpAsService(Bot, Config, as_docker);
                }).Start();
            }
            betterAtHomeService = new BetterAtHome(this, Config);
            datastorage = new Datastorage(this);
        }
        public Cli(JsonConfig Config, bool as_docker, bool use_self_keep_alive)
        {
            exitBot = false;
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
