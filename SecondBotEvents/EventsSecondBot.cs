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
#if (DEBUG)
            new WikiMake();
#endif
                LogFormater.Info("Welcome to Secondbot [Events build] version: " + AssemblyInfo.GetGitHash());
                EventsSecondBot worker = new(args);

                while (worker.Exit() == false)
                {
                    if (worker.Ready == true)
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
        public HttpService HttpService { get { return (HttpService) GetService("HttpService"); } }
        public CommandsService CommandsService { get { return (CommandsService)GetService("CommandsService"); } }
        public BotClientService BotClient { get { return (BotClientService)GetService("BotClientService"); } }
        public DiscordService DiscordService { get { return (DiscordService)GetService("DiscordService"); } }
        public InteractionService InteractionService { get { return (InteractionService)GetService("InteractionService"); } }
        public DataStoreService DataStoreService { get { return (DataStoreService)GetService("DataStoreService"); } }
        public HomeboundService HomeboundService { get { return (HomeboundService)GetService("HomeboundService"); } }
        public EventsService EventsService { get { return (EventsService)GetService("EventsService"); } }
        public DialogService DialogService { get { return (DialogService)GetService("DialogService"); } }

        public SmtpService SmtpService { get { return (SmtpService)GetService("SmtpService"); } }

        public ChatGptService ChatGptService { get { return (ChatGptService)GetService("ChatGptService"); } }

        public TriggerOnEventService TriggerOnEventService { get { return (TriggerOnEventService)GetService("TriggerOnEventService"); } }

        public CurrentOutfitFolder CurrentOutfitFolder {  get { return (CurrentOutfitFolder)GetService("CurrentOutfitFolder"); } }
        public RecoveryService RecoveryService { get { return (RecoveryService)GetService("RecoveryService"); } }
        public RLVService RLV { get { return (RLVService)GetService("RLVService"); } }

        public CustomCommandsService CustomCommandsService { get { return (CustomCommandsService)GetService("CustomCommandsService"); } }


        private EventHandler<SystemStatusMessage> SystemStatusMessages;
        public event EventHandler<SystemStatusMessage> SystemStatusMessagesEvent
        {
            add { lock (SystemStatusMessagesEventsLockable) { SystemStatusMessages += value; } }
            remove { lock (SystemStatusMessagesEventsLockable) { SystemStatusMessages -= value; } }
        }

        private readonly object SystemStatusMessagesEventsLockable = new();

        public void TriggerSystemStatusMessageEvent(bool change, string message)
        {
            EventHandler<SystemStatusMessage> handler = SystemStatusMessages;
            handler?.Invoke(this, new SystemStatusMessage(change, message));
        }

        private EventHandler<BotClientNotice> BotclientEventNotices;
        public event EventHandler<BotClientNotice> BotClientNoticeEvent
        {
            add { lock (BotclientEventNoticesLockable) { BotclientEventNotices += value; } }
            remove { lock (BotclientEventNoticesLockable) { BotclientEventNotices -= value; } }
        }
        private readonly object BotclientEventNoticesLockable = new();

        public void TriggerBotClientEvent(bool asRestart,bool asDC)
        {
            EventHandler<BotClientNotice> handler = BotclientEventNotices;
            handler?.Invoke(this, new BotClientNotice(asRestart, asDC));
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
            if (SecondbotHelpers.notempty(Environment.GetEnvironmentVariable("basic_Username")) == true)
            {
                fromEnv = true;
            }
            fromFolder = "defaultBot";
            if (args.Length == 1)
            {
                fromFolder = args[0];
            }
            LogFormater.Info("Giving SL a chance - waiting 4 secs");
            Thread.Sleep(4000);
            StartServices();
        }

        public bool Ready = false;

        public void RestartServices()
        {
            StopServices();
            StartServices();
        }

        protected Dictionary<string, BotServices> services = [];

        public BotServices GetService(string classname)
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

        public void StartServices()
        {
            Ready = false;
            services = [];
            lastStatus = [];
            StopService("RecoveryService"); // kill the recovery service if its still running
            RegisterService("BotClientService", false);
            RegisterService("DataStoreService");
            RegisterService("RLVService");
            RegisterService("CurrentOutfitFolder");
            RegisterService("HttpService");
            RegisterService("CommandsService");
            RegisterService("DiscordService");
            RegisterService("InteractionService");
            RegisterService("HomeboundService");
            RegisterService("EventsService");
            RegisterService("DialogService");
            RegisterService("CustomCommandsService");
            RegisterService("TriggerOnEventService");
            RegisterService("RelayService");
            RegisterService("ChatGptService");
            //RegisterService("RecoveryService");
            RegisterService("SmtpService");
            if (BotClient.IsLoaded() == false)
            {
                LogFormater.Info("Config is not loaded :(");
                exitNow = true;
                return;
            }
            BotClient.Start();
            Ready = true;
        }

        Dictionary<string, string> lastStatus = [];
        public void Status()
        {
            string Output = "";
            string addon = "";
            foreach(KeyValuePair<string, BotServices> A in services)
            {
                string StatusMessage = A.Value.Status();
                if (StatusMessage != lastStatus[A.Key])
                {
                    Output = Output + addon + " [" + A.Key + "] ~ " + StatusMessage;
                    lastStatus[A.Key] = StatusMessage;
                    addon = " | ";
                }
            }
            if(Output != "")
            {
                TriggerSystemStatusMessageEvent(true, Output);
                LogFormater.Info(Output);
                return;
            }
            TriggerSystemStatusMessageEvent(false, "");
        }

        public void StopService(string service)
        {
            if (services.ContainsKey(service) == false)
            {
                return;
            }
            services[service].Stop();
            services.Remove(service);
        }


        public void StopServices(string skip="")
        {
            Ready = false;
            Dictionary<string, BotServices> copy = services;
            foreach (KeyValuePair<string, BotServices> A in copy)
            {
                if((A.Key == "BotClientService") || (A.Key == skip))
                {
                    continue;
                }
                A.Value.Stop();
                services.Remove(A.Key);
            }
            if (skip == "")
            {
                services["BotClientService"].Stop();
                services.Remove("BotClientService");
            }
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
        public bool isDC = false;
        public bool isStart = false;
        public BotClientNotice(bool asRestart=false, bool asDC=false)
        {
            isRestart = asRestart;
            isDC = asDC;
            if((isRestart == false) && (isDC == false))
            {
                isStart = true;
            }
        }
    }

    public class SystemStatusMessage(bool setChanged, string setMessage)
    {
        public bool changed = setChanged;
        public string message = setMessage;
    }
}
