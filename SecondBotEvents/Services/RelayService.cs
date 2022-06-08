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
        protected List<RelaySys> Relays = new List<RelaySys>();
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
            Console.WriteLine("Interaction Service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            botConnected = false;
            GetClient().Network.SimConnected += BotLoggedIn;
            Console.WriteLine("Interaction Service [Standby]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            botConnected = true;
            Console.WriteLine("Interaction Service [Active]");
        }

        public override void Start()
        {
            Stop();
            master.BotClientNoticeEvent += BotClientRestart;
            Console.WriteLine("Interaction Service [Starting]");
        }

        public override void Stop()
        {
            master.BotClientNoticeEvent -= BotClientRestart;
            if (master.BotClient != null)
            {
                if (GetClient() != null)
                {

                }
            }
            Console.WriteLine("Interaction Service [Stopping]");
        }
    }

    public class RelaySys
    {
        protected RelayService master;
        public bool enabled = false;
        public string fromType = "";
        public string fromOption = "";
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
        }

        protected HttpClient HTTPclient = new HttpClient();
        public void TriggerWith(string sourceType, string message, string filterOption1, string filterOption2=null, string filterOption3 = null)
        {
            // avatarim, {message}, avatarUUID
            // groupim, {message}, groupUUID
            // localchat, {message}, channel, TalkerType, TalkerUUID
            // discord, {message}, serverid, channelid, talkerid
            // *, {message}
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
             *  fromOption=[type@avatar], [type@object], [type@service]
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
             *  fromType=*
             *  fromOption=*
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
            if(enabled == false) { return; } // not enabled 
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
                        Dictionary<string, string> values = new Dictionary<string, string>
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

