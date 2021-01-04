using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BetterSecondBot.DiscordSupervisor
{
    public class Discord_super : DiscordEvents
    {
        protected int warmup = 0;
        protected override void keep_alive()
        {
            while (exit_super == false)
            {
                string NewStatusMessage = GetStatus();
                if (NewStatusMessage != LastStatusMessage)
                {
                    NewStatusMessage = NewStatusMessage.Trim();
                    if (DiscordClientConnected == true)
                    {
                        DiscordStatusUpdate();
                    }
                    if (NewStatusMessage.Replace(" ", "") != "")
                    {
                        if (LastStatusMessage != NewStatusMessage)
                        {
                            LastStatusMessage = NewStatusMessage;
                            if (HasBasicBot() == true)
                            {
                                controler.Bot.Log2File(LogFormater.Status(LastStatusMessage, false), ConsoleLogLogLevel.Status);
                            }
                            else
                            {
                                Console.WriteLine(LastStatusMessage);
                            }
                        }
                    }
                }
                Thread.Sleep(1000);
            }
        }
        public Discord_super(JsonConfig configfile, bool as_docker)
        {
            myconfig = configfile;
            running_as_docker = as_docker;
            startService();
        }

        protected void startService()
        {
            if (myconfig.DiscordFull_Enable == true)
            {
                _ = StartDiscordClientService();
                login_bot(false);
                keep_alive();
            }
            else
            {
                login_bot(true);
            }
        }

        protected override void login_bot(bool self_keep_alive)
        {
            controler = new CliExitOnLogout(myconfig, running_as_docker, self_keep_alive);
            attach_events();
        }

    }
}
