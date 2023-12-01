using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace SecondBotEvents.Services
{
    public class CustomCommandsService: BotServices
    {
        protected CustomCommandsConfig myConfig;
        protected bool botConnected = false;
        protected Dictionary<string, CustomCommand> commands = new Dictionary<string, CustomCommand>();
        public bool HasCommand(string C)
        {
            return commands.ContainsKey(C);
        }
        public KeyValuePair<bool, string> RunCommand(SignedCommand C)
        {
            if(commands.ContainsKey(C.command) == false)
            {
                return new KeyValuePair<bool, string>(false, "Unknown custom command");
            }
            CustomCommand Cc = commands[C.command];
            if(C.args.Count() < Cc.args)
            {
                return new KeyValuePair<bool, string>(false, "Incorrect number of args");
            }
            int commandsRun = 0;
            foreach(string A in Cc.steps)
            {
                string Av = A;
                int loop = 1;
                while(loop <= Cc.args)
                {
                    Av = Av.Replace("[C_ARG_"+ loop.ToString()+"]", C.args[loop - 1]);
                    loop++;
                }
                KeyValuePair<bool,string> reply = master.CommandsService.CommandInterfaceCaller(Av, false, true);
                if(reply.Key == true)
                {
                    commandsRun++;
                }
            }
            if(commandsRun == 0)
            {
                return new KeyValuePair<bool, string>(false, "all custom commands have failed for: "+C.command);
            }
            return new KeyValuePair<bool, string>(false, C.command+": ran with "+commandsRun.ToString()+" commands issued");
        }
        public CustomCommandsService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new CustomCommandsConfig(master.fromEnv, master.fromFolder);
            int loop = 1;
            while(loop <= myConfig.GetCount())
            {
                CustomCommand C = new CustomCommand();
                C.trigger = myConfig.GetCommandTrigger(loop);
                C.args = myConfig.GetCommandArgs(loop);
                int loop2 = 1;
                while(loop2 <= myConfig.GetCommandSteps(loop))
                {
                    C.steps.Add(myConfig.GetCommandStep(loop, loop2));
                    loop2++;
                }
                commands.Add(C.trigger, C);
                loop++;
            }
        }
        public override string Status()
        {
            if (myConfig == null)
            {
                return "Config broken";
            }
            else if (myConfig.GetHideStatusOutput() == true)
            {
                return "hidden";
            }
            else if (botConnected == false)
            {
                return "Waiting for client";
            }
            return "Active "+myConfig.GetCount().ToString()+" custom commands";
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            LogFormater.Info("CustomCommands service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            GetClient().Network.SimConnected += BotLoggedIn;
            LogFormater.Info("CustomCommands service [waiting for new client]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            botConnected = true;
        }

        public override void Start()
        {
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
        }

        public override void Stop()
        {
            running = false;
            master.BotClientNoticeEvent -= BotClientRestart;
        }
    }

    public class CustomCommand
    {
        public string trigger = "";
        public int args = 0;
        public List<string> steps = new List<string>();
    }
}
