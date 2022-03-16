using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Services
{
    internal class http
    {
        protected EventsSecondBot master;
        public http(EventsSecondBot setMaster)
        {
            master = setMaster;
            Console.WriteLine("Starting HTTP service");
        }
    }
}
