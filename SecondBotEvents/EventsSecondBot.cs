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

            while(worker.exit() == false)
            {
                Thread.Sleep(1000);
            }
        }
    }

    internal class EventsSecondBot
    {
        public http HttpService = null;
        public BotClient botClient = null;


        private EventHandler<botClientNotice> botclient_eventNotices;
        public event EventHandler<botClientNotice> botClientNoticeEvent
        {
            add { lock (botclient_event_attach) { botclient_eventNotices += value; } }
            remove { lock (botclient_event_attach) { botclient_eventNotices -= value; } }
        }

        private readonly object botclient_event_attach = new object();

        public void triggerBotClientEvent(bool asRestart)
        {
            EventHandler<botClientNotice> handler = botclient_eventNotices;
            handler?.Invoke(this, new botClientNotice(asRestart));
        }

        public string getVersion()
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
            restartServices();
        }

        public void restartServices()
        {
            stopServices();
            startServices();
        }
        protected void startServices()
        {
            HttpService = new http(this);
            HttpService.start();
            botClient = new BotClient(this);
            if(botClient.basicCfg.isLoaded() == false)
            {
                Console.WriteLine("Config is not loaded :(");
                exitNow = true;
                return;
            }
            botClient.start();

        }
        protected void stopServices()
        {
            if (HttpService != null)
            {
                HttpService.stop();
            }
            if(botClient != null)
            {
                botClient.stop();
            }
            HttpService = null;
            botClient = null;
        }

        public bool exit()
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

    public class botClientNotice
    {
        public bool isRestart = false;
        public botClientNotice(bool asRestart=false)
        {
            isRestart = asRestart;
        }
    }
}
