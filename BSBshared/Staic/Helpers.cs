using BetterSecondBotShared.Json;
using BetterSecondBotShared.logs;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace BetterSecondBotShared.Static
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
    public static class helpers
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
                //url = url.Replace("%20", " ");
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
        public static bool botRequired(JsonConfig test)
        {
            if (helpers.notempty(test.Basic_BotUserName) && helpers.notempty(test.Basic_BotPassword) && helpers.notempty(test.Security_MasterUsername) && helpers.notempty(test.Security_SignedCommandkey))
            {
                // required values are set
                MakeJsonConfig Testing = new MakeJsonConfig();
                string[] testingfor = new[] { "Basic_BotUserName", "Basic_BotPassword", "Security_SignedCommandkey", "Security_WebUIKey" };
                bool default_value_found = false;
                foreach (string a in testingfor)
                {
                    if (Testing.GetCommandArgTypes(a).First() == MakeJsonConfig.GetProp(test, a))
                    {
                        LogFormater.Warn("" + a + " is currently set to the default");
                        default_value_found = true;
                        break;
                    }
                }
                if (default_value_found == false)
                {
                    LogFormater.Status("User => " + test.Basic_BotUserName);
                    LogFormater.Status("Master => " + test.Security_MasterUsername);
                }
                return !default_value_found;
            }
            else
            {
                if (helpers.notempty(test.Basic_BotUserName) == false)
                {
                    LogFormater.Warn("Basic_BotUserName is null or empty");
                }
                if (helpers.notempty(test.Basic_BotPassword) == false)
                {
                    LogFormater.Warn("Basic_BotPassword is null or empty");
                }
                if (helpers.notempty(test.Security_MasterUsername) == false)
                {
                    LogFormater.Warn("Security_MasterUsername is null or empty");
                }
                if (helpers.notempty(test.Security_SignedCommandkey) == false)
                {
                    LogFormater.Warn("Security_SignedCommandkey is null or empty");
                }
                return false;
            }
        }
        public static string create_dirty_table(string[] items)
        {
            return create_dirty_table(items, 6);
        }
        public static string create_dirty_table(string[] items, int cols)
        {

            bool row_open = false;
            int col_counter = 0;
            StringBuilder reply = new StringBuilder();
            reply.Append("<table border='1'>");
            foreach (string A in items)
            {
                if (row_open == false)
                {
                    reply.Append("<tr>");
                    row_open = true;
                }
                reply.Append("<td>" + A + "</td>");
                col_counter++;
                if (col_counter >= cols)
                {
                    reply.Append("</tr>");
                    col_counter = 0;
                    row_open = false;
                }
            }
            while (row_open == true)
            {
                reply.Append("<td></td>");
                col_counter++;
                if (col_counter >= cols)
                {
                    reply.Append("</tr>");
                    row_open = false;
                }
            }
            reply.Append("</table>");
            return reply.ToString();
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
