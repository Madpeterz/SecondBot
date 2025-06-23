﻿using SecondBotEvents.Config;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Threading;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using Swan;
using OpenMetaverse;

namespace SecondBotEvents.Services
{
    public class RabbitService : BotServices
    {
        protected async Task<Task> CommandMessageEvent(object o, BasicDeliverEventArgs e)
        {
            try
            {
                JsonCommandFormat result = JsonConvert.DeserializeObject<JsonCommandFormat>(Encoding.UTF8.GetString(e.Body.ToArray()));
                if (result == null || string.IsNullOrEmpty(result.CommandName))
                {
                    if (myConfig.GetLogDebug() == true)
                    {
                        LogFormater.Warn("Command Message format is invalid or missing fields. " + Encoding.UTF8.GetString(e.Body.ToArray()));
                    }
                    return Task.CompletedTask;
                }
                if (myConfig.GetLogDebug() == true)
                {
                    LogFormater.Info($"Command Message UUID: {result.CommandName}, args: {String.Join(",", result.commandArgs)}");
                }
                SignedCommand C = new(master.CommandsService, "rabbit", result.CommandName, "", result.commandArgs, 0, "", false, 0, "", false);
                master.CommandsService.RunCommand("rabbit", C, false);
            }
            catch (Exception E)
            {
                LogFormater.Warn("Command Message decode failed " + E.Message + " raw=" + Encoding.UTF8.GetString(e.Body.ToArray()));
            }
            return Task.CompletedTask;
        }
        protected async Task<Task> NotecardMessageEvent(object o, BasicDeliverEventArgs e)
        {
            try
            {
                JsonNotecardFormat result = JsonConvert.DeserializeObject<JsonNotecardFormat>(Encoding.UTF8.GetString(e.Body.ToArray()));
                if (
                    result == null ||
                    string.IsNullOrEmpty(result.Name) ||
                    string.IsNullOrEmpty(result.UUID) ||
                    string.IsNullOrEmpty(result.Content) ||
                    string.IsNullOrEmpty(result.Subject)
                )
                {
                    if (myConfig.GetLogDebug() == true)
                    {
                        LogFormater.Warn("Notecard Message format is invalid or missing fields. " + Encoding.UTF8.GetString(e.Body.ToArray()));
                    }
                    return Task.CompletedTask;
                }
                if (myConfig.GetLogDebug() == true)
                {
                    LogFormater.Info($"Notecard Message Name: {result.Name}, Subject: {result.Subject}");
                }
                if (UUID.TryParse(result.UUID, out UUID uuid) == false)
                {
                    LogFormater.Warn("Notecard Message UUID is not valid: " + result.UUID);
                    return Task.CompletedTask;
                }
                master.BotClient.SendNotecard(result.Name, result.Content, uuid, false, result.Subject);
            }
            catch (Exception E)
            {
                LogFormater.Warn("Notecard Message decode failed " + E.Message + " raw=" + Encoding.UTF8.GetString(e.Body.ToArray()));
            }
            return Task.CompletedTask;
        }
        protected async Task<Task> ImMessageEvent(object o, BasicDeliverEventArgs e)
        {
            Console.WriteLine("Message received: " + Encoding.UTF8.GetString(e.Body.ToArray()));
            try
            {
                JsonMessageFormat result = JsonConvert.DeserializeObject<JsonMessageFormat>(Encoding.UTF8.GetString(e.Body.ToArray()));
                if (result == null || string.IsNullOrEmpty(result.UUID) || string.IsNullOrEmpty(result.Message))
                {
                    if (myConfig.GetLogDebug() == true)
                    {
                        LogFormater.Warn("IM Message format is invalid or missing fields. " + Encoding.UTF8.GetString(e.Body.ToArray()));
                    }
                    return Task.CompletedTask;
                }
                if (myConfig.GetLogDebug() == true)
                {
                    LogFormater.Info($"IM Message UUID: {result.UUID}, Message: {result.Message}");
                }
                if (UUID.TryParse(result.UUID, out UUID uuid) == false)
                {
                    LogFormater.Warn("IM Message UUID is not valid: " + result.UUID);
                    return Task.CompletedTask;
                }
                GetClient().Self.InstantMessage(uuid, result.Message);
            }
            catch (Exception E)
            {
                LogFormater.Warn("IM Message decode failed " + E.Message + " raw=" + Encoding.UTF8.GetString(e.Body.ToArray()));
            }
            return Task.CompletedTask;
        }
        protected async Task<Task> GroupMessageEvent(object o, BasicDeliverEventArgs e)
        {
            Console.WriteLine("Message received: " + Encoding.UTF8.GetString(e.Body.ToArray()));
            try
            {
                JsonMessageFormat result = JsonConvert.DeserializeObject<JsonMessageFormat>(Encoding.UTF8.GetString(e.Body.ToArray()));
                if (result == null || string.IsNullOrEmpty(result.UUID) || string.IsNullOrEmpty(result.Message))
                {
                    if (myConfig.GetLogDebug() == true)
                    {
                        LogFormater.Warn("Group IM Message format is invalid or missing fields. " + Encoding.UTF8.GetString(e.Body.ToArray()));
                    }
                    return Task.CompletedTask;
                }
                if (myConfig.GetLogDebug() == true)
                {
                    LogFormater.Info($"Group Message UUID: {result.UUID}, Message: {result.Message}");
                }
                if (UUID.TryParse(result.UUID, out UUID uuid) == false)
                {
                    LogFormater.Warn("Group Message UUID is not valid: " + result.UUID);
                    return Task.CompletedTask;
                }
                GetClient().Self.InstantMessageGroup(uuid, result.Message);
            }
            catch (Exception E)
            {
                LogFormater.Warn("Group IM Message decode failed " + E.Message + " raw=" + Encoding.UTF8.GetString(e.Body.ToArray()));
            }
            return Task.CompletedTask;
        }


