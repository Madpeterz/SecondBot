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
    class Options : API_supported_interface
    {
        [Option('j', "json", Required = false, HelpText = "see api")]
        public string Json { get; set; }

        public override int ApiCommandsCount { get { return 1; } }

        public override string[] GetCommandsList()
        { 
            return new string[] { "j"};
        }
        public override string[] GetCommandArgTypes(string cmd)
        {
            return new string[] { "Text" };
        }
        public override string[] GetCommandArgHints(string cmd)
        {
            return new string[] { "\"Bot4.json\"" };
        }

        public override string GetCommandHelp(string cmd)
        {
            return "json config file [Defaults to: mybot.json] <br/>Example: \"BetterSecondBot.exe -j=otherbot.json\"";
        }
        public override int GetCommandArgs(string cmd)
        {
            return 1;
        }
    }
}


