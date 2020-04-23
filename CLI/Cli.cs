using System;
using CommandLine;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using Newtonsoft.Json;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using BSB;
using BSB.bottypes;
using BetterSecondBot.HttpServer;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.API;
using BetterSecondBotShared.Static;
using BetterSecondBotShared.logs;
using BetterSecondBot.WikiMake;
namespace BetterSecondBot
{
    class Program
    {
        static void Main(string[] args)
        {
            if (helpers.notempty(Environment.GetEnvironmentVariable("BotRunningInDocker")) == true)
            {
                new CliDocker();
            }
            else
            {
                new CliHardware(args);
            }
            ConsoleLog.Status("- Exiting in 5 secs -");
            Thread.Sleep(5000);
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


