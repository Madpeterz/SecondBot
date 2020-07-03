using BetterSecondBotShared.bottypes;
using BetterSecondBotShared.logs;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB.bottypes
{
    public abstract class VirtualsBot : BasicBot
    {
        #region DiscordOutputTarget
        public virtual bool send_to_discord(string command,string message)
        {
            return false;
        }
        #endregion
        public virtual Dictionary<UUID, string> GetIMChatWindowKeyNames()
        {
            return new Dictionary<UUID, string>();
        }
        public virtual void AddToLocalChat(string name, string message)
        {

        }
        #region IMchat
        public virtual List<UUID> IMChatWindows()
        {
            return new List<UUID>();
        }
        public virtual bool ImChatWindowHasUnread(UUID chat_window)
        {
            return false;
        }
        public virtual List<string> GetIMChatWindow(UUID chat_window)
        {
            return new List<string>();
        }
        public virtual void AddToIMchat(UUID avatar, string name, string message)
        {

        }
        #endregion


        #region group
        protected List<UUID> active_group_chat_sessions = new List<UUID>();
        public virtual List<UUID> GetActiveGroupchatSessions { get { return new List<UUID>(); } }
        #endregion
        public virtual List<string> getLocalChatHistory()
        {
            return new List<string>() { };
        }
        #region GroupIMs
        public virtual bool HasUnreadGroupchats()
        {
            return false;
        }
        public virtual UUID[] UnreadGroupchatGroups()
        {
            return new UUID[] { };
        }
        public virtual List<string> GetGroupchat(UUID group)
        {
            return new List<string>();
        }
        public virtual void AddToGroupchat(UUID group, string name, string message)
        {

        }
        public virtual bool GroupHasUnread(UUID group)
        {
            return false;
        }
        public virtual void ClearGroupchat(UUID group)
        {

        }
        public virtual void ClearAllGroupchat()
        {

        }
        #endregion
        #region events
        protected virtual void PermissionsHandler(object sender, ScriptQuestionEventArgs e)
        {
            ConsoleLog.Debug("PermissionsHandler proc not overridden");
        }
        protected virtual void AvatarAnimationHandler(object sender, AvatarAnimationEventArgs e)
        {
            ConsoleLog.Debug("AvatarAnimationHandler proc not overridden");
        }

        protected virtual void SitHandler(object sender, AvatarSitResponseEventArgs e)
        {
            ConsoleLog.Debug("SitHandler proc not overridden");
        }
        protected virtual void MessageHandler(object sender, InstantMessageEventArgs e)
        {
            ConsoleLog.Debug("MessageHandler proc not overridden");
        }
        #endregion
        #region Dialogs
        public virtual void SetRelayDialogsChannel(int channel)
        {
        }
        public virtual void SetRelayDialogsAvatar(UUID avatar)
        {
        }
        public virtual void SetRelayDialogsHTTP(string httpurl)
        {
        }
        public virtual void SetTrackDialogs(bool status)
        {
        }
        public virtual bool DialogReply(int DialogID, string button)
        {
            return false;
        }
        #endregion

    }
}
