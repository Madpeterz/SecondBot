using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Services
{
    public class CommandsService : Services
    {
        public CommandsConfig myConfig = null;
        public bool acceptNewCommands = false;
        public CommandsService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new CommandsConfig(master.fromEnv, master.fromFolder);
        }

        Dictionary<string,int> acceptTokens = new Dictionary<string,int>();
        public bool AllowToken(string token)
        {
            if(acceptTokens.ContainsKey(token) == false)
            {
                return false;
            }
            if (acceptTokens[token] != -1)
            {
                acceptTokens[token] = acceptTokens[token] - 1;
                if(acceptTokens[token] == 0)
                {
                    acceptTokens.Remove(token);
                }
            }
            return true;
        }

        protected string SingleUseToken()
        {
            bool found = false;
            string token = "";
            while(found == false)
            {
                token = SecondbotHelpers.GetSHA1(token + SecondbotHelpers.UnixTimeNow().ToString());
                if(acceptTokens.ContainsKey(token) == false)
                {
                    addNewToken(token, 1);
                    found = true;
                }
            }
            return token;
        }

        protected void addNewToken(string token,int uses)
        {
            if (acceptTokens.ContainsKey(token) == false)
            {
                acceptTokens[token] = 0;
            }
            acceptTokens[token] = uses;
        }

        public override void Start()
        {
            master.BotClientNoticeEvent += BotClientRestart;
        }

        public override void Stop()
        {
            acceptNewCommands = false;
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            acceptNewCommands = false;
            Console.WriteLine("Commands service [Attached to new client]");
            master.botClient.client.Network.LoggedOut += BotLoggedOut;
            master.botClient.client.Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            master.botClient.client.Network.SimConnected += BotLoggedIn;
            Console.WriteLine("Commands service [Waiting for connect]");
        }

        protected void BotImMessage(object o, InstantMessageEventArgs e)
        {
            if(acceptNewCommands == false)
            {
                return;
            }
            if (e.IM.FromAgentName == master.botClient.client.Self.Name)
            {
                return;   
            }
            if (myConfig.GetAllowIMcontrol() == false)
            {
                return;
            }
            bool acceptMessage = true;
            bool requireSigning = false;
            switch (e.IM.Dialog)
            {
                case InstantMessageDialog.MessageFromObject:
                    {
                        requireSigning = true;
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        if(e.IM.GroupIM == false)
                        {
                            acceptMessage = false;
                            break;
                        }
                        if (myConfig.GetOnlySelectedAvs() == false)
                        {
                            acceptMessage = false;
                            break;
                        }
                        acceptMessage = myConfig.GetAvsCSV().Contains(e.IM.FromAgentName);
                        break;
                    }
                default:
                    {
                        acceptMessage = false;
                        break;
                    }
            }
            if (requireSigning == true)
            {
                acceptMessage = acceptMessageSigned(e.IM.Message);
            }
            if (acceptMessage == false)
            {
                return;
            }

        }

        protected bool acceptMessageSigned(string message)
        {
            return false;
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            master.botClient.client.Network.SimConnected -= BotLoggedIn;
            master.botClient.client.Self.IM += BotImMessage;
            acceptNewCommands = true;
            Console.WriteLine("Commands service [accepting IM commands]");
        }
    }
}
