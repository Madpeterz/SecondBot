using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;
using Discord;
using Discord.Webhook;
using BetterSecondBotShared.Json;
using BetterSecondBotShared.Static;

namespace BSB.bottypes
{
    public class SecondBot : DialogsBot
    {
        protected override void AfterBotLoginHandler()
        {
            base.AfterBotLoginHandler();
            if (helpers.notempty(myconfig.master) == true)
            {
                if (myconfig.OnStartLinkupWithMaster == true)
                {
                    CommandsInterface.Call("IM", String.Join("~#~", new string[] { myconfig.master, "Hello master I am now online and ready to accept teleport offers" }));
                    CommandsInterface.Call("FriendRequest", myconfig.master, Client.Self.AgentID);
                    CommandsInterface.Call("FriendFullPerms", String.Join("~#~", new string[] { myconfig.master, "true" }), Client.Self.AgentID);
                }
            }
        }

        protected override void FriendshipResponse(object o,FriendshipResponseEventArgs E)
        {
            base.FriendshipResponse(o, E);
            if(E.Accepted == true)
            {
                if(E.AgentName == myconfig.master)
                {
                    CommandsInterface.Call("FriendFullPerms", String.Join("~#~", new string[] { myconfig.master, "true" }), Client.Self.AgentID);
                }
            }
        }
    }
}
