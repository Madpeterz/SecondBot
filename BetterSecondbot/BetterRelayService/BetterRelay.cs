using BetterSecondBot.DiscordSupervisor;
using BetterSecondBotShared.IO;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using BetterSecondBot.bottypes;
using Newtonsoft.Json;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;

namespace BetterSecondBot.BetterRelayService
{
    public class BetterRelay
    {
        Discord_super superV = null;
        Cli controler = null;

        List<relay_config> DiscordRelay = new List<relay_config>();
        List<relay_config> LocalchatRelay = new List<relay_config>();
        List<relay_config> ObjectIMRelay = new List<relay_config>();
        List<relay_config> AvatarIMRelay = new List<relay_config>();
        List<relay_config> GroupChatRelay = new List<relay_config>();

        public BetterRelay(Cli setcontroler, Discord_super setdiscord, bool running_in_docker)
        {
            controler = setcontroler;
            superV = setdiscord;
            if(running_in_docker == true)
            {
                loadRelayConfigFromENV();
            }
            else
            {
                loadRelayConfigFromFile();
            }
            attach_events();
        }

        protected void ApplyRelayConfig(string sourcename,string sourcevalue,string targetname,string targetvalue,bool jsonEncoded)
        {
            relay_config relay = new relay_config();
            relay.encode_as_json = jsonEncoded;
            relay.sourcename = sourcename;
            relay.sourcevalue = sourcevalue;
            relay.targetname = targetname;
            relay.targetvalue = targetvalue;
            if (relay.sourcename == "localchat")
            {
                LocalchatRelay.Add(relay);
            }
            else if (relay.sourcename == "groupchat")
            {
                GroupChatRelay.Add(relay);
            }
            else if (relay.sourcename == "avatarim")
            {
                AvatarIMRelay.Add(relay);
            }
            else if (relay.sourcename == "objectim")
            {
                ObjectIMRelay.Add(relay);
            }
            else if (relay.sourcename == "discord")
            {
                DiscordRelay.Add(relay);
            }
        }

        protected void loadRelayConfigFromENV()
        {
            int loop = 1;
            bool found = true;
            string[] needBits = new string[]{ "asJson", "sourceType", "sourceFilter", "targetType", "targetConfig" };
            while (found == true)
            {
                found = false;
                bool configfound = true;
                Dictionary<string, string> config = new Dictionary<string, string>();
                foreach(string a in needBits)
                {
                    if (helpers.notempty(Environment.GetEnvironmentVariable("CustomRelay_" + loop.ToString()+"_"+a)) == false)
                    {
                        configfound = false;
                        break;
                    }
                    else
                    {
                        config.Add(a, "CustomRelay_" + loop.ToString() + "_" + a);
                    }
                }
                if(configfound == false)
                {
                    break;
                }
                found = true;
                bool asJson = false;
                bool.TryParse(config["asJson"], out asJson);
                ApplyRelayConfig(config["sourceType"], config["sourceFilter"], config["targetType"], config["targetConfig"], asJson);
                loop++;
            }
        }



        protected void loadRelayConfigFromFile()
        {
            JsonCustomRelays Demo = new JsonCustomRelays() { sourceFilter = "all", sourceType = "localchat", targetType = "groupchat", targetConfig = UUID.Zero.ToString() };
            JsonCustomRelaysSet LoadedRelays = new JsonCustomRelaysSet
            {
                Entrys = new JsonCustomRelays[] { Demo }
            };
            string targetfile = "customrelays.json";
            SimpleIO io = new SimpleIO();
            io.ChangeRoot(controler.getFolderUsed());
            if (SimpleIO.FileType(targetfile, "json") == false)
            {
                io.WriteJsonRelays(LoadedRelays, targetfile);
                return;
            }
            if (io.Exists(targetfile) == false)
            {
                io.WriteJsonRelays(LoadedRelays, targetfile);
                return;
            }
            string json = io.ReadFile(targetfile);
            if (json.Length > 0)
            {
                try
                {
                    LoadedRelays = JsonConvert.DeserializeObject<JsonCustomRelaysSet>(json);
                    foreach (JsonCustomRelays loaded in LoadedRelays.Entrys)
                    {
                        ApplyRelayConfig(loaded.sourceType, loaded.sourceFilter, loaded.targetType, loaded.targetConfig, loaded.encodeJson);
                    }
                }
                catch
                {
                    io.MarkOld(targetfile);
                    io.WriteJsonRelays(LoadedRelays, targetfile);
                }
                return;
            }
        }

        public void unattach_events()
        {
            controler.getBot().MessageEvent -= SLMessageHandler;
            superV.MessageEvent -= DiscordMessageHandler;
        }

        protected void attach_events()
        {
            controler.getBot().MessageEvent += SLMessageHandler;
            superV.MessageEvent += DiscordMessageHandler;
        }

        protected HttpClient HTTPclient = new HttpClient();

