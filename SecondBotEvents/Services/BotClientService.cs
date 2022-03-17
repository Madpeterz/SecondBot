using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Services
{
    public class BotClientService : Services
    {
        public GridClient client = null;
        protected BasicConfig basicCfg = null;
        public BotClientService(EventsSecondBot setMaster) : base(setMaster)
        {
            basicCfg = new BasicConfig(master.fromEnv, master.fromFolder);
        }

        public bool IsLoaded()
        {
            return basicCfg.IsLoaded();
        }

        public override void Start()
        {
            Console.WriteLine("Client service [Starting]");
            Login();
        }
        public override void Stop()
        {
            Console.WriteLine("Client service [Stopping]");
            ResetClient();
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            Console.WriteLine("Client service ~ Logged out");
        }

        protected void BotSimConnected(object o, SimConnectedEventArgs e)
        {
            Console.WriteLine("Client service ~ Connected to sim: "+e.Simulator.Name);
        }

        protected void BotSimDisconnected(object o, SimDisconnectedEventArgs e)
        {
            Console.WriteLine("Client service ~ Disconnected from sim: " + e.Simulator.Name);
        }

        protected void BotDisconnected(object o, DisconnectedEventArgs e)
        {
            Console.WriteLine("Client service ~ Network disconnected: " + e.Message);
        }

        protected void BotLoginStatus(object o, LoginProgressEventArgs e)
        {
            Console.WriteLine("Client service ~ Login status: " + e.Status.ToString());
        }

        protected void ResetClient()
        {
            bool isRestart = false;
            if (client != null)
            {
                client.Network.Logout();
                isRestart = true;
            }
            client = null;
            client = new GridClient();
            master.TriggerBotClientEvent(isRestart);
            client.Network.SimConnected += BotSimConnected;
            client.Network.LoggedOut += BotLoggedOut;
            client.Network.Disconnected += BotDisconnected;
            client.Network.SimDisconnected += BotSimDisconnected;
            client.Network.LoginProgress += BotLoginStatus;
        }
        public void Login()
        {
            ResetClient();
            LoginParams loginParams = new LoginParams(
                client,
                basicCfg.GetFirstName(),
                basicCfg.GetLastName(),
                basicCfg.GetPassword(),
                "secondbot",
                master.GetVersion()
            );
            if (basicCfg.GetLoginURI() != "secondlife")
            {
                loginParams.URI = basicCfg.GetLoginURI();
            }
            client.Network.BeginLogin(loginParams);
        }


    }
}
