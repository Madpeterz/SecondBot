﻿using EmbedIO;
using EmbedIO.Actions;
using EmbedIO.Routing;
using EmbedIO.Utilities;
using EmbedIO.WebApi;
using OpenMetaverse;
using RestSharp.Serializers;
using SecondBotEvents.Config;
using Swan.Logging;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using Swan;

namespace SecondBotEvents.Services
{
    public class HttpService : BotServices
    {
        protected WebServer HTTPendpoint = null;
        public new HttpConfig myConfig;
        protected bool acceptBotCommands = false;
        public HttpService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new HttpConfig(master.fromEnv, master.fromFolder);
            if (myConfig.GetEnabled() == false)
            {
                LogFormater.Info(Status());
                return;
            }
        }

        public bool GetAcceptBotCommands()
        {
            return acceptBotCommands;
        }

        public override string Status()
        {
            if (myConfig == null)
            {
                return "No Config";
            }
            else if (myConfig.GetHideStatusOutput() == true)
            {
                return "hidden";
            }
            else if (myConfig.GetEnabled() == false)
            {
                return "- Not requested -";
            }
            else if (HTTPendpoint == null)
            {
                return "not online";
            }
            else if (acceptBotCommands == false)
            {
                return "Active ~ Service";
            }
            return "Active ~ Commands";
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isStart == false)
            {
                return;
            }
            acceptBotCommands = false;
            LogFormater.Info("HTTP service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            acceptBotCommands = false;
            GetClient().Network.SimConnected += BotLoggedIn;
            LogFormater.Info("HTTP service [Bot commands disabled]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            acceptBotCommands = true;
            CreateWebServer();
        }

        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            if (updateEnabled)
            {
                myConfig.setEnabled(setEnabledTo);
            }
            if (myConfig.GetEnabled() == false)
            {
                LogFormater.Info("HTTP service [Disabled]");
                return;
            }
            Stop();
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
        }

        public override void Stop()
        {
            if(running == true)
            {
                LogFormater.Info("HTTP service [Stopping]");
            }
            running = false;
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            master.BotClientNoticeEvent -= BotClientRestart;
            if (HTTPendpoint != null)
            {
                HTTPendpoint.Dispose();
                HTTPendpoint = null;
                
            }
        }

        private void CreateWebServer()
        {
            Swan.Logging.Logger.UnregisterLogger<ConsoleLogger>();
            HTTPendpoint = new WebServer(o => o
                .WithUrlPrefix("http://*:80")
                .WithMode(HttpListenerMode.EmbedIO))
                .WithCors()
                .WithWebApi("/api", SerializationCallback, m => m.WithController(() => new HttpWebBot(master)))
                .WithModule(new ActionModule("/json", HttpVerbs.Post, ctx => ctx.SendDataAsync(JsonInput(ctx))))
                .WithModule(new ActionModule("/", HttpVerbs.Any, ctx => ctx.SendDataAsync(new { Message = "Error" }))
            );
            HTTPendpoint.RunAsync();
            LogFormater.Info("HTTP service [Enabled] on port 80");
        }

