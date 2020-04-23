using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using BSB.bottypes;
using BetterSecondBotShared.API;
using BetterSecondBotShared.logs;

namespace BSB.RLV
{
    public class RLVcontrol : API_supported_interface
    {
        protected RLVbot bot;
        protected UUID last_caller_uuid = UUID.Zero;
        protected string last_caller_name = "";

        public void SetCallerDetails(UUID callerUUID, string callerName)
        {
            last_caller_name = callerName;
            last_caller_uuid = callerUUID;
        }

        public RLVcontrol(RLVbot linktobot)
        {
            bot = linktobot;
            API_type = typeof(RLV_command);
            LoadCommandsList();
        }

        protected List<string> suppress_warning = new List<string>();
        public bool Call(string command, string[] args)
        {
            command = command.ToLowerInvariant();
            RLV_command cmd = (RLV_command)GetCommand(command);
            if (cmd != null)
            {
                if (args.Length >= cmd.MinArgs)
                {
                    cmd.Setup(bot, last_caller_uuid, last_caller_name);
                    return cmd.CallFunction(args);
                }
                else
                {
                    ConsoleLog.Warn("[RLVapi/Failed] " + command + ": Incorrect number of args");
                }
            }
            else
            {
                if(suppress_warning.Contains(command) == false)
                {
                    suppress_warning.Add(command);
                    ConsoleLog.Warn("[RLVapi/Failed] " + command + ": I have no fucking idea what you are talking about");
                }
            }
            return false;
        }
    }
    public abstract class RLV_command_1arg : RLV_command
    {
        public override int MinArgs { get { return 1; } }
    }
    public abstract class RLV_command : API_interface
    {
        public override string Helpfile { get { return "[Class helpfile] an RLV_command."; } }
        public virtual string SetFlagName { get { return CommandName.ToLowerInvariant(); } }
        protected RLVbot bot = null;
        protected UUID caller_uuid = UUID.Zero;
        protected string caller_name = "";

        public virtual bool AsExceptionRule { get { return false; } }
        

        public void Setup(RLVbot setBot, UUID callerUUID, string callerName)
        {
            bot = setBot;
            caller_name = callerName;
            caller_uuid = callerUUID;
        }

        public virtual bool CallFunction(string[] args)
        {
            return Failed("Function not overridden");
        }
        public bool Failed(string why_failed)
        {
            ConsoleLog.Warn("[RLVapi/Failed] " + CommandName + ": " + why_failed);
            return false;
        }

        protected bool SetFlag(string signal,string arg)
        {
            return SetFlag(signal, arg, caller_uuid);
        }
        protected bool SetFlag(string signal, string arg,UUID caller)
        {
            if ((signal == "y") || (signal == "rem"))
            {
                return bot.RemoveRule(AsExceptionRule, SetFlagName, caller);
            }
            else if ((signal == "n") || (signal == "add"))
            {
                return bot.UpdateRule(AsExceptionRule, SetFlagName, arg, caller);
            }
            return Failed("[RLV/Debug] arg only accepts y/n or add/rem for control signaling [Sig: " + signal+" Arg: "+arg+"]");
        }
    }
    public abstract class RLV_1arg_force : RLV_command
    {
        public override string Helpfile { get { return "[Class helpfile] Requires you send =force at the end of the command..."; } }
        public override string[] ArgTypes { get { return new[] { "TEXT" }; } }
        public override string[] ArgHints { get { return new[] { "The word force"}; } }
        public override int MinArgs { get { return 1; } }
        public override bool CallFunction(string[] args)
        {
            if (args.Length > 0)
            {
                if (args[0] == "force") return true;
                return Failed("Magic word force is missing");
            }
            return Failed("[RLV/WARN] RLV_1arg_force invaild number of args");
        }
    }


    public abstract class RLV_UUID_flag_4arg_yn : RLV_UUID_flag_optional_arg_yn
    {
        public override int MinArgs { get { return 4; } }
        public override string Helpfile { get { return "[Class helpfile] RLV / 4arg"; } }
        public override string[] ArgTypes { get { return new[] { "Unknown", "Unknown", "Unknown", "Unknown" }; } }
        public override string[] ArgHints { get { return new[] { "Unknown", "Unknown", "Unknown", "Unknown" }; } }
    }

    public abstract class RLV_UUID_flag_optional_arg_yn : RLV_command
    {
        public override int MinArgs { get { return 1; } }
        public override string Helpfile { get { return "[Class helpfile] RLV / UUID_flag_optional_arg_yn"; } }
        public override string[] ArgTypes { get { return new[] { "Unknown", "Unknown"}; } }
        public override string[] ArgHints { get { return new[] { "Unknown", "Unknown" }; } }

        public override bool CallFunction(string[] args)
        {
            if (args.Length == 2)
            {
                return SetFlag(args[^1], args[0]);
            }
            else if (args.Length > 0)
            {
                return SetFlag(args[^1], "set");
            }
            return Failed("[RLV/WARN] RLV_UUID_flag_optional_arg_yn invaild number of args");
        }
    }

    public abstract class RLV_UUID_flag_yn : RLV_command
    {
        public override int MinArgs { get { return 1; } }
        public override string Helpfile { get { return "[Class helpfile] RLV / UUID_flag_yn "; } }
        public override string[] ArgTypes { get { return new[] { "Unknown", "Unknown" }; } }
        public override string[] ArgHints { get { return new[] { "Unknown", "Unknown" }; } }
        public override bool CallFunction(string[] args)
        {
            if (args.Length >= 1)
            {
                return SetFlag(args[0], "set");
            }
            return Failed("[RLV/WARN] RLV_UUID_flag_yn invaild number of args");
        }
    }
    public abstract class RLV_UUID_flag_get : RLV_command
    {
        public abstract string LookupFlag { get; }
        public override int MinArgs { get { return 1; } }
        public override string Helpfile { get { return "[Class helpfile]<br/>Gets the flag value "+LookupFlag+" and returns it on channel [ARG 1]"; } }
        public override string[] ArgTypes { get { return new[] { "Number"}; } }
        public override string[] ArgHints { get { return new[] { "Channel" }; } }
        public override bool CallFunction(string[] args)
        {
            if (args.Length == 1)
            {
                _ = int.TryParse(args[0], out int channel);
                if (channel >= 0)
                {
                    if (bot.Getuuid_rules.ContainsKey(caller_uuid) == true)
                    {
                        if (bot.Getuuid_rules[caller_uuid].ContainsKey(LookupFlag) == true)
                        {
                            bot.GetClient.Self.Chat(bot.Getuuid_rules[caller_uuid][LookupFlag], channel, ChatType.Normal);
                        }
                    }
                    return true;
                }
                else
                {
                    return Failed("Channel number not vaild");
                }
            }
            return Failed("[RLV/WARN] RLV_UUID_flag_get invaild number of args");
        }
    }
    public abstract class RLV_UUID_flag_arg_yn : RLV_command
    {
        public override int MinArgs { get { return 2; } }
        public override string Helpfile { get { return "[Class helpfile]<br/> RLV / UUID_flag_arg_yn<br/>When clearing please set [ARG 1] to Anything<br/>Flags are tracked per object"; } }
        public override string[] ArgTypes { get { return new[] { "Mixed", "Flag" }; } }
        public override string[] ArgHints { get { return new[] { "The arg value you are setting", "[Y/N] or [Rem/Add]" }; } }
        public override bool CallFunction(string[] args)
        {
            if (args.Length == 2)
            {
                return SetFlag(args[1], args[0]);
            }
            return Failed("[RLV/WARN] RLV_UUID_flag_arg_yn invaild number of args");
        }
    }


}
