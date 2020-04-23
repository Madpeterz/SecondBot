using OpenMetaverse;
using System;
using System.Collections.Generic;
using System.Text;

namespace BSB
{
    public class parcel_static
    {
        public static string[] get_flag_names()
        {
            System.Type enumType = typeof(ParcelFlags);
            List<string> flagnames = new List<string>(System.Enum.GetNames(enumType));
            flagnames.Remove("None");
            return flagnames.ToArray();
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
