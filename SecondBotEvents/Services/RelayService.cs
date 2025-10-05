using System.Text.Json;
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
        protected new RelayConfig myConfig;
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
            if (e.isStart == false)
            {
                return;
            }
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
            triggerRelayEvent(
                "discord", 
                e.Source.ToString(), 
                e.Channel.ToString(), 
                e.Author.Username, 
                e.CleanContent,
                e.Channel.Name,
                e.Author.GlobalName
            );
        }

        readonly string[] hard_blocked_agents = ["secondlife", "second life"];

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
                        string sourcename = e.Type.ToString().ToLowerInvariant();
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
                        string triggerdisplayname = e.SourceID.ToString();
                        if (source == "avatar")
                        {
                            triggerdisplayname = master.DataStoreService.GetDisplayName(e.SourceID);
                        }
                        // trigger localchat
                        triggerRelayEvent(
                            source: "localchat", 
                            filter: source, 
                            sourceuuid: e.SourceID.ToString(), 
                            triggername: e.FromName, 
                            message: e.Message,
                            sourcename: sourcename,
                            triggerdisplayname: triggerdisplayname
                         );
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
                        triggerRelayEvent(
                            source: "object",
                            filter: e.IM.FromAgentID.ToString(),
                            sourceuuid: e.IM.IMSessionID.ToString(),
                            triggername: e.IM.FromAgentName,
                            message: e.IM.Message,
                            sourcename: e.IM.Dialog.ToString(),
                            triggerdisplayname: "objectim"
                         );
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        if (master.DataStoreService.GetIsGroup(e.IM.IMSessionID) == false)
                        {
                            // trigger avatar IM
                            triggerRelayEvent(
                                source: "avatar",
                                filter: e.IM.FromAgentID.ToString(),
                                sourceuuid: e.IM.IMSessionID.ToString(),
                                triggername: e.IM.FromAgentName,
                                message: e.IM.Message,
                                sourcename: e.IM.Dialog.ToString(),
                                triggerdisplayname: master.DataStoreService.GetDisplayName(e.IM.FromAgentID)
                             );
                        }
                        // trigger group IM
                        string groupname = "?";
                        if(GetClient().Groups.GroupName2KeyCache.ContainsKey(e.IM.IMSessionID) == true)
                        {
                            groupname = GetClient().Groups.GroupName2KeyCache[e.IM.IMSessionID];
                        }
                        triggerRelayEvent(
                            source: "group",
                            filter: groupname,
                            sourceuuid: e.IM.IMSessionID.ToString(),
                            triggername: e.IM.FromAgentName,
                            message: e.IM.Message,
                            sourcename: e.IM.Dialog.ToString(),
                            triggerdisplayname: master.DataStoreService.GetDisplayName(e.IM.FromAgentID)
                         );
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
        
        protected void triggerRelayEvent(
            string source, 
            string filter, 
            string sourceuuid, 
            string triggername, 
            string message,
            string sourcename="",
            string triggerdisplayname=""

        )
        {
            int loop = 1;
            if(message.Contains("{Relay}") == true)
            {
                return;
            }
            message = "{Relay} " + message;
            relayEvent a = new()
            {
                message = message,
                name = triggername,
                sourcetype = source,
                sourceoption = filter
            };
            string eventMessage = JsonSerializer.Serialize(a, JsonOptions.UnsafeRelaxed);
            relayEventExtended relayEventExtended = new()
            {
                source = source,
                sourceid = filter,
                sourcename = sourcename,
                triggerid = sourceuuid,
                triggername = triggername,
                triggerdisplayname = triggerdisplayname,
                message = message,
                unixtime = SecondbotHelpers.UnixTimeNow().ToString()
            };
            string eventMessageExtended = JsonSerializer.Serialize(relayEventExtended, JsonOptions.UnsafeRelaxed);

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
                string useEventMessage = myConfig.GetRelayJson(index) ? eventMessageExtended : eventMessage;
                if (targetType == "localchat")
                {
                    if(int.TryParse(targetOption, out int target) == false)
                    {
                        continue;
                    }
                    if(target < 0)
                    {
                        continue;
                    }
                    GetClient().Self.Chat(useEventMessage, target, ChatType.Normal);
                }
                else if(targetType == "group")
                {
                    if (UUID.TryParse(targetOption, out UUID targetuuid) == false)
                    {
                        continue;
                    }
                    GetClient().Self.InstantMessageGroup(targetuuid, useEventMessage);
                }
                else if(targetType == "avatar")
                {
                    if (UUID.TryParse(targetOption, out UUID targetuuid) == false)
                    {
                        continue;
                    }
                    GetClient().Self.InstantMessage(targetuuid, useEventMessage);
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
                    request.AddParameter("event", useEventMessage);
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
                    master.DiscordService.SendMessageToChannel(targetserver, targetchannel, useEventMessage);
                }
                else if(targetType == "rabbit")
                {
                    master.RabbitService.SendMessage(targetOption, useEventMessage);
                }
            }
        }


        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            if (updateEnabled)
            {
                myConfig.setEnabled(setEnabledTo);
            }
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

    public class relayEventExtended
    {
        public string source = "";
        public string sourceid = "";
        public string sourcename = "";
        public string triggerid = "";
        public string triggername = "";
        public string triggerdisplayname = "";
        public string message = "";
        public string unixtime = "";
    }
}


