using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Services
{
    internal abstract class service
    {
        protected EventsSecondBot master;
        public service(EventsSecondBot setMaster)
        {
            master = setMaster;
        }

        public virtual void start()
        {

        }
        public virtual void stop()
        {

        }
        public void restart()
        {
            stop();
            start();
        }
    }
}
