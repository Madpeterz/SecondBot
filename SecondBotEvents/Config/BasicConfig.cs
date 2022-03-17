using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class BasicConfig : Config
    {
        public BasicConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void MakeSettings()
        {
            filename = "basic";
            settings.Add("username");
            settings.Add("password");
            settings.Add("loginURI");
        }

        public string GetUsername()
        {
            return ReadSettingAsString("username","Firstname Lastname");
        }

        public string GetPassword()
        {
            return ReadSettingAsString("password", "passwordHere");
        }

        public string GetFirstName()
        {

            string[] bits = ReadSettingAsString("username", "Firstname Lastname").Split(" ");
            return bits[0];
        }

        public string GetLastName()
        {
            string[] bits = ReadSettingAsString("username", "Firstname Lastname").Split(" ");
            if(bits.Length == 2)
            {
                return bits[1];
            }
            return "Resident";
        }

        public string GetLoginURI()
        {
            return ReadSettingAsString("loginURI", "secondlife");
        }
    }
}
