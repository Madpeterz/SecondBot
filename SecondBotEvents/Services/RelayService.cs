using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;

namespace SecondBotEvents.Services
{
    public class RelayService : BotServices
    {
        protected RelayConfig myConfig;
        protected bool botConnected = false;
        protected List<RelaySys> Relays = new();
        public RelayService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new RelayConfig(master.fromEnv, master.fromFolder);
        }

        public override string Status()
        {
            if (myConfig == null)
            {
                return "Config broken";
            }
            if (botConnected == false)
            {
                return "Waiting for bot";
            }
            return "Active";
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            botConnected = false;
            Console.WriteLine("Relay Service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            botConnected = false;
            GetClient().Network.SimConnected += BotLoggedIn;
            Console.WriteLine("Relay Service [Standby]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            GetClient().Self.IM += BotImMessage;
            GetClient().Self.ChatFromSimulator += LocalChat;
            botConnected = true;
            Console.WriteLine("Relay Service [Active]");
        }

        readonly string[] hard_blocked_agents = new[] { "secondlife", "second life" };

        protected void LocalChat(object o, ChatEventArgs e)
        {
            switch (e.Type)
            {
                case ChatType.OwnerSay:
                case ChatType.Whisper:
                case ChatType.Normal:
                case ChatType.Shout:
                case ChatType.RegionSayTo:
                    {
                        if (hard_blocked_agents.Contains(e.FromName.ToLowerInvariant()) == true)
                        {
                            break;
                        }
                        string source = ":person_bald:";
                        if (e.SourceType == ChatSourceType.Object)
                        {
                            source = ":diamond_shape_with_a_dot_inside:";
                        }
                        else if (e.SourceType == ChatSourceType.System)
                        {
                            source = ":comet:";
                        }
                        if (e.Type == ChatType.OwnerSay)
                        {
                            source = ":robot:";
                        }
                        else if (e.Type == ChatType.RegionSayTo)
                        {
                            source = ":dart:";
                        }
                        // trigger localchat
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        protected void BotImMessage(object o, InstantMessageEventArgs e)
        {
            switch (e.IM.Dialog)
            {
                case InstantMessageDialog.MessageFromObject:
                    {
                        // trigger object IM
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        if (master.DataStoreService.GetIsGroup(e.IM.IMSessionID) == false)
                        {
                            // trigger avatar IM
                            break;
                        }
                        // trigger group IM
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }


        public override void Start()
        {
            Stop();
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
            Console.WriteLine("Relay Service [Starting]");
        }

        public override void Stop()
        {
            if(running == true)
            {
                Console.WriteLine("Relay Service [Stopping]");
            }
            running = false;
            master.BotClientNoticeEvent -= BotClientRestart;
            if (master.BotClient != null)
            {
                if (GetClient() != null)
                {

                }
            }
            
        }
    }

    public class RelaySys
    {
        protected RelayService master;
        public bool enabled = false;
        public string fromType = "";
        public string fromOption = "";

        public ulong fromDiscordArg = 0; // discord channel/user/server filter
        public UUID fromUUID = UUID.Zero; // object/avatar/group UUID filter
        public int fromChannelNumber = 0; // localchat channel number filter
        public string fromChatType = "";


        public string toType = "";
        public string toOption = "";
        protected int filterOnIndex = 0;
        protected int relayID = 0;

        protected UUID targetuuid = UUID.Zero;
        protected int targetchannel = 0;

        protected ulong targetulong1 = 0;
        protected ulong targetulong2 = 0;

        protected string targeturl = "";
        protected string targetsharedsecret = "";
        protected int nonce = 1;

        public RelaySys(RelayService setMaster, bool setEnabled, string setFromType, string setFromOption, string setToType, string setToOption)
        {
            master = setMaster;
            enabled = setEnabled;
            fromType = setFromType;
            fromOption = setFromOption;
            toType = setToType;
            toOption = setToOption;
            Vaildate();
        }


        protected void Vaildate()
        {
            bool setFlag = enabled;
            enabled = false;
            if(VaildateFrom() == false)
            {
                return;
            }
            if (vaildToOptions.Contains(toType) == false)
            {
                return;
            }
            
            enabled = setFlag;
        }

        protected bool VaildateFrom()
        {
            string[] vaildFromOptions = new[] { "avatarim", "groupim", "localchat", "discord", "objectchat", "*" };
            if (vaildFromOptions.Contains(fromType) == false)
            {
                return false;
            }
            string[] bits = fromOption.Split("@");
            if(bits[0] == "*")
            {
                fromOption = "*";
                return true;
            }
            if(fromType == "discord")
            {
                return VaildateFromDiscord();
            }
            else if (fromType == "objectchat")
            {
                return VaildateFromObjectChat();
            }
            else if (fromType == "localchat")
            {
                return VaildateFromLocalChat();
            }
            else if(fromType == "groupim")
            {
                return VaildateFromGroupChat();
            }
            else if(fromType == "avatarim")
            {
                return VaildateFromAvatarIm();
            }
            return false;
        }

        protected bool VaildateFromAvatarIm()
        {
            string[] bits = fromOption.Split("@");
            string[] vaildFromFilters = new string[] { "avatar" };
            if (bits.Length != 2)
            {
                return false;
            }
            if (vaildFromFilters.Contains(bits[0]) == false)
            {
                return false;
            }
            if (UUID.TryParse(bits[1], out fromUUID) == false)
            {
                return false;
            }
            fromOption = bits[0];
            return true;
        }

        protected bool VaildateFromGroupChat()
        {
            string[] bits = fromOption.Split("@");
            string[] vaildFromFilters = new string[] { "group" };
            if (bits.Length != 2)
            {
                return false;
            }
            if (vaildFromFilters.Contains(bits[0]) == false)
            {
                return false;
            }
            fromOption = bits[0];
            return UUID.TryParse(bits[1], out fromUUID);
        }

        protected bool VaildateFromLocalChat()
        {
            string[] bits = fromOption.Split("@");
            string[] vaildFromFilters = new string[] { "channel", "type", "uuid" };
            if (bits.Length != 2)
            {
                return false;
            }
            if (vaildFromFilters.Contains(bits[0]) == false)
            {
                return false;
            }
            fromOption = bits[0];
            if(fromOption == "channel")
            {
                return int.TryParse(bits[1], out fromChannelNumber);
            }
            else if (fromOption == "type")
            {
                vaildFromFilters = new string[] { "avatar", "object", "service" };
                fromChatType = bits[1];
                return vaildFromFilters.Contains(bits[1]);
            }
            return UUID.TryParse(bits[1], out fromUUID);
        }

        protected bool VaildateFromObjectChat()
        {
            string[] bits = fromOption.Split("@");
            string[] vaildFromFilters = new string[] { "object" };
            if (bits.Length != 2)
            {
                return false;
            }
            if (vaildFromFilters.Contains(bits[0]) == false)
            {
                return false;
            }
            if (UUID.TryParse(bits[1], out fromUUID) == false)
            {
                return false;
            }
            fromOption = bits[0];
            return true;
        }

        protected bool VaildateFromDiscord()
        {
            string[] bits = fromOption.Split("@");
            string[] vaildFromFilters = new string[] { "server", "channel", "user" };
            if(bits.Length != 2)
            {
                return false;
            }
            if(vaildFromFilters.Contains(bits[0]) == false)
            {
                return false;
            }
            if (ulong.TryParse(bits[1], out fromDiscordArg) == false)
            {
                return false;
            }
            fromOption = bits[0];
            return true;
        }


        readonly string[] vaildToOptions = new[] { "avatar", "group", "localchat", "discord", "http" };
        readonly 

        protected HttpClient HTTPclient = new();
        public void TriggerWith(string sourceType, string message, string filterOption1, string filterOption2=null, string filterOption3 = null)
        {
            // avatarim, {message}, avatarUUID
            // groupim, {message}, groupUUID
            // localchat, {message}, channel, TalkerType, TalkerUUID
            // discord, {message}, serverid, channelid, talkerid
            // objectchat, {message}, objectUUID
            /*
             *  fromType=discord
             *  fromOption=*
             *  fromOption=server@XXX
             *  fromOption=channel@XXX
             *  fromOption=user@XXX
             *  
             *  fromType=localchat
             *  fromOption=*
             *  fromOption=channel@4
             *  fromOption=type@avatar|object
             *  fromOption=uuid@XXX-XXX-XXX-XXX
             *  
             *  fromType=groupim
             *  fromOption=*
             *  fromOption=group@XXX-XXX-XXX-XXX
             *  
             *  fromType=avatarim
             *  fromOption=*
             *  fromOption=avatar@XXX-XXX-XXX-XXX
             *  
             *  fromType=objectchat
             *  fromOption=*
             *  fromOption=object@XXX-XXX-XXX-XXX
             *  
             *  toType=discord
             *  toOption=channelid
             *  
             *  toType=http
             *  toOption=url@sharedsecret
             *  
             *  toType=localchat
             *  toOption=channel
             *  
             *  toType=avatar
             *  toOption=avataruuid
             *  
             *  toType=group
             *  toOption=groupuuid
             */
            if (enabled == false) { return; } // not enabled 
            if (message.Contains("{RELAY}") == true) { return; } // no messages from a relay should be relayed we dont like loops here
            else if ((sourceType != fromType) && (sourceType != "*")) { return; } // not for this relay
            else if ((filterOnIndex == 0) && (fromOption != "*")) { return; } // invaild filter selected [but some how not disabled]
            else if ((filterOnIndex == 1) && (fromOption != filterOption1)) { return; } // did not match filter
            else if ((filterOnIndex == 2) && (fromOption != filterOption2)) { return; } // did not match filter
            else if ((filterOnIndex == 3) && (fromOption != filterOption3)) { return; } // did not match filter
            message = "{RELAY}" + message;
            switch (toType)
            {
                case "discord":
                    {
                        master.master.DiscordService.SendMessageToChannel(targetulong1, targetulong2, message);
                        break;
                    }
                case "http":
                    {
                        long unixtime = SecondbotHelpers.UnixTimeNow();
                        Dictionary<string, string> values = new()
                        {
                            { "fromType", fromType },
                            { "fromOption", fromOption },
                            { "arg1", filterOption1 },
                            { "arg2", filterOption2 },
                            { "arg3", filterOption3 },
                            { "message", message },
                            { "unixtime", unixtime.ToString() },
                            { "nonce", nonce.ToString() }
                        };
                        nonce++;
                        string raw = "";
                        foreach(string A in values.Values)
                        {
                            raw += A;
                        }
                        values.Add("hash", SecondbotHelpers.GetSHA1(raw + targetsharedsecret));

                        var content = new FormUrlEncodedContent(values);
                        try
                        {
                            HTTPclient.PostAsync(targeturl, content);
                        }
                        catch (Exception e)
                        {
                            LogFormater.Crit("[Relay] HTTP failed: " + e.Message + "");
                        }
                        break;
                    }
                case "localchat":
                    {
                        master.GetClient().Self.Chat(message, targetchannel, ChatType.Normal);
                        break;
                    }
                case "avatar":
                    {
                        master.GetClient().Self.InstantMessage(targetuuid, message);
                        break;
                    }
                case "group":
                    {
                        master.GetClient().Self.InstantMessageGroup(targetuuid, message);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}

