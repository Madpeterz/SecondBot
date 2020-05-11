using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using BSB;
using BSB.bottypes;
using BetterSecondBotShared.API;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;

namespace BetterSecondBot.HttpServer
{
    public class HTTPCommandsInterface : API_supported_interface
    {
        protected SecondBot bot;
        protected SecondBotHttpServer httpserver;

        public HTTPCommandsInterface(SecondBot linktobot, SecondBotHttpServer sethttpserver)
        {
            bot = linktobot;
            httpserver = sethttpserver;
            API_type = typeof(HTTP_commands);
            LoadCommandsList();
        }

        public string[] Call(string command, string arg)
        {
            command = command.ToLowerInvariant();
            HTTP_commands cmd = (HTTP_commands)GetCommand(command);
            if (cmd != null)
            {
                cmd.Setup(bot,httpserver);
                return cmd.CallFunction(arg.Split(new[] { "|||" }, StringSplitOptions.None));
            }
            else
            {
                ConsoleLog.Debug("" + command + " [Failed]: I have no fucking idea what you are talking about");
                return new[] { "Failed", "I have no fucking idea what you are talking about" };
            }
        }
    }

    public abstract class HTTP_commands : API_interface
    {
        protected SecondBot bot;
        protected SecondBotHttpServer httpserver;
        public void Setup(SecondBot setBot,SecondBotHttpServer Sethttpserver)
        {
            bot = setBot;
            httpserver = Sethttpserver;
        }
        public virtual string[] CallFunction(string[] args)
        {
            if (ArgsCheck(args) == true)
            {
                return new[] { "OK", "No override" };
            }
            return Failed("Incorrect number of args");
        }
        protected bool ArgsCheck(string[] args)
        {
            if (args.Length >= Min_Required_args)
            {
                return true;
            }
            return false;
        }
        protected string[] Failed(string why_failed)
        {
            ConsoleLog.Warn("HTTP: " + CommandName + " [Failed]: " + why_failed);
            return new [] {"Failed",why_failed};
        }
    }


}
