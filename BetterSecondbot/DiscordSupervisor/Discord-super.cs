using BetterSecondbot.BetterAtHome;
using BetterSecondBot.BetterRelayService;
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
        protected BetterRelay relayService;
        protected BetterAtHome betterAtHomeService;
        protected override void keep_alive()
        {
            while (exit_super == false)
            {
                if (HasBasicBot() == false)
                {
                    StatusMessageHandler(null, new BetterSecondBot.bottypes.StatusMessageEvent(false, "None"));
                }
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
                keep_alive();
            }
            else
            {
                login_bot(true);
            }
        }

        protected override void login_bot(bool self_keep_alive)
        {
            if(relayService != null)
            {
                relayService.unattach_events();
                relayService = null;
            }
            if (controler != null)
            {
                unattach_events();
                controler.Bot.KillMePlease();
                controler = null;
            }
            controler = new CliExitOnLogout(myconfig, running_as_docker, self_keep_alive);
            controler.Bot.setDiscordClient(DiscordClient);
            relayService = new BetterRelay(controler, this, running_as_docker);
            betterAtHomeService = new BetterAtHome(controler, myconfig);
            attach_events();
        }

    }
}
