using SecondBotEvents.Config;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents.Services
{
    internal class http: service
    {
        http_config myConfig;
        public http(EventsSecondBot setMaster) : base(setMaster)
        {
            myConfig = new http_config(master.fromEnv,master.fromFolder);
        }
        public override void start()
        {
            if (myConfig.getEnabled() == true)
            {
                Console.WriteLine("HTTP service [Enabled]");
                return;
            }
            Console.WriteLine("HTTP service [Disabled]");
        }
        public override void stop()
        {
            if (myConfig.getEnabled() == false)
            {
                return;
            }
            Console.WriteLine("HTTP service [Stopping]");
        }
    }
}
