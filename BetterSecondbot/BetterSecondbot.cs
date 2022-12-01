using System;
using System.Threading;
using System.Reflection;
using System.Linq;
using BetterSecondBotShared.Static;
using BetterSecondBotShared.logs;
using OpenMetaverse.Imaging;

namespace BetterSecondBot
{
    static class Program
    {
        static void Main(string[] args)
        {
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += new UnhandledExceptionEventHandler(PanicMode);
            if (helpers.notempty(Environment.GetEnvironmentVariable("Basic_BotUserName")) == true)
            {
                new CliDocker();
            }
            else
            {
                new CliHardware(args);
            }
            LogFormater.Status("- Exiting in 5 secs -");
            Thread.Sleep(5000);
        }

        static void PanicMode(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = (Exception)args.ExceptionObject;
            Console.WriteLine("PanicMode caught : " + e.Message);
            Console.WriteLine("Runtime terminating: {0}", args.IsTerminating);
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
}


