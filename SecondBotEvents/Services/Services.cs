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
