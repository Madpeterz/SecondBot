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
        protected long ConnectionLostAt = 0;
        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            Info("Home regions count: " + myconfig.Basic_HomeRegions.Length.ToString() + "");
            last_tp_attempt_unixtime = helpers.UnixTimeNow() + 30;
            if (reconnect == true)
            {
                ResetAtHome();
            }
            else
            {
                Client.Self.AlertMessage += AlertEvent;
                Client.Network.SimChanged += ChangeSim;
            }
            if (Client.Network.Connected == true)
            {
                if (Client.Network.CurrentSim != null)
                {
                    if (IsSimHome(Client.Network.CurrentSim.Name) == true)
                    {
                        if (UUID.TryParse(myconfig.Setting_DefaultSit_UUID, out UUID sit_UUID) == true)
                        {
                            Client.Self.RequestSit(sit_UUID, Vector3.Zero);
                        }
                    }
                }
            }
            after_login_fired = true;
        }

        protected string FirstLoginStage()
        {
            long dif = helpers.UnixTimeNow() - last_reconnect_attempt;
            if (login_auto_logout == true)
            {
                if (last_reconnect_attempt == 0)
                {
                    last_reconnect_attempt = helpers.UnixTimeNow();
                    return login_status;
                }
                else
                {
                    if (auto_logout_login_recover == false)
                    {
                        auto_logout_login_recover = true;
                        last_reconnect_attempt = helpers.UnixTimeNow();
                        return "W4>" + login_status + " (20 secs)";
                    }
                    else if (dif >= 20)
                    {
                        login_auto_logout = false;
                        auto_logout_login_recover = false;
                        last_reconnect_attempt = helpers.UnixTimeNow();
                        Start(true);
                        return "Restarting first login [Using Alt locations]";
                    }
                    return "Waiting for cooldown";
                }
            }
            else
            {
                return login_status;
            }
        }

        protected string LoggedInConnectionLost()
        {
            long dif = helpers.UnixTimeNow() - last_reconnect_attempt;
            if (reconnect_mode == true)
            {
                last_tested_home_id = -1;
                last_tp_attempt_unixtime = 0;
                after_login_fired = false;
                teleported = false;
                reconnect_mode = true;
                last_reconnect_attempt = helpers.UnixTimeNow();
                return "Connection lost - switching to recovery mode";
            }
            else
            {
                if (dif <= 60)
                {
                    return "W4> Network reset timeout [60 secs]";
                }
                else
                {
                    last_reconnect_attempt = helpers.UnixTimeNow();
                    reconnect = true;
                    Start(true);
                    return "Attempting reconnect";
                }
            }
        }

        protected string LoggedOutNextAction()
        {
            if (login_auto_logout == true)
            {
                return FirstLoginStage();
            }
            if ((attempted_first_login == true) && (login_failed == true))
            {
                return LoggedInConnectionLost();
            }
            return "";
        }
        protected int last_tested_home_id = -1;
        protected long last_tp_attempt_unixtime;
        protected bool after_login_fired;

        
        protected long last_reconnect_attempt;
        protected bool reconnect_mode;
        protected string last_attempted_teleport_region = "";
        protected Dictionary<string, long> avoid_sims = new Dictionary<string, long>();
        protected bool SimShutdownAvoid;
        protected long SimShutdownAvoidCooldown = 0;
        protected bool auto_logout_login_recover;

        protected void ChangeSim(object sender,SimChangedEventArgs e)
        {
            if (Client.Network.CurrentSim.Name != last_attempted_teleport_region)
            {
                if (IsSimHome(Client.Network.CurrentSim.Name) == false)
                {
                    SetTeleported();
                }
                else
                {
                    if (UUID.TryParse(myconfig.Setting_DefaultSit_UUID, out UUID sit_UUID) == true)
                    {
                        Client.Self.RequestSit(sit_UUID, Vector3.Zero);
                    }
                }
            }
            else
            {
                if (SimShutdownAvoid == true)
                {
                    Status("Avoided sim shutdown will attempt to go home in 4 mins");
                    SimShutdownAvoid = false;
                }
            }
        }
        protected void AlertEvent(object sender,AlertMessageEventArgs e)
        {
            if(e.Message.Contains("restart") == true)
            {
                // oh snap region is dead run away
                Info("--- Sim Restarting ---");
                if (avoid_sims.ContainsKey(Client.Network.CurrentSim.Name) == false)
                {
                    avoid_sims.Add(Client.Network.CurrentSim.Name, helpers.UnixTimeNow() + (10 * 60));
                }
                GotoNextHomeRegion(true);
            }
        }

        public void ResetAtHome()
        {
            Debug("@home / ResetAtHome");
            last_tested_home_id = -1;
            after_login_fired = false;
            teleported = false;
            reconnect_mode = false;
            last_tp_attempt_unixtime = 0;
            avoid_sims = new Dictionary<string, long>();
        }



        protected override void AvoidSim(string simname)
        {
            if (avoid_sims.ContainsKey(simname) == false)
            {
                Debug("@home / AvoidSim: " + simname);
                avoid_sims.Add(simname, helpers.UnixTimeNow() + 240); // @home will avoid that sim for the next 4 mins
            }
        }

        protected void ExpireOldAvoidSims()
        {
            if (avoid_sims.Count() > 0)
            {
                List<string> remove_keys = new List<string>();
                long now = helpers.UnixTimeNow();
                if (GetClient.Network.Connected == false)
                {
                    now -= 240; // when logged out, add 240s to all sims we are avoiding
                }
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
            string addon = "";
            if(IsSimHome(Client.Network.CurrentSim.Name) == true)
            {
                addon = "Home region";
            }
            else
            {
                if (SimShutdownAvoid == false)
                {
                    long dif = SimShutdownAvoidCooldown - helpers.UnixTimeNow();
                    if (dif < 0)
                    {
                        dif = helpers.UnixTimeNow() - last_tp_attempt_unixtime;
                        if (dif > 30)
                        {
                            if (teleported == false)
                            {
                                addon = GotoNextHomeRegion(false);
                            }
                            else
                            {
                                addon = "- Teleported by script/master -";
                            }
                        }
                        else
                        {
                            addon = "Busy with TP cooldown";
                        }
                    }
                    else
                    {
                        if (avoid_sims.Keys.Contains(Client.Network.CurrentSim.Name) == true)
                        {
                            dif = helpers.UnixTimeNow() - last_tp_attempt_unixtime;
                            if (dif > 45)
                            {
                                addon = GotoNextHomeRegion(true);
                            }
                            else
                            {
                                addon = "Avoid teleport underway";
                            }
                        }
                        else
                        {
                            double mins = Math.Round((double)dif / 60);
                            int min = (int)mins;
                            addon = "Hiding from the storm ("+min.ToString()+" mins remain)";
                        }
                    }
                }
                else
                {
                    addon = "Avoiding sim shutdown [TP underway]";
                }
            }
            return addon;
        }

        public string GotoNextHomeRegion()
        {
            return GotoNextHomeRegion(false);
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

        protected string GotoNextHomeRegion(bool panic_mode)
        {
            if (panic_mode == false)
            {
                if (myconfig.Basic_HomeRegions.Length > 0)
                {
                    last_tp_attempt_unixtime = helpers.UnixTimeNow();
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
                    if (UseSLurl != "")
                    {
                        string simname = helpers.RegionnameFromSLurl(UseSLurl);
                        if (simname != last_attempted_teleport_region)
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
                    }
                    return "No other sims found to teleport to [Maybe only 1 home region?]";
                }
                return "No home regions";
            }
            else
            {
                SimShutdownAvoid = true;
                SimShutdownAvoidCooldown = helpers.UnixTimeNow() + (60 * 10);
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
                if (UseSLurl != "")
                {
                    TeleportWithSLurl(UseSLurl);
                    Status("Attempting panic evac to: "+ last_attempted_teleport_region+"");
                    AvoidSim(UseSLurl); // black list that region so we dont try to go back there if we get a shutdown notice again
                }
                else
                {
                    Status("No vaild SLurl found, Teleporting to backup hub");
                    string[] Hubs = new[] { "https://maps.secondlife.com/secondlife/Morris/28/228/40/", "https://maps.secondlife.com/secondlife/Ahern/28/28/40/",
                            "https://maps.secondlife.com/secondlife/Bonifacio/228/228/40/","https://maps.secondlife.com/secondlife/Dore/228/28/40/" };
                    TeleportWithSLurl(Hubs[new Random().Next(0, Hubs.Length - 1)]);
                }
                return "Panic mode";
            }
        }


        protected string AtHomeStatus(string message)
        {
            return " [@Home: " + message+"]";
        }

        protected string AtHome_laststatus = "";
        public override string GetStatus()
        {
            ExpireOldAvoidSims();
            string reply;
            if (Client.Network.Connected == true)
            {
                if (Client.Network.CurrentSim != null)
                {
                    if(after_login_fired == false)
                    {
                        AfterBotLoginHandler();
                    }
                    reply = AtHomeStatus(LoggedinAthome());
                }
                else
                {
                    reply = AtHomeStatus("No Sim");
                }
            }
            else
            {
                reply = AtHomeStatus(LoggedOutNextAction());
            }
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
