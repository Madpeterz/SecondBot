using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OpenMetaverse;

namespace BSB.bottypes
{
    public abstract class MessageSwitcherBot : EventsBot
    {
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
                    BotChatControler(e.Message, e.FromName, e.SourceID, av, false, UUID.Zero, true, fromme);
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
        protected virtual void BotChatControler(string message, string sender_name, UUID sender_uuid, bool avatar,bool group, UUID group_uuid, bool localchat,bool fromme)
        {
            if (fromme == false)
            {
                if (localchat == false)
                {
                    if (avatar == false)
                    {
                        bool signed_command = false;
                        string signing_code = "";
                        string[] input = message.Split(new[] { "@@@" }, StringSplitOptions.None);
                        if (input.Count() == 2)
                        {
                            signed_command = true;
                            message = input[0];
                            signing_code = input[1];
                        }
                        List<string> bits = message.Split(new[] { "|||" }, StringSplitOptions.None).ToList();
                        if (bits.Count == 1)
                        {
                            bits.Add("");
                        }
                        if (signed_command == true)
                        {
                            CoreCommandLib(sender_uuid, false, bits.ElementAt(0), bits.ElementAt(1), signing_code);
                        }
                    }
                    else if (myconfig.master == sender_name)
                    {
                        List<string> bits = message.Split(new[] { "|||" }, StringSplitOptions.None).ToList();
                        if (bits.Count == 1)
                        {
                            bits.Add("");
                        }
                        CoreCommandLib(sender_uuid, true, bits.ElementAt(0), bits.ElementAt(1));
                    }
                }
            }
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
                        if (mygroups.ContainsKey(e.IM.IMSessionID) == true)
                        {
                            AddToGroupchat(e.IM.IMSessionID, e.IM.FromAgentName, e.IM.Message);
                            BotChatControler(e.IM.Message, e.IM.FromAgentName, e.IM.FromAgentID, true, true, e.IM.IMSessionID, false, fromme);
                        }
                        else
                        {
                            BotChatControler(e.IM.Message, e.IM.FromAgentName, e.IM.FromAgentID, true, false, UUID.Zero, false, fromme);
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
