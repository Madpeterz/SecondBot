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
        protected bool friendslist_ready;
        protected bool masteruuid_ready;
        protected bool started_friendship_hug;
        protected bool sentFriendHugIM;

        protected virtual void FriendsListReady(object sender,FriendsReadyEventArgs e)
        {
            friendslist_ready = true;
            TryHugMaster();
        }

        protected void TryHugMaster()
        {
            if ((masteruuid_ready == true) && (friendslist_ready == true) && (started_friendship_hug == false))
            {
                started_friendship_hug = true;
                if (Client.Friends.FriendList.ContainsKey(master_uuid) == false)
                {
                    CommandsInterface.Call("IM", String.Join("~#~", new string[] { myconfig.Security_MasterUsername, "Hello master, I am sending you a friend request now!" }));
                }
                CommandsInterface.Call("FriendFullPerms", String.Join("~#~", new string[] { master_uuid.ToString(), "true" }), Client.Self.AgentID);
            }
        }

        protected override void BotStartHandler()
        {
            base.BotStartHandler();
            friendslist_ready = false;
            if (reconnect == false)
            {
                Client.Friends.friendsListReady += FriendsListReady;
            }
        }
        protected override void FoundMasterAvatar()
        {
            if (helpers.notempty(myconfig.Security_MasterUsername) == true)
            {
                masteruuid_ready = true;
                TryHugMaster();
            }
        }

        protected override void FriendshipResponse(object o,FriendshipResponseEventArgs E)
        {
            base.FriendshipResponse(o, E);
            if(E.Accepted == true)
            {
                if(E.AgentName == myconfig.Security_MasterUsername)
                {
                    CommandsInterface.Call("FriendFullPerms", String.Join("~#~", new string[] { myconfig.Security_MasterUsername, "true" }), Client.Self.AgentID);
                }
            }
        }



    }
}
