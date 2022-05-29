using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Services
{
    public abstract class Services
    {
        public EventsSecondBot master;
        public Services(EventsSecondBot setMaster)
        {
            master = setMaster;
        }

        protected GridClient getClient()
        {
            return master.botClient.client;
        }
        public virtual string Status()
        {
            return "";
        }

        public virtual void Start()
        {

        }
        public virtual void Stop()
        {

        }
        public void Restart()
        {
            Stop();
            Start();
        }
    }
}
