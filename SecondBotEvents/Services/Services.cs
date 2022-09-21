﻿using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Services
{
    public abstract class BotServices
    {
        public EventsSecondBot master;
        public BotServices(EventsSecondBot setMaster)
        {
            master = setMaster;
        }

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
