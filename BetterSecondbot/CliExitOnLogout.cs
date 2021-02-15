using BetterSecondBot.HttpService;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using BetterSecondBot.bottypes;
using System.Threading;
using static BetterSecondBot.Program;
using BetterSecondbot.BetterAtHome;

namespace BetterSecondBot
{
    public class CliExitOnLogout
    {
        public bool Exited { get { return exitBot; } }
        protected bool exitBot = false;
        public SecondBot Bot;
        protected BetterAtHome betterAtHomeService;

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
                Thread.Sleep(1000);
            }
            betterAtHomeService = null;
        }
        public CliExitOnLogout(JsonConfig Config, bool as_docker, bool use_self_keep_alive)
        {
            exitBot = false;
            LogFormater.Status("Mode: Cli [Basic]");
            if (helpers.botRequired(Config) == true)
            {
                Bot = new SecondBot();
                Bot.Setup(Config, AssemblyInfo.GetGitHash());
                betterAtHomeService = new BetterAtHome(this, Config);
                if (as_docker == true)
                {
                    Bot.AsDocker();
                }
                Bot.Start();
                if (Config.Http_Enable == true)
                {
                    /*
                    SecondBotHttpServer my_http_server = new SecondBotHttpServer();
                    my_http_server.StartHttpServer(Bot, Config);
                    */
                    new Thread(() =>
                    {
                        new HttpAsService(Bot, Config, as_docker);
                    }).Start();
                }
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
