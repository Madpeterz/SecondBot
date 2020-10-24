using BetterSecondBot.HttpServer;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using BSB.bottypes;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using static BetterSecondBot.Program;

namespace BetterSecondBot
{
    public class CliExitOnLogout
    {
        public CliExitOnLogout(JsonConfig Config, bool as_docker)
        {
            LogFormater.Status("Mode: Cli [Basic]");
            if (helpers.botRequired(Config) == true)
            {
                SecondBot Bot = new SecondBot();
                Bot.Setup(Config, AssemblyInfo.GetGitHash());
                if(as_docker == true)
                {
                    Bot.AsDocker();
                }
                Bot.Start();
                if (Config.Http_Enable == true)
                {
                    SecondBotHttpServer my_http_server = new SecondBotHttpServer();
                    my_http_server.StartHttpServer(Bot, Config);
                }
                while (Bot.KillMe == false)
                {
                    string NewStatusMessage = Bot.GetStatus();
                    if (NewStatusMessage != Bot.LastStatusMessage)
                    {
                        NewStatusMessage = NewStatusMessage.Trim();
                        if (NewStatusMessage.Replace(" ", "") != "")
                        {
                            Bot.LastStatusMessage = NewStatusMessage;
                            Bot.Log2File(LogFormater.Status(Bot.LastStatusMessage, false), ConsoleLogLogLevel.Status);
                        }
                    }
                    Thread.Sleep(1000);
                }
                Bot.GetClient.Network.Logout();
            }
            else
            {
                LogFormater.Warn("Required settings missing");
            }
        }
    }
}
