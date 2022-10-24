using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Config
{
    public class CustomCommandsConfig : Config
    {
        public CustomCommandsConfig(bool fromENV, string fromFolder = "") : base(fromENV, fromFolder) { }

        protected override void MakeSettings()
        {
            filename = "customcommands";
            settings.Add("count");
            int loop = 1;
            while(loop <= GetCount())
            {
                settings.Add(loop.ToString() + "_trigger");
                settings.Add(loop.ToString() + "_args");
                settings.Add(loop.ToString() + "_steps");
                int loop2 = 1;
                while(loop2 <= GetCommandSteps(loop))
                {
                    settings.Add(loop.ToString() + "_commands_"+loop2.ToString());
                    loop2++;
                }
                loop++;
            }
        }

        public string GetCommandStep(int commandIndex, int step)
        {
            return ReadSettingAsString(commandIndex.ToString() + "_commands_"+ step.ToString(), "Logoff");
        }

        public string GetCommandTrigger(int commandIndex)
        {
            return ReadSettingAsString(commandIndex.ToString() + "_trigger", "exitplease");
        }

        public int GetCommandSteps(int commandIndex)
        {
            return ReadSettingAsInt(commandIndex.ToString() + "_steps", 0);
        }

        public int GetCommandArgs(int commandIndex)
        {
            return ReadSettingAsInt(commandIndex.ToString() + "_args", 0);
        }

        public int GetCount()
        {
            return ReadSettingAsInt("count", 1);
        }
    }

}