using System;
using System.Text;

namespace BetterSecondBotShared.logs
{
    public enum ConsoleLogLogLevel
    {
        Debug, // [Not logged unless in debug]
        Info, // [Not logged unless in debug]
        Status,
        Warn,
        Crit
    };
    public static class ConsoleLog
    {
        public static void Warn(string message)
        {
            Add(message, ConsoleLogLogLevel.Warn);
        }
        public static void Crit(string message)
        {
            Add(message, ConsoleLogLogLevel.Crit);
        }
        public static void Info(string message)
        {
            Add(message, ConsoleLogLogLevel.Info);
        }
        public static void Status(string message)
        {
            Add(message, ConsoleLogLogLevel.Status);
        }
        public static void Debug(string message)
        {
            Add(message, ConsoleLogLogLevel.Debug);
        }
        private static void Add(string message, ConsoleLogLogLevel Level)
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
            }
            n.Append(message);
            Console.WriteLine(n.ToString());
        }
    }
}