        protected string JsonInput(IHttpContext httpContext)
        {
            if (master.HttpService.GetAcceptBotCommands() == false)
            {
                return "No bot connected yet";
            }
            string body = httpContext.GetRequestBodyAsStringAsync().Await();
            if(body == null || body.Length < 1)
            {
                return "No body sent";
            }
            SignedCommand C = new(master.CommandsService, "http", body, !master.HttpService.myConfig.GetDisableCommandValidation(),
                master.CommandsService.myConfig.GetEnforceTimeWindow(),
                master.CommandsService.myConfig.GetTimeWindowSecs(),
                master.CommandsService.myConfig.GetSharedSecret());
            if (C.accepted == false)
            {
                return "Command request rejected";
            }
            return JsonConvert.SerializeObject(master.CommandsService.RunCommand("HTTP", C));
        }
        public static async Task SerializationCallback(IHttpContext context, object? data)
        {
            Validate.NotNull(nameof(context), context).Response.ContentType = MimeType.Json;
            using var text = context.OpenResponseText(new UTF8Encoding(false));
            await text.WriteAsync(data.ToString()).ConfigureAwait(false);
        }
    }

    public abstract class CommandsAPI(EventsSecondBot setmaster)
    {
        protected EventsSecondBot master = setmaster;
        protected UUID avataruuid = UUID.Zero;
        protected Parcel targetparcel = null;

        protected GridClient GetClient()
        {
            return master.BotClient.client;
        }

        protected KeyValuePair<bool, string> SetupCurrentParcel()
        {
            if (GetClient().Network.CurrentSim == null)
            {
                return new KeyValuePair<bool, string>(false, "Error not in a sim");
            }
            int localid = GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, GetClient().Self.SimPosition);
            if (GetClient().Network.CurrentSim.Parcels.ContainsKey(localid) == false)
            {
                Thread.Sleep(2000);
                localid = GetClient().Parcels.GetParcelLocalID(GetClient().Network.CurrentSim, GetClient().Self.SimPosition);
                if (GetClient().Network.CurrentSim.Parcels.ContainsKey(localid) == false)
                {
                    return new KeyValuePair<bool, string>(false, "Parcel data not ready");
                }
            }
            targetparcel = GetClient().Network.CurrentSim.Parcels[localid];
            return new KeyValuePair<bool, string>(true, "");
        }

        protected void ProcessAvatar(string avatar)
        {
            if (UUID.TryParse(avatar, out avataruuid) == true)
            {
                // sent a UUID not  a name, thats fine
                // try and load the name anyway
                master.DataStoreService.GetAvatarName(avataruuid);
                return;
            }
            avataruuid = UUID.Zero;
            string UUIDfetch = master.DataStoreService.GetAvatarUUID(avatar);
            if (UUIDfetch != "lookup")
            {
                if(UUID.TryParse(UUIDfetch, out avataruuid) == false)
                {
                    avataruuid = UUID.Zero;
                }
            }
        }

        protected object BasicReply(string input)
        {
            return BasicReply(input, Array.Empty<string>(), GetCallingCommand());
        }

        protected object BasicReply(string input, string[] args)
        {
            return BasicReply(input, args, GetCallingCommand());
        }

        protected object BasicReply(string input, string[] args, string command)
        {
            master.DataStoreService.AddCommandToHistory(true, command, args);
            return new { command, args, reply = input, status = true };
        }

        protected object Failure(string input, string command, string[] args, string whyfailed)
        {
            master.DataStoreService.AddCommandToHistory(false, command, args);
            return new { command, args, reply = input, status = false, whyfailed };
        }

        protected object Failure(string input, string[] args)
        {
            return Failure(input, GetCallingCommand(), args, null);
        }

        protected object Failure(string input)
        {
            return Failure(input, GetCallingCommand(), Array.Empty<string>(), null);
        }

        protected static string GetCallingCommand()
        {
            string commandName = "Unable to get";
            try
            {
                int index = 2;
                do
                {
                    commandName = (new System.Diagnostics.StackTrace()).GetFrame(index).GetMethod().Name;
                    index++;
                }
                while (commandName.StartsWith("InvokeWith") == true);
            }
            catch (Exception)
            {
                commandName = "errorFetchCommandName";
            }
            return commandName;
        }

    }

    [AttributeUsage(AttributeTargets.Class)]
    public class ClassInfo(string classinfo) : Attribute
    {
        public string classinfo = classinfo;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CmdType(CommandType setType) : Attribute
    {
        public CommandType cmdtype = setType;
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CmdTypeGet : CmdType
    {
        public CmdTypeGet() : base(CommandType.Get)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CmdTypeSet : CmdType
    {
        public CmdTypeSet() : base(CommandType.Set)
        {
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class CmdTypeDo : CmdType
    {
        public CmdTypeDo() : base(CommandType.Do)
        {
        }
    }

    public enum CommandType
    {
        Get,
        Set,
        Do,
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class About(string about) : Attribute
    {
        public string about = about;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReturnHints(string hint) : Attribute
    {
        public string hint = hint;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ReturnHintsFailure(string hint) : Attribute
    {
        public string hint = hint;
    }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class ArgHints(string name, string about, string defaultValueType=null, string exampleValue=null, string[] acceptedValues=null) : Attribute
    {
        public string name = name;
        public string about = about;
        public string defaultValueType = defaultValueType ?? string.Empty; // Default to empty string if not provided
        public string exampleValue = exampleValue ?? string.Empty; // Default to empty string if not provided
        public string[] acceptedValues = acceptedValues ?? Array.Empty<string>(); // Default to empty array if not provided
    }

    public class SecondbotWebApi(EventsSecondBot setmaster) : WebApiController
    {
        protected EventsSecondBot master = setmaster;
    }

    public class HttpWebBot(EventsSecondBot setmaster) : SecondbotWebApi(setmaster)
    {
        [About("Runs a command on the bot")]
        [ReturnHints("command reply")]
        [ReturnHintsFailure("Bad token signing")]
        [ArgHints("commandName", "the name of the command to run")]
        [ArgHints("args", "a list of args formated with ~#~ as the split points")]
        [ArgHints("signing", "a sha1 hash that meets level 3 API signing spec [see github wiki]")]
        [ArgHints("unixtime", "the time you signed the command")]
        [Route(HttpVerbs.Post, "/Run")]

        public string Run([FormField] string commandName, [FormField] string args, [FormField] string signing, [FormField] string unixtime)
        {
            string[] myArgs = Array.Empty<string>();
            if(args != null)
            {
                myArgs = args.Split("~#~", StringSplitOptions.RemoveEmptyEntries);
            }
            if(int.TryParse(unixtime, out int cmdUnixtime) == false)
            {
                return "Invaild unixtime was sent";
            }
            if (master.HttpService.GetAcceptBotCommands() == false)
            {
                return "No bot connected yet";
            }
            SignedCommand C = new(
                master.CommandsService,"http",
                commandName, signing, myArgs, cmdUnixtime, null,true, master.CommandsService.myConfig.GetTimeWindowSecs(),
                master.CommandsService.myConfig.GetSharedSecret(), !master.HttpService.myConfig.GetDisableCommandValidation());
            if(C.accepted == false)
            {
                return "Command request rejected";
            }
            return JsonConvert.SerializeObject(master.CommandsService.RunCommand("HTTP",C));
        }
    }

}
