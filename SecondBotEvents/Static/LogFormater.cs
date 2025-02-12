using System;
using System.Text;

namespace SecondBotEvents
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
        public static string Warn(string message,bool send_to_console)
        {
            return Add(message, ConsoleLogLogLevel.Warn, send_to_console);
        }
        public static string Crit(string message, bool send_to_console)
        {
            return Add(message, ConsoleLogLogLevel.Crit, send_to_console);
        }
        public static string Info(string message, bool send_to_console)
        {
            return Add(message, ConsoleLogLogLevel.Info, send_to_console);
        }
        public static string Status(string message, bool send_to_console)
        {
            return Add(message, ConsoleLogLogLevel.Status, send_to_console);
        }
        public static string Debug(string message, bool send_to_console)
        {
            return Add(message, ConsoleLogLogLevel.Debug, send_to_console);
        }
        public static string Warn(string message)
        {
            return Warn(message, true);
        }
        public static string Crit(string message)
        {
            return Crit(message, true);
        }
        public static string Info(string message)
        {
            return Info(message, true);
        }
        public static string Status(string message)
        {
            return Status(message, true);
        }
        public static string Debug(string message)
        {
            return Debug(message, true);
        }

        public static string GetClockStamp()
        {
            var date = DateTime.Now;
            StringBuilder n = new();
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
            return n.ToString();
        }

        private static string Add(string message, ConsoleLogLogLevel Level, bool send_to_console = true)
        {
            
            StringBuilder n = new();
            n.Append(GetClockStamp());
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
            if(send_to_console == true)
            {
                Console.WriteLine(n.ToString());
            }
            return n.ToString();
        }
    }
}
