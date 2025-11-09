# 日志服务设计文档

## 1. 服务概述

日志服务负责系统运行日志、用户操作日志和错误日志的记录、管理和查询，为系统提供统一的日志管理功能，支持日志级别控制、日志文件管理和日志导出。

## 2. 服务接口

```csharp
public interface ILoggingService
{
    /// <summary>
    /// 记录日志
    /// </summary>
    /// <param name="level">日志级别</param>
    /// <param name="message">日志消息</param>
    void Log(LogLevel level, string message);
    
    /// <summary>
    /// 记录错误日志
    /// </summary>
    /// <param name="message">错误消息</param>
    /// <param name="ex">异常对象</param>
    void LogError(string message, Exception ex = null);
    
    /// <summary>
    /// 记录信息日志
    /// </summary>
    /// <param name="message">信息消息</param>
    void LogInfo(string message);
    
    /// <summary>
    /// 记录警告日志
    /// </summary>
    /// <param name="message">警告消息</param>
    void LogWarning(string message);
    
    /// <summary>
    /// 记录调试日志
    /// </summary>
    /// <param name="message">调试消息</param>
    void LogDebug(string message);
    
    /// <summary>
    /// 记录致命错误日志
    /// </summary>
    /// <param name="message">致命错误消息</param>
    /// <param name="ex">异常对象</param>
    void LogFatal(string message, Exception ex = null);
    
    /// <summary>
    /// 获取日志内容
    /// </summary>
    /// <returns>日志内容字符串</returns>
    string GetLogContent();
    
    /// <summary>
    /// 获取日志条目列表
    /// </summary>
    /// <returns>日志条目列表</returns>
    List<LogEntry> GetLogEntries();
    
    /// <summary>
    /// 导出日志到文件
    /// </summary>
    /// <param name="filePath">导出文件路径</param>
    /// <returns>导出是否成功</returns>
    bool ExportLog(string filePath);
    
    /// <summary>
    /// 清除日志
    /// </summary>
    void ClearLog();
    
    /// <summary>
    /// 设置日志级别
    /// </summary>
    /// <param name="level">日志级别</param>
    void SetLogLevel(LogLevel level);
    
    /// <summary>
    /// 获取当前日志级别
    /// </summary>
    LogLevel CurrentLogLevel { get; }
    
    /// <summary>
    /// 设置日志文件路径
    /// </summary>
    /// <param name="filePath">日志文件路径</param>
    void SetLogFilePath(string filePath);
    
    /// <summary>
    /// 设置日志文件大小限制
    /// </summary>
    /// <param name="maxSizeInMB">最大文件大小（MB）</param>
    void SetLogFileSizeLimit(int maxSizeInMB);
    
    /// <summary>
    /// 设置日志文件滚动策略
    /// </summary>
    /// <param name="strategy">滚动策略</param>
    void SetLogRollingStrategy(LogRollingStrategy strategy);
    
    /// <summary>
    /// 日志条目添加事件
    /// </summary>
    event EventHandler<LogEntryAddedEventArgs> LogEntryAdded;
}
```

## 3. 实现类设计

### 3.1 LoggingService

```csharp
public class LoggingService : ILoggingService
{
    // 实现ILoggingService接口的所有方法
    // 提供日志记录、管理和查询功能
}
```

### 3.2 LogFileManager

```csharp
public class LogFileManager
{
    public void WriteToFile(LogEntry entry, string filePath);
    public void CheckFileSize(string filePath, int maxSizeInMB);
    public void RollLogFile(string filePath, LogRollingStrategy strategy);
    
    // 内部辅助方法
    private string GenerateRolledFileName(string originalPath, LogRollingStrategy strategy);
    private void ArchiveOldLogs(string directory, int maxArchivedFiles);
}
```

### 3.3 LogFormatter

```csharp
public class LogFormatter
{
    public string FormatLogEntry(LogEntry entry, LogFormat format);
    
    // 不同格式的格式化方法
    private string FormatAsText(LogEntry entry);
    private string FormatAsCsv(LogEntry entry);
    private string FormatAsJson(LogEntry entry);
}
```

## 4. 数据模型引用

- **LogEntry**: 日志条目数据模型
- **LogLevel**: 日志级别枚举
- **LogRollingStrategy**: 日志滚动策略枚举
- **LogFormat**: 日志格式枚举
- **LogEntryAddedEventArgs**: 日志条目添加事件参数数据模型

## 5. 关键功能实现

### 5.1 日志级别管理
- 支持多级别日志（Debug, Info, Warning, Error, Fatal）
- 支持级别过滤
- 支持运行时级别调整

### 5.2 日志存储
- 支持文件存储
- 支持内存缓存
- 支持日志文件自动滚动
- 支持日志文件大小限制

### 5.3 日志格式
- 支持文本格式
- 支持CSV格式
- 支持JSON格式
- 支持自定义格式

### 5.4 日志查询
- 支持按时间范围查询
- 支持按级别查询
- 支持按内容搜索
- 支持分页查询

### 5.5 日志导出
- 支持导出为文本文件
- 支持导出为CSV文件
- 支持导出为JSON文件
- 支持导出为Excel文件

### 5.6 性能优化
- 使用异步日志写入
- 使用缓冲区减少I/O操作
- 支持批量处理

## 6. 扩展功能

### 6.1 远程日志
- 支持日志远程传输
- 支持日志集中管理

### 6.2 日志分析
- 支持简单的日志统计
- 支持错误模式识别
- 支持性能分析

### 6.3 用户操作日志
- 支持用户操作追踪
- 支持操作审计
- 支持权限管理集成

## 7. 依赖关系

- 依赖LogEntry等日志相关数据模型
- 依赖FileHandler进行文件操作
- 可选依赖其他日志框架（如NLog, log4net）

## 8. 安全性

- 敏感信息日志过滤
- 日志文件访问权限控制
- 日志完整性保护

## 9. 配置项

- 日志级别
- 日志文件路径
- 文件大小限制
- 滚动策略
- 导出格式