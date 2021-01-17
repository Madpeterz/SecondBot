using System;
using System.Collections.Generic;
using System.Linq;
using OpenMetaverse;
using BetterSecondBotShared.Static;
using BetterSecondBotShared.logs;

namespace BSB.bottypes
{
    public abstract class AtHome : MessageSwitcherBot
    {
        protected long last_tp_attempt_unixtime;
        protected string last_attempted_teleport_region = "";
        protected bool after_login_fired;
        protected int last_tested_home_id = -1;
        protected Dictionary<string, long> avoid_sims = new Dictionary<string, long>();
        protected bool teleport_lockout;

        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            last_tested_home_id = -1;
            last_tp_attempt_unixtime = helpers.UnixTimeNow() + 30;
            if (after_login_fired == false)
            {
                Info("Home regions count: " + myconfig.Basic_HomeRegions.Length.ToString() + "");
                Client.Self.AlertMessage += AlertEvent;
                Client.Network.SimChanged += ChangeSim;
                after_login_fired = true;
                reconnect = true;
            }
        }

       
        protected void ChangeSim(object sender,SimChangedEventArgs e)
        {
            if (IsSimHome(Client.Network.CurrentSim.Name) == false)
            {
                if (teleport_lockout == false)
                {
                    if (last_attempted_teleport_region != "")
                    {
                        if (last_attempted_teleport_region != Client.Network.CurrentSim.Name)
                        {

                            teleport_lockout = true;
                            Info("Teleport by script/master detected - @home lock out enabled");
                        }
                    }
                    else
                    {
                        last_attempted_teleport_region = Client.Network.CurrentSim.Name;
                        Info("Connected to region");
                    }
                }
            }
            else
            {
                if (UUID.TryParse(myconfig.Setting_DefaultSit_UUID, out UUID sit_UUID) == true)
                {
                    Client.Self.RequestSit(sit_UUID, Vector3.Zero);
                }
            }
        }
        protected void AlertEvent(object sender,AlertMessageEventArgs e)
        {
            if(e.Message.Contains("restart") == true)
            {
                // oh snap region is dead run away
                Info("--- Sim Restarting ---");
                AvoidSim(Client.Network.CurrentSim.Name);
                GotoNextHomeRegion();
            }
        }

        public void ResetAtHome()
        {
            Debug("@home / ResetAtHome");
            last_tested_home_id = -1;
            teleport_lockout = false;
            last_attempted_teleport_region = "";
            avoid_sims = new Dictionary<string, long>();
            last_tp_attempt_unixtime = helpers.UnixTimeNow() + 30;
        }



        protected override void AvoidSim(string simname)
        {
            if (avoid_sims.ContainsKey(simname) == false)
            {
                Debug("@home / Avoiding sim: " + simname);
                avoid_sims.Add(simname, helpers.UnixTimeNow() + 240); // @home will avoid that sim for the next 4 mins
            }
        }

        protected void ExpireOldAvoidSims()
        {
            if (avoid_sims.Count() > 0)
            {
                List<string> remove_keys = new List<string>();
                long now = helpers.UnixTimeNow();
                foreach (KeyValuePair<string,long> avoids in avoid_sims)
                {
                    if(avoids.Value < now)
                    {
                        remove_keys.Add(avoids.Key);
                    }
                }
                if(remove_keys.Count() > 0)
                {
                    foreach (string a in remove_keys)
                    {
                        avoid_sims.Remove(a);
                    }
                }
            }
        }


        public bool IsSimHome(string simname)
        {
            simname = simname.ToLowerInvariant();
            if (helpers.notempty(myconfig.Basic_HomeRegions) == false)
            {
                return false;
            }
            else if (myconfig.Basic_HomeRegions.Length == 0)
            {
                return false;
            }
            else
            {
                bool reply = false;
                foreach (string sl_url in myconfig.Basic_HomeRegions)
                {
                    string[] bits = helpers.ParseSLurl(sl_url);
                    if (helpers.notempty(bits) == true)
                    {
                        if (bits.Length == 4)
                        {
                            if(bits[0].ToLowerInvariant() == simname)
                            {
                                return true;
                            }
                        }
                    }
                }
                return reply;
            }
            
        }

        public string LoggedinAthome()
        {
            string addon = "Waiting for action";
            if (teleport_lockout == false)
            {
                if (IsSimHome(Client.Network.CurrentSim.Name) == true)
                {
                    addon = "Home region";
                }
                else
                {
                    long dif = last_tp_attempt_unixtime - helpers.UnixTimeNow();
                    if (dif > 0)
                    {
                        addon = "Waiting for TP cooldown";
                    }
                    else
                    {
                        GotoNextHomeRegion();
                    }
                }
            }
            else
            {
                addon = "Lockout";
            }
            return addon;
        }


