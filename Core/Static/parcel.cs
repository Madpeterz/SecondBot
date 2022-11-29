using BetterSecondBot.bottypes;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BetterSecondBot
{
    public static class parcel_static
    {
        public static bool set_parcel_music(CommandsBot bot, Parcel p, string url)
        {
            if (p.MusicURL != url)
            {
                p.MusicURL = url;
                p.Update(bot.GetClient, bot.GetClient.Network.CurrentSim, false);
            }
            return true;
        }
        public static void ParcelSetFlag(ParcelFlags F, Parcel p, bool set)
        {
            if (p.Flags.HasFlag(F))
            {
                p.Flags -= F;
            }
            if (set == true)
            {
                p.Flags |= F;
            }
        }
        public static bool has_parcel_perm(Parcel P, CommandsBot bot)
        {
            bool has_perm = true;
            if (P.OwnerID != bot.GetClient.Self.AgentID)
            {
                if (P.IsGroupOwned)
                {
                    if (bot.MyGroups.ContainsKey(P.GroupID) == true)
                    {
                        bot.GetClient.Groups.ActivateGroup(P.GroupID);
                    }
                    else
                    {
                        has_perm = false;
                    }
                }
                else
                {
                    if(bot.GetClient.Network.CurrentSim.IsEstateManager == false)
                    {
                        has_perm = false;
                    }
                }
            }
            return has_perm;

        }
        public static string[] get_flag_names()
        {
            System.Type enumType = typeof(ParcelFlags);
            List<string> flagnames = new List<string>(System.Enum.GetNames(enumType));
            flagnames.Remove("None");
            return flagnames.ToArray();
        }

        public static Dictionary<string, string> get_media_list()
        {
            Dictionary<string, string> flags = new Dictionary<string, string>();
            flags.Add("MediaAutoScale", "Bool (True|False)");
            flags.Add("MediaLoop", "Bool (True|False)");
            flags.Add("MediaID", "UUID (Texture)");
            flags.Add("MediaURL", "String");
            flags.Add("MediaDesc", "String");
            flags.Add("MediaHeight", "Int (256 to 1024)");
            flags.Add("MediaWidth", "Int (256 to 1024)");
            flags.Add("MediaType", "String [\"IMG-PNG\",\"IMG-JPG\",\"VID-MP4\",\"VID-AVI\" or \"Custom-MIME_TYPE_CODE\"]");
            return flags;
        }
        public static Dictionary<string, ParcelFlags> get_flags_list()
        {
            Dictionary<string, ParcelFlags> flags = new Dictionary<string, ParcelFlags>();
            Type enumType = typeof(ParcelFlags);
            Array enumValues = Enum.GetValues(enumType);
            Array enumNames = Enum.GetNames(enumType);
            for (int i = 0; i < enumValues.Length; i++)
            {
                object value = enumValues.GetValue(i);
                ParcelFlags realValue = (ParcelFlags)value;
                if (realValue != 0)
                {
                    string name = (string)enumNames.GetValue(i);
                    flags.Add(name, realValue);
                }
            }
            return flags;
        }

    }
}
