using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;
using OpenMetaverse.Assets;
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
                addon = " [@home Connection lost - switching to recovery mode]";
            }
            else if ((login_auto_logout == false) && (reconnect_mode == false) && (after_login_fired == false))
            {
                addon = " [@home " + login_status + "]";
            }
            else if ((login_auto_logout == false) && (reconnect_mode == true) && (dif > 120))
            {
                addon = " [@home Attempting reconnect]";
                last_reconnect_attempt = helpers.UnixTimeNow();
                reconnect = true;
                Start();
            }
            else if ((login_auto_logout == false) && (reconnect_mode == true) && (dif <= 120))
            {
                addon = " [@home W4>Reconnect attempt timer]";
            }
            else if ((login_auto_logout == true) && (auto_logout_login_recover == false))
            {
                auto_logout_login_recover = true;
                last_reconnect_attempt = helpers.UnixTimeNow();
                addon = " [@home W4>" + login_status + " (10 secs)]";
            }
            else if ((login_auto_logout == true) && (auto_logout_login_recover == true) && (dif >= 10))
            {
                login_auto_logout = false;
                auto_logout_login_recover = false;
                last_reconnect_attempt = helpers.UnixTimeNow();
                addon = " [@home restarting first login]";
                Start();
            }
            else
            {
                addon = " [@home "+ login_status+"]";
            }
            return addon;
        }
        protected string LoggedInNextAction()
        {
            string addon = "";
            if (Client.Self.SittingOn > 0)
            {
                AfterLoginSitDown = true;
                addon = " [@home Sit-override]";
            }
            else
            {
                long dif = helpers.UnixTimeNow() - last_tp_attempt_unixtime;
                if (teleported == true)
                {
                    addon = " [@home disabled ~ Teleported :: use: \"resetathome\" to clear]";
                }
                else if (Client.Network.CurrentSim == null)
                {
                    addon = " [@home " + login_status + "]";
                }
                else if (after_login_fired == false)
                {
                    addon = " [@home " + login_status + "]";
                }
                else if (IsSimHome(Client.Network.CurrentSim.Name) == true)
                {
                    if (UUID.TryParse(myconfig.DefaultSitUUID, out UUID arg_is_uuid) == true)
                    {
                        if (AfterLoginSitDown == false)
                        {
                            addon = " [@home HomeSim ~ Attempting to sit down]";
                            Client.Self.RequestSit(arg_is_uuid, Vector3.Zero);
                        }
                        else
                        {
                            addon = " [@home HomeSim ~ Auto sit disabled :: use: \"resetathome\" to clear]";
                        }
                    }
                    else
                    {
                        addon = " [@home HomeSim]";
                    }
                }
                else if (dif <= 20)
                {
                    addon = " [@home W4>Cooldown]";
                }
                else if (myconfig.homeRegion.Length == 0)
                {
                    addon = " [@home No home regions]";
                }
                else if (Client.Network.CurrentSim.Name == last_attempted_teleport_region)
                {
                    addon = " [@home Teleported to a known sim]";
                    last_attempted_teleport_region = "";
                }
                else
                {
                    addon = GotoNextHomeRegion();
                }
            }
            return addon;
        }
                                        
   
        protected int last_tested_home_id = -1;
        protected long last_tp_attempt_unixtime;
        protected bool after_login_fired;
        protected bool teleported;
        protected long last_reconnect_attempt;
        protected bool reconnect_mode;
        protected string last_attempted_teleport_region = "";
        protected Dictionary<string, long> avoid_sims = new Dictionary<string, long>();
        protected bool SimShutdownAvoid;
        protected bool auto_logout_login_recover;
        protected bool AfterLoginSitDown = false;

        protected void ChangeSim(object sender,SimChangedEventArgs e)
        {
            if (Client.Network.CurrentSim.Name != last_attempted_teleport_region)
            {
                if (IsSimHome(Client.Network.CurrentSim.Name) == false)
                {
                    SetTeleported();
                }
            }
            else
            {
                if (SimShutdownAvoid == true)
                {
                    ConsoleLog.Status("Avoided sim shutdown will attempt to go home in 4 mins");
                    SimShutdownAvoid = false;
                    last_tp_attempt_unixtime = helpers.UnixTimeNow() + 240;
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
            AfterLoginSitDown = false;
        }

        public void SetTeleported()
        {
            teleported = true;
        }

        protected void AvoidSim(string simname)
        {
            if(avoid_sims.ContainsKey(simname) == false)
            {
                avoid_sims.Add(simname, helpers.UnixTimeNow() + 240); // @home will avoid that sim for the next 4 mins
            }
        }

        protected static bool inbox(float expected, float current, float drift)
        {
            return inbox(expected, current, drift, true);
        }
        protected static bool inbox(float expected,float current, float drift, bool current_value)
        {
            if (current > (expected + drift))
            {
                return false;
            }
            else if (current < (expected - drift))
            {
                return false;
            }
            return current_value;
        }

        public bool IsSimHome(string simname)
        {
            simname = simname.ToLowerInvariant();
            if (helpers.notempty(myconfig.homeRegion) == false) return false;
            else if (myconfig.homeRegion.Length == 0) return false;
            else
            {
                bool reply = false;
                foreach (string sl_url in myconfig.homeRegion)
                {
                    if (sl_url.ToLowerInvariant().Contains(simname) == true)
                    {
                        bool inrange = true;
                        string[] bits = helpers.ParseSLurl(sl_url);
                        if (helpers.notempty(bits) == true)
                        {
                            if (bits.Length == 4)
                            {
                                if (GetClient.Self.SittingOn == 0)
                                {
                                    float.TryParse(bits[1], out float posX);
                                    float.TryParse(bits[2], out float posY);
                                    float.TryParse(bits[3], out float posZ);
                                    float drift_range = 10;
                                    inrange = inbox(Client.Self.SimPosition.X, posX, drift_range, inrange);
                                    inrange = inbox(Client.Self.SimPosition.Y, posY, drift_range, inrange);
                                    inrange = inbox(Client.Self.SimPosition.Z, posZ, drift_range, inrange);
                                }
                            }
                        }
                        reply = inrange;
                        break;
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
                if (myconfig.homeRegion.Length > 1)
                {
                    last_tp_attempt_unixtime = helpers.UnixTimeNow();
                    last_tested_home_id++;
                    if (myconfig.homeRegion.Length >= last_tested_home_id)
                    {
                        last_tested_home_id = 0;
                    }
                    TeleportWithSLurl(myconfig.homeRegion[last_tested_home_id]);
                    return " [@home **** active teleport: "+ last_attempted_teleport_region+"***]";
                }
                else return " [@home No home regions]";
            }
            else
            {
                SimShutdownAvoid = true;
                string UseSLurl = "";
                foreach (string Slurl in myconfig.homeRegion)
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
                return base.GetStatus() + LoggedInNextAction();
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
                AfterLoginSitDown = false;
                reconnect_mode = false;
            }
            else
            {
                Client.Self.AlertMessage += AlertEvent;
                Client.Network.SimChanged += ChangeSim;
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
                if (myconfig.homeRegion.Contains(sl_url) == false)
                {
                    List<string> old = myconfig.homeRegion.ToList();
                    old.Add(sl_url);
                    myconfig.homeRegion = old.ToArray();
                }
            }
            last_tested_home_id = -1;
        }
    }
}
