using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;

namespace BetterSecondBot.bottypes
{
    public abstract class MessageSwitcherBot : EventsBot
    {
        protected bool connected_to_groups = false;
        public override string GetStatus()
        {
            if(connected_to_groups == false)
            {
                if (mygroups.Keys.Count() > 0)
                {
                    foreach (UUID group in mygroups.Keys)
                    {
                        Client.Self.RequestJoinGroupChat(group);
                    }
                    connected_to_groups = true;
                }
            }
            return base.GetStatus();
        }

        public override void AfterBotLoginHandler()
        {
            connected_to_groups = false;
            base.AfterBotLoginHandler();
        }

        readonly string[] hard_blocked_agents = new string[] { "secondlife", "second life"};
        protected override void ChatInputHandler(object sender, ChatEventArgs e)
        {
            bool fromme = false;
            if (e.FromName == GetClient.Self.Name)
            {
                fromme = true;
            }
            if (e.Type != ChatType.StartTyping)
            {
                if (e.Type != ChatType.StopTyping)
                {
                    bool av = false;
                    if (e.SourceType == ChatSourceType.Agent)
                    {
                        av = true;
                    }
                    if (e.Message != "")
                    {
                        string name_lower = e.FromName.ToLowerInvariant();
                        if (hard_blocked_agents.Contains(name_lower) == false)
                        {
                            BotChatControler(e.Message, e.FromName, e.SourceID, av, false, UUID.Zero, true, fromme);
                        }
                    }
                }
            }
        }
        protected virtual void BotChatControler(string message, string sender_name, UUID sender_uuid)
        {
            BotChatControler(message, sender_name, sender_uuid, false, false, UUID.Zero, false, false);
        }
        protected virtual void BotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar)
        {
            BotChatControler(message, sender_name, sender_uuid, avatar, false, UUID.Zero, false, false);
        }
        protected virtual void BotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid)
        {
            BotChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, false, false);
        }
        protected virtual void BotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar, bool group, UUID group_uuid, bool localchat)
        {
            BotChatControler(message, sender_name, sender_uuid, avatar, group, group_uuid, localchat, false);
        }

        protected override void MessageHandler(object sender, InstantMessageEventArgs e)
        {
            bool fromme = false;
            if (e.IM.FromAgentName == GetClient.Self.Name)
            {
                fromme = true;
            }
            switch (e.IM.Dialog)
            {
                case InstantMessageDialog.GroupInvitation:
                    {
                        GroupInvite(e);
                        break;
                    }
                case InstantMessageDialog.FriendshipOffered:
                    {
                        FriendshipOffer(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.FromAgentID);
                        break;
                    }
                case InstantMessageDialog.RequestLure:
                    {
                        RequestLure(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.FromAgentID);
                        break;
                    }
                case InstantMessageDialog.RequestTeleport:
                    {
                        RequestTeleport(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.FromAgentID);
                        break;
                    }
                case InstantMessageDialog.InventoryOffered:
                    {
                        break;
                    }
                case InstantMessageDialog.TaskInventoryOffered:
                    {
                        break;
                    }
                case InstantMessageDialog.MessageFromObject:
                    {
                        BotChatControler(e.IM.Message, e.IM.FromAgentName, e.IM.FromAgentID);
                        break;
                    }
                case InstantMessageDialog.MessageFromAgent: // shared with SessionSend
                case InstantMessageDialog.SessionSend:
                    {
                        string forcecheck = e.IM.FromAgentName.ToLowerInvariant().Replace(" ", "");
                        if (forcecheck != "secondlife")
                        {
                            if (mygroups.ContainsKey(e.IM.IMSessionID) == true)
                            {
                                AddToGroupchat(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.Message);
                                BotChatControler(e.IM.Message, e.IM.FromAgentName, e.IM.FromAgentID, true, true, e.IM.IMSessionID, false, fromme);
                            }
                            else
                            {
                                BotChatControler(e.IM.Message, e.IM.FromAgentName, e.IM.FromAgentID, true, false, UUID.Zero, false, fromme);
                            }
                        }
                        break;
                    }
                default:
                    {
                        break;
                    }
            }
        }
    }
}
