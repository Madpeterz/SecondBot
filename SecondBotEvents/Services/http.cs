using EmbedIO;
using EmbedIO.Actions;
using OpenMetaverse;
using SecondBotEvents.Config;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Services
{
    internal class http: service
    {
        protected WebServer HTTPendpoint = null;
        protected http_config myConfig;
        protected bool acceptBotCommands = false;
        public http(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new http_config(master.fromEnv,master.fromFolder);
            if (myConfig.getEnabled() == false)
            {
                Console.WriteLine(status());
                return;
            }
            master.botClientNoticeEvent += botClientRestart;
        }

        public string status()
        {
            if(myConfig == null)
            {
                return "HTTP service [Config status broken]";
            }
            if (myConfig.getEnabled() == false)
            {
                return "HTTP service [- Not requested -]";
            }
            // @todo HTTP server check here
            if(HTTPendpoint == null)
            {
                return "HTTP endpoint not online";
            }
            if (acceptBotCommands == false)
            {
                return "Active [Service]";
            }
            return "Active [Commands]";
        }

        protected void botClientRestart(object o, botClientNotice e)
        {
            acceptBotCommands = false;
            Console.WriteLine("HTTP service [Attached to new client]");
            master.botClient.client.Network.LoggedOut += botLoggedOut;
            master.botClient.client.Network.SimConnected += botLoggedIn;
        }

        protected void botLoggedOut(object o, LoggedOutEventArgs e)
        {
            acceptBotCommands = false;
            master.botClient.client.Network.SimConnected += botLoggedIn;
            Console.WriteLine("HTTP service [Bot commands disabled]");
        }

        protected void botLoggedIn(object o, SimConnectedEventArgs e)
        {
            master.botClient.client.Network.SimConnected -= botLoggedIn;
            acceptBotCommands = true;
            Console.WriteLine("HTTP service [Bot commands enabled]");
        }

        public override void start()
        {
            if (myConfig.getEnabled() == false)
            {
                Console.WriteLine("HTTP service [Disabled]");
                return;
            }
            stop();
            Console.WriteLine("HTTP service [Enabled]");
            CreateWebServer();
        }

        public override void stop()
        {
            if (myConfig.getEnabled() == false)
            {
                return;
            }
            if(HTTPendpoint != null)
            {
                HTTPendpoint.Dispose();
                HTTPendpoint = null;
                Console.WriteLine("HTTP service [Stopping]");
            }
            
        }

        private void CreateWebServer()
        {
            Swan.Logging.Logger.UnregisterLogger<ConsoleLogger>();
            HTTPendpoint = new WebServer(o => o
                 .WithUrlPrefix(myConfig.getHost())
                 .WithMode(HttpListenerMode.EmbedIO))
                .WithCors()
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" }))
            );
            HTTPendpoint.RunAsync();
        }
    }
}
