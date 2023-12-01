using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class DialogRelayConfig : Config
    {
        public DialogRelayConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "dialogrelay";
            settings.Add("RelayChannel");
            settings.Add("RelayAvatar");
            settings.Add("RelayObjectOwnerOnly");
            settings.Add("RelayHttpurl");
            settings.Add("HideStatusOutput");
        }

        public string GetRelayHttpurl()
        {
            return ReadSettingAsString("RelayHttpurl", "-");
        }

        public int GetRelayChannel()
        {
            return ReadSettingAsInt("RelayChannel", -1);
        }

        public string GetRelayAvatar()
        {
            return ReadSettingAsString("RelayAvatar", UUID.Zero.ToString());
        }

        public string RelayObjectOwnerOnly()
        {
            return ReadSettingAsString("RelayObjectOwnerOnly", UUID.Zero.ToString());
        }
    }
}