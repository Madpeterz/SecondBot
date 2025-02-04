using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;
using System.Net.Http;
using OpenMetaverse.ImportExport.Collada14;
using OpenMetaverse.Packets;
using OpenAI.Managers;
using OpenAI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI.ObjectModels.RequestModels;
using OpenAI.ObjectModels;
using OpenAI.ObjectModels.ResponseModels;
using System.Threading.Tasks;
using OpenAI.Interfaces;
using System.Diagnostics;
using Swan;
using StackExchange.Redis;
using Discord.Rest;

namespace SecondBotEvents.Services
{
    public class ChatGptService : BotServices
    {
        public new ChatGptConfig myConfig = null;
        public ChatGptService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new ChatGptConfig(master.fromEnv, master.fromFolder);
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
        }

        protected int chatHistorySize = 5;
        protected int localchatRateLimit = 3;
        protected int groupchatRateLimit = 3;
        protected int imchatRateLimit = 3;

        protected ConnectionMultiplexer redis = null;
        protected IDatabase redisDb = null;
        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            if (updateEnabled == true)
            {
                myConfig.setEnabled(setEnabledTo);
            }
            if (myConfig.GetEnabled() == false)
            {
                Stop();
                return;
            }
            if(myConfig.GetUseRedis() == true)
            {
                try
                {
                    ConfigurationOptions configRedis = new ConfigurationOptions()
                    {
                        EndPoints = { myConfig.GetRedisSource() }
                    };
                    redis = ConnectionMultiplexer.Connect(configRedis);
                    redisDb = redis.GetDatabase();
                }
                catch (Exception ex)
                {
                    LogFormater.Warn("Redis failed to connect:" + ex.Message);
                }

            }
            chatHistorySize = inrange(3,10,myConfig.GetChatHistoryMessages());
            localchatRateLimit = inrange(1, 10, myConfig.GetLocalchatRateLimiter());
            groupchatRateLimit = inrange(1, 10, myConfig.GetGroupReplyRateLimiter());
            imchatRateLimit = inrange(1, 10, myConfig.GetImReplyRateLimiter());
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
        }

        protected int inrange(int value, int min, int max)
        {
            if (value < min) return min;
            else if(value > max) return max;
            return value;
        }

        public override void Stop()
        {
            if (running == true)
            {
                running = false;
                LogFormater.Info("ChatGpt [Stopping]");
                if (master != null)
                {
                    master.BotClientNoticeEvent -= BotClientRestart;
                }
                if (GetClient() != null)
                {
                    if (GetClient().Network != null)
                    {
                        GetClient().Network.SimConnected -= BotLoggedIn;
                    }
                    if (GetClient().Self != null)
                    {
                        GetClient().Self.IM -= BotImMessage;
                        GetClient().Self.ChatFromSimulator -= BotLocalchat;
                    }
                }
            }
        }

        public override string Status()
        {
            if (myConfig == null)
            {
                return "No Config";
            }
            else if (myConfig.GetEnabled() == false)
            {
                return "Disabled";
            }
            upkeep();
            if (chatHistoryAI.Count() > 0)
            {
                return "Enabled running: " + chatHistoryAI.Count().ToString() + " chat windows";
            }
            return "Enabled No chat history";
        }

        readonly string[] hard_blocked_agents = new[] { "secondlife", "second life" };
        protected void BotLocalchat(object o, ChatEventArgs e)
        {
            if (e.SourceID == GetClient().Self.AgentID)
            {
                return;
            }
            if (myConfig.GetLocalchatReply() == false)
            {
                return;
            }
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
                        if (e.SourceType == ChatSourceType.Object)
                        {
                            break;
                        }
                        else if (e.SourceType == ChatSourceType.System)
                        {
                            break;
                        }
                        if (e.Type == ChatType.OwnerSay)
                        {
                            break;
                        }
                        // trigger localchat
                        GetAiReply(localchatRateLimit, UUID.Zero, UUID.Zero, e.FromName, e.Message, false, false);
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
            if (e.IM.FromAgentID == GetClient().Self.AgentID)
            {
                return;
            }
            switch (e.IM.Dialog)
            {
                case InstantMessageDialog.MessageFromObject:
                    {
                        // object IM
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        if (master.DataStoreService.GetIsGroup(e.IM.IMSessionID) == false)
                        {
                            // trigger avatar IM
                            if (myConfig.GetAllowImReplys() == false)
                            {
                                break;
                            }
                            if (myConfig.GetImReplyFriendsOnly() == true)
                            {
                                if (GetClient().Friends.FriendList.ContainsKey(e.IM.FromAgentID) == false)
                                {
                                    break;
                                }
                            }
                            // request GPT reply to avatar IM
                            GetAiReply(imchatRateLimit, e.IM.FromAgentID, e.IM.FromAgentID, e.IM.FromAgentName, e.IM.Message, true);
                            break;
                        }
                        // trigger group IM
                        if (myConfig.GetAllowGroupReplys() == true)
                        {
                            break;
                        }
                        if (myConfig.GetGroupReplyForGroup() != e.IM.IMSessionID.ToString())
                        {
                            break;
                        }
                        GetAiReply(groupchatRateLimit, e.IM.FromAgentID, e.IM.FromAgentID, e.IM.FromAgentName, e.IM.Message, false, true);
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }

        Dictionary<UUID, List<KeyValuePair<string, string>>> chatHistoryAI = new Dictionary<UUID, List<KeyValuePair<string, string>>>();
        Dictionary<UUID, long> ChatHistoryLastAccessed = new Dictionary<UUID, long>();
        Dictionary<UUID, long> ChatRateLimiter = new Dictionary<UUID, long>();

        protected long lastUpkeep = 0;
        protected void upkeep()
        {
            long dif = SecondbotHelpers.UnixTimeNow() - lastUpkeep;
            if (dif < 30)
            {
                return; // upkeep not needed
            }
            lastUpkeep = SecondbotHelpers.UnixTimeNow();
            // check for expired chat windows (longer than 2 mins from last message)
            lock (ChatHistoryLastAccessed) lock (chatHistoryAI) lock (ChatRateLimiter)
                    {
                        List<UUID> needcleaning = new List<UUID>();
                        long now = SecondbotHelpers.UnixTimeNow();
                        foreach (KeyValuePair<UUID, long> entry in ChatHistoryLastAccessed)
                        {
                            dif = now - entry.Value;
                            if(dif > (myConfig.GetChatHistoryTimeout()*60))
                            {
                                needcleaning.Add(entry.Key);
                            }
                        }
                        foreach(UUID a in needcleaning)
                        {
                            if(chatHistoryAI.ContainsKey(a))
                            {
                                chatHistoryAI.Remove(a);
                            }
                            if(ChatHistoryLastAccessed.ContainsKey(a))
                            {
                                ChatHistoryLastAccessed.Remove(a);
                            }
                            if (ChatRateLimiter.ContainsKey(a))
                            {
                                ChatRateLimiter.Remove(a);
                            }
                        }
                    }
        }

        protected bool RedisLive()
        {
            // are we using redis?
            if (myConfig.GetUseRedis() == false)
            {
                return false; // not using redis no need to do any more checks
            }
            else if (redis == null)
            {
                return false; // redis is dead
            }
            else if (redisDb == null)
            {
                return false; // no connection to DB
            }
            else if (redis.IsConnected == false)
            {
                // redis is down, use local storage
                return false;
            }
            return true;
        }

        protected List<KeyValuePair<string, string>> GetHistoryFromStorage(UUID store,bool avatarchat,bool groupchat)
        {
            // are we using redis?
            if (RedisLive() == false)
            {
                return new List<KeyValuePair<string, string>>();
            }
            // is redis enabled for this message type?
            if ((myConfig.GetRedisImchat() == true) && (avatarchat == true))
            {
                // we need to read from redis for this IM chat window
                return ReadChatFromRedis(store);
            }
            else if ((myConfig.GetRedisGroupchat() == true) && (groupchat == true))
            {
                // we need to read from redis for this group chat window
                return ReadChatFromRedis(store);
            }
            else if(myConfig.GetRedisLocalchat() == true) 
            {
                // we need to read from redis for this local chat window
                return ReadChatFromRedis(store);
            }
            // Redis is not enabled for this message type, use local storage
            return new List<KeyValuePair<string, string>>();

        }

        protected List<KeyValuePair<string, string>> ReadChatFromRedis(UUID store)
        {
            try
            {
                RedisKey readkey = new RedisKey(myConfig.GetRedisPrefix() + store.Guid.ToString());
                if (redisDb.KeyExists(readkey) == false)
                {
                    string rawstring = redisDb.StringGet(readkey);
                    if (rawstring == null)
                    {
                        return chatHistoryAI[store]; // nothing in memory use the default store
                    }
                    redisDb.KeyExpire(readkey, TimeSpan.FromMinutes(myConfig.GetRedisMaxageMins())); // update the expire value
                    return JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(rawstring);
                }
                return NoRedisStoreFound(store);
            }
            catch (Exception ex)
            {
                LogFormater.Warn("Redis failed to unpack json using default store:" + ex.Message);
                return NoRedisStoreFound(store);
            }
        }

        protected List<KeyValuePair<string, string>> NoRedisStoreFound(UUID store)
        {
            if (chatHistoryAI.ContainsKey(store) == false)
            {
                return new List<KeyValuePair<string, string>>();
            }
            return chatHistoryAI[store];
        }

        protected List<KeyValuePair<string, string>> GetHistory(UUID store, bool avatarchat, bool groupchat, string talkingto)
        {
            // update access markers
            if (ChatHistoryLastAccessed.ContainsKey(store) == false)
            {
                ChatHistoryLastAccessed.Add(store, 0);
            }
            ChatHistoryLastAccessed[store] = SecondbotHelpers.UnixTimeNow();
            List<KeyValuePair<string, string>> history = GetHistoryFromStorage(store, avatarchat, groupchat);
            if(history.Count == 0)
            {
                // no history loaded from redis, check in memory
                if (chatHistoryAI.ContainsKey(store) == false)
                {
                    // totaly new chat build it out
                    chatHistoryAI.Add(store, new List<KeyValuePair<string, string>>());
                    string yourname = GetClient().Self.FirstName;
                    if (myConfig.GetCustomName() != "<!FIRSTNAME!>")
                    {
                        yourname = myConfig.GetCustomName();
                    }
                    if (avatarchat == true)
                    {
                        history.Add(new KeyValuePair<string, string>("system", "You are " + yourname + ", " + myConfig.GetChatPrompt() + ", you are talking to " + talkingto + "."));
                    }
                    else if (groupchat == true)
                    {
                        history.Add(new KeyValuePair<string, string>("system", "You are " + yourname + ", " + myConfig.GetChatPrompt() + ", you are talking to a group of people."));
                    }
                    else
                    {
                        history.Add(new KeyValuePair<string, string>("system", "You are " + yourname + ", " + myConfig.GetChatPrompt() + ", you are talking to people in a public place."));
                    }
                    // history ready, save to redis if in use
                    if(StoreHistory(store, history, avatarchat, groupchat) == false)
                    {
                        // unable to save to redis, use local storage
                        chatHistoryAI[store] = history;
                        return history;
                    }
                }
                else
                {
                    history = chatHistoryAI[store];
                }
                return history;
            }
            return history;
        }

        protected bool StoreHistory(UUID store, List<KeyValuePair<string, string>> history, bool avatarchat, bool groupchat)
        {
            if (RedisLive() == false)
            {
                return false;
            }
            bool allowStore = false;
            if ((myConfig.GetRedisImchat() == true) && (avatarchat == true))
            {
                allowStore = true;
            }
            else if ((myConfig.GetRedisGroupchat() == true) && (groupchat == true))
            {
                // we need to read from redis for this group chat window
                allowStore = true;
            }
            else if (myConfig.GetRedisLocalchat() == true)
            {
                // we need to read from redis for this local chat window
                allowStore = true;
            }
            if(allowStore == false)
            {
                return false;
            }
            int maxsize = inrange(1, 9999, myConfig.GetRedisCountLocal());
            if(avatarchat == true)
            {
                maxsize = inrange(1, 9999, myConfig.GetRedisCountIm());
            }
            else if(groupchat == true)
            {
                maxsize = inrange(1, 9999, myConfig.GetRedisCountGroup());
            }
            // trim the history using targeted max values
            while (history.Count() > maxsize + 1)
            {
                history.RemoveAt(1);
            }
            RedisKey writekey = new RedisKey(myConfig.GetRedisPrefix() + store.Guid.ToString());
            try
            {
                string savestring = JsonConvert.SerializeObject(history);
                if (savestring == null)
                {
                    LogFormater.Warn("Redis failed to convert history into savable format");
                    return false;
                }
                RedisValue storeme = new RedisValue(savestring);
                if(redisDb.StringSet(writekey, storeme) == true)
                {
                    return redisDb.KeyExpire(writekey, TimeSpan.FromMinutes(myConfig.GetRedisMaxageMins()));
                }
                return false;
            }
            catch (Exception ex)
            {
                LogFormater.Warn("Redis failed to save into store:" + ex.Message);
                return false;
            }
        }

        protected List<KeyValuePair<string, string>> AddHistory(UUID store, bool avatarchat, bool groupchat, string talkingto, string role, string message)
        {
            List<KeyValuePair<string, string>> history = GetHistory(store, avatarchat, groupchat, talkingto);
            history.Add(new KeyValuePair<string, string>(role, message));
            if(StoreHistory(store, history, avatarchat, groupchat) == false) // try and save using redis
            {
                // unable to save to redis, write to local storage
                return BasicSaveHistory(store, history);
            }
            return history;
        }

        protected List<KeyValuePair<string, string>> BasicSaveHistory(UUID store, List<KeyValuePair<string, string>> history)
        {
            // trim the history using basic config
            while (history.Count() > chatHistorySize + 1)
            {
                history.RemoveAt(1);
            }
            chatHistoryAI[store] = history;
            return history;
        }

        protected async void GetAiReply(int ratelimiter, UUID replyTo, UUID user, string name, string message, bool avatarchat = false, bool groupchat = false)
        {
            bool allowedChat = true;
            lock(ChatRateLimiter)
            {
                if (ChatRateLimiter.ContainsKey(user) == false)
                {
                    ChatRateLimiter.Add(user, 0);
                }
                long dif = SecondbotHelpers.UnixTimeNow() - ChatRateLimiter[user];
                if(dif <= ratelimiter)
                {
                    allowedChat = false;
                }
            }
            if(allowedChat == false)
            {
                return;
            }
            lock (ChatRateLimiter)
            {
                ChatRateLimiter[user] = SecondbotHelpers.UnixTimeNow() + 1;
            }
                List<ChatMessage> messages = new List<ChatMessage>();
            lock (chatHistoryAI) lock (ChatHistoryLastAccessed)
                {
                    // convert history into the AI format while adding the new message
                    try
                    {
                        foreach (KeyValuePair<string, string> entry in AddHistory(user, avatarchat, groupchat, name, "user", "" + name + " says " + message))
                        {
                            string role = entry.Key.ToLower();
                            if (role != "system" && role != "user" && role != "assistant")
                            {
                                throw new ArgumentException("Invalid role provided in history. Role must be 'system', 'user', or 'assistant'.");
                            }
                            string thismessage = entry.Value;
                            if(thismessage == null)
                            {
                                throw new ArgumentException("Message can not be empty");
                            }
                            if (role == "system") messages.Add(ChatMessage.FromSystem(thismessage));
                            else if (role == "user") messages.Add(ChatMessage.FromUser(thismessage));
                            else if (role == "assistant") messages.Add(ChatMessage.FromAssistant(thismessage));
                        }
                    }
                    catch (Exception ex)
                    {
                        if (myConfig.GetShowDebug() == true)
                        {
                            LogFormater.Warn("error building AI request:" + ex.Message);
                        }
                        return;
                    }
                }
            if(messages.Count() == 0)
            {
                LogFormater.Warn("No messages given");
                return;
            }
            try
            {
                string replyMessage = "";
                var openAiService = new OpenAIService(new OpenAiOptions()
                {
                    ApiKey = myConfig.GetApiKey(),
                });
                if((myConfig.GetOrganizationId() != null) && (myConfig.GetOrganizationId() != "none"))
                {
                    openAiService = new OpenAIService(new OpenAiOptions()
                    {
                        ApiKey = myConfig.GetApiKey(),
                        Organization = myConfig.GetOrganizationId(),
                    });
                }
                if(myConfig.GetProvider() != "openai")
                {
                    openAiService = new OpenAIService(new OpenAiOptions()
                    {
                        ApiKey = myConfig.GetApiKey(),
                        Organization = myConfig.GetOrganizationId(),
                        BaseDomain= myConfig.GetProvider(),
                    });
                }
                ChatCompletionCreateResponse completionResult = null;
                if (myConfig.GetProvider() != "openai")
                {
                    completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                    {
                        Messages = messages,
                        Model = myConfig.GetUseModel(),
                    });
                }
                else
                {
                    completionResult = await openAiService.ChatCompletion.CreateCompletion(new ChatCompletionCreateRequest
                    {
                        Messages = messages,
                        Model = openAI_Models.GetModel(myConfig.GetUseModel())
                    });

                }
                if (completionResult == null)
                {
                    return;
                }
                if (completionResult.Successful)
                {
                    replyMessage = completionResult.Choices.First().Message.Content;
                }
                if (replyMessage != "")
                {
                    lock (chatHistoryAI) lock (ChatHistoryLastAccessed)
                        {
                            AddHistory(user, avatarchat, groupchat, name, "assistant", replyMessage);
                        }
                    if (myConfig.GetFakeTypeDelay() == true)
                    {
                        if ((avatarchat == false) && (groupchat == false))
                        {
                            GetClient().Self.AnimationStart(Animations.TYPE, true);
                        }


                        double timespanwait = EstimateTypingTime(replyMessage, 0.4);

                        if (timespanwait > 3)
                        {
                            timespanwait = 3;
                        }
                        else if (timespanwait < 1)
                        {
                            timespanwait = 1;
                        }
                        await Task.Delay(TimeSpan.FromSeconds(timespanwait));
                    }
                    if (avatarchat == true)
                    {
                        GetClient().Self.InstantMessage(replyTo, replyMessage);
                    }
                    else if (groupchat == true)
                    {
                        GetClient().Self.InstantMessageGroup(replyTo, replyMessage);
                    }
                    else
                    {
                        GetClient().Self.Chat(replyMessage, 0, ChatType.Normal);
                        if (myConfig.GetFakeTypeDelay() == true)
                        {
                            GetClient().Self.AnimationStop(Animations.TYPE, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (myConfig.GetShowDebug() == true)
                {
                    LogFormater.Warn("An error occurred:" + ex.Message);
                }
            }
        }

        private static readonly Random _random = new Random();

        public static double EstimateTypingTime(string input, double randomizationFactor)
        {
            // Average typing speed in characters per minute
            double typingSpeedCPM = 200.0;

            // Calculate the length of the input string
            int length = input.Length;

            // Calculate the base time in minutes
            double baseTimeInMinutes = length / typingSpeedCPM;

            // Convert minutes to seconds
            double baseTimeInSeconds = baseTimeInMinutes * 60;

            if(baseTimeInSeconds > 3)
            {
                baseTimeInSeconds = 3;
            }

            // Calculate the randomized adjustment
            // randomizationFactor of 0 results in no change
            // randomizationFactor of 1 results in instant typing (0 seconds)
            double adjustment = _random.NextDouble() * randomizationFactor * baseTimeInSeconds;

            // Apply the randomization factor
            double estimatedTimeInSeconds = baseTimeInSeconds - adjustment;

            // Ensure the time is not negative
            return Math.Max(estimatedTimeInSeconds, 0);
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isStart == false)
            {
                return;
            }
            LogFormater.Info("ChatGpt [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            GetClient().Network.SimConnected += BotLoggedIn;
            LogFormater.Info("ChatGpt [Waiting for connect]");
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            GetClient().Self.IM += BotImMessage;
            GetClient().Self.ChatFromSimulator += BotLocalchat;
            LogFormater.Info("ChatGpt [accepting chat input]");
        }
    }

    public class openAI_Models
    {
        public static string GetModel(string input)
        {
            if (input == "gpt-3.5-turbo")
            {
                return Models.Gpt_3_5_Turbo;
            }
            else if (input == "gpt-4-mini")
            {
                return Models.Gpt_4o_mini;
            }
            else if (input == "gpt-4-turbo")
            {
                return Models.Gpt_4_turbo;
            }
            return input;
        }
    }
}
