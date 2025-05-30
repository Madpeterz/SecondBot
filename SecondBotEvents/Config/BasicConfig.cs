﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class BasicConfig(bool fromENV, string fromFolder = "") : Config(fromENV, fromFolder)
    {
        protected override void MakeSettings()
        {
            filename = "basic";
            settings.Add("Username");
            settings.Add("Password");
            settings.Add("LoginURI");
            settings.Add("LogCommands");
            settings.Add("DefaultHoverHeight");
        }

        public double GetDefaultHoverHeight()
        {
            return ReadSettingAsDouble("DefaultHoverHeight", 0.1);
        }
        public bool GetLogCommands()
        {
            return ReadSettingAsBool("LogCommands", true);
        }
        public string GetUsername()
        {
            return ReadSettingAsString("Username","Firstname Lastname");
        }

        public string GetPassword()
        {
            return ReadSettingAsString("Password", "passwordHere");
        }

        public string GetFirstName()
        {

            string[] bits = GetUsername().Split(" ");
            return bits[0];
        }

        public string GetLastName()
        {
            string[] bits = GetUsername().Split(" ");
            if(bits.Length == 2)
            {
                return bits[1];
            }
            return "Resident";
        }

        public string GetLoginURI()
        {
            return ReadSettingAsString("LoginURI", "secondlife");
        }
    }
}
