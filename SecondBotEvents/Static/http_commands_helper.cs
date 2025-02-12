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
            Dictionary<string, Type> reply = new()
            {
                { "animation", typeof(AnimationCommands) },
                { "avatars", typeof(Avatars) },
                { "chat", typeof(Chat) },
                { "core", typeof(Core) },
                { "dialogs", typeof(Dialogs) },
                { "discord", typeof(DiscordCommands) },
                { "estate", typeof(Estate) },
                { "friends", typeof(Friends) },
                { "funds", typeof(Funds) },
                { "group", typeof(GroupCommands) },
                { "info", typeof(Info) },
                { "inventory", typeof(InventoryCommands) },
                { "movement", typeof(Movement) },
                { "notecard", typeof(Notecard) },
                { "parcel", typeof(ParcelCommands) },
                { "self", typeof(Self) },
                { "streamadmin", typeof(StreamAdmin) },
                { "services", typeof(SecondBotEvents.Commands.Services) }
            };
            return reply;
        }
    }
}
