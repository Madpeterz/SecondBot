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
        protected string LoggedOutNextAction()
        {
            string addon = "";
            long dif = helpers.UnixTimeNow() - last_reconnect_attempt;
            if ((login_auto_logout == false) && (reconnect_mode == false) && (after_login_fired == true))
            {
                last_tested_home_id = -1;
                last_tp_attempt_unixtime = 0;
                after_login_fired = false;
                teleported = false;
                reconnect_mode = true;
                last_reconnect_attempt = helpers.UnixTimeNow();
                addon = " [Connection lost - switching to recovery mode]";
            }
            else if ((login_auto_logout == false) && (reconnect_mode == false) && (after_login_fired == false))
            {
                addon = " [" + login_status + "]";
            }
            else if ((login_auto_logout == false) && (reconnect_mode == true) && (dif > 120))
            {
                addon = " [@Attempting reconnect]";
                last_reconnect_attempt = helpers.UnixTimeNow();
                reconnect = true;
                Start();
            }
            else if ((login_auto_logout == false) && (reconnect_mode == true) && (dif <= 120))
            {
                addon = " [W4>Reconnect attempt timer]";
            }
            else if ((login_auto_logout == true) && (auto_logout_login_recover == false))
            {
                auto_logout_login_recover = true;
                last_reconnect_attempt = helpers.UnixTimeNow();
                addon = " [W4>" + login_status + " (10 secs)]";
            }
            else if ((login_auto_logout == true) && (auto_logout_login_recover == true) && (dif >= 10))
            {
                login_auto_logout = false;
                auto_logout_login_recover = false;
                last_reconnect_attempt = helpers.UnixTimeNow();
                addon = " [Restarting first login]";
                Start();
            }
            else
            {
                addon = " [" + login_status + "]";
            }
            return addon;
        }
        protected int last_tested_home_id = -1;
        protected long last_tp_attempt_unixtime;
        protected bool after_login_fired;

        
        protected long last_reconnect_attempt;
        protected bool reconnect_mode;
        protected string last_attempted_teleport_region = "";
        protected Dictionary<string, long> avoid_sims = new Dictionary<string, long>();
        protected bool SimShutdownAvoid;
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
                    ConsoleLog.Status("Avoided sim shutdown will attempt to go home in 4 mins");
                    SimShutdownAvoid = false;
                }
            }
        }
        protected void AlertEvent(object sender,AlertMessageEventArgs e)
        {
            if(e.Message.Contains("restart") == true)
            {
                // oh snap region is dead run away
                ConsoleLog.Info("--- Sim Restarting ---");
                if (avoid_sims.ContainsKey(Client.Network.CurrentSim.Name) == false)
                {
                    avoid_sims.Add(Client.Network.CurrentSim.Name, helpers.UnixTimeNow() + (10 * 60));
                }
                GotoNextHomeRegion(true);
            }
        }

        public void ResetAtHome()
        {
            last_tested_home_id = -1;
            last_tp_attempt_unixtime = 0;
            teleported = false;
            reconnect_mode = true;
        }



        protected void AvoidSim(string simname)
        {
            if(avoid_sims.ContainsKey(simname) == false)
            {
                avoid_sims.Add(simname, helpers.UnixTimeNow() + 240); // @home will avoid that sim for the next 4 mins
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
                    if (sl_url.ToLowerInvariant().Contains(simname) == true)
                    {
                        reply = true;
                    }
                }
                return reply;
            }
            
        }

        public string GotoNextHomeRegion()
        {
            return GotoNextHomeRegion(false);
        }

        protected string GotoNextHomeRegion(bool panic_mode)
        {
            if (panic_mode == false)
            {
                if (myconfig.Basic_HomeRegions.Length > 0)
                {
                    last_tp_attempt_unixtime = helpers.UnixTimeNow();
                    last_tested_home_id++;
                    if (myconfig.Basic_HomeRegions.Length >= last_tested_home_id)
                    {
                        last_tested_home_id = 0;
                    }
                    TeleportWithSLurl(myconfig.Basic_HomeRegions[last_tested_home_id]);
                    return " [@home **** active teleport: "+ last_attempted_teleport_region+"***]";
                }
                return " [@home No home regions]";
            }
            else
            {
                SimShutdownAvoid = true;
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
                    ConsoleLog.Status("Attempting panic evac to: "+ last_attempted_teleport_region+"");
                    AvoidSim(UseSLurl); // black list that region so we dont try to go back there if we get a shutdown notice again
                }
                else
                {
                    ConsoleLog.Status("No vaild SLurl found, Teleporting to backup hub");
                    string[] Hubs = new[] { "https://maps.secondlife.com/secondlife/Morris/28/228/40/", "https://maps.secondlife.com/secondlife/Ahern/28/28/40/",
                            "https://maps.secondlife.com/secondlife/Bonifacio/228/228/40/","https://maps.secondlife.com/secondlife/Dore/228/28/40/" };
                    TeleportWithSLurl(Hubs[new Random().Next(0, Hubs.Length - 1)]);
                }
                return "Panic mode";
            }
        }

        

        public override string GetStatus()
        {
            if (Client.Network.Connected == true)
            {
                return base.GetStatus();
            }
            else
            {
                return base.GetStatus() + LoggedOutNextAction();
            }
        }

        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            last_tp_attempt_unixtime = helpers.UnixTimeNow() + 30;
            if (reconnect == true)
            {
                last_tested_home_id = -1;
                after_login_fired = false;
                teleported = false;
                reconnect_mode = false;
            }
            else
            {
                Client.Self.AlertMessage += AlertEvent;
                Client.Network.SimChanged += ChangeSim;
            }
            if (UUID.TryParse(myconfig.Setting_DefaultSit_UUID, out UUID sit_UUID) == true)
            {
                Client.Self.RequestSit(sit_UUID, Vector3.Zero);
            }
            after_login_fired = true;
        }

        public void TeleportWithSLurl(string sl_url)
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
                    }
                }
            }
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
