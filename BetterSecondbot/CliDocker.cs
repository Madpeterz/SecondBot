using BetterSecondBot.DiscordSupervisor;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BetterSecondBot
{
    public class CliDocker
    {
        public CliDocker()
        {
            Thread.Sleep(3000);
            LogFormater.Status("-> Getting config from Docker");
            JsonConfig Config = MakeJsonConfig.FromENV();
            Config = MakeJsonConfig.Http_config_check(Config);
            new DiscordTTS(Config,true);
        }
    }
}
