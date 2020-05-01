using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using System.Net.Http;
using BSB.bottypes;
using BetterSecondBotShared.API;
using BetterSecondBotShared.Static;
using BetterSecondBotShared.logs;

namespace BSB.Commands
{
    public class pending_avatar_lookup
    {
        public pending_avatar_lookup(CoreCommandsInterface set_master,CoreCommand set_cmd,string[] set_args)
        {
            Master = set_master;
            Command = set_cmd;
            args = set_args.ToList();
        }
        public KeyValuePair<bool,string> Callnow()
        {
            bool result = Command.CallFunction(args.ToArray());
            return new KeyValuePair<bool, string>(result, Command.GetInfoBlob);
        }
        public bool needs_lookup()
        {
            List<int> need_lookup_on_indexs = new List<int>();
            int loop = 0;
            foreach (string atype in Command.ArgTypes)
            {
                if (loop < args.Count)
                {
                    if (atype == "Avatar")
                    {
                        need_lookup_on_indexs.Add(loop);
                    }
                    else if(atype == "Smart")
                    {
                        if(args[loop].StartsWith("http") == false)
                        {
                            // not a url request
                            if (int.TryParse(args[loop],out _) == false)
                            {
                                // Not a channel
                                need_lookup_on_indexs.Add(loop);
                            }
                        }
                    }
                    loop++;
                }
                else
                {
                    break;
                }
            }
            bool has_required_lookups = false;
            foreach (int A in need_lookup_on_indexs)
            {
                string value = args[A];
                if(UUID.TryParse(args[A],out _) == false)
                {
                    // not a uuid
                    string get_or_request_uuid = Master.GetBot.FindAvatarName2Key(args[A]);
                    if(get_or_request_uuid == "lookup")
                    {
                        has_required_lookups = true;
                    }
                    else
                    {
                        args[A] = get_or_request_uuid;
                    }
                }
            }
            return has_required_lookups;
        }
        protected CoreCommand Command;
        protected List<string> args;
        protected CoreCommandsInterface Master;
    }
    public class CoreCommandsInterface : API_supported_interface
    {
        protected CommandsBot bot;
        public CommandsBot GetBot { get { return bot; } }

        protected long giveup_waiting_for_lookup = 240;
        protected int next_lookup_id;
        protected Dictionary<int, pending_avatar_lookup> commands_pending_avatar_lookups = new Dictionary<int, pending_avatar_lookup>();
        protected Dictionary<int, long> commands_pending_avatar_lookups_ages = new Dictionary<int, long>();

        public void StatusTick()
        {
            List<int> purge_indexs = new List<int>();
            List<int> call_indexs = new List<int>();
            long now = helpers.UnixTimeNow();
            foreach (KeyValuePair<int,long> pair in commands_pending_avatar_lookups_ages)
            {
                if (commands_pending_avatar_lookups[pair.Key].needs_lookup() == true)
                {
                    long dif = now - pair.Value;
                    if (dif > giveup_waiting_for_lookup)
                    {
                        purge_indexs.Add(pair.Key);
                    }
                }
                else
                {
                    call_indexs.Add(pair.Key);
                }
            }
            foreach(int a in purge_indexs)
            {
                commands_pending_avatar_lookups_ages.Remove(a);
                commands_pending_avatar_lookups.Remove(a);
            }
            foreach(int a in call_indexs)
            {
                commands_pending_avatar_lookups[a].Callnow();
                commands_pending_avatar_lookups_ages.Remove(a);
                commands_pending_avatar_lookups.Remove(a);
            }
        }

        public CoreCommandsInterface(CommandsBot linktobot)
        {
            bot = linktobot;
            API_type = typeof(CoreCommand);
            LoadCommandsList();
        }
        protected HttpClient HTTPclient = new HttpClient();

