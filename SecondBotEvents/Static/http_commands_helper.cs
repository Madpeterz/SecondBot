using SecondBotEvents.Commands;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents
{
    public static class http_commands_helper
    {
        public static Dictionary<string, Type> getCommandModules()
        {
            Dictionary<string, Type> reply = new Dictionary<string, Type>();
            reply.Add("animation", typeof(AnimationCommands));
            reply.Add("avatars", typeof(Avatars));
            reply.Add("chat", typeof(Chat));
            reply.Add("core", typeof(Core));
            reply.Add("dialogs", typeof(Dialogs));
            reply.Add("discord", typeof(DiscordCommands));
            reply.Add("estate", typeof(Estate));
            reply.Add("friends", typeof(Friends));
            reply.Add("funds", typeof(Funds));
            reply.Add("group", typeof(GroupCommands));
            reply.Add("info", typeof(Info));
            reply.Add("inventory", typeof(InventoryCommands));
            reply.Add("movement", typeof(Movement));
            reply.Add("notecard", typeof(Notecard));
            reply.Add("parcel", typeof(ParcelCommands));
            reply.Add("self", typeof(Self));
            reply.Add("streamadmin", typeof(StreamAdmin));
            reply.Add("services", typeof(SecondBotEvents.Commands.Services));
            return reply;
        }
    }
}