        public new RabbitConfig myConfig = null;
        protected bool hasSecondbot = false;
        public RabbitService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new RabbitConfig(master.fromEnv, master.fromFolder);
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
        }
        ConnectionFactory factory;
        IConnection connection;
        Dictionary<string, IChannel> channels = new Dictionary<string, IChannel>();
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
            LogFormater.Info("Rabbit service [Starting]");
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
        }

        protected void connectService()
        {
            if (connection != null && connection.IsOpen == true)
            {
                connection.CloseAsync().Await();
                connection = null;
            }
            var endpoints = new System.Collections.Generic.List<AmqpTcpEndpoint> {
              new AmqpTcpEndpoint(myConfig.GetHostIP(),myConfig.GetHostPort()),
            };
            ConnectionFactory factory = new ConnectionFactory();
            factory.UserName = myConfig.GetHostUsername();
            factory.Password = myConfig.GetHostPassword();
            try
            {
                connection = factory.CreateConnectionAsync(endpoints).Await();
                if (connection == null || connection.IsOpen == false)
                {
                    LogFormater.Crit("Rabbit service [Failed to connect to RabbitMQ] stopping", true);
                    Stop();
                    return;
                }

                createChannelWorker(myConfig.GetNotecardQueue(), "NotecardMessageEvent");
                createChannelWorker(myConfig.GetCommandQueue(), "CommandMessageEvent");
                createChannelWorker(myConfig.GetImQueue(), "ImMessageEvent");
                createChannelWorker(myConfig.GetGroupImQueue(), "GroupMessageEvent");
            }
            catch (Exception ex)
            {
                LogFormater.Crit("Rabbit service [Failed to connect to RabbitMQ] " +
                    "" + myConfig.GetHostIP() + ":" + myConfig.GetHostPort() + " " +
                    "with user: " + myConfig.GetHostUsername() + "\n" +
                    "error:" + ex.Message, true);
                Stop();
                return;
            }


        }

        protected async void createChannelWorker(string queueName, string functionpointer)
        {
            IChannel channel = connection.CreateChannelAsync().Await();
            channels.Add(queueName, channel);
            var consumer = new AsyncEventingBasicConsumer(channel);

            // Use reflection to get the method by name and create a delegate
            var method = typeof(RabbitService).GetMethod(functionpointer, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (method == null)
                throw new InvalidOperationException($"Handler method '{functionpointer}' not found.");

            // Create delegate matching AsyncEventHandler<BasicDeliverEventArgs>
            AsyncEventHandler<BasicDeliverEventArgs> handler =
                (AsyncEventHandler<BasicDeliverEventArgs>)Delegate.CreateDelegate(
                    typeof(AsyncEventHandler<BasicDeliverEventArgs>), this, method);

            consumer.ReceivedAsync += handler;

            string consumerTag = Guid.NewGuid().ToString(); // Generate a unique consumer tag.
            channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null
            ).Await();
            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: true,
                consumerTag: consumerTag,
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer
            );
            if (myConfig.GetLogDebug() == true)
            {
                LogFormater.Info($"Rabbit service [Consumer started for queue: {queueName}]");
            }
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isStart == false)
            {
                return;
            }
            LogFormater.Info("Rabbit service [Attached to new client]");
            hasSecondbot = true;
            connectService();
        }

        public override void Stop()
        {
            if (running == true)
            {
                running = false;
                LogFormater.Info("Rabbit service [Stopping]");
                if (master != null)
                {
                    master.BotClientNoticeEvent -= BotClientRestart;
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
            return "Enabled";
        }
    }

    public class JsonMessageFormat
    {
        public string UUID;
        public string Message;
    }

    public class JsonCommandFormat
    {
        public string CommandName;
        public string[] commandArgs;
    }

    public class JsonNotecardFormat
    {
        public string Name;
        public string Subject;
        public string Content;
        public string UUID;
    }
}
