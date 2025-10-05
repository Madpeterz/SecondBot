using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class DiscordConfig(bool fromENV, string fromFolder = "") : Config(fromENV, fromFolder)
    {
        protected override void MakeSettings()
        {
            filename = "discord";
            settings.Add("Enabled");
            settings.Add("ServerID");
            settings.Add("ClientToken");
            settings.Add("AllowDiscordCommands");
            settings.Add("InteractionEnabled");
            settings.Add("InteractionCommandName");
            settings.Add("InteractionHttpTarget");
            settings.Add("InteractionChannelNumber");
            settings.Add("hideChatterName");
            settings.Add("ClearChatOnConnect");
            settings.Add("HideStatusOutput");
        }

        public bool GetClearChatOnConnect()
        {
            return ReadSettingAsBool("ClearChatOnConnect", true);
        }

        public bool GetEnabled()
        {
            return ReadSettingAsBool("Enabled");
        }

        public bool GethideChatterName()
        {
            return ReadSettingAsBool("hideChatterName", false);
        }

        public ulong GetServerID()
        {
            return ReadSettingAsUlong("ServerID", 0);
        }

        public string GetClientToken()
        {
            return ReadSettingAsString("ClientToken","tokenPlzKThanks");
        }

        public bool GetAllowDiscordCommands()
        {
            return ReadSettingAsBool("AllowDiscordCommands");
        }

        public bool GetInteractionEnabled()
        {
            return ReadSettingAsBool("InteractionEnabled");
        }

        public string GetInteractionHttpTarget()
        {
            return ReadSettingAsString("InteractionHttpTarget", "https://localhost/interaction.php");
        }

        public string GetInteractionCommandName()
        {
            return ReadSettingAsString("InteractionCommandName","Go");
        }

        public string GetInteractionChannelNumber()
        {
            return ReadSettingAsString("InteractionChannelNumber", "-1");
        }

        public void SetInteractionEnabled(bool enabled)
        {
            mysettings["InteractionEnabled"] = enabled.ToString();
        }
    }
}