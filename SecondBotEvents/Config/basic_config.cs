using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class basic_config : config
    {
        public basic_config(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }
        protected override void makeSettings()
        {
            filename = "basic";
            settings.Add("username");
            settings.Add("password");
            settings.Add("loginuri");
        }

        public string getUsername()
        {
            return readSetting("username","Firstname Lastname");
        }

        public string getPassword()
        {
            return readSetting("password", "passwordHere");
        }

        public string getFirstName()
        {

            string[] bits = readSetting("username", "Firstname Lastname").Split(" ");
            return bits[0];
        }

        public string getLastName()
        {
            string[] bits = readSetting("username", "Firstname Lastname").Split(" ");
            if(bits.Length == 2)
            {
                return bits[1];
            }
            return "Resident";
        }

        public string getLoginURI()
        {
            return readSetting("loginuri", "secondlife");
        }
    }
}
