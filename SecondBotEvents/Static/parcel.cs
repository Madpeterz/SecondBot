using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace SecondBotEvents
{
    public static class ParcelStatic
    {
        public static bool SetParcelMusic(GridClient bot, Parcel p, string url)
        {
            if (p.MusicURL != url)
            {
                p.MusicURL = url;
                p.Update(bot);
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
        public static bool HasParcelPerm(Parcel P, GridClient bot)
        {
            bool has_perm = true;
            if (P.OwnerID != bot.Self.AgentID)
            {
                if (P.IsGroupOwned)
                {
                    if (bot.Groups.GroupName2KeyCache.ContainsKey(P.GroupID) == true)
                    {
                        bot.Groups.ActivateGroup(P.GroupID);
                    }
                    else
                    {
                        has_perm = false;
                    }
                }
                else
                {
                    if(bot.Network.CurrentSim.IsEstateManager == false)
                    {
                        has_perm = false;
                    }
                }
            }
            return has_perm;

        }
        public static string[] GetFlagNames()
        {
            System.Type enumType = typeof(ParcelFlags);
            List<string> flagnames = new(System.Enum.GetNames(enumType));
            flagnames.Remove("None");
            return [.. flagnames];
        }

        public static Dictionary<string, string> GetMediaList()
        {
            Dictionary<string, string> flags = new()
            {
                { "MediaAutoScale", "Bool (True|False)" },
                { "MediaLoop", "Bool (True|False)" },
                { "MediaID", "UUID (Texture)" },
                { "MediaURL", "String" },
                { "MediaDesc", "String" },
                { "MediaHeight", "Int (256 to 1024)" },
                { "MediaWidth", "Int (256 to 1024)" },
                { "MediaType", "String [\"IMG-PNG\",\"IMG-JPG\",\"VID-MP4\",\"VID-AVI\" or \"Custom-MIME_TYPE_CODE\"]" }
            };
            return flags;
        }
        public static Dictionary<string, ParcelFlags> GetFlagsList()
        {
            Dictionary<string, ParcelFlags> flags = [];
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
