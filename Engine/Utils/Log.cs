using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Runtime.CompilerServices;

namespace Engine
{
    public static class Log
    {
        public enum LogLevel
        {
            Debug,
            Info,
            Warn,
            Error,
            Success
        }

        private static readonly object _lock = new object();

        public static void Info(string message,
                                [CallerFilePath] string file = "",
                                [CallerLineNumber] int line = 0,
                                [CallerMemberName] string member = "")
        {
            LogMessage(LogLevel.Info, message, file, line, member);
        }

        public static void Debug(string message,
                                [CallerFilePath] string file = "",
                                [CallerLineNumber] int line = 0,
                                [CallerMemberName] string member = "")
        {
            LogMessage(LogLevel.Debug, message, file, line, member);
        }

        public static void Warn(string message,
                                [CallerFilePath] string file = "",
                                [CallerLineNumber] int line = 0,
                                [CallerMemberName] string member = "")
        {
            LogMessage(LogLevel.Warn, message, file, line, member);
        }
        public static void Error(string message,
                                [CallerFilePath] string file = "",
                                [CallerLineNumber] int line = 0,
                                [CallerMemberName] string member = "")
        {
            LogMessage(LogLevel.Error, message, file, line, member);
        }

        public static void Success(string message,
                                [CallerFilePath] string file = "",
                                [CallerLineNumber] int line = 0,
                                [CallerMemberName] string member = "")
        {
            LogMessage(LogLevel.Success, message, file, line, member);
        }
        
        public static void LogMessage(LogLevel level, string message,
                                string file = "",
                                int line = 0,
                               string member = "")
        {
            lock (_lock) // thread-safe color changes
            {
                var prevColor = Console.ForegroundColor;
                Console.ForegroundColor = LevelToColor(level);

                string timestamp = DateTime.Now.ToString("HH:mm:ss");
                string filename = System.IO.Path.GetFileName(file);
                //Console.WriteLine($"[{timestamp}] [{level}] {filename}:{line} ({member}) - {message}");
                Console.WriteLine($"[{timestamp}] [{level}] [{filename}:{line}] {message}");

                Console.ForegroundColor = prevColor;
            }
        }

        private static ConsoleColor LevelToColor(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => ConsoleColor.Gray,
                LogLevel.Info => ConsoleColor.White,
                LogLevel.Warn => ConsoleColor.Yellow,
                LogLevel.Error => ConsoleColor.Red,
                LogLevel.Success => ConsoleColor.Green,
                _ => ConsoleColor.White
            };
        }
    }
}