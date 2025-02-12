using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class SmtpConfig(bool fromENV, string fromFolder = "") : Config(fromENV, fromFolder)
    {
        protected override void MakeSettings()
        {
            filename = "smtp";
            settings.Add("Enabled");
            settings.Add("AllowEmailAsReplyTarget");
            settings.Add("AllowCommandSendMail");
            settings.Add("UseAllowedRecivers");
            settings.Add("AllowedReciversCSV");
            settings.Add("AllowSendAlertStatus");
            settings.Add("AllowSendLoginNotice");
            settings.Add("MailReplyAddress");
            settings.Add("Port");
            settings.Add("Host");
            settings.Add("useSSL");
            settings.Add("User");
            settings.Add("Token");
            settings.Add("SendAlertsAndLoginsTo");
            settings.Add("HideStatusOutput");
            
        }
        public bool GetUseAllowedRecivers()
        {
            return ReadSettingAsBool("UseAllowedRecivers", false);
        }
        public string[] GetAllowedRecivers()
        {
            return ReadSettingAsString("AllowedReciversCSV", "none").Split(',');
        }
        public bool GetUseSSL()
        {
            return ReadSettingAsBool("useSSL", false);
        }

        public string GetMailReplyAddress()
        {
            return ReadSettingAsString("MailReplyAddress", "me@myemail.tld");
        }

        public string GetSendAlertsAndLoginsTo()
        {
            return ReadSettingAsString("SendAlertsAndLoginsTo", "me@somewhere.tld");
        }
        public int GetPort()
        {
            return ReadSettingAsInt("Port", 587);
        }

        public string GetHost()
        {
            return ReadSettingAsString("Host", "smtp.mail.example");
        }

        public string GetUser()
        {
            return ReadSettingAsString("User", "me@email.addr.tld");
        }

        public string GetToken()
        {
            return ReadSettingAsString("Token", "none");
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled", false);
        }

        public bool GetAllowEmailAsReplyTarget()
        {
            return ReadSettingAsBool("AllowEmailAsReplyTarget", false);
        }

        public bool GetAllowCommandSendMail()
        {
            return ReadSettingAsBool("AllowCommandSendMail", false);
        }

        public bool GetAllowSendAlertStatus()
        {
            return ReadSettingAsBool("AllowSendAlertStatus", false);
        }

        public bool GetAllowSendLoginNotice()
        {
            return ReadSettingAsBool("AllowSendLoginNotice", false);
        }
    }
}