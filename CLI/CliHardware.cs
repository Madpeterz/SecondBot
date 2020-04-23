using BetterSecondBot.WikiMake;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using Newtonsoft.Json;
using System;
using static BetterSecondBot.Program;
using BetterSecondBotShared.logs;
namespace BetterSecondBot
{
    public class CliHardware
    {
        public CliHardware(string[] args)
        {
            SimpleIO io = new SimpleIO();
            JsonConfig Config = new JsonConfig();
            string json_file = "";
#if DEBUG
            ConsoleLog.Status("Hardware/Debug version");
            json_file = "debug.json";
#else
            ConsoleLog.Status("Hardware/Live version");
            if(args.Length == 1)
            {
                json_file = ""+args[0]+".json";
            }
            else
            {
                json_file = "mybot.json";
                ConsoleLog.Warn("Using: mybot.json as the config");
            }

#endif
            if (SimpleIO.dir_exists("wiki") == false)
            {
                ConsoleLog.Info("Basic Wiki [Creating]");
                new DebugModeCreateWiki(AssemblyInfo.GetGitHash(), io);
                ConsoleLog.Info("Basic Wiki [Ready]");
                io = new SimpleIO();
            }
            bool ok_to_try_start = false;
            if (SimpleIO.FileType(json_file, "json") == true)
            {
                if (io.Exists(json_file))
                {
                    string json = io.ReadFile(json_file);
                    if (json.Length > 0)
                    {
                        try
                        {
                            Config = JsonConvert.DeserializeObject<JsonConfig>(json);
                            ok_to_try_start = true;
                        }
                        catch (Exception e)
                        {
                            ConsoleLog.Warn("Unable to read config file\n moving config to " + json_file + ".old and creating a empty config\nError was: " + e.Message + "");
                            io.makeOld(json_file);
                            Config = new JsonConfig();
                            io.WriteJsonConfig(MakeJsonConfig.GetDefault(), json_file);
                        }
                    }
                    else
                    {
                        ConsoleLog.Warn("Json config is empty creating an empty one for you");
                        io.WriteJsonConfig(MakeJsonConfig.GetDefault(), json_file);
                    }
                }
                else
                {
                    ConsoleLog.Warn("Json config does not Exist creating it for you");
                    io.WriteJsonConfig(MakeJsonConfig.GetDefault(), json_file);
                }
            }
            else ConsoleLog.Crit("you must select a .json file for config! example \"BetterSecondBot.exe mybot\" will use the mybot.json file!");
            if(ok_to_try_start == true)
            {
                Config = MakeJsonConfig.http_config_check(Config);
                if (Config.HttpAsCnC == true)
                {
                    new HttpCnC(Config);
                }
                else
                {
                    new CliExitOnLogout(Config);
                }
            }
        }
    }
}
