using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MimeKit;
using OpenMetaverse;
using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecondBotEvents.Services
{
    public class SmtpService : BotServices
    {
        protected new SmtpConfig myConfig;
        protected int sent = 0;
        protected int failed = 0;
        protected bool botConnected = false;
        public SmtpService(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new SmtpConfig(master.fromEnv, master.fromFolder);
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
        }
        public override string Status()
        {
            if (myConfig == null)
            {
                return "No Config";
            }
            else if(myConfig.GetEnabled() == false)
            {
                return "Disabled";
            }    
            else if (myConfig.GetHideStatusOutput() == true)
            {
                return "hidden";
            }
            else if(botConnected == false)
            {
                return "Standby";
            }
            return "send: "+sent.ToString()+" ~ Failed: "+failed.ToString();
        }

        protected void BotAlertMessage(object o, AlertMessageEventArgs e)
        {
            if (myConfig == null)
            {
                return;
            }
            else if (myConfig.GetEnabled() == false)
            {
                return;
            }
            if (myConfig.GetAllowSendAlertStatus() == false)
            {
                return;
            }
            if(myConfig.GetSendAlertsAndLoginsTo() == "me@somewhere.tld")
            {
                return;
            }
            SendMail("Bot - Alert message",e.Message, myConfig.GetSendAlertsAndLoginsTo());
        }

        public KeyValuePair<bool,string> commandEmail(string to,string subject, string message)
        {
            if(myConfig == null)
            {
                return new KeyValuePair<bool, string>(false,"No SMTP config");
            }
            else if(myConfig.GetEnabled() == false)
            {
                return new KeyValuePair<bool, string>(false, "SMTP service not enabled");
            }
            else if(myConfig.GetAllowCommandSendMail() == false)
            {
                return new KeyValuePair<bool, string>(false, "Send email via commands not enabled");
            }
            return SendMail(subject, message, to);
        }

        public KeyValuePair<bool, string> sendReplyTarget(string to,string command,string output)
        {
            if (myConfig == null)
            {
                return new KeyValuePair<bool, string>(false, "No SMTP config");
            }
            else if (myConfig.GetEnabled() == false)
            {
                return new KeyValuePair<bool, string>(false, "SMTP service not enabled");
            }
            else if (myConfig.GetAllowEmailAsReplyTarget() == false)
            {
                return new KeyValuePair<bool, string>(false, "Send email via command reply is not enabled");
            }
            return SendMail(command+"##commandreply", output, to);
        }

        protected KeyValuePair<bool, string> SendMail(string subject, string body, string to)
        {
            if(myConfig.GetMailReplyAddress() == "me@myemail.tld")
            {
                LogFormater.Crit("you need to set the reply address");
                failed++;
                return new KeyValuePair<bool, string>(false, "you need to set the reply address");
            }
            if(myConfig.GetUseAllowedRecivers() == true)
            {
                if(myConfig.GetAllowedRecivers().Contains(to) == false)
                {
                    return new KeyValuePair<bool, string>(false, to + " is not in allowed recivers");
                }
            }
            try
            {
                MimeMessage message = new();
                message.From.Add(new MailboxAddress(myConfig.GetMailReplyAddress() + " via secondbot", myConfig.GetMailReplyAddress()));
                message.To.Add(new MailboxAddress(to, to));
                message.Subject = subject;
                message.Body = new TextPart("plain") { Text = body };
                using (var client = new SmtpClient())
                {
                    client.Connect(myConfig.GetHost(), myConfig.GetPort(), myConfig.GetUseSSL());

                    // Note: only needed if the SMTP server requires authentication
                    if (myConfig.GetToken() != "none")
                    {
                        client.Authenticate(myConfig.GetUser(), myConfig.GetToken());
                    }

                    client.Send(message);
                    client.Disconnect(true);
                }
                sent++;
                return new KeyValuePair<bool, string>(true, "Sending");
            }
            catch (Exception ex)
            {
                LogFormater.Warn("Failed to send email: "+ex.Message);
                failed++;
                return new KeyValuePair<bool, string>(false, "Failed to send email: " + ex.Message);
            }
        }

        protected void BotClientRestart(object o, BotClientNotice e)
        {
            if (e.isStart == false)
            {
                return;
            }
            botConnected = false;
            LogFormater.Info("SMTP Service [Attached to new client]");
            GetClient().Network.LoggedOut += BotLoggedOut;
            GetClient().Network.SimConnected += BotLoggedIn;
            GetClient().Self.AlertMessage += BotAlertMessage;
        }

        protected void BotLoggedOut(object o, LoggedOutEventArgs e)
        {
            botConnected = false;
            LogFormater.Info("SMTP Service [Standby]");
        }
        protected void BotLoggedIn(object o, SimConnectedEventArgs e)
        {
            bool loginEvent = false;
            if(botConnected == false)
            {
                loginEvent = true;
            }
            LogFormater.Info("SMTP Service [Active]");
            botConnected = true;
            if (loginEvent == true)
            {
                if (myConfig.GetAllowSendAlertStatus() == false)
                {
                    return;
                }
                if (myConfig.GetSendAlertsAndLoginsTo() == "me@somewhere.tld")
                {
                    return;
                }
                SendMail("Bot - Login message", GetClient().Self.Name + " has signed in", myConfig.GetSendAlertsAndLoginsTo());
            }
            
        }

        public override void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            if (updateEnabled)
            {
                myConfig.setEnabled(setEnabledTo);
            }
            if (myConfig.GetEnabled() == false)
            {
                return;
            }
            Stop();
            running = true;
            master.BotClientNoticeEvent += BotClientRestart;
            LogFormater.Info("SMTP Service [Starting]");
        }

        public override void Stop()
        {
            if(running == true)
            {
                LogFormater.Info("SMTP Service [Stopping]");
            }
            running = false;
            master.BotClientNoticeEvent -= BotClientRestart;
            if (master.BotClient != null)
            {
                if (GetClient() != null)
                {
                    GetClient().Network.SimConnected -= BotLoggedIn;
                    GetClient().Self.AlertMessage -= BotAlertMessage;
                }
            }
            
        }
    }
}



