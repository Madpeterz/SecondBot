using System;
using System.Collections.Generic;
using System.Linq;
using OpenMetaverse;
using BetterSecondBotShared.Static;
using BetterSecondBotShared.logs;

namespace BetterSecondBot.bottypes
{
    public class NextHomeRegionArgs: EventArgs
    {
    }

    public abstract class AtHome : MessageSwitcherBot
    {
        protected string BetterAtHomeAction = "";
        private EventHandler<SimChangedEventArgs> _ChangeSim;
        private EventHandler<AlertMessageEventArgs> _AlertMessage;
        private EventHandler<LoginProgressEventArgs> _LoginProgess;
        private EventHandler<NextHomeRegionArgs> _NextHomeRegion;

        private readonly object _ChangeSimLock = new object();
        private readonly object _AlertMessageLock = new object();
        private readonly object _LoginProgressLock = new object();
        private readonly object _NextHomeRegionLock = new object();


        public event EventHandler<SimChangedEventArgs> ChangeSimEvent
        {
            add { lock (_ChangeSimLock) { _ChangeSim += value; } }
            remove { lock (_ChangeSimLock) { _ChangeSim -= value; } }
        }
        public event EventHandler<AlertMessageEventArgs> AlertMessage
        {
            add { lock (_AlertMessageLock) { _AlertMessage += value; } }
            remove { lock (_AlertMessageLock) { _AlertMessage -= value; } }
        }
        public event EventHandler<LoginProgressEventArgs> LoginProgess
        {
            add { lock (_LoginProgressLock) { _LoginProgess += value; } }
            remove { lock (_LoginProgressLock) { _LoginProgess -= value; } }
        }
        public event EventHandler<NextHomeRegionArgs> NextHomeRegion
        {
            add { lock (_NextHomeRegionLock) { _NextHomeRegion += value; } }
            remove { lock (_NextHomeRegionLock) { _NextHomeRegion -= value; } }
        }

        protected override void LoginHandler(object o, LoginProgressEventArgs e)
        {
            base.LoginHandler(o, e);
            EventHandler<LoginProgressEventArgs> handler = _LoginProgess;
            handler?.Invoke(this, e);
        }

        protected void ChangeSim(object sender,SimChangedEventArgs e)
        {
            EventHandler<SimChangedEventArgs> handler = _ChangeSim;
            handler?.Invoke(this, e);
        }

        protected void AlertEvent(object sender,AlertMessageEventArgs e)
        {
            EventHandler<AlertMessageEventArgs> handler = _AlertMessage;
            handler?.Invoke(this, e);
        }

        public void GotoNextHomeRegion()
        {
            NextHomeRegionArgs e = new NextHomeRegionArgs();
            EventHandler<NextHomeRegionArgs> handler = _NextHomeRegion;
            handler?.Invoke(this, e);
        }

        public void ResetAtHome()
        {

        }

        public void SetBetterAtHomeAction(string BetterAtHomeAction)
        {
            this.BetterAtHomeAction = BetterAtHomeAction;
        }

        public override string GetStatus()
        {
            return base.GetStatus() + " <@BetterAtHome: " + BetterAtHomeAction + ">";
        }

        public string TeleportWithSLurl(string sl_url)
        {
            string[] bits = helpers.ParseSLurl(sl_url);
            if (helpers.notempty(bits) == true)
            {
                if (bits.Length == 4)
                {
                    float.TryParse(bits[1], out float posX);
                    float.TryParse(bits[2], out float posY);
                    float.TryParse(bits[3], out float posZ);
                    string regionName = bits[0];
                    Client.Self.Teleport(regionName, new Vector3(posX, posY, posZ));
                    return "ok";
                }
                return "Invaild bits length for SLurl";
            }
            return "No bits decoded";
        }
    }
}
