using BetterSecondBot;
using BetterSecondBotShared.Static;
using OpenMetaverse;
using BetterSecondBot.bottypes;

namespace BetterSecondbot.DataStorage
{
    public class Datastorage
    {
        protected Cli controler = null;
        protected bool attachedClientEvents = false;
        public Datastorage(Cli setcontroler)
        {
            controler = setcontroler;
            attach_events();
        }

        protected void attach_events()
        {
            controler.Bot.ChangeSimEvent += ChangedSim;
            controler.Bot.LoginProgess += LoginProcess;
            controler.Bot.StatusMessageEvent += StatusPing;
        }

        protected void StatusPing(object o, StatusMessageEvent e)
        {
            if(simname != "")
            {
                if (controler.Bot.GetClient.Network.CurrentSim != null)
                {
                    if (controler.Bot.GetClient.Network.CurrentSim.IsEstateManager == true)
                    {
                        long dif = helpers.UnixTimeNow() - controler.Bot._lastUpdatedSimBlacklist.lastupdated;
                        if (dif > 120)
                        {
                            controler.Bot.GetClient.Estate.RequestInfo();
                        }
                    }
                }
            }
        }

        protected void LoginProcess(object o, LoginProgressEventArgs e)
        {
            simname = "";
            if (e.Status == LoginStatus.Success)
            {
                ChangedSim(null, null);
            }
            if(attachedClientEvents == false)
            {
                attachedClientEvents = true;
                controler.Bot.GetClient.Estate.EstateBansReply += EstateBansReply;
            }
        }

        protected string simname = "";
        protected void ChangedSim(object o, SimChangedEventArgs e)
        {
            simname = "";
            if (controler.Bot.GetClient.Network.CurrentSim != null)
            {
                simname = controler.Bot.GetClient.Network.CurrentSim.Name;
            }
            if(simname != "")
            {
                if(controler.Bot.GetClient.Network.CurrentSim.IsEstateManager == true)
                {
                    controler.Bot.GetClient.Estate.RequestInfo();
                }
            }
        }

        protected void EstateBansReply(object o, EstateBansReplyEventArgs e)
        {
            simBlacklistDataset working = new simBlacklistDataset();
            working.lastupdated = helpers.UnixTimeNow();
            foreach(UUID av in e.Banned)
            {
                working.banned.Add(av,"lookup"); // controler.Bot.FindAvatarKey2Name(av)
            }
            working.regionname = controler.Bot.GetClient.Network.CurrentSim.Name;
            controler.Bot.UpdateSimBlacklist(working);
        }

    }
}
