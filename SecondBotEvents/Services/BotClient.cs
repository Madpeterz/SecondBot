using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Services
{
    internal class BotClient : service
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

        protected void botLoggedOut(object o, LoggedOutEventArgs e)
        {
            Console.WriteLine("Client service ~ Logged out");
        }

        protected void botLoggedIn(object o, SimConnectedEventArgs e)
        {
            Console.WriteLine("Client service ~ Connected to sim: "+e.Simulator.Name);
        }

        protected void botLoseSim(object o, SimDisconnectedEventArgs e)
        {
            Console.WriteLine("Client service ~ Disconnected from sim: " + e.Simulator.Name);
        }

        protected void botDisconnected(object o, DisconnectedEventArgs e)
        {
            Console.WriteLine("Client service ~ Network lost: " + e.Message);
        }

        protected void botLoginStatus(object o, LoginProgressEventArgs e)
        {
            Console.WriteLine("Client service ~ Login status: " + e.Status.ToString());
        }

        protected void resetClient()
        {
            bool isRestart = false;
            if (client != null)
            {
                client.Network.Logout();
                isRestart = true;
            }
            client = null;
            client = new GridClient();
            master.triggerBotClientEvent(isRestart);
            client.Network.SimConnected += botLoggedIn;
            client.Network.LoggedOut += botLoggedOut;
            client.Network.Disconnected += botDisconnected;
            client.Network.SimDisconnected += botLoseSim;
            client.Network.LoginProgress += botLoginStatus;
        }
        public void login()
        {
            resetClient();
            LoginParams loginParams = new LoginParams(
                client,
                basicCfg.getFirstName(),
                basicCfg.getLastName(),
                basicCfg.getPassword(),
                "secondbot",
                master.getVersion()
            );
            if (basicCfg.getLoginURI() != "secondlife")
            {
                loginParams.URI = basicCfg.getLoginURI();
            }
            client.Network.BeginLogin(loginParams);
        }


    }
}
