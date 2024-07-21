using Newtonsoft.Json;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using OpenMetaverse.ImportExport.Collada14;
using System.Configuration;
using RestSharp;
using System.Security.Policy;
using Discord.WebSocket;

namespace SecondBotEvents.Services
{
    public class RelayService : BotServices
    {
        protected RelayConfig myConfig;
        protected bool botConnected = false;
        public RelayService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new RelayConfig(master.fromEnv, master.fromFolder);
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
            else if (botConnected == false)
            {
                return "Waiting for bot";
            }
            return "Active";
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            botConnected = false;
            LogFormater.Info("Relay Service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            botConnected = false;
            GetClient().Network.SimConnected += BotLoggedIn;
            LogFormater.Info("Relay Service [Standby]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            GetClient().Self.IM += BotImMessage;
            GetClient().Self.ChatFromSimulator += LocalChat;
            master.DiscordService.DiscordMessageEvent += DiscordMessage;
            botConnected = true;
            LogFormater.Info("Relay Service [Active]");
        }

        protected void DiscordMessage(object o, SocketMessage e)
        {
            triggerRelayEvent("discord", e.Source.ToString(), e.Channel.ToString(), e.Author.Username, e.CleanContent);
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
                        string source = "avatar";
                        if (e.SourceType == ChatSourceType.Object)
                        {
                            source = "object";
                        }
                        else if (e.SourceType == ChatSourceType.System)
                        {
                            source = "system";
                        }
                        if (e.Type == ChatType.OwnerSay)
                        {
                            source = "ownersay";
                        }
                        // trigger localchat
                        triggerRelayEvent("localchat", source, e.SourceID.ToString(), e.FromName, e.Message);
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
                        triggerRelayEvent("object", e.IM.FromAgentID.ToString(), e.IM.IMSessionID.ToString(), e.IM.FromAgentName, e.IM.Message);
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        if (master.DataStoreService.GetIsGroup(e.IM.IMSessionID) == false)
                        {
                            // trigger avatar IM
                            triggerRelayEvent("avatar", e.IM.FromAgentID.ToString(), e.IM.IMSessionID.ToString(), e.IM.FromAgentName, e.IM.Message);
                            break;
                        }
                        // trigger group IM
                        string groupname = "?";
                        if(GetClient().Groups.GroupName2KeyCache.ContainsKey(e.IM.IMSessionID) == true)
                        {
                            groupname = GetClient().Groups.GroupName2KeyCache[e.IM.IMSessionID];
                        }
                        triggerRelayEvent("group", groupname, e.IM.IMSessionID.ToString(), e.IM.FromAgentName, e.IM.Message);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        
        protected void triggerRelayEvent(string source, string filter, string sourceuuid, string name, string message)
        {
            int loop = 1;
            if(message.Contains("{Relay}") == true)
            {
                return;
            }
            message = "{Relay} " + message;
            relayEvent a = new relayEvent();
            a.message = message;
            a.name = name;
            a.sourcetype = source;
            a.sourceoption = filter;
            string eventMessage = JsonConvert.SerializeObject(a);
            while (loop <= myConfig.GetRelayCount())
            {
                int index = loop;
                loop++;
                if (myConfig.GetRelayEnabled(index) != true)
                {
                    continue;
                }
                if(myConfig.RelaySourceType(index) != source)
                {
                    continue;
                }
                if ((myConfig.RelaySourceOption(index) != filter) && (myConfig.RelaySourceOption(index) != sourceuuid))
                {
                    continue;
                }
                string targetType = myConfig.RelayTargetType(index);
                string targetOption = myConfig.RelayTargetOption(index);
                if(targetType == "localchat")
                {
                    if(int.TryParse(targetOption, out int target) == false)
                    {
                        continue;
                    }
                    if(target < 0)
                    {
                        continue;
                    }
                    GetClient().Self.Chat(eventMessage, target, ChatType.Normal);
                }
                else if(targetType == "group")
                {
                    if (UUID.TryParse(targetOption, out UUID targetuuid) == false)
                    {
                        continue;
                    }
                    GetClient().Self.InstantMessageGroup(targetuuid, eventMessage);
                }
                else if(targetType == "avatar")
                {
                    if (UUID.TryParse(targetOption, out UUID targetuuid) == false)
                    {
                        continue;
                    }
                    GetClient().Self.InstantMessage(targetuuid, eventMessage);
                }
                else if(targetType == "http")
                {
                    long unixtime = SecondbotHelpers.UnixTimeNow();
                    string token = SecondbotHelpers.GetSHA1(unixtime.ToString() + "RelayService" + GetClient().Self.AgentID + eventMessage + master.CommandsService.myConfig.GetSharedSecret());
                    var client = new RestClient(targetOption);
                    var request = new RestRequest("Relay/Service", Method.Post);
                    request.AddParameter("token", token);
                    request.AddParameter("unixtime", unixtime.ToString());
                    request.AddParameter("method", "Relay");
                    request.AddParameter("action", "Service");
                    request.AddParameter("botname", GetClient().Self.Name);
                    request.AddParameter("event", eventMessage);
                    request.AddHeader("content-type", "application/x-www-form-urlencoded");
                    client.ExecutePostAsync(request);
                }
                else if(targetType == "discord")
                {
                    if(master.DiscordService.isRunning() == false)
                    {
                        continue;
                    }
                    string[] bits = targetOption.Split('|');
                    if(bits.Length != 2 )
                    {
                        continue;
                    }
                    if (ulong.TryParse(bits[0],out ulong targetserver) == false)
                    {
                        continue;
                    }
                    if (ulong.TryParse(bits[1], out ulong targetchannel) == false)
                    {
                        continue;
                    }
                    master.DiscordService.SendMessageToChannel(targetserver, targetchannel, message);
                }
            }
        }


        public override void Start()
        {
            Stop();
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
            LogFormater.Info("Relay Service [Starting]");
        }

        public override void Stop()
        {
            if(running == true)
            {
                LogFormater.Info("Relay Service [Stopping]");
            }
            running = false;
            master.BotClientNoticeEvent -= BotClientRestart;
            
        }
    }

    public class relayEvent
    {
        public string name = "";
        public string message = "";
        public string sourcetype = "";
        public string sourceoption = "";
    }
}

