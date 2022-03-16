using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Services
{
    internal class BotClient: service
    {
        public GridClient client = null;
        public basic_config basicCfg = null;
        public BotClient(EventsSecondBot setMaster) : base(setMaster)
        {
            basicCfg = new basic_config(master.fromEnv, master.fromFolder);
        }
        public override void start()
        {
            Console.WriteLine("Client service [Starting]");
            login();
        }
        public override void stop()
        {
            Console.WriteLine("Client service [Stopping]");
            resetClient();
        }

        public void resetClient()
        {
            if (client != null)
            {
                client.Network.Logout();
            }
            client = null;
            client = new GridClient();
        }
        public void login()
        {
            if (client == null)
            {
                resetClient();
            }
            LoginParams loginParams = new LoginParams();
            loginParams.AgreeToTos = true;
            loginParams.Version = "Build: " + master.getVersion();
            loginParams.FirstName = basicCfg.getFirstName();
            loginParams.LastName = basicCfg.getLastName();
            loginParams.Password = basicCfg.getPassword();
            loginParams.Timeout = 4000;
            loginParams.URI = basicCfg.getLoginURI();
            if (loginParams.URI == "secondlife")
            {
                loginParams.URI = Settings.AGNI_LOGIN_SERVER;
            }
            client.Network.Login(loginParams);
        }
    }
}
