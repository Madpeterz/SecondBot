using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Discord;
using Discord.Webhook;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;
using Discord.WebSocket;
using System.Threading.Tasks;
using System.Threading;

namespace BSB.bottypes
{
    public abstract class DialogsBot : ChatRelay
    {
        protected bool TrackDialogs;
        protected int RelayDialogsToChannel;
        protected UUID RelayDialogsToAvatar = UUID.Zero;
        protected string RelayDialogsToHttp = "";

        public override void SetRelayDialogsChannel(int channel)
        {
            RelayDialogsToChannel = channel;
        }
        public override void SetRelayDialogsAvatar(UUID avatar)
        {
            RelayDialogsToAvatar = avatar;
        }
        public override void SetRelayDialogsHTTP(string httpurl)
        {
            RelayDialogsToHttp = httpurl;
        }
        public override void SetTrackDialogs(bool status)
        {
            TrackDialogs = status;
        }

        public override bool DialogReply(int DialogID,string button)
        {
            if(pending_dialogs.ContainsKey(DialogID) == true)
            {
                int ButtonIndex = pending_dialogs[DialogID].ButtonLabels.IndexOf(button);
                if(ButtonIndex != -1)
                {
                    Client.Self.ReplyToScriptDialog(pending_dialogs[DialogID].Channel, ButtonIndex, button, pending_dialogs[DialogID].ObjectID);
                    pending_dialogs.Remove(DialogID);
                    pending_dialogs_age.Remove(DialogID);
                    return true;
                }
            }
            return false;
        }

        protected Dictionary<int, ScriptDialogEventArgs> pending_dialogs = new Dictionary<int, ScriptDialogEventArgs>();
        protected Dictionary<int, long> pending_dialogs_age = new Dictionary<int, long>();

        public override string GetStatus()
        {
            List<int> purgeDialogs = new List<int>();
            long now = helpers.UnixTimeNow();
            foreach(KeyValuePair<int,long> Dlog in pending_dialogs_age)
            {
                long dif = now - Dlog.Value;
                if(dif >= (60*2))
                {
                    purgeDialogs.Add(Dlog.Key);
                }
            }
            foreach(int P in purgeDialogs)
            {
                pending_dialogs.Remove(P);
                pending_dialogs_age.Remove(P);
            }
            return base.GetStatus();
        }

        public override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            if (reconnect == false)
            {
                Client.Self.ScriptDialog += DialogsHandler;
            }
        }

        protected void DialogsHandler(object sender,ScriptDialogEventArgs e)
        {
            if (TrackDialogs == true)
            {
                int dialogID = AddPendingDialog(e);
                string Dmess = e.Message;
                if(Dmess.Length > 30)
                {
                    Dmess = Dmess.Substring(0, 30);
                }
                string DialogMessage = "NewDialog###" + dialogID.ToString() + "###"+ Dmess + "###" + String.Join("~|~",e.ButtonLabels) + "";
                if (RelayDialogsToChannel != 0)
                {
                    CommandsInterface.SmartCommandReply(true,RelayDialogsToChannel.ToString(), DialogMessage,"NewDialogNotice");
                }
                if(RelayDialogsToAvatar != UUID.Zero)
                {
                    CommandsInterface.SmartCommandReply(true, RelayDialogsToAvatar.ToString(), DialogMessage, "NewDialogNotice");
                }
                if(RelayDialogsToHttp != null)
                {
                    if (RelayDialogsToHttp.StartsWith("http") == true)
                    {
                        CommandsInterface.SmartCommandReply(true, RelayDialogsToAvatar.ToString(), DialogMessage, "NewDialogNotice");
                    }
                }
            }
        }

        protected int AddPendingDialog(ScriptDialogEventArgs e)
        {
            int selected = new Random().Next(566678);
            while (pending_dialogs.ContainsKey(selected))
            {
                selected = new Random().Next(566678);
            }
            pending_dialogs.Add(selected, e);
            pending_dialogs_age.Add(selected, helpers.UnixTimeNow());
            return selected;
        }

    }
}
