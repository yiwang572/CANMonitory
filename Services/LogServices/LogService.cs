using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CANMonitor.Interfaces;

namespace CANMonitor.Services.LogServices
{
    /// <summary>
    /// 日志服务实现类
    /// 提供应用程序的日志记录功能，支持文件日志、控制台日志和内存缓存
    /// </summary>
    public class LogService : ILogService
    {
        // 当前日志级别
        private LogLevel _currentLevel = LogLevel.Info;
        
        // 是否启用文件日志
        private bool _fileLoggingEnabled = false;
        
        // 是否启用控制台日志
        private bool _consoleLoggingEnabled = false;
        
        // 日志文件路径
        private string _logFilePath = string.Empty;
        
        // 最大文件大小（字节）
        private long _maxFileSize = 10485760; // 10MB
        
        // 最大备份文件数
        private int _maxBackupFiles = 5;
        
        // 内存日志缓存
        private List<LogEntry> _logCache = new List<LogEntry>();
        
        // 缓存锁对象
        private readonly object _cacheLock = new object();
        
        // 文件锁对象
        private readonly object _fileLock = new object();
        
        // 最大缓存条目数
        private const int MAX_CACHE_ENTRIES = 1000;

        /// <summary>
        /// 记录调试信息
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Debug(string message)
        {
            Log(LogLevel.Debug, message, null);
        }

        /// <summary>
        /// 记录调试信息，支持格式化
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public void Debug(string format, params object[] args)
        {
            string message = string.Format(format, args);
            Log(LogLevel.Debug, message, null);
        }

        /// <summary>
        /// 记录信息日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Info(string message)
        {
            Log(LogLevel.Info, message, null);
        }

        /// <summary>
        /// 记录信息日志，支持格式化
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public void Info(string format, params object[] args)
        {
            string message = string.Format(format, args);
            Log(LogLevel.Info, message, null);
        }

        /// <summary>
        /// 记录警告日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Warning(string message)
        {
            Log(LogLevel.Warning, message, null);
        }

        /// <summary>
        /// 记录警告日志，支持格式化
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public void Warning(string format, params object[] args)
        {
            string message = string.Format(format, args);
            Log(LogLevel.Warning, message, null);
        }

        /// <summary>
        /// 记录错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Error(string message)
        {
            Log(LogLevel.Error, message, null);
        }

        /// <summary>
        /// 记录错误日志，支持格式化
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public void Error(string format, params object[] args)
        {
            string message = string.Format(format, args);
            Log(LogLevel.Error, message, null);
        }

        /// <summary>
        /// 记录错误日志，并包含异常信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void Error(string message, Exception exception)
        {
            Log(LogLevel.Error, message, exception);
        }

        /// <summary>
        /// 记录致命错误日志
        /// </summary>
        /// <param name="message">日志消息</param>
        public void Fatal(string message)
        {
            Log(LogLevel.Fatal, message, null);
        }

        /// <summary>
        /// 记录致命错误日志，支持格式化
        /// </summary>
        /// <param name="format">格式化字符串</param>
        /// <param name="args">格式化参数</param>
        public void Fatal(string format, params object[] args)
        {
            string message = string.Format(format, args);
            Log(LogLevel.Fatal, message, null);
        }

        /// <summary>
        /// 记录致命错误日志，并包含异常信息
        /// </summary>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象</param>
        public void Fatal(string message, Exception exception)
        {
            Log(LogLevel.Fatal, message, exception);
        }

        /// <summary>
        /// 设置日志级别
        /// </summary>
        /// <param name="level">日志级别</param>
        public void SetLogLevel(LogLevel level)
        {
            _currentLevel = level;
            Info($"日志级别已设置为: {level}");
        }

