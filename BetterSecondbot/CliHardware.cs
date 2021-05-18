using BetterSecondBot.WikiMake;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using Newtonsoft.Json;
using System;
using static BetterSecondBot.Program;
using BetterSecondBotShared.logs;
using BetterSecondBot.DiscordSupervisor;

namespace BetterSecondBot
{
    public class CliHardware
    {
        public CliHardware(string[] args)
        {
            SimpleIO io = new SimpleIO();
            JsonConfig Config = new JsonConfig();
            string loadFolder = "debug";
            string json_file = "bot.json";
#if DEBUG
            LogFormater.Debug("!! RUNNING IN DEBUG !!");

#else
            LogFormater.Status("Hardware config");
            if(args.Length == 1)
            {
                loadFolder = args[0];
            }
            else
            {
                loadFolder = "default";
                io.ChangeRoot("default");
                LogFormater.Warn("Using: using default folder");
            }
#endif
            io.ChangeRoot(loadFolder);
            if (SimpleIO.DirExists("wiki") == false)
            {
                LogFormater.Info("Basic Wiki [Creating]");
                new DebugModeCreateWiki(AssemblyInfo.GetGitHash(), io);
                LogFormater.Info("Basic Wiki [Ready]");
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
                            LogFormater.Warn("Unable to read config file\n moving config to " + json_file + ".old and creating a empty config\nError was: " + e.Message + "");
                            io.MarkOld(json_file);
                            Config = new JsonConfig();
                            io.WriteJsonConfig(MakeJsonConfig.GetDefault(), json_file);
                        }
                    }
                    else
                    {
                        LogFormater.Warn("Json config is empty creating an empty one for you");
                        io.WriteJsonConfig(MakeJsonConfig.GetDefault(), json_file);
                    }
                }
                else
                {
                    LogFormater.Warn("Json config does not Exist creating it for you");
                    io.WriteJsonConfig(MakeJsonConfig.GetDefault(), json_file);
                }
            }
            else
            {
                LogFormater.Crit("you must select a .json file for config! example \"BetterSecondBot.exe mybot\" will use the mybot.json file!");
            }
            if(ok_to_try_start == true)
            {
                Config = MakeJsonConfig.Http_config_check(Config);
                new Discord_super(Config, false, loadFolder);
            }
        }
    }
}
