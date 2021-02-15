using System;
using OpenMetaverse;
using BetterSecondBotShared.Static;
using System.Collections.Generic;

namespace BetterSecondBot.bottypes
{
    public class NextHomeRegionArgs: EventArgs
    {
    }

    public abstract class AttachableBot : MessageSwitcherBot
    {
        protected string BetterAtHomeAction = "";

        protected simBlacklistDataset lastUpdatedSimBlacklist = new simBlacklistDataset();
        public simBlacklistDataset _lastUpdatedSimBlacklist { get { return lastUpdatedSimBlacklist; } }

        public void UpdateSimBlacklist(simBlacklistDataset current)
        {
            lastUpdatedSimBlacklist = current;
        }

        public void ResetAtHome()
        {

        }

        public void SetBetterAtHomeAction(string BetterAtHomeAction)
        {
            this.BetterAtHomeAction = BetterAtHomeAction;
        }

        public override string GetStatus()
        {
            return base.GetStatus() + " <@BetterAtHome: " + BetterAtHomeAction + ">";
        }

        public string TeleportWithSLurl(string sl_url)
        {
            string[] bits = helpers.ParseSLurl(sl_url);
            if (helpers.notempty(bits) == true)
            {
                if (bits.Length == 4)
                {
                    float.TryParse(bits[1], out float posX);
                    float.TryParse(bits[2], out float posY);
                    float.TryParse(bits[3], out float posZ);
                    string regionName = bits[0];
                    Client.Self.Teleport(regionName, new Vector3(posX, posY, posZ));
                    return "ok";
                }
                return "Invaild bits length for SLurl";
            }
            return "No bits decoded";
        }
    }


    public class simBlacklistDataset
    {
        public long lastupdated = 0;
        public Dictionary<UUID, string> banned = new Dictionary<UUID, string>();
        public string regionname = "";
    }
}
