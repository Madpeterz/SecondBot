using System;
using System.Collections.Generic;
using System.Text;
using static OpenMetaverse.ParcelManager;

namespace BSB.Commands.Parcel
{
    class GetParcelBanlist : ParcelCommand_CheckParcel_1arg_smart
    {
        public override string Helpfile { get { return "Fetchs the current parcels banlist and sends it to the smart reply target on [ARG 1]"; } }

        public override bool CallFunction(string[] args)
        {
            if (base.CallFunction(args) == true)
            {
                Dictionary<string, string> reply = new Dictionary<string, string>();
                foreach(ParcelAccessEntry e in targetparcel.AccessBlackList)
                {
                    string name = bot.FindAvatarKey2Name(e.AgentID);
                    reply.Add(e.AgentID.ToString(), name);
                }
                return bot.GetCommandsInterface.SmartCommandReply(true, args[0], targetparcel.Name, CommandName, reply);
            }
            return false;
        }
    }
}
