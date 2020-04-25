using BetterSecondBot.HttpServer;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace BetterSecondBot
{
    class HttpCnC
    {
        protected http_server my_http_server;
        public HttpCnC(JsonConfig BotConfig)
        {
            ConsoleLog.Status("Mode: HTTP C&C");
            my_http_server = new http_server();
            my_http_server.HTTPCnCmode = true;
            my_http_server.ShutdownHTTP = false;
            my_http_server.start_http_server(BotConfig);
            string last_cnc_status = "";
            while (my_http_server.ShutdownHTTP == false)
            {
                string NewStatusMessage =  my_http_server.GetStatus();
                if (NewStatusMessage != last_cnc_status)
                {
                    last_cnc_status = NewStatusMessage;
                    ConsoleLog.Status(last_cnc_status);
                }
                Thread.Sleep(1000);
            }
            ConsoleLog.Status("Shutting down now");
            if (my_http_server.GetBot != null)
            {
                my_http_server.GetBot.GetClient.Network.Logout();
            }
        }
    }
}
