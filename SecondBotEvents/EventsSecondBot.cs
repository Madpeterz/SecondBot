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
            EventsSecondBot worker = new EventsSecondBot();

            while(worker.exit() == false)
            {
                Thread.Sleep(1000);
            }
        }
    }

    internal class EventsSecondBot
    {
        protected http HttpService = null;
        public EventsSecondBot()
        {
            HttpService = new http(this);
        }
        public bool exit()
        {
            return false;
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
}
