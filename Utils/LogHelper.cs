using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Utils
{
    public class LogHelper
    {
        private static readonly string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", $"log_{DateTime.Now:yyyyMMdd}.txt");

        static LogHelper()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logFilePath));
        }

        public static void Log(string message, string level = "INFO")
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
            File.AppendAllText(logFilePath, logEntry + Environment.NewLine);
        }

        public static void LogError(string message, Exception ex = null)
        {
            Log($"{message} {(ex != null ? $"- {ex.Message}\n{ex.StackTrace}" : "")}", "ERROR");
        }
    }
}