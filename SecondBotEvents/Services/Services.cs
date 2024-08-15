using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;
using SecondBotEvents.Config;

namespace SecondBotEvents.Services
{
    public abstract class BotServices
    {
        public EventsSecondBot master;
        protected Config.Config myConfig;
        protected bool running = false;
        public BotServices(EventsSecondBot setMaster)
        {
            master = setMaster;
        }

        public void enableAndStart()
        {
            Restart(true, true);
        }

        public void changeConfig(string configKey, string configValue)
        {
            myConfig.updateKey(configKey, configValue);
        }

        public bool isRunning() { return running; }

        public GridClient GetClient()
        {
            if(master == null)
            {
                return null;
            } 
            else if(master.BotClient == null)
            {
                return null;
            }
            return master.BotClient.client;
        }
        public virtual string Status()
        {
            return "";
        }

        public virtual void Start(bool updateEnabled = false, bool setEnabledTo = false)
        {
            running = true;
        }
        public virtual void Stop()
        {
            running = false;
        }
        public void Restart(bool updateEnabled=false, bool setEnabledTo=false)
        {
            if (running == true)
            {
                Stop();
            }
            Start(updateEnabled, setEnabledTo);
        }
    }
}
