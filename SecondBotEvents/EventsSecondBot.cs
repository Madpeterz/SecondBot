using SecondBotEvents.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Collections.Generic;

namespace SecondBotEvents
{
    internal class Startup
    {
        static void Main(string[] args)
        {
            LogFormater.Info("Welcome to Secondbot [Events build] version: " + AssemblyInfo.GetGitHash());
            EventsSecondBot worker = new EventsSecondBot(args);

            while(worker.Exit() == false)
            {
                if(worker.Ready == true)
                {
                    worker.Status();
                }
                Thread.Sleep(1000);
            }
            LogFormater.Warn("Shutdown in 5 secs");
            Thread.Sleep(5000);
        }
    }

    public class EventsSecondBot
    {
        public HttpService HttpService { get { return (HttpService) getService("HttpService"); } }
        public CommandsService CommandsService { get { return (CommandsService)getService("CommandsService"); } }
        public BotClientService botClient { get { return (BotClientService)getService("BotClientService"); } }
        public DiscordService DiscordService { get { return (DiscordService)getService("DiscordService"); } }
        public InteractionService InteractionService { get { return (InteractionService)getService("InteractionService"); } }
        public DataStoreService DataStoreService { get { return (DataStoreService)getService("DataStoreService"); } }
        public HomeboundService HomeboundService { get { return (HomeboundService)getService("HomeboundService"); } }


        private EventHandler<BotClientNotice> botclient_eventNotices;
        public event EventHandler<BotClientNotice> BotClientNoticeEvent
        {
            add { lock (botclient_event_attach) { botclient_eventNotices += value; } }
            remove { lock (botclient_event_attach) { botclient_eventNotices -= value; } }
        }

        private readonly object botclient_event_attach = new object();

        public void TriggerBotClientEvent(bool asRestart)
        {
            EventHandler<BotClientNotice> handler = botclient_eventNotices;
            handler?.Invoke(this, new BotClientNotice(asRestart));
        }



        public string GetVersion()
        {
            return AssemblyInfo.GetGitHash();
        }

        public bool fromEnv = false;
        public string fromFolder = "";
        protected bool exitNow = false;
        public EventsSecondBot(string[] args)
        {
            if (SecondbotHelpers.notempty(Environment.GetEnvironmentVariable("basic_username")) == true)
            {
                fromEnv = true;
            }
            fromFolder = "defaultBot";
            if (args.Length == 1)
            {
                fromFolder = args[0];
            }
            StartServices();
        }

        public bool Ready = false;

        public void RestartServices()
        {
            StopServices();
            StartServices();
        }

        protected Dictionary<string, BotServices> services = new Dictionary<string, BotServices>();

        public BotServices getService(string classname)
        {
            if(services.ContainsKey(classname) == false)
            {
                return null;
            }
            return services[classname];
        }
        protected BotServices RegisterService(string classname, bool autoStart = true)
        {
            BotServices instance = (BotServices)GetInstance("SecondBotEvents.Services." + classname);
            services.Add(classname, instance);
            lastStatus.Add(classname, "?");
            if (autoStart == true)
            {
                instance.Start();
            }
            return instance;
        }

        protected object GetInstance(string strFullyQualifiedName)
        {
            Type type = Type.GetType(strFullyQualifiedName);
            if (type != null)
                return Activator.CreateInstance(type, this);
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(strFullyQualifiedName);
                if (type != null)
                    return Activator.CreateInstance(type, this);
            }
            return null;
        }

        protected void StartServices()
        {
            services = new Dictionary<string, BotServices>();
            lastStatus = new Dictionary<string, string>();
            RegisterService("BotClientService", false);
            RegisterService("DataStoreService");
            RegisterService("HttpService");
            RegisterService("CommandsService");
            RegisterService("DiscordService");
            RegisterService("InteractionService");
            RegisterService("HomeboundService");
            if (botClient.IsLoaded() == false)
            {
                Console.WriteLine("Config is not loaded :(");
                exitNow = true;
                return;
            }
            botClient.Start();
            Ready = true;

        }

        Dictionary<string, string> lastStatus = new Dictionary<string, string>();
        long lastChange = 0;
        public void Status()
        {
            string Output = "";
            string addon = "";
            foreach(KeyValuePair<string, BotServices> A in services)
            {
                string StatusMessage = A.Value.Status();
                if(StatusMessage != lastStatus[A.Key])
                {
                    Output = Output + addon + " [" + A.Key + "] ~ " + StatusMessage;
                    lastStatus[A.Key] = StatusMessage;
                    addon = " | ";
                }
            }
            if(Output != "")
            {
                LogFormater.Info(Output);
                lastChange = SecondbotHelpers.UnixTimeNow();
                return;
            }
            long dif = SecondbotHelpers.UnixTimeNow() - lastChange;
            if(dif > 60)
            {
                string[] bits = lastStatus.Keys.ToArray();
                foreach (string A in bits)
                {
                    lastStatus[A] = "";
                }
            }
        }

        protected void StopServices()
        {
            Ready = false;
            Dictionary<string, BotServices> copy = services;
            foreach (KeyValuePair<string, BotServices> A in copy)
            {
                if(A.Key == "BotClientService")
                {
                    continue;
                }
                A.Value.Stop();
                services.Remove(A.Key);
            }
            services["BotClientService"].Stop();
            services.Remove("BotClientService");
        }

        public bool Exit()
        {
            return exitNow;
        }

    }

    public static class AssemblyInfo
    {
        /// <summary> Gets the git hash value from the assembly
        /// or null if it cannot be found. </summary>
        public static string GetGitHash()
        {
            var asm = typeof(AssemblyInfo).Assembly;
            var attrs = asm.GetCustomAttributes<AssemblyMetadataAttribute>();
            return attrs.FirstOrDefault(a => a.Key == "GitHash")?.Value;
        }
    }

    public class BotClientNotice
    {
        public bool isRestart = false;
        public BotClientNotice(bool asRestart=false)
        {
            isRestart = asRestart;
        }
    }
}
