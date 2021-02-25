using BetterSecondBot;
using BetterSecondBotShared.logs;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace BetterSecondbot.Tracking
{
    public class BetterTracking
    {
        protected Cli controler = null;

        protected bool output_to_url = false;

        protected bool output_to_channel = false;
        protected int output_channel = 0;


        protected Dictionary<UUID, long> AvatarSeen = new Dictionary<UUID, long>();

        public BetterTracking(Cli setcontroler)
        {
            controler = setcontroler;
            if(controler.Bot.getMyConfig.Setting_Tracker.ToLowerInvariant() == "false")
            {
                LogFormater.Info("Tracker - Disabled because set to false");
                return;
            }
            if(int.TryParse(controler.Bot.getMyConfig.Setting_Tracker,out output_channel) == true)
            {
                LogFormater.Info("Tracker - Enabled Chat on channel: "+ output_channel.ToString());
                output_to_channel = true;
                attachEvents();
            }
            else if (controler.Bot.getMyConfig.Setting_Tracker.StartsWith("http") == true)
            {
                LogFormater.Info("Tracker - Enabled HTTP: "+ controler.Bot.getMyConfig.Setting_Tracker);
                output_to_url = true;
                attachEvents();
            }
            else
            {
                LogFormater.Info("Tracker - Disabled not a vaild channel or http");
            }
        }
        protected void attachEvents()
        {
            controler.Bot.GetClient.Grid.CoarseLocationUpdate += LocationUpdate;
        }

        protected void output(UUID av, string mode)
        {
            string output = av.ToString() + "|||" + mode;
            if (output_to_channel == true)
            {
                controler.Bot.GetClient.Self.Chat(output, output_channel, ChatType.Normal);
            }
            if(output_to_url == true)
            {
                Thread newThread = new Thread(this.SendHTTPData);
                newThread.Start(output);
            }
        }

        protected HttpClient HTTPclient = new HttpClient();

        protected void SendHTTPData(object data)
        {
            Dictionary<string, string> values = new Dictionary<string, string>
                    {
                        { "reply", data.ToString() },
                    };

            var content = new FormUrlEncodedContent(values);
            try
            {
                HTTPclient.PostAsync(controler.Bot.getMyConfig.Setting_Tracker, content);
            }
            catch (Exception e)
            {
                LogFormater.Crit("[Tracker] HTTP failed: " + e.Message + "");
            }
        }

        protected void LocationUpdate(object o, CoarseLocationUpdateEventArgs e)
        {
            if (e.NewEntries.Count() > 0)
            {
                foreach (UUID av in e.NewEntries)
                {
                    output(av, "entry");
                }
            }
            if (e.RemovedEntries.Count() > 0)
            {
                foreach (UUID av in e.RemovedEntries)
                {
                    output(av, "exit");
                }
            }

        }
    }
}
