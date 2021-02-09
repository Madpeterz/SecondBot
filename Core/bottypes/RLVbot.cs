using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.Json;
using OpenMetaverse;
using BetterSecondBotShared.logs;

namespace BSB.bottypes
{
    public abstract class RLVbot : RLVbotHelper
    {
        // normal bot stuff 
        protected RLV.RLVcontrol RLVinterface;
        public RLV.RLVcontrol GetRLVinterface { get { return RLVinterface; } }

        public override void Setup(JsonConfig config, string Version)
        {
            base.Setup(config, Version);
            if (reconnect == false)
            {
                if (myconfig.Setting_AllowRLV == true)
                {
                    Info("RLV: Enabled");
                    RLVinterface = new RLV.RLVcontrol(this,false);
                }
            }
        }

        protected override void ChatInputHandler(object sender, ChatEventArgs e)
        {
            base.ChatInputHandler(sender, e);
            if (e.Type == ChatType.OwnerSay)
            {
                if (myconfig.Setting_AllowRLV == true)
                {
                    if (e.Message.StartsWith("@") == true)
                    {
                        string message_text = e.Message.Substring(1);
                        bool permissive_allow = true;
                        if (permissive_allow == true)
                        {
                            string[] requested_commands = message_text.Split(',');
                            foreach (string R in requested_commands)
                            {
                                List<string> args = new List<string>();
                                string command = "";
                                string[] build = R.Split('=');
                                string[] build2 = build[0].Split(':');
                                if (build2.Length == 2)
                                {
                                    string[] build3 = build2[1].Split(';');
                                    foreach (string build_arg in build3)
                                    {
                                        args.Add(build_arg);
                                    }
                                }
                                command = build2[0];
                                if (build.Length == 2)
                                {
                                    args.Add(build[1]);
                                }
                                if (blacklist_recvim.Contains(command) == false)
                                {
                                    if (Notify.ContainsKey("#") == true)
                                    {
                                        foreach (int channel in Notify["#"])
                                        {
                                            Client.Self.Chat("/" + message_text, channel, ChatType.Normal);
                                        }
                                    }
                                    if (Notify.ContainsKey(command) == true)
                                    {
                                        foreach (int channel in Notify[command])
                                        {
                                            Client.Self.Chat("/" + message_text, channel, ChatType.Normal);
                                        }
                                    }
                                    RLVinterface.SetCallerDetails(e.SourceID, e.FromName);
                                    RLVinterface.Call(command, args.ToArray());
                                }
                                else
                                {
                                    Debug("RLV API -> blacklisted [" + command + "]");
                                }
                            }
                        }
                    }
                }
            }
        }
    }


    public class RLVbotHelper : AwaitReplyEventBot
    {
        #region PermissiveMode
        // permissive
        protected bool permissive_mode = true;
        protected UUID permissive_mode_set_by = UUID.Zero;
        public UUID Getpermissive_mode_set_by { get { return permissive_mode_set_by; } }

        public void SetPermissiveMode(UUID caller)
        {
            SetPermissiveMode(caller, false);
        }
        public void SetPermissiveMode(UUID caller, bool status)
        {
            bool allow = true;
            if((permissive_mode == false) && (caller != permissive_mode_set_by))
            {
                allow = false;
            }
            if(allow == true)
            {
                permissive_mode = status;
                if (status == false)
                {
                    permissive_mode_set_by = UUID.Zero;
                }
                else
                {
                    permissive_mode_set_by = caller;
                }
            }
        }
        #endregion

        #region blacklist
        // Blacklist
        protected List<string> blacklist_sendim = new List<string>();
        protected List<string> blacklist_recvim = new List<string>();
        public List<string> Getblacklist_sendim { get { return blacklist_sendim; } }
        public List<string> Getblacklist_recvim { get { return blacklist_recvim; } }
        #endregion

