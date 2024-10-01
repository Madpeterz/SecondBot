using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Services;
using System.Collections.Generic;
using System.Linq;


namespace SecondBotEvents.Commands
{
    [ClassInfo("Control services via commands (Requires the command enable service control flag be set to true)<br/><hr/> controlable services:<br/><hr/>" +
        "BotClientService<br/>"+
        "RLVService<br/>" +
        "HttpService<br/>" +
        "DiscordService<br/>" +
        "InteractionService<br/>" +
        "HomeboundService<br/>" +
        "EventsService<br/>" +
        "DialogService<br/>" +
        "TriggerOnEventService<br/>" +
        "RelayService<br/>" +
        "ChatGptService")]
    public class Services : CommandsAPI
    {
        public Services(EventsSecondBot setmaster) : base(setmaster)
        {
        }

        protected List<string> controlable = new List<string> { 
            "BotClientService",
            "RLVService",
            "HttpService",
            "DiscordService",
            "InteractionService",
            "HomeboundService",
            "EventsService",
            "DialogService",
            "TriggerOnEventService",
            "RelayService",
            "ChatGptService"
        };

        [About("Marks a service as enabled and starts it")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Unknown service")]
        [ReturnHintsFailure("Unable to manage selected service")]
        [ReturnHintsFailure("Service control is not enabled by config")]
        public object EnableAndStartService(string servicename)
        {
            if(master.CommandsService.GetAllowServiceCommands() == false)
            {
                return Failure("Service control is not enabled by config");
            }
            if(controlable.Contains(servicename) == false)
            {
                return Failure("Unknown service");
            }
            BotServices service = master.GetService(servicename);
            if (service == null)
            {
                return Failure("Unknown service");
            }
            service.enableAndStart();
            return BasicReply("ok");
        }

        [About("stops a service if it is running")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Unknown service")]
        [ReturnHintsFailure("Unable to manage selected service")]
        [ReturnHintsFailure("Service control is not enabled by config")]
        public object StopService(string servicename)
        {
            if (master.CommandsService.GetAllowServiceCommands() == false)
            {
                return Failure("Service control is not enabled by config");
            }
            if (controlable.Contains(servicename) == false)
            {
                return Failure("Unknown service");
            }
            BotServices service = master.GetService(servicename);
            if (service == null)
            {
                return Failure("Unknown service");
            }
            service.Stop();
            return BasicReply("ok");
        }

        [About("restarts a service if it is running")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Unknown service")]
        [ReturnHintsFailure("Unable to manage selected service")]
        [ReturnHintsFailure("Service control is not enabled by config")]
        public object RestartService(string servicename)
        {
            if (master.CommandsService.GetAllowServiceCommands() == false)
            {
                return Failure("Service control is not enabled by config");
            }
            if (controlable.Contains(servicename) == false)
            {
                return Failure("Unknown service");
            }
            BotServices service = master.GetService(servicename);
            if (service == null)
            {
                return Failure("Unknown service");
            }
            service.Restart();
            return BasicReply("ok");
        }

        [About("Change or add a config value to a service")]
        [ReturnHints("ok")]
        [ReturnHintsFailure("Unknown service")]
        [ReturnHintsFailure("Unable to manage selected service")]
        [ReturnHintsFailure("Service control is not enabled by config")]
        public object ChangeConfigValue(string servicename,string configname,string configvalue)
        {
            if (master.CommandsService.GetAllowServiceCommands() == false)
            {
                return Failure("Service control is not enabled by config");
            }
            if (controlable.Contains(servicename) == false)
            {
                return Failure("Unknown service");
            }
            BotServices service = master.GetService(servicename);
            if (service == null)
            {
                return Failure("Unknown service");
            }
            service.changeConfig(configname, configvalue);
            return BasicReply("ok");
        }





    }
}