        public bool SmartCommandReply(string target, string output)
        {
            return SmartCommandReply(target, output, "", false);
        }
        public bool SmartCommandReply(string target, string output, string command)
        {
            return SmartCommandReply(target, output, command, false);
        }
        public bool SmartCommandReply(string target,string output,string command,bool vaildate_only)
        {
            string mode = "CHAT";
            UUID target_avatar = UUID.Zero;
            int target_channel = 0;
            if (target.StartsWith("http://"))
            {
                mode = "HTTP";
            }
            else if (UUID.TryParse(target, out target_avatar) == true)
            {
                mode = "IM";
            }
            else
            {
                int.TryParse(target, out target_channel);
            }
            if (vaildate_only == false)
            {
                if (mode == "CHAT")
                {
                    bot.GetClient.Self.Chat(output, target_channel, ChatType.Normal);
                }
                else if (mode == "IM")
                {
                    bot.GetClient.Self.InstantMessage(target_avatar, output);
                }
                else if (mode == "HTTP")
                {
                    Dictionary<string, string> values = new Dictionary<string, string>
                    {
                        { "reply", output },
                        { "unixtime", helpers.UnixTimeNow().ToString() }
                    };

                    if (command != "")
                    {
                        values.Add("command", command);
                    }
                    var content = new FormUrlEncodedContent(values);
                    try
                    {
                        HTTPclient.PostAsync(target, content);
                    }
                    catch (Exception e)
                    {
                        ConsoleLog.Crit("[SmartReply] HTTP failed: " + e.Message + "");
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            else
            {
                if ((mode == "CHAT") || (mode == "IM") || (mode == "HTTP"))
                {
                    return true;
                }
                return false;
            }
        }

        protected List<string> suppress_warnings = new List<string>();
        public bool Call(string command)
        {
            return Call(command, "",UUID.Zero);
        }

        protected KeyValuePair<bool,string> CallCommand(string command, string arg, UUID caller)
        {
            command = command.ToLowerInvariant();
            CoreCommand cmd = (CoreCommand)GetCommand(command);
            if (cmd != null)
            {
                cmd.Setup(bot, caller);
                string[] args = new string[] { };
                if (helpers.notempty(arg) == true)
                {
                    args = arg.Split(new [] { "~#~" }, StringSplitOptions.None);
                }
                if ((cmd.ArgTypes.Contains("Avatar") == true) || (cmd.ArgTypes.Contains("Smart") == true))
                {
                    if (cmd.Min_Required_args <= args.Length)
                    {
                        pending_avatar_lookup pending = new pending_avatar_lookup(this, cmd, args);
                        if (pending.needs_lookup() == true)
                        {
                            commands_pending_avatar_lookups.Add(next_lookup_id, pending);
                            commands_pending_avatar_lookups_ages.Add(next_lookup_id, helpers.UnixTimeNow());
                            next_lookup_id++;
                            if (next_lookup_id == 13000)
                            {
                                next_lookup_id = 0;
                            }
                            return new KeyValuePair<bool, string>(true,"Lookup underway");
                        }
                        else
                        {
                            return pending.Callnow();
                        }
                    }
                    else
                    {
                        return new KeyValuePair<bool, string>(false, "Required args count not sent! expected: "+ cmd.Min_Required_args.ToString()+" but got "+ args.Length.ToString()+ "");
                    }
                }
                else
                {
                    bool result = cmd.CallFunction(args);
                    return new KeyValuePair<bool, string>(result, cmd.GetInfoBlob);
                }
            }
            else
            {
                if (suppress_warnings.Contains(command) == false)
                {
                    suppress_warnings.Add(command);
                    return new KeyValuePair<bool, string>(false, "unknown");
                }
            }
            return new KeyValuePair<bool, string>(false, "--");
        }
        public bool Call(string command, string arg,UUID caller)
        {
            KeyValuePair<bool, string> result = CallCommand(command, arg, caller);
            bot.CommandHistoryAdd(command, arg, result.Key,result.Value);
            return result.Key;
        }
    }

    public abstract class CoreCommand_1arg : CoreCommand
    {
        public override int Min_Required_args { get { return 1; } }
    }
    public abstract class CoreCommand_2arg : CoreCommand
    {
        public override int Min_Required_args { get { return 2; } }
    }
    public abstract class CoreCommand_3arg : CoreCommand
    {
        public override int Min_Required_args { get { return 3; } }
    }
    public abstract class CoreCommand_4arg : CoreCommand
    {
        public override int Min_Required_args { get { return 4; } }
    }
    public abstract class CoreCommand_SmartReply_1arg : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Smart" }; } }
        public override string[] ArgHints { get { return new[] { "Mixed [Channel|Avatar uuid|Avatar name|http url]" }; } }
    }

    public abstract class CoreCommand : API_interface
    {
        protected CommandsBot bot;

        /// <summary>
        /// The UUID of the object/Avatar that called this function, you should still test if its not UUID zero and is a avatar
        /// </summary>
        protected UUID caller = UUID.Zero;
        public void Setup(CommandsBot setBot,UUID setCaller)
        {
            bot = setBot;
            caller = setCaller;
        }
        public virtual void Callback(string[] args, EventArgs e,bool status)
        {
            bot.CommandHistoryAdd("Callback finished-> " + this.GetType().Name, String.Join(",", args), status, GetInfoBlob);
        }
        public virtual void Callback(string[] args, EventArgs e)
        {
            bot.CommandHistoryAdd("Callback finished-> " + this.GetType().Name, String.Join(",", args), true, GetInfoBlob);
        }
        public virtual bool CallFunction(string[] args)
        {
            if (helpers.notempty(args) == true)
            {
                if (args.Length < Min_Required_args)
                {
                    return Failed("Incorrect number of args");
                }
                else
                {
                    return true;
                }
            }
            else
            {
                if(Min_Required_args == 0)
                {
                    return true;
                }
                else
                {
                    return Failed("Incorrect number of args");
                }
            }
        }
        public bool Failed(string why_failed)
        {
            InfoBlob = why_failed;
            return false;
        }
    }


}