        /// <summary>
        /// 启用文件日志
        /// </summary>
        /// <param name="logFilePath">日志文件路径</param>
        /// <param name="maxFileSize">最大文件大小（字节）</param>
        /// <param name="maxBackupFiles">最大备份文件数</param>
        public void EnableFileLogging(string logFilePath, long maxFileSize = 10485760, int maxBackupFiles = 5)
        {            
            try
            {
                // 确保日志文件目录存在
                string directory = Path.GetDirectoryName(logFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                _logFilePath = logFilePath;
                _maxFileSize = maxFileSize;
                _maxBackupFiles = maxBackupFiles;
                _fileLoggingEnabled = true;
                
                Info($"文件日志已启用: {logFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"启用文件日志失败: {ex.Message}");
                _fileLoggingEnabled = false;
            }
        }

        /// <summary>
        /// 禁用文件日志
        /// </summary>
        public void DisableFileLogging()
        {            
            _fileLoggingEnabled = false;
            Info("文件日志已禁用");
        }

        /// <summary>
        /// 启用控制台日志
        /// </summary>
        public void EnableConsoleLogging()
        {            
            _consoleLoggingEnabled = true;
            Info("控制台日志已启用");
        }

        /// <summary>
        /// 禁用控制台日志
        /// </summary>
        public void DisableConsoleLogging()
        {            
            _consoleLoggingEnabled = false;
            Info("控制台日志已禁用");
        }

        /// <summary>
        /// 获取最近的日志条目
        /// </summary>
        /// <param name="count">要获取的日志条目数量</param>
        /// <returns>日志条目列表</returns>
        public List<LogEntry> GetRecentLogEntries(int count = 100)
        {            
            lock (_cacheLock)
            {
                // 返回最近的指定数量的日志条目
                return _logCache
                    .OrderByDescending(entry => entry.Timestamp)
                    .Take(count)
                    .OrderBy(entry => entry.Timestamp)
                    .ToList();
            }
        }

        /// <summary>
        /// 清除内存中的日志缓存
        /// </summary>
        public void ClearLogCache()
        {            
            lock (_cacheLock)
            {
                _logCache.Clear();
            }
        }

        /// <summary>
        /// 核心日志记录方法
        /// </summary>
        /// <param name="level">日志级别</param>
        /// <param name="message">日志消息</param>
        /// <param name="exception">异常对象（可选）</param>
        private void Log(LogLevel level, string message, Exception exception)
        {            
            // 检查日志级别
            if (level < _currentLevel)
                return;

            // 创建日志条目
            LogEntry entry = new LogEntry
            {
                Timestamp = DateTime.Now,
                Level = level,
                Message = message,
                ExceptionInfo = exception?.ToString()
            };

            // 添加到内存缓存
            AddToCache(entry);

            // 输出到控制台
            if (_consoleLoggingEnabled)
            {
                WriteToConsole(entry);
            }

            // 写入到文件
            if (_fileLoggingEnabled)
            {
                WriteToFile(entry);
            }
        }

        /// <summary>
        /// 添加日志条目到缓存
        /// </summary>
        private void AddToCache(LogEntry entry)
        {            
            lock (_cacheLock)
            {
                // 添加新条目
                _logCache.Add(entry);

                // 如果缓存超过最大条目数，则移除最旧的条目
                if (_logCache.Count > MAX_CACHE_ENTRIES)
                {
                    int removeCount = _logCache.Count - MAX_CACHE_ENTRIES;
                    _logCache.RemoveRange(0, removeCount);
                }
            }
        }

        /// <summary>
        /// 将日志条目写入控制台
        /// </summary>
        private void WriteToConsole(LogEntry entry)
        {            
            try
            {
                // 根据日志级别设置控制台颜色
                ConsoleColor originalColor = Console.ForegroundColor;

                switch (entry.Level)
                {
                    case LogLevel.Debug:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;
                    case LogLevel.Info:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                    case LogLevel.Warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;
                    case LogLevel.Error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                    case LogLevel.Fatal:
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        break;
                }

                Console.WriteLine(entry.ToString());

                // 恢复原始颜色
                Console.ForegroundColor = originalColor;
            }
            catch { }
        }

        /// <summary>
        /// 将日志条目写入文件
        /// </summary>
        private void WriteToFile(LogEntry entry)
        {            
            if (string.IsNullOrEmpty(_logFilePath))
                return;

            lock (_fileLock)
            {
                try
                {
                    // 检查文件大小并进行滚动
                    CheckFileSizeAndRoll();

                    // 写入日志条目
                    using (StreamWriter writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                    {
                        writer.WriteLine(entry.ToString());
                    }
                }
                catch (Exception ex)
                {
                    // 如果写入日志失败，尝试输出到控制台
                    Console.WriteLine($"写入日志文件失败: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// 检查日志文件大小并进行滚动
        /// </summary>
        private void CheckFileSizeAndRoll()
        {            
            try
            {
                if (File.Exists(_logFilePath))
                {
                    FileInfo fileInfo = new FileInfo(_logFilePath);
                    
                    // 如果文件大小超过最大限制，进行文件滚动
                    if (fileInfo.Length >= _maxFileSize)
                    {
                        // 删除最旧的备份文件
                        for (int i = _maxBackupFiles - 1; i >= 0; i--)
                        {
                            string oldBackupPath = $"{_logFilePath}.{i + 1}";
                            string newBackupPath = $"{_logFilePath}.{i + 2}";

                            if (i == _maxBackupFiles - 1 && File.Exists(oldBackupPath))
                            {
                                File.Delete(oldBackupPath);
                            }
                            else if (File.Exists(oldBackupPath))
                            {
                                File.Move(oldBackupPath, newBackupPath);
                            }
                        }

                        // 将当前日志文件重命名为备份文件
                        File.Move(_logFilePath, $"{_logFilePath}.1");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"日志文件滚动失败: {ex.Message}");
            }
        }
    }
}