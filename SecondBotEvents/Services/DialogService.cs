using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using System.Linq;
using Swan;
using SecondBotEvents.Commands;
using RestSharp;
using Swan.Parsers;
using Newtonsoft.Json.Linq;

namespace SecondBotEvents.Services
{
    public class DialogService : BotServices
    {
        protected bool botConnected = false;
        protected new DialogRelayConfig myConfig;
        public DialogService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new DialogRelayConfig(master.fromEnv, master.fromFolder);
            if(myConfig.GetRelayChannel() >= 0)
            {
                DialogRelayChannels.Add(myConfig.GetRelayChannel());
            }
            if(UUID.TryParse(myConfig.GetRelayAvatar(), out UUID newAV) == true)
            {
                if(newAV != UUID.Zero)
                {
                    DialogRelayAvatars.Add(newAV);
                }
            }
            if (UUID.TryParse(myConfig.RelayObjectOwnerOnly(), out UUID newObjectOwnerAv) == true)
            {
                if (newObjectOwnerAv != UUID.Zero)
                {
                    objectOwnerOnly = newObjectOwnerAv;
                }
            }
            if(myConfig.GetRelayHttpurl().StartsWith("http") == true)
            {
                DialogRelayHTTP.Add(myConfig.GetRelayHttpurl());
            }
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
            if (botConnected == false)
            {
                return "Waiting for client";
            }
            if(DialogWindows.Count > 0)
            {
                return DialogWindows.Count().ToString()+" Dialogs waiting";
            }
            CleanUp();
            return "Active";
        }

        public string AvatarRelayTarget(UUID avatar)
        {
            lock (DialogRelayAvatars)
            {
                if (DialogRelayAvatars.Contains(avatar) == false)
                {
                    DialogRelayAvatars.Add(avatar);
                    return "added";
                }
                DialogRelayAvatars.Remove(avatar);
                return "removed";
            }
        }

        public string ChannelRelayTarget(int channel)
        {
            lock (DialogRelayChannels)
            {
                if (DialogRelayChannels.Contains(channel) == false)
                {
                    DialogRelayChannels.Add(channel);
                    return "added";
                }
                DialogRelayChannels.Remove(channel);
                return "removed";
            }
        }

        public string HttpRelayTarget(string url)
        {
            lock (DialogRelayHTTP)
            {
                if (DialogRelayHTTP.Contains(url) == false)
                {
                    DialogRelayHTTP.Add(url);
                    return "added";
                }
                DialogRelayHTTP.Remove(url);
                return "removed";
            }
        }

        public string DialogAction(int dialogID, string button)
        {
            lock(DialogWindows)
            {
                if(DialogWindows.ContainsKey(dialogID) == false)
                {
                    return "Invaild dialog window";
                }
                else if (DialogWindows[dialogID].ButtonLabels.Contains(button) == false)
                {
                    return "Invaild dialog button";
                }
                ScriptDialogEventArgs e = DialogWindows[dialogID];
                GetClient().Self.ReplyToScriptDialog(e.Channel, e.ButtonLabels.IndexOf(button), button, e.ObjectID);
                DialogWindowsExpire.Remove(dialogID);
                DialogWindows.Remove(dialogID);
                return "action";
            }
        }

        protected long lastclean = 0;
        protected void CleanUp()
        {
            long dif = SecondbotHelpers.UnixTimeNow() - lastclean;
            if(dif > 30)
            {
                lastclean = SecondbotHelpers.UnixTimeNow();
                lock(DialogWindows)
                {
                    long unixtime = SecondbotHelpers.UnixTimeNow();
                    List<int> removeids = new List<int>();
                    foreach(KeyValuePair<int,long> a in DialogWindowsExpire)
                    {
                        if(a.Value > unixtime)
                        {
                            removeids.Add(a.Key);
                        }
                    }
                    foreach(int id in removeids)
                    {
                        DialogWindowsExpire.Remove(id);
                        DialogWindows.Remove(id);
                    }
                }
            }
        }



        protected Dictionary<int, ScriptDialogEventArgs> DialogWindows = new Dictionary<int, ScriptDialogEventArgs>();
        protected Dictionary<int, long> DialogWindowsExpire = new Dictionary<int, long>();
        protected int nextDialogWindow = 1;
        protected List<int> DialogRelayChannels = new List<int>();
        protected List<UUID> DialogRelayAvatars = new List<UUID>();
        protected List<string> DialogRelayHTTP = new List<string>();
        protected UUID objectOwnerOnly = UUID.Zero;

        protected void DialogWindowEvent(object sender, ScriptDialogEventArgs e)
        {
            lock (DialogWindows)
            {
                if((objectOwnerOnly != UUID.Zero) && (objectOwnerOnly != e.OwnerID))
                {
                    return;
                }
                int windowid = nextDialogWindow;
                nextDialogWindow++;
                if(nextDialogWindow > 999)
                {
                    nextDialogWindow = 1;
                }
                DialogWindows.Add(windowid, e);
                long expiresAt = SecondbotHelpers.UnixTimeNow() + 440;
                DialogWindowsExpire.Add(windowid, expiresAt);
                DialogWindow window = new DialogWindow();
                window.buttons = e.ButtonLabels.ToArray<string>();
                window.dialogid = windowid;
                window.message = e.Message;
                window.objectname = e.ObjectName;
                window.expires = expiresAt;
                string eventMessage = JsonConvert.SerializeObject(window);
                foreach(int a in DialogRelayChannels)
                {
                    if(a >= 0)
                    {
                        GetClient().Self.Chat(eventMessage, a, ChatType.Normal);
                    }
                }
                foreach(UUID av in DialogRelayAvatars)
                {
                    GetClient().Self.InstantMessage(av, eventMessage);
                }
                foreach(string url in DialogRelayHTTP)
                {
                    long unixtime = SecondbotHelpers.UnixTimeNow();
                    string token = SecondbotHelpers.GetSHA1(unixtime.ToString() + "DialogRelay"+GetClient().Self.AgentID+ eventMessage + master.CommandsService.myConfig.GetSharedSecret());
                    var client = new RestClient(url);
                    var request = new RestRequest("Dialog/Relay", Method.Post);
                    request.AddParameter("token", token);
                    request.AddParameter("unixtime", unixtime.ToString());
                    request.AddParameter("method", "Dialog");
                    request.AddParameter("action", "Relay");
                    request.AddParameter("botname", GetClient().Self.Name);
                    request.AddParameter("event", eventMessage);
                    request.AddHeader("content-type", "application/x-www-form-urlencoded");
                    client.ExecutePostAsync(request);
                }

            }
        }


        protected void BotClientRestart(object o, BotClientNotice e)
        {
            LogFormater.Info("Dialog service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            GetClient().Network.SimConnected += BotLoggedIn;
            LogFormater.Info("Dialog service [waiting for new client]");
            if (GetClient() != null)
            {
                GetClient().Self.ScriptDialog -= DialogWindowEvent;
            }
        }

        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            GetClient().Network.SimConnected -= BotLoggedIn;
            botConnected = true;
            // attach events
            GetClient().Self.ScriptDialog += DialogWindowEvent;
        }

        public override void Start()
        {
            Stop();
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
        }

        public override void Stop()
        {
            running = false;
            master.BotClientNoticeEvent -= BotClientRestart;
        }
    }

    public class DialogWindow
    {
        public string message;
        public string[] buttons;
        public string objectname;
        public int dialogid;
        public long expires;
    }
}
