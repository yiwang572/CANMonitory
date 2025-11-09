using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Interfaces
{
    /// <summary>
    /// 日志服务接口
    /// 提供应用程序的日志记录功能，支持多种日志级别和格式化选项
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志消息</param>
        void Debug(string message);

        /// <summary>
        /// 记录调试信息，支持格式化
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Debug(string format, params object[] args);

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Info(string message);

        /// <summary>
        /// 记录信息日志，支持格式化
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Warning(string message);

        /// <summary>
        /// 记录警告日志，支持格式化
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Warning(string format, params object[] args);

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Error(string message);

        /// <summary>
        /// 记录错误日志，支持格式化
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Error(string format, params object[] args);

        /// <summary>
        /// 记录错误日志，并包含异常信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        void Error(string message, Exception exception);

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        void Fatal(string message);

        /// <summary>
        /// 记录致命错误日志，支持格式化
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        void Fatal(string format, params object[] args);

        /// <summary>
        /// 记录致命错误日志，并包含异常信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        void Fatal(string message, Exception exception);

        /// <summary>
        /// 设置日志级别
        /// </summary>
        /// <param name="level">日志级别</param>
        void SetLogLevel(LogLevel level);

        /// <summary>
        /// 启用文件日志
        /// </summary>
        /// <param name="logFilePath">日志文件路径</param>
        /// <param name="maxFileSize">最大文件大小（字节）</param>
        /// <param name="maxBackupFiles">最大备份文件数</param>
        void EnableFileLogging(string logFilePath, long maxFileSize = 10485760, int maxBackupFiles = 5);

        /// <summary>
        /// 禁用文件日志
        /// </summary>
        void DisableFileLogging();

        /// <summary>
        /// 启用控制台日志
        /// </summary>
        void EnableConsoleLogging();

        /// <summary>
        /// 禁用控制台日志
        /// </summary>
        void DisableConsoleLogging();

        /// <summary>
        /// 获取最近的日志条目
        /// </summary>
        /// <param name="count">要获取的日志条目数量</param>
        /// <returns>日志条目列表</returns>
        List<LogEntry> GetRecentLogEntries(int count = 100);

        /// <summary>
        /// 清除内存中的日志缓存
        /// </summary>
        void ClearLogCache();
    }

    /// <summary>
    /// 日志级别枚举
    /// </summary>
    public enum LogLevel
    {
        /// <summary>
        /// 调试级别
        /// </summary>
        Debug,
        
        /// <summary>
        /// 信息级别
        /// </summary>
        Info,
        
        /// <summary>
        /// 警告级别
        /// </summary>
        Warning,
        
        /// <summary>
        /// 错误级别
        /// </summary>
        Error,
        
        /// <summary>
        /// 致命错误级别
        /// </summary>
        Fatal
    }

    /// <summary>
    /// 日志条目类
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// 日志时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }
        
        /// <summary>
        /// 日志级别
        /// </summary>
        public LogLevel Level { get; set; }
        
        /// <summary>
        /// 日志消息
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// 异常信息（如果有）
        /// </summary>
        public string ExceptionInfo { get; set; }
        
        /// <summary>
        /// 将日志条目转换为字符串
        /// </summary>
        public override string ToString()
        {
            string exceptionPart = !string.IsNullOrEmpty(ExceptionInfo) ? $"\n异常: {ExceptionInfo}" : string.Empty;
            return $"[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{Level}] {Message}{exceptionPart}";
        }
    }
}