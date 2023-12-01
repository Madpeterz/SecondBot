using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace SecondBotEvents
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            switch (input)
            {
                case null: throw new ArgumentNullException(nameof(input));
                case "": throw new ArgumentException($"{nameof(input)} cannot be empty", nameof(input));
                default: return input.First().ToString().ToUpper() + input.Substring(1);
            }
        }
    }
    public static class SecondbotHelpers
    {
        //

        public static byte[] ReadResourceFileBinary(Assembly targetenv, string filename)
        {
            try
            {
                byte[] dataset;
                string[] files = targetenv.GetManifestResourceNames();
                string resourceName = files.Single(str => str.EndsWith(filename));
                using (Stream stream = targetenv.GetManifestResourceStream(resourceName))
                using (BinaryReader br = new BinaryReader(stream))
                {
                    const int bufferSize = 4096;
                    using (var ms = new MemoryStream())
                    {
                        byte[] buffer = new byte[bufferSize];
                        int count;
                        while ((count = br.Read(buffer, 0, buffer.Length)) != 0)
                            ms.Write(buffer, 0, count);
                        dataset = ms.ToArray();
                    }
                }
                return dataset;
            }
            catch (Exception e)
            {
                return Encoding.UTF8.GetBytes(e.Message);
            }
        }
        public static string ReadResourceFile(Assembly targetenv,string filename)
        {
            try
            {
                string[] files = targetenv.GetManifestResourceNames();
                string resourceName = files.Single(str => str.EndsWith(filename));
                string result = "";
                using (Stream stream = targetenv.GetManifestResourceStream(resourceName))
                using (StreamReader reader = new StreamReader(stream))
                {
                    result = reader.ReadToEnd();
                }
                return result;
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        public static bool inrange(float current,float min,float max)
        {
            if(current <= max)
            {
                if(current >= min)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool inrange(double current, double min, double max)
        {
            if (current <= max)
            {
                if (current >= min)
                {
                    return true;
                }
            }
            return false;
        }
        public static bool inbox(float expected, float current, float drift)
        {
            return distance_check(expected, current, drift, true);
        }
        public static bool distance_check(float expected, float current, float drift, bool current_value)
        {
            if (current > (expected + drift))
            {
                return false;
            }
            else if (current < (expected - drift))
            {
                return false;
            }
            return current_value;
        }
        public static string GetSHA1(string text)
        {
            SHA1 hash = SHA1CryptoServiceProvider.Create();
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(text);
            byte[] hashBytes = hash.ComputeHash(plainTextBytes);
            string localChecksum = BitConverter.ToString(hashBytes)
            .Replace("-", "").ToLowerInvariant();
            return localChecksum;
        }
        public static string[] ParseSLurl(string url)
        {
            if (url != null)
            {
                url = url.Replace("http://maps.secondlife.com/secondlife/", "");
                url = url.Replace("%20", " ");
                string[] bits = url.Split('/');
                if (bits.Length == 4)
                {
                    return bits;
                }
            }
            return null;
        }
        public static string RegionnameFromSLurl(string url)
        {
            string[] bits = ParseSLurl(url);
            if (bits != null)
            {
                if (bits.Length == 4)
                {
                    return bits[0];
                }
            }
            return "";
        }
        public static long UnixTimeNow()
        {
            var timeSpan = (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0));
            return (long)timeSpan.TotalSeconds;
        }
        public static bool notempty(string V)
        {
            if (V != null)
            {
                if (V.Length > 0)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool isempty(string V)
        {
            if (V != null)
            {
                if (V.Length > 0)
                {
                    return false;
                }
            }
            return true;
        }

        public static bool notempty(string[] V)
        {
            if (V != null)
            {
                return true;
            }
            return false;
        }
    }
}
