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
            controler.getBot().ChangeSimEvent += ChangedSim;
            controler.getBot().LoginProgess += LoginProcess;
            controler.getBot().StatusMessageEvent += StatusPing;
        }

        protected void StatusPing(object o, StatusMessageEvent e)
        {
            if(simname != "")
            {
                if (controler.getBot().GetClient.Network.CurrentSim != null)
                {
                    if (controler.getBot().GetClient.Network.CurrentSim.IsEstateManager == true)
                    {
                        long dif = helpers.UnixTimeNow() - controler.getBot()._lastUpdatedSimBlacklist.lastupdated;
                        if (dif > 120)
                        {
                            controler.getBot().GetClient.Estate.RequestInfo();
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
                controler.getBot().GetClient.Estate.EstateBansReply += EstateBansReply;
            }
        }

        protected string simname = "";
        protected void ChangedSim(object o, SimChangedEventArgs e)
        {
            simname = "";
            if (controler.getBot().GetClient.Network.CurrentSim != null)
            {
                simname = controler.getBot().GetClient.Network.CurrentSim.Name;
            }
            if(simname != "")
            {
                if(controler.getBot().GetClient.Network.CurrentSim.IsEstateManager == true)
                {
                    controler.getBot().GetClient.Estate.RequestInfo();
                }
            }
        }

        protected void EstateBansReply(object o, EstateBansReplyEventArgs e)
        {
            simBlacklistDataset working = new simBlacklistDataset();
            working.lastupdated = helpers.UnixTimeNow();
            foreach(UUID av in e.Banned)
            {
                working.banned.Add(av,"lookup"); // controler.getBot().FindAvatarKey2Name(av)
            }
            working.regionname = controler.getBot().GetClient.Network.CurrentSim.Name;
            controler.getBot().UpdateSimBlacklist(working);
        }

    }
}
