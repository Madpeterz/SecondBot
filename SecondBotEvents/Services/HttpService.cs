using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.WebApi;
using OpenMetaverse;
using SecondBotEvents.Commands;
using SecondBotEvents.Config;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SecondBotEvents.Services
{
    public class HttpService : Services
    {
        protected WebServer HTTPendpoint = null;
        protected HttpConfig myConfig;
        protected bool acceptBotCommands = false;
        public HttpService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new HttpConfig(master.fromEnv, master.fromFolder);
            if (myConfig.GetEnabled() == false)
            {
                Console.WriteLine(Status());
                return;
            }
        }

        public override string Status()
        {
            if (myConfig == null)
            {
                return "HTTP service [Config status broken]";
            }
            if (myConfig.GetEnabled() == false)
            {
                return "HTTP service [- Not requested -]";
            }
            // @todo HTTP server check here
            if (HTTPendpoint == null)
            {
                return "HTTP endpoint not online";
            }
            if (acceptBotCommands == false)
            {
                return "Active [Service]";
            }
            return "Active [Commands]";
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            acceptBotCommands = false;
            Console.WriteLine("HTTP service [Attached to new client]");
            getClient().Network.LoggedOut += BotLoggedOut;
            getClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            acceptBotCommands = false;
            getClient().Network.SimConnected += BotLoggedIn;
            Console.WriteLine("HTTP service [Bot commands disabled]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            getClient().Network.SimConnected -= BotLoggedIn;
            acceptBotCommands = true;
            Console.WriteLine("HTTP service [Bot commands enabled]");
        }

        public override void Start()
        {
            if (myConfig.GetEnabled() == false)
            {
                Console.WriteLine("HTTP service [Disabled]");
                return;
            }
            Stop();
            master.BotClientNoticeEvent += BotClientRestart;
            Console.WriteLine("HTTP service [Enabled]");
            CreateWebServer();
        }

        public override void Stop()
        {
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            master.BotClientNoticeEvent -= BotClientRestart;
            if (HTTPendpoint != null)
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
                 .WithUrlPrefix(myConfig.GetHost())
                 .WithMode(HttpListenerMode.EmbedIO))
                .WithCors()
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" }))
            );
            HTTPendpoint.RunAsync();
        }
    }

    public abstract class CommandsAPI
    {
        protected EventsSecondBot master;
        public CommandsAPI(EventsSecondBot setmaster)
        {
            master = setmaster;
        }

        protected UUID avataruuid = UUID.Zero;
        protected Parcel targetparcel = null;

        protected GridClient getClient()
        {
            return master.botClient.client;
        }

        protected KeyValuePair<bool, string> SetupCurrentParcel()
        {
            if (getClient().Network.CurrentSim == null)
            {
                return new KeyValuePair<bool, string>(false, "Error not in a sim");
            }
            int localid = getClient().Parcels.GetParcelLocalID(getClient().Network.CurrentSim, getClient().Self.SimPosition);
            if (getClient().Network.CurrentSim.Parcels.ContainsKey(localid) == false)
            {
                Thread.Sleep(2000);
                localid = getClient().Parcels.GetParcelLocalID(getClient().Network.CurrentSim, getClient().Self.SimPosition);
                if (getClient().Network.CurrentSim.Parcels.ContainsKey(localid) == false)
                {
                    return new KeyValuePair<bool, string>(false, "Parcel data not ready");
                }
            }
            targetparcel = getClient().Network.CurrentSim.Parcels[localid];
            return new KeyValuePair<bool, string>(true, "");
        }

        protected void ProcessAvatar(string avatar)
        {
            avataruuid = UUID.Zero;
            string UUIDfetch = master.DataStoreService.getAvatarUUID(avatar);
            if (UUIDfetch != "lookup")
            {
                UUID.TryParse(UUIDfetch, out avataruuid);
            }
        }

        protected void SuccessNoReturn(string command)
        {
            SuccessNoReturn(command, new string[] { });
        }
        protected void SuccessNoReturn(string command, string[] args)
        {
            // @todo Command history storage system
        }

        protected object BasicReply(string input, string command)
        {
            return BasicReply(input, command, new string[] { });
        }

        protected object BasicReply(string input)
        {
            return BasicReply(input, getCallingCommand(), new string[] { });
        }

        protected object BasicReply(string input, string command, string[] args)
        {
            // @todo Command history storage system
            return new { reply = input, status = true };
        }

        protected object Failure(string input, string command, string[] args, string whyfailed)
        {
            // @todo Command history storage system
            return new { reply = input, status = false };
        }

        protected object Failure(string input, string command, string[] args)
        {
            return Failure(input, command, args, null);
        }


        protected object Failure(string input, string[] args)
        {
            return Failure(input, getCallingCommand(), args, null);
        }

        protected object Failure(string input)
        {
            return Failure(input, getCallingCommand(), new string[] { }, null);
        }

        protected string getCallingCommand()
        {
            return (new System.Diagnostics.StackTrace()).GetFrame(2).GetMethod().Name;
        }

        protected object Failure(string input, string command)
        {
            return Failure(input, command, new string[] { });
        }

    }


    [AttributeUsage(AttributeTargets.Method)]
    public class About : Attribute
    {
        public string about = "";
        public About(string about)
        {
            this.about = about;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReturnHints : Attribute
    {
        public string hint = "";
        public bool good = true;
        public ReturnHints(string hint)
        {
            this.hint = hint;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReturnHintsFailure : ReturnHints
    {
        public ReturnHintsFailure(string hint) : base(hint)
        {
            this.good = false;
        }
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ArgHints : Attribute
    {
        public string name = "";
        public string about = "";

        public ArgHints(string name, string about)
        {
            this.name = name;
            this.about = about;
        }
    }
}
