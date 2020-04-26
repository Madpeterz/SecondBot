using BSB.bottypes;
using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB
{
    public static class parcel_static
    {
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
                    has_perm = false;
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

            System.Type enumType = typeof(ParcelFlags);
            System.Type enumUnderlyingType = System.Enum.GetUnderlyingType(enumType);
            System.Array enumValues = System.Enum.GetValues(enumType);
            System.Array enumNames = System.Enum.GetNames(enumType);
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
