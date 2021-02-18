using BetterSecondBot.HttpService;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.Static
{
    public static class http_commands_helper
    {
        public static Dictionary<string, Type> getCommandModules()
        {
            Dictionary<string, Type> reply = new Dictionary<string, Type>();
            reply.Add("animation", typeof(HTTP_Animation));
            reply.Add("avatars", typeof(HTTP_Avatars));
            reply.Add("chat", typeof(HTTP_Chat));
            reply.Add("core", typeof(HTTP_Core));
            reply.Add("dialogs", typeof(HTTP_Dialogs));
            reply.Add("estate", typeof(HTTP_Estate));
            reply.Add("friends", typeof(HTTP_Friends));
            reply.Add("group", typeof(HTTP_Group));
            reply.Add("home", typeof(HTTP_Home));
            reply.Add("im", typeof(HTTP_IM));
            reply.Add("info", typeof(HTTP_Info));
            reply.Add("inventory", typeof(HTTP_Inventory));
            reply.Add("movement", typeof(HTTP_Movement));
            reply.Add("notecard", typeof(HTTP_Notecard));
            reply.Add("parcel", typeof(HTTP_Parcel));
            reply.Add("self", typeof(HTTP_Self));
            reply.Add("streamadmin", typeof(HTTP_StreamAdmin));
            return reply;
        }
    }
}
