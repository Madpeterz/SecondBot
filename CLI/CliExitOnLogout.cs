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
        public CliExitOnLogout(JsonConfig Config)
        {
            ConsoleLog.Status("Mode: Cli [Basic]");
            if (helpers.botRequired(Config) == true)
            {
                SecondBot Bot = new SecondBot();
                Bot.Setup(Config, AssemblyInfo.GetGitHash());
                Bot.Start();
                if (Config.Http_Enable == true)
                {
                    SecondBotHttpServer my_http_server = new SecondBotHttpServer();
                    my_http_server.StartHttpServer(Bot,Config);
                }
                while (Bot.KillMe == false)
                {
                    string NewStatusMessage = Bot.GetStatus();
                    if (NewStatusMessage != Bot.LastStatusMessage)
                    {
                        Bot.LastStatusMessage = NewStatusMessage;
                        ConsoleLog.Status(Bot.LastStatusMessage);
                    }
                    Thread.Sleep(1000);
                }
                Bot.GetClient.Network.Logout();
            }
            else
            {
                ConsoleLog.Warn("Required settings missing");
            }
        }
    }
}
