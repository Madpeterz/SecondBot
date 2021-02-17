using BetterSecondBot.bottypes;
using BetterSecondBot.Static;
using EmbedIO;
using EmbedIO.Routing;
using EmbedIO.WebApi;
using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Newtonsoft.Json;
using System.Reflection;
using System.Threading;
using BetterSecondBotShared.Static;

namespace BetterSecondBot.HttpService
{
    public class HTTP_Movement : WebApiControllerWithTokens
    {
        public HTTP_Movement(SecondBot mainbot, TokenStorage setuptokens) : base(mainbot, setuptokens) { }

        [About("uses the AutoPilot to move to a location")]
        [ReturnHints("Error Unable to AutoPilot to location")]
        [ReturnHints("Accepted")]
        [ReturnHints("Convert to vector has failed")]
        [ReturnHints("?  value out of range 0-?")]
        [ArgHints("x", "URLARG", "X location to AutoPilot to")]
        [ArgHints("y", "URLARG", "y location to AutoPilot to")]
        [ArgHints("z", "URLARG", "z location to AutoPilot to")]
        [Route(HttpVerbs.Get, "/AutoPilot/{x}/{y}/{z}/{token}")]
        public object AutoPilot(string x, string y, string z, string token)
        {
            if (tokens.Allow(token, "movement", "AutoPilot", getClientIP()) == false)
            {
                return Failure("Token not accepted");
            }
            if (Vector3.TryParse("<" + x + "," + y + "," + z + ">", out Vector3 pos) == false)
            {
                return Failure("Convert to vector has failed");
            }
            if (helpers.inrange(pos.X, 0, 255) == false)
            {
                return Failure("x value out of range 0-255");
            }
            if (helpers.inrange(pos.Y, 0, 255) == false)
            {
                return Failure("y value out of range 0-255");
            }
            if (helpers.inrange(pos.Z, 0, 5000) == false)
            {
                return Failure("z value out of range 0-5000");
            }
            bot.GetClient.Self.AutoPilotCancel();
            bot.GetClient.Self.Movement.TurnToward(pos, true);
            Thread.Sleep(500);
            uint Globalx, Globaly;
            Utils.LongToUInts(bot.GetClient.Network.CurrentSim.Handle, out Globalx, out Globaly);
            bot.GetClient.Self.AutoPilot((ulong)(Globalx + pos.X), (ulong)(Globaly + pos.Y), pos.Z);
            return true;
        }

        [About("Attempt to teleport to a new region")]
        [ReturnHints("Error Unable to Teleport to location")]
        [ReturnHints("Accepted")]
        [ArgHints("region", "URLARG", "the name of the region we are going to")]
        [ArgHints("x", "URLARG", "X location to teleport to")]
        [ArgHints("y", "URLARG", "y location to teleport to")]
        [ArgHints("z", "URLARG", "z location to teleport to")]
        [Route(HttpVerbs.Get, "/Teleport/{region}/{x}/{y}/{z}/{token}")]
        public object Teleport(string region, string x, string y, string z, string token)
        {
            if (tokens.Allow(token, "core", "Teleport", getClientIP()) == true)
            {
                bool status = TeleportRequest(new string[] { region, x, y, z });
                if (status == true)
                {
                    return BasicReply("Accepted");
                }
                return Failure("Error Unable to Teleport to location");
            }
            return Failure("Token not accepted");
        }



        protected bool TeleportRequest(string[] args)
        {
            bot.GetClient.Self.AutoPilotCancel();
            if (args[0].Contains("http://maps.secondlife.com/secondlife/") == true)
            {
                bot.TeleportWithSLurl(args[0]);
                return true;
            }
            else
            {
                float posX = 128;
                float posY = 128;
                float posZ = 0;
                string regionName = bot.GetClient.Network.CurrentSim.Name;
                bool ok = true;
                int offset = 0;
                string[] tp_args = args[0].Split('/');
                if ((tp_args.Length == 4) || (tp_args.Length == 1))
                {
                    regionName = tp_args[0];
                    offset = 1;
                }
                if (tp_args.Length >= 3)
                {
                    float.TryParse(tp_args[offset + 0], out posX);
                    float.TryParse(tp_args[offset + 1], out posY);
                    float.TryParse(tp_args[offset + 2], out posZ);
                }
                else if (tp_args.Length == 2)
                {
                    ok = false;
                }
                if (ok == true)
                {
                    bot.SetTeleported();
                    bool status = bot.GetClient.Self.Teleport(regionName, new Vector3(posX, posY, posZ));
                    bot.ResetAnimations();
                    return status;
                }
                else
                {
                    return false;
                }
            }
        }
    }
}
