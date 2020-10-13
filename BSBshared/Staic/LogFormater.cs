using System;
using System.Text;

namespace BetterSecondBotShared.logs
{
    public enum ConsoleLogLogLevel
    {
        Status,
        Info,
        Crit,
        Warn,
        Debug,
    };
    public static class LogFormater
    {
        public static string Warn(string message)
        {
            return Add(message, ConsoleLogLogLevel.Warn);
        }
        public static string Crit(string message)
        {
            return Add(message, ConsoleLogLogLevel.Crit);
        }
        public static string Info(string message)
        {
            return Add(message, ConsoleLogLogLevel.Info);
        }
        public static string Status(string message)
        {
            return Add(message, ConsoleLogLogLevel.Status);
        }
        public static string Debug(string message)
        {
            return Add(message, ConsoleLogLogLevel.Debug);
        }
        private static string Add(string message, ConsoleLogLogLevel Level)
        {
            var date = DateTime.Now;
            StringBuilder n = new StringBuilder();
            n.Append("[");
            if (date.Hour < 10)
            {
                n.Append("0");
            }
            n.Append(date.Hour.ToString());
            n.Append(":");
            if (date.Minute < 10)
            {
                n.Append("0");
            }
            n.Append(date.Minute.ToString());
            n.Append("] ");
            switch (Level)
            {
#if DEBUG
                case ConsoleLogLogLevel.Debug:
                    {
                        n.Append("Debug - ");
                        break;
                    }
                case ConsoleLogLogLevel.Info:
                    {
                        n.Append("Info - ");
                        break;
                    }
#endif
                case ConsoleLogLogLevel.Status:
                    {
                        n.Append("Status - ");
                        break;
                    }
                case ConsoleLogLogLevel.Warn:
                    {
                        n.Append("Warn - ");
                        break;
                    }
                case ConsoleLogLogLevel.Crit:
                    {
                        n.Append("Crit - ");
                        break;
                    }
                default:
                    {
                        n.Append("Log - ");
                        break;
                    }
            }
            n.Append(message);
            return n.ToString();
        }
    }
}
