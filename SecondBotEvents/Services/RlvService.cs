namespace SecondBotEvents.Services
{
    using global::SecondBotEvents.Commands;
    using global::SecondBotEvents.Config;
    using OpenMetaverse;
    using System;
    using System.Collections.Generic;
    using System.Data;

    namespace SecondBotEvents.Services
    {
        public class RlvService : BotServices
        {
            protected bool botConnected = false;
            protected RlvConfig myConfig;
            protected List<RLVRule> RlvRules = new List<RLVRule>();

            protected List<RLVRule> NotifyRules = new List<RLVRule>();
            

            public RlvService(EventsSecondBot setMaster) : base(setMaster)
            {
                myConfig = new RlvConfig(master.fromEnv, master.fromFolder);
            }
            public override string Status()
            {
                if (myConfig == null)
                {
                    return "Config broken";
                }
                else if(myConfig.GetEnabled() == false)
                {
                    return "Disabled by config";
                }
                else if (myConfig.GetHideStatusOutput() == true)
                {
                    return "hidden";
                }
                else if (botConnected == false)
                {
                    return "Waiting for client";
                }
                return "Active";
            }

            protected void BotClientRestart(object o, BotClientNotice e)
            {
                LogFormater.Info("Rlv service [Attached to new client]");
                GetClient().Network.LoggedOut += BotLoggedOut;
                GetClient().Network.SimConnected += BotLoggedIn;
            }

            protected void BotLoggedOut(object o, LoggedOutEventArgs e)
            {
                GetClient().Network.SimConnected += BotLoggedIn;
                GetClient().Self.IM -= BotImMessage;
                GetClient().Self.ChatFromSimulator -= LocalChat;
                botConnected = false;
                LogFormater.Info("Rlv service [waiting for new client]");
            }

            protected void BotLoggedIn(object o, SimConnectedEventArgs e)
            {
                GetClient().Network.SimConnected -= BotLoggedIn;
                GetClient().Self.IM += BotImMessage;
                GetClient().Self.ChatFromSimulator += LocalChat;
                botConnected = true;
            }

            public override void Start()
            {
                if (myConfig.GetEnabled() == false)
                {
                    return;
                }
                running = true;
                master.BotClientNoticeEvent += BotClientRestart;
            }

            public override void Stop()
            {
                if (myConfig.GetEnabled() == false)
                {
                    return;
                }
                running = false;
                master.BotClientNoticeEvent -= BotClientRestart;
            }

            protected int RlvoutputChannel = 0;

            protected void rlvEvent(UUID fromAvatar, string message, bool fromAv, UUID Source)
            {
                if((fromAvatar != GetClient().Self.AgentID) && (master.CommandsService.AvatarUUIDIsMaster(fromAvatar) == false))
                {
                    return;
                }
                if(message.StartsWith("@") == false)
                {
                    return;
                }
                
                string[] bits = message.Split(',');
                foreach(string bit in bits)
                {
                    string[] subbits = bit.Split("=", 2);
                    // if 2 subbits then command = args
                    // if 1 then just command
                    string command = subbits[0];
                    string? subcommand = null;
                    string args = "";
                    if (subbits.Length > 1)
                    {
                        args = subbits[1];
                    }
                    subbits = command.Split(":");
                    command = subbits[0];
                    if (subbits.Length > 1)
                    {
                        subcommand = subbits[1];
                    }
                    rlvCommand(command, subcommand, args, fromAv, Source);
                }
            }
            // https://wiki.secondlife.com/wiki/LSL_Protocol/RestrainedLoveAPI#Audience
            // currently to inventory

            protected void rlvCommand(string command,string? subcommand, string args,bool fromAv,UUID Source)
            {
                int argAsInt = RlvoutputChannel;
                if(int.TryParse(args, out argAsInt) == false)
                {
                    argAsInt = RlvoutputChannel;
                }
                switch(command)
                {
                    case "version":
                    case "versionnew":
                        {
                            ChatOutput("RestrainedLife viewer v2.8 (Secondbot [Events] "+master.GetVersion()+")", argAsInt);
                            break;
                        }
                    case "versionnum":
                        {
                            ChatOutput("2080000", argAsInt);
                            break;
                        }
                    case "versionnumbl":
                        {
                            ChatOutput("2080000" + GetBlackListAsString(subcommand), argAsInt);
                            break;
                        }
                    case "getblacklist":
                        {
                            if(fromAv == false)
                            {
                                break;
                            }
                            ImChatOutput(Source, "Current RLV blacklist is "+GetBlackListAsString(subcommand));
                            break;
                        }
                    case "notify":
                        {
                            string[] bits = subcommand.Split(';');
                            if (bits.Length == 1)
                            {
                                bits = new string[] { "*", bits[0] };
                            }
                            if ((args == "add") || (args == "y"))
                            {
                                NotifyRules = AddRule(Source, bits[0], bits[1], "*", NotifyRules);
                                break;
                            }
                            NotifyRules = RemoveRule(bits[0], bits[1],"*", NotifyRules);
                            break;
                        }
                    case "sitground":
                        {
                            if (args != "force")
                            {
                                break;
                            }
                            Self A = new Self(master);
                            A.Sit("ground");
                            break;
                        }
                    case "sit":
                        {
                            if ((args == "add") || (args == "y") || (args == "rem") || (args == "n"))
                            {
                                RlvRules = RemoveRule(command, "*", "*", RlvRules);
                                if ((args == "add") || (args == "y"))
                                {
                                    RlvRules = AddRule(Source, command, "*", "*", RlvRules);
                                }
                                break;
                            }
                            if (args != "force")
                            {
                                break;
                            }
                            Self A = new Self(master);
                            A.Sit(subcommand);
                            break;
                        }
                    case "unsit":
                        {
                            if ((args == "add") || (args == "y") || (args == "rem") || (args == "n"))
                            {
                                RlvRules = RemoveRule(command, "*", "*", RlvRules);
                                if ((args == "add") || (args == "y"))
                                {
                                    RlvRules = AddRule(Source, command, "*", "*", RlvRules);
                                }
                                break;
                            }
                            if (args != "force")
                            {
                                break;
                            }
                            Self A = new Self(master);
                            A.Stand();
                            break;
                        }
                    case "detach":
                        {
                            if ((args == "add") || (args == "y") || (args == "rem") || (args == "n"))
                            {
                                RlvRules = RemoveRule(command, subcommand, "*", RlvRules);
                                if ((args == "add") || (args == "y"))
                                {
                                    RlvRules = AddRule(Source, command, subcommand, "*", RlvRules);
                                }
                                break;
                            }
                            if (args != "force")
                            {
                                break;
                            }
                            InventoryCommands A = new InventoryCommands(master);
                            A.Detach(subcommand);
                            break;
                        }
                    case "getsitid":
                        {
                            uint index = GetClient().Self.SittingOn;
                            if (index > 0)
                            {
                                if (!GetClient().Network.CurrentSim.ObjectsPrimitives.ContainsKey(index))
                                {
                                    ChatOutput("!", argAsInt);
                                    break;
                                }
                                ChatOutput(GetClient().Network.CurrentSim.ObjectsPrimitives[index].ID.ToString(), argAsInt);
                                break;
                            }
                            ChatOutput(UUID.Zero.ToString(), argAsInt);
                            break;
                        }
                    case "chatwhisper":
                    case "chatnormal":
                    case "chatshout":
                    case "sendchat":
                    case "setcam_unlock":
                    case "camunlock":
                    case "alwaysrun":
                    case "temprun":
                    case "fly":
                    case "recvchat_sec":
                    case "sendgesture":
                    case "emote":
                    case "recvemote_sec":
                    case "sendim_sec":
                    case "tplm":
                    case "tploc":
                    case "tplure_sec":
                    case "showinv":
                    case "viewnote":
                    case "viewscript":
                    case "viewtexture":
                    case "rez":
                    case "editworld":
                    case "editattach":
                    case "share_sec":
                    case "permissive":
                        {
                            RlvRules = RemoveRule(command, "*", "*", RlvRules);
                            if ((args == "add") || (args == "y"))
                            {
                                RlvRules = AddRule(Source, command, "*", "*", RlvRules);
                            }
                            break;
                        }
                    case "setrot":
                        {
                            if(args != "force")
                            {
                                break;
                            }
                            if(double.TryParse(subcommand, System.Globalization.NumberStyles.Float, Utils.EnUsCulture, out double rot) == false)
                            {
                                break;
                            }
                            GetClient().Self.Movement.UpdateFromHeading(Math.PI / 2d - rot, true);
                            break;
                        }
                    case "tpto":
                        {
                            if (args != "force")
                            {
                                break;
                            }
                            if (subcommand == null)
                            {
                                break;
                            }
                            string[] bits = subcommand.Split(';');
                            if(bits.Length == 2)
                            {
                                // look at (ignored as docs suck)
                                subcommand = bits[0];
                            }
                            bits = subcommand.Split('/');
                            if(bits.Length == 3)
                            {
                                master.HomeboundService.MarkTeleport();
                                Movement A = new Movement(master);
                                A.Teleport(GetClient().Network.CurrentSim.Name, bits[0], bits[1], bits[2]);
                                break;
                            }
                            else if(bits.Length == 4)
                            {
                                master.HomeboundService.MarkTeleport();
                                Movement A = new Movement(master);
                                A.Teleport(bits[0], bits[1], bits[2], bits[3]);
                                break;
                            }
                            break;
                        }
                    case "edit":
                    case "getcam_fov":
                    case "camtextures":
                    case "camavdist":
                    case "camdrawmax":
                    case "camdrawmin":
                    case "camdistmax":
                    case "camzoommin":
                    case "camzoommax":
                    case "setcam_textures":
                    case "setcam_fov":
                    case "adjustheight":
                    case "standtp":
                        {
                            break;
                        }
                    case "redirchat":
                    case "camdrawalphamax":
                    case "camdrawalphamin":
                    case "setcam_avdistmin":
                    case "setcam_avdistmax":
                    case "setcam_fovmax":
                    case "setcam_fovmin":
                    case "recvchat":
                    case "recvchatfrom":
                    case "rediremote":
                    case "recvemotefrom":
                    case "recvemote":
                    case "sendchannel":
                    case "sendchannel_sec":
                    case "sendchannel_except":
                    case "sendim":
                    case "sendimto":
                    case "startim":
                    case "startimto":
                    case "recvim":
                    case "recvimfrom":
                    case "tplocal":
                    case "accepttp":
                    case "accepttprequest":
                    case "tprequest":
                    case "editobj":
                    case "share":
                    case "tplure":
                        {
                            RlvRules = RemoveRule(command, subcommand, "*", RlvRules);
                            if ((args == "add") || (args == "y"))
                            {
                                RlvRules = AddRule(Source, command, subcommand, "*", RlvRules);
                            }
                            break;
                        }
                    case "getcam_fovmax":
                    case "getcam_avdistmax":
                        {
                            GetMaxRulesetValue(command.Replace("get", "set"), argAsInt);
                            break;
                        }
                    case "getcam_zoommin":
                    case "getcam_avdistmin":
                    case "getcam_fovmin":
                        {
                            GetMinRulesetValue(command.Replace("get","set"), argAsInt);
                            break;
                        }
                }
            }
            protected void GetMaxRulesetValue(string ruleName, int channelout)
            {
                List<RLVRule> appliedRules = GetRules(ruleName);
                int maxDis = 9999;
                foreach (RLVRule rule in appliedRules)
                {
                    if (int.TryParse(rule.Option, out int value) == true)
                    {
                        if (value < maxDis)
                        {
                            maxDis = value;
                        }
                    }
                }
                ChatOutput(maxDis.ToString(), channelout);
            }

            protected void GetMinRulesetValue(string ruleName, int channelout)
            {
                List<RLVRule> appliedRules = GetRules(ruleName);
                int minDis = 0;
                foreach (RLVRule rule in appliedRules)
                {
                    if (int.TryParse(rule.Option, out int value) == true)
                    {
                        if (value > minDis)
                        {
                            minDis = value;
                        }
                    }
                }
                ChatOutput(minDis.ToString(), channelout);
            }

            protected List<RLVRule> GetRules(string command)
            {
                List<RLVRule> reply = new List<RLVRule>();
                foreach(RLVRule rule in RlvRules)
                {
                    if(rule.Behaviour == command)
                    {
                        reply.Add(rule);
                    }
                }
                return reply;

            }

            protected List<RLVRule> AddRule(UUID Source,string Behaviour, string Option, string Param, List<RLVRule> ruleset)
            {
                RLVRule newRule = new RLVRule();
                newRule.Behaviour = "permissive";
                newRule.Sender = Source;
                newRule.Option = "*";
                newRule.Param = "*";
                ruleset.Add(newRule);
                return ruleset;
            }

            protected List<RLVRule> RemoveRule(string Behaviour, string Option,string Param,List<RLVRule> ruleset)
            {
                bool foundRule = true;
                while (foundRule == true)
                {
                    foundRule = false;
                    int index = -1;
                    foreach (RLVRule rule in ruleset)
                    {
                        if ((rule.Behaviour == Behaviour) && (rule.Option == Option) && (rule.Param == Param))
                        {
                            index = NotifyRules.IndexOf(rule);
                            break;
                        }
                    }
                    if (index != -1)
                    {
                        foundRule = true;
                        ruleset.RemoveAt(index);
                    }
                }
                return ruleset;
            }

            protected string GetBlackListAsString(string? filter=null)
            {
                return "";
            }

            protected void ImChatOutput(UUID target, string message)
            {

            }

            protected void ChatOutput(string message, int chan=0)
            {
                GetClient().Self.Chat(message, chan, ChatType.Normal);
            }

            protected void LocalChat(object o, ChatEventArgs e)
            {
                if(myConfig.GetAllowRlvViaObjectOwnerSay() == false)
                {
                    return;
                }
                switch (e.Type)
                {
                    case ChatType.OwnerSay:
                        {
                            rlvEvent(e.OwnerID, e.Message,false, e.SourceID);
                            break;
                        }
                    case ChatType.Whisper:
                    case ChatType.Normal:
                    case ChatType.Shout:
                    case ChatType.RegionSayTo:
                    default:
                        {
                            break;
                        }
                }
            }

            protected void BotImMessage(object o, InstantMessageEventArgs e)
            {
                if (myConfig.GetAllowRlvViaIm() == false)
                {
                    return;
                }
                switch (e.IM.Dialog)
                {
                    case InstantMessageDialog.MessageFromObject:
                        {
                            // trigger object IM
                            rlvEvent(e.IM.FromAgentID, e.IM.Message,false, e.IM.FromAgentID);
                            break;
                        }
                    case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                    case InstantMessageDialog.SessionSend:
                        {
                            if (master.DataStoreService.GetIsGroup(e.IM.IMSessionID) == true)
                            {
                                break;
                            }
                            // trigger avatar IM
                            rlvEvent(e.IM.FromAgentID, e.IM.Message, true, e.IM.FromAgentID);
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

    public class RLVRule
    {
        public string Behaviour { set; get; }
        public string Option { set; get; }
        public string Param { set; get; }
        public UUID Sender { set; get; }
        public string SenderName { set; get; }

        public override string ToString()
        {
            return string.Format("{0}: {1}:{2}={3} [{4}]", SenderName, Behaviour, Option, Param, Sender);
        }
    }

}
