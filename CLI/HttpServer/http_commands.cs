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
    public abstract class HTTPCommandsInterface : API_supported_interface
    {
        protected SecondBot bot;
        protected http_server httpserver;

        protected HTTPCommandsInterface(SecondBot linktobot, http_server sethttpserver)
        {
            bot = linktobot;
            httpserver = sethttpserver;
        }

        public string[] Call(string command, string arg)
        {
            command = command.ToLowerInvariant();
            HTTP_commands cmd = (HTTP_commands)GetCommand(command);
            if (cmd != null)
            {
                cmd.Setup(bot,httpserver);
                return cmd.CallFunction(arg.Split(new[] { "~#~" }, StringSplitOptions.None));
            }
            else
            {
                ConsoleLog.Debug("" + command + " [Failed]: I have no fucking idea what you are talking about");
                return new[] { "Failed", "I have no fucking idea what you are talking about" };
            }
        }
    }
    public class HTTPCommandsInterfaceGet : HTTPCommandsInterface
    {
        public HTTPCommandsInterfaceGet(SecondBot linktobot, http_server sethttpserver) : base(linktobot, sethttpserver) 
        {
            API_type = typeof(HTTP_commands_get);
            LoadCommandsList();
        } 
    }
    public class HTTPCommandsInterfacePost : HTTPCommandsInterface
    {
        public HTTPCommandsInterfacePost(SecondBot linktobot, http_server sethttpserver) : base(linktobot, sethttpserver) 
        {
            API_type = typeof(HTTP_commands_post);
            LoadCommandsList();
        } 
    }


    public abstract class HTTP_commands_get : HTTP_commands
    {
    }
    public abstract class HTTP_commands_post : HTTP_commands
    {
    }

    public abstract class HTTP_commands : API_interface
    {
        protected SecondBot bot;
        protected http_server httpserver;
        public void Setup(SecondBot setBot,http_server Sethttpserver)
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
            if (args.Length >= MinArgs)
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