        protected long login_failed_retry_at = 0;
        public string AtHomeReconnect()
        {
            if (attempted_first_login == true)
            {
                if (login_failed == false)
                {
                    return login_status;
                }
                else
                {
                    if(login_failed_retry_at == 0)
                    {
                        login_failed_retry_at = helpers.UnixTimeNow() + 40;
                        return login_status + " Retrying in 40 secs";
                    }
                    else
                    {
                        long dif = login_failed_retry_at - helpers.UnixTimeNow();
                        if(dif > 0)
                        {
                            login_failed_retry_at = 0;
                            attempted_first_login = false;
                            Start(true);
                            return "Attempting reconnect";
                        }
                        else
                        {
                            return "Waiting for reconnect - " + dif.ToString() + " secs";
                        }
                    }
                }
            }
            else
            {
                return "Waiting";
            }

        }

        protected override string[] getNextHomeArgs()
        {
            string UseSLurl = "";
            foreach (string Slurl in myconfig.Basic_HomeRegions)
            {
                string simname = helpers.RegionnameFromSLurl(Slurl);
                if (avoid_sims.Keys.Contains(simname) == false)
                {
                    UseSLurl = Slurl;
                    break;
                }
            }
            string[] bits = helpers.ParseSLurl(UseSLurl);
            if (helpers.notempty(bits) == true)
            {
                if (bits.Length == 4)
                {
                    return bits;
                }
            }
            return new string[] { };
        }

        public string GotoNextHomeRegion()
        {
            if (myconfig.Basic_HomeRegions.Length > 0)
            {
                last_tp_attempt_unixtime = helpers.UnixTimeNow();
                List<string> tested_sims = new List<string>();
                string UseSLurl = "";
                string simname = "";
                while ((tested_sims.Count() != myconfig.Basic_HomeRegions.Length) && (UseSLurl == ""))
                {
                    last_tested_home_id++;
                    if (last_tested_home_id >= myconfig.Basic_HomeRegions.Length)
                    {
                        last_tested_home_id = 0;
                    }
                    UseSLurl = myconfig.Basic_HomeRegions[last_tested_home_id];
                    simname = helpers.RegionnameFromSLurl(UseSLurl);
                    if (simname != last_attempted_teleport_region)
                    {
                        if (avoid_sims.ContainsKey(simname) == true)
                        {
                            UseSLurl = "";
                        }
                    }
                    tested_sims.Add(simname);
                }
                if (UseSLurl != "")
                {
                    string whyrejected = TeleportWithSLurl(UseSLurl);
                    if (whyrejected == "ok")
                    {
                        AvoidSim(simname);
                        return "**** active teleport: " + last_attempted_teleport_region + "***";
                    }
                    AvoidSim(simname);
                    return "TP to " + simname + " rejected - " + whyrejected;
                }
                return "No other sims found to teleport to";
            }
            return "No home regions";
        }


        protected string AtHomeStatus(string message)
        {
            return " [@Home: " + message+"]";
        }

        protected string AtHome_laststatus = "";
        public override string GetStatus()
        {
            ExpireOldAvoidSims();
            string reply = "Disconnected: No action";
            if (Client.Network.Connected == true)
            {
                if (Client.Network.CurrentSim != null)
                {
                    if(after_login_fired == false)
                    {
                        AfterBotLoginHandler();
                    }
                    reply = LoggedinAthome();
                }
                else
                {
                    reply = "Connected: No Sim";
                }
            }
            else
            {
                reply = AtHomeReconnect();
            }
            reply = AtHomeStatus(reply);
            if (reply != AtHome_laststatus)
            {
                AtHome_laststatus = reply;
                return base.GetStatus() + reply;
            }
            return base.GetStatus();
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
                    if (avoid_sims.ContainsKey(regionName) == false)
                    {
                        last_tp_attempt_unixtime = helpers.UnixTimeNow();
                        last_attempted_teleport_region = regionName;
                        Client.Self.Teleport(regionName, new Vector3(posX, posY, posZ));
                        return "ok";
                    }
                    return "Sim in avoid list";
                }
                return "Invaild bits length for SLurl";
            }
            return "No bits decoded";
        }

        public void SetHome(string sl_url)
        {
            if (sl_url != null)
            {
                if (myconfig.Basic_HomeRegions.Contains(sl_url) == false)
                {
                    List<string> old = myconfig.Basic_HomeRegions.ToList();
                    old.Add(sl_url);
                    myconfig.Basic_HomeRegions = old.ToArray();
                }
            }
            last_tested_home_id = -1;
        }
    }
}