        #region UUID rules Exceptions
        // UUID based rules / Exceptions 
        protected Dictionary<UUID, Dictionary<string, string>> uuid_rules = new Dictionary<UUID, Dictionary<string, string>>();
        protected Dictionary<UUID, Dictionary<string, string>> uuid_exceptions = new Dictionary<UUID, Dictionary<string, string>>();
        public Dictionary<UUID, Dictionary<string, string>> Getuuid_rules { get { return uuid_rules; } }
        public Dictionary<UUID, Dictionary<string, string>> Getuuid_exceptions { get { return uuid_exceptions; } }
        public bool Clearrule(bool as_exception, UUID soruce)
        {
            return RemoveRule(as_exception, null, soruce);
        }
        public bool UpdateRule(bool as_exception, string rule, string arg, UUID source)
        {
            if (as_exception == false)
            {
                lock (uuid_rules)
                {
                    if (uuid_rules.ContainsKey(source) == false)
                    {
                        uuid_rules.Add(source, new Dictionary<string, string>());
                    }
                    if (uuid_rules[source].ContainsKey(rule) == true)
                    {
                        uuid_rules[source].Remove(rule);
                    }
                    uuid_rules[source].Add(rule, arg);
                }
            }
            else
            {
                lock (uuid_exceptions)
                {
                    if (uuid_exceptions.ContainsKey(source) == false)
                    {
                        uuid_exceptions.Add(source, new Dictionary<string, string>());
                    }
                    if (uuid_exceptions[source].ContainsKey(rule) == true)
                    {
                        uuid_exceptions[source].Remove(rule);
                    }
                    uuid_exceptions[source].Add(rule, arg);
                }
            }
            return true;
        }
        public bool RemoveRule(bool as_exception, string rule, UUID source)
        {
            if (as_exception == false)
            {
                lock (uuid_rules)
                {
                    if (uuid_rules.ContainsKey(source) == true)
                    {
                        if (rule != null)
                        {
                            if (uuid_rules[source].ContainsKey(rule) == true)
                            {
                                uuid_rules[source].Remove(rule);
                            }
                            if (uuid_rules[source].Count == 0)
                            {
                                uuid_rules.Remove(source);
                            }
                        }
                        else
                        {
                            uuid_rules.Remove(source);
                        }
                    }
                }
            }
            else
            {
                lock (uuid_exceptions)
                {
                    if (rule != null)
                    {
                        if (uuid_exceptions[source].ContainsKey(rule) == true)
                        {
                            uuid_exceptions[source].Remove(rule);
                        }
                        if (uuid_exceptions[source].Count == 0)
                        {
                            uuid_exceptions.Remove(source);
                        }
                    }
                    else
                    {
                        uuid_exceptions.Remove(source);
                    }
                }
            }
            return true;
        }
        #endregion

        #region Flags
        // Flags 
        protected bool recvchat_channel_zero_lock = true;
        protected bool sendchannel_lock;
        protected bool recvemote_lock;
        protected bool sendim_lock;
        public void SetLock(string name, bool status)
        {
            if (name == "sendim_lock") { sendim_lock = status; }
            else if (name == "recvemote_lock") { recvemote_lock = status; }
            else if (name == "sendchannel_lock") { sendchannel_lock = status; }
            else if (name == "recvchat_channel_zero_lock") { recvchat_channel_zero_lock = status; }
        }
        #endregion

        #region Notify
        // Notify
        protected Dictionary<string, List<int>> Notify = new Dictionary<string, List<int>>();
        public Dictionary<string, List<int>> Getnotify { get { return Notify; } }

        public void NotifyUpdate(bool Add, string word)
        {
            NotifyUpdate(Add, word, -1);
        }

        public void NotifyUpdate(bool Add, string word,int channel)
        {
            lock (Notify)
            {
                if (Add == true)
                {
                    if (Notify.ContainsKey(word) == false)
                    {
                        Notify.Add(word, new List<int>());
                    }
                    if (channel > -1)
                    {
                        if (Notify[word].Contains(channel) == false)
                        {
                            Notify[word].Add(channel);
                        }
                    }
                }
                else
                {
                    if (Notify.ContainsKey(word) == true)
                    {
                        if (Notify[word].Contains(channel) == true)
                        {
                            Notify[word].Remove(channel);
                            if(Notify[word].Count == 0)
                            {
                                Notify.Remove(word);
                            }
                        }
                        if (channel == -1)
                        {
                            Notify.Remove(word);
                        }

                    }
                }
            }
        }
        #endregion

    }
}
