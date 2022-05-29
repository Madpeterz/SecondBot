using SecondBotEvents.Services;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace SecondBotEvents
{
    internal class Startup
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Secondbot [Events build] version: " + AssemblyInfo.GetGitHash());
            EventsSecondBot worker = new EventsSecondBot(args);

            while(worker.Exit() == false)
            {
                Thread.Sleep(1000);
            }
        }
    }

    public class EventsSecondBot
    {
        public HttpService HttpService = null;
        public CommandsService CommandsService = null;
        public BotClientService botClient = null;
        public DiscordService DiscordService = null;
        public InteractionService InteractionService = null;
        public DataStoreService DataStoreService = null;


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

        public void RestartServices()
        {
            StopServices();
            StartServices();
        }
        protected void StartServices()
        {
            DataStoreService = new DataStoreService(this);
            DataStoreService.Start();
            botClient = new BotClientService(this);
            HttpService = new HttpService(this);
            CommandsService = new CommandsService(this);
            DiscordService = new DiscordService(this);
            InteractionService = new InteractionService(this);
            if (botClient.IsLoaded() == false)
            {
                Console.WriteLine("Config is not loaded :(");
                exitNow = true;
                return;
            }
            HttpService.Start();
            CommandsService.Start();
            DiscordService.Start();
            InteractionService.Start();
            botClient.Start();
        }
        protected void StopServices()
        {
            if(DataStoreService != null)
            {
                DataStoreService.Stop();
            }
            if (HttpService != null)
            {
                HttpService.Stop();
            }
            if (CommandsService != null)
            {
                CommandsService.Stop();
            }
            if (DiscordService != null)
            {
                DiscordService.Stop();
            }
            if(InteractionService != null)
            {
                InteractionService.Stop();
            }
            if(botClient != null)
            {
                botClient.Stop();
            }
            HttpService = null;
            botClient = null;

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