        protected async Task<Task> TriggerRelay(string sourcetype, string name,string message, string filtervalue, string discordServerid, string discordChannelid)
        {
            List<relay_config> Dataset = new List<relay_config>();
            if (sourcetype == "localchat")
            {
                Dataset = LocalchatRelay;
            }
            else if (sourcetype == "groupchat")
            {
                Dataset = GroupChatRelay;
            }
            else if (sourcetype == "avatarim")
            {
                Dataset = AvatarIMRelay;
            }
            else if (sourcetype == "objectim")
            {
                Dataset = ObjectIMRelay;
            }
            else if (sourcetype == "discord")
            {
                Dataset = DiscordRelay;
            }
            if(message.Contains("[relay]") == true)
            {
                return Task.CompletedTask;
            }

            string message_no_name = "[relay]" + message;
            string message_no_addon = message;
            message = "[relay] " + sourcetype + " # " + name + ": " + message;


            foreach (relay_config cfg in Dataset)
            {
                string sendmessage = message;
                if(cfg.encode_as_json == true)
                {
                    relay_packet packet = new relay_packet();
                    packet.source_message = message_no_name;
                    packet.source_user = name;
                    if (sourcetype == "discord")
                    {
                        packet.discord_channelid = discordChannelid;
                    }
                    sendmessage = JsonConvert.SerializeObject(packet);
                }

                if (((cfg.sourcevalue == "all") && (sourcetype != "discordchat")) || (cfg.sourcevalue == filtervalue))
                {
                    if(cfg.targetname == "discordTTS")
                    {
                        string[] cfga = cfg.targetvalue.Split("@");
                        if (cfga.Length == 2)
                        {
                            if ((cfga[0] + "@" + cfga[1]) != filtervalue)
                            {
                                await controler.getBot().SendMessageToDiscord(cfga[0], cfga[1], message_no_addon, true);
                            }
                        }
                    }
                    else if (cfg.targetname == "discord")
                    {
                        string[] cfga = cfg.targetvalue.Split("@");
                        if (cfga.Length == 2)
                        {
                            if ((cfga[0] + "@" + cfga[1]) != filtervalue)
                            {
                                await controler.getBot().SendMessageToDiscord(cfga[0], cfga[1], message_no_addon, false);
                            }
                        }
                    }
                    else if (cfg.targetname == "http")
                    {
                        Dictionary<string, string> values = new Dictionary<string, string>
                        {
                            { "reply", sendmessage },
                        };

                        var content = new FormUrlEncodedContent(values);
                        try
                        {
                            await HTTPclient.PostAsync(cfg.targetvalue, content);
                        }
                        catch (Exception e)
                        {
                            LogFormater.Crit("[BetterRelay] HTTP failed: " + e.Message + "");
                        }
                    }
                    else if (cfg.targetname == "localchat")
                    {
                        int chan = 0;
                        int.TryParse(cfg.targetvalue, out chan);
                        controler.getBot().GetClient.Self.Chat(sendmessage, chan, ChatType.Normal);
                    }
                    else if (cfg.targetname == "avatarchat")
                    {
                        if (cfg.targetvalue != filtervalue)
                        {
                            if (UUID.TryParse(cfg.targetvalue, out UUID target) == true)
                            {
                                controler.getBot().SendIM(target, sendmessage);
                            }
                        }
                    }
                    else if (cfg.targetname == "groupchat")
                    {
                        if (cfg.targetvalue != filtervalue)
                        {
                            controler.getBot().CallAPI("Groupchat", new[] { cfg.targetvalue, sendmessage });
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }

        protected void DiscordMessageHandler(object sender, DiscordMessageEvent e)
        {
            _ = TriggerRelay("discord", e.name, e.message, "" + e.server.ToString() + "@" + e.channel.ToString(),e.server.ToString(),e.channel.ToString());
        }

        protected void SLMessageHandler(object sender, MessageEventArgs e)
        {
            if (e.fromme == false)
            {
                if (e.localchat == true)
                {
                    _ = TriggerRelay("localchat", e.sender_name, e.message, e.sender_uuid.ToString(), "", "");
                }
                else if (e.avatar == false)
                {
                    _ = TriggerRelay("objectim", e.sender_name, e.message, e.sender_uuid.ToString(), "", "");
                }
                else if (e.group == true)
                {
                    _ = TriggerRelay("groupchat", e.sender_name, e.message, e.group_uuid.ToString(), "", "");
                }
                else if (e.avatar == true)
                {
                    _ = TriggerRelay("avatarim", e.sender_name, e.message, e.sender_uuid.ToString(), "", "");
                }
            }

        }

    }


    public class relay_config
    {
        public string sourcename = "";
        public string sourcevalue = "";
        public string targetname = "";
        public string targetvalue = "";
        public bool encode_as_json = false;
    }

    public class relay_packet
    {
        public string source_user = "";
        public string source_message = "";
        public string discord_channelid = "notused";
    }
}
