using log4net.Core;
using OpenMetaverse;
using OpenMetaverse.Assets;
using SecondBotEvents.Config;
using Swan;
using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;

namespace SecondBotEvents.Services
{
    public class BotClientService : BotServices
    {
        public GridClient client = null;
        public BasicConfig basicCfg = null;

        protected bool ExitBot = false;

        protected Timer AutoRestartLoginTimer;
        public BotClientService(EventsSecondBot setMaster) : base(setMaster)
        {
            basicCfg = new BasicConfig(master.fromEnv, master.fromFolder);
        }

        protected void RestartTimer(object o, ElapsedEventArgs e)
        {
            AutoRestartLoginTimer.Stop();
            Restart();
        }

        public bool IsLoaded()
        {
            return basicCfg.IsLoaded();
        }

        public override void Start()
        {
            running = true;
            LogFormater.Info("Client service [Starting]");
            AutoRestartLoginTimer = new Timer();
            AutoRestartLoginTimer.Interval = 30 * 1000;
            AutoRestartLoginTimer.AutoReset = false;
            AutoRestartLoginTimer.Elapsed += RestartTimer;
            Login();
        }
        public override void Stop()
        {
            if(running == true)
            {
                LogFormater.Info("Client service [Stopping]");
            }
            running = false;
            AutoRestartLoginTimer.Stop();
            ResetClient();
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            LogFormater.Info("Client service ~ Logged out");
        }

        protected void BotSimConnected(object o, SimConnectedEventArgs e)
        {
            if(AutoRestartLoginTimer.Enabled == true)
            {
                GetClient().Self.SetHoverHeight(basicCfg.GetDefaultHoverHeight());
            }
            AutoRestartLoginTimer.Stop();
            LogFormater.Info("Client service ~ Connected to sim: "+e.Simulator.Name);
        }

        protected void BotSimDisconnected(object o, SimDisconnectedEventArgs e)
        {
            LogFormater.Info("Client service ~ Disconnected from sim: " + e.Simulator.Name);
        }

        protected void BotDisconnected(object o, DisconnectedEventArgs e)
        {
            LogFormater.Info("Client service ~ Network disconnected: " + e.Message);
        }

        protected void BotLoginStatus(object o, LoginProgressEventArgs e)
        { 
            if(e.FailReason != "")
            {
                LogFormater.Info("Client service ~ {FAILED} Login status: " + e.FailReason.ToString());
                client.Network.Logout();
                ResetClient();
                return;
            }
            LogFormater.Info("Client service ~ Login status: " + e.Status.ToString());
        }
        public void SendIM(UUID avatar, string message)
        {
            master.DataStoreService.BotRecordReplyIM(avatar,message);
            client.Self.InstantMessage(avatar, message);
        }

        public bool SendNotecard(string name, string content, UUID sendToUUID, bool attachDateTime=true)
        {
            bool returnstatus = true;
            if (attachDateTime == true)
            {
                name = name + " " + DateTime.Now;
            }
            client.Inventory.RequestCreateItem(
                client.Inventory.FindFolderForType(AssetType.Notecard),
                name,
                name + " Created via SecondBot notecard API",
                AssetType.Notecard,
                UUID.Random(),
                InventoryType.Notecard,
                PermissionMask.All,
                (bool Success, InventoryItem item) =>
                {
                    if (Success)
                    {
                        AssetNotecard empty = new AssetNotecard { BodyText = "\n" };
                        empty.Encode();
                        client.Inventory.RequestUploadNotecardAsset(empty.AssetData, item.UUID,
                        (bool emptySuccess, string emptyStatus, UUID emptyItemID, UUID emptyAssetID) =>
                        {
                            if (emptySuccess)
                            {
                                empty.BodyText = content;
                                empty.Encode();
                                client.Inventory.RequestUploadNotecardAsset(empty.AssetData, emptyItemID,
                                (bool finalSuccess, string finalStatus, UUID finalItemID, UUID finalID) =>
                                {
                                    if (finalSuccess)
                                    {
                                        LogFormater.Info("Sending notecard now");
                                        client.Inventory.GiveItem(finalItemID, name, AssetType.Notecard, sendToUUID, false);
                                    }
                                    else
                                    {
                                        returnstatus = false;
                                        LogFormater.Warn("Unable to request notecard upload");
                                    }

                                });
                            }
                            else
                            {
                                LogFormater.Warn("The fuck empty success notecard create");
                                returnstatus = false;
                            }
                        });
                    }
                    else
                    {
                        LogFormater.Warn("Unable to find default notecards folder");
                        returnstatus = false;
                    }
                }
            );
            return returnstatus;
        }

        protected void ResetClient()
        {
            bool isRestart = false;
            if (client != null)
            {
                client.Network.Logout();
                isRestart = true;
            }
            client = null;
            client = new GridClient();
            master.TriggerBotClientEvent(isRestart);
            client.Network.SimConnected += BotSimConnected;
            client.Network.LoggedOut += BotLoggedOut;
            client.Network.Disconnected += BotDisconnected;
            client.Network.SimDisconnected += BotSimDisconnected;
            client.Network.LoginProgress += BotLoginStatus;
        }
        public void Login()
        {
            ResetClient();
            LoginParams loginParams = new LoginParams(
                client,
                basicCfg.GetFirstName(),
                basicCfg.GetLastName(),
                basicCfg.GetPassword(),
                "secondbot",
                master.GetVersion()
            );
            if (basicCfg.GetLoginURI() != "secondlife")
            {
                loginParams.URI = basicCfg.GetLoginURI();
            }
            AutoRestartLoginTimer.Start();
            client.Network.BeginLogin(loginParams);
        }

        public override string Status()
        {
            if (client == null)
            {
                return "No Client [Restart needed]";
            }
            else if (client.Network == null)
            {
                return "No network - [Starting / Shutting down]";
            }
            else if (client.Network.Connected == false)
            {
                return "Not connected";
            }
            else if (client.Network.CurrentSim == null)
            {
                return "No sim";
            }
            Vector3 pos = client.Self.SimPosition;
            string loc = "" + Math.Round(pos.X).ToString() + "," + Math.Round(pos.Y).ToString() + "," + Math.Round(pos.Z).ToString();
            return client.Network.CurrentSim.Name + " " + loc;
        }
    }
}
