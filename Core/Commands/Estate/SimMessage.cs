using System;
using System.Collections.Generic;
using System.Text;
using BetterSecondBotShared.logs;
using BetterSecondBotShared.Static;
using OpenMetaverse;

namespace BSB.Commands.Estate
{
    public class SimMessage : CoreCommand_1arg
    {
        public override string[] ArgTypes { get { return new[] { "Text" }; } }
        public override string[] ArgHints { get { return new[] { "Message to send" }; } }
        public override string Helpfile { get { return "Sends the message [ARG 1] to the current sim"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                if (bot.GetClient.Network.CurrentSim.IsEstateManager == true)
                {
                    bot.GetClient.Estate.SimulatorMessage(args[0]);
                    return true;
                }
                else
                {
                    return Failed("Not an estate manager here");
                }
            }
            return false;
        }
    }
}
