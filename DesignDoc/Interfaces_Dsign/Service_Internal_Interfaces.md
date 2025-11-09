# 服务层内部接口设计文档

## 1. 接口概述

本文档定义了服务层内部各模块之间的交互接口，确保服务层模块之间通过统一的抽象接口进行通信，实现服务层内部的解耦和模块化设计。

## 2. 接口设计原则

- **高内聚低耦合**：各服务模块职责单一，通过接口协作
- **依赖抽象**：服务间依赖抽象接口而非具体实现
- **可替换性**：接口实现可根据需求替换
- **标准化**：接口命名和参数设计保持一致风格
- **可测试性**：接口设计便于单元测试和模拟

## 3. 核心服务接口定义

### 3.1 ICanService

```csharp
/// <summary>
/// CAN通信服务接口
/// </summary>
public interface ICanService : IDisposable
{
    // 事件定义
    event EventHandler<CanMessage> MessageReceived;
    event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;
    event EventHandler<ErrorEventArgs> ErrorOccurred;
    
    // 连接管理方法
    Task<bool> ConnectAsync(int channel, int baudRate);
    Task DisconnectAsync(int channel);
    Task DisconnectAllAsync();
    bool IsConnected(int channel);
    List<CanChannel> GetAvailableChannels();
    
    // 消息收发方法
    Task<bool> SendMessageAsync(int channel, CanMessage message);
    Task<bool> SendMessagesAsync(int channel, List<CanMessage> messages);
    
    // 消息过滤方法
    void AddFilter(int channel, CanFilter filter);
    void RemoveFilter(int channel, CanFilter filter);
    void ClearFilters(int channel);
    List<CanFilter> GetActiveFilters(int channel);
    
    // 通道配置方法
    Task<bool> ConfigureChannelAsync(int channel, CanChannelConfig config);
    CanChannelConfig GetChannelConfig(int channel);
    
    // 统计信息方法
    CanStatistics GetChannelStatistics(int channel);
    void ResetStatistics(int channel);
    
    // 硬件信息方法
    DeviceInfo GetDeviceInfo(int channel);
    List<int> GetSupportedBaudRates(int channel);
}

// 通道配置类
public class CanChannelConfig
{
    public int BaudRate { get; set; }
    public int CanFDBaudRate { get; set; }
    public bool IsCanFDEnabled { get; set; }
    public int ReceiveBufferSize { get; set; }
    public int TransmitBufferSize { get; set; }
    public bool EnableLoopback { get; set; }
    public bool EnableSilentMode { get; set; }
}

// 设备信息类
public class DeviceInfo
{
    public string DeviceName { get; set; }
    public string DeviceId { get; set; }
    public string FirmwareVersion { get; set; }
    public string HardwareVersion { get; set; }
    public int MaxChannels { get; set; }
    public bool SupportsCanFD { get; set; }
    public DateTime DriverVersion { get; set; }
}
```

### 3.2 IDbcService

```csharp
/// <summary>
/// DBC文件解析与管理服务接口
/// </summary>
public interface IDbcService
{
    // 事件定义
    event EventHandler<DbcLoadedEventArgs> DbcLoaded;
    event EventHandler<DbcUnloadedEventArgs> DbcUnloaded;
    event EventHandler<ErrorEventArgs> ErrorOccurred;
    
    // DBC文件操作方法
    Task<DbcParserResult> LoadDbcFileAsync(string filePath);
    Task<DbcParserResult> LoadDbcFromStreamAsync(Stream stream, string fileName = null);
    bool UnloadDbcFile();
    bool IsDbcLoaded();
    string GetCurrentDbcFilePath();
    
    // DBC内容查询方法
    DbcFile GetCurrentDbcFile();
    List<DbcMessage> GetMessages();
    List<DbcSignal> GetSignals();
    List<DbcNode> GetNodes();
    
    // 信号解析方法
    List<SignalValue> ParseMessageSignals(CanMessage message);
    double ParseSignalValue(CanMessage message, string signalName);
    SignalValue ParseSingleSignal(CanMessage message, string signalName);
    
    // 信号编码方法
    CanMessage EncodeMessageWithSignals(uint messageId, Dictionary<string, double> signalValues);
    bool EncodeSignalValue(CanMessage message, string signalName, double value);
    
    // 搜索和过滤方法
    DbcMessage GetMessageById(uint messageId);
    DbcMessage GetMessageByName(string messageName);
    DbcSignal GetSignalByName(string signalName);
    List<DbcMessage> GetMessagesByNode(string nodeName);
    List<DbcSignal> GetSignalsByMessage(uint messageId);
    List<DbcSignal> GetSignalsByNode(string nodeName);
    
    // 验证方法
    bool ValidateDbcFile(string filePath);
    List<string> ValidateDbcContent(DbcFile dbcFile);
    
    // 导出方法
    bool ExportToCsv(string filePath, ExportType exportType);
    bool ExportToXml(string filePath);
    bool ExportToJson(string filePath);
}

// 事件参数类
public class DbcLoadedEventArgs : EventArgs
{
    public string FilePath { get; set; }
    public DbcFile DbcFile { get; set; }
}

public class DbcUnloadedEventArgs : EventArgs
{
    public string FilePath { get; set; }
}

// 导出类型枚举
public enum ExportType
{
    Messages,
    Signals,
    Nodes,
    All
}
```

### 3.3 IObdService

```csharp
/// <summary>
/// OBD诊断服务接口
/// </summary>
public interface IObdService : IDisposable
{
    // 事件定义
    event EventHandler<ObdResponse> ResponseReceived;
    event EventHandler<ObdRequest> RequestSent;
    event EventHandler<SessionStatusChangedEventArgs> SessionStatusChanged;
    event EventHandler<ErrorEventArgs> ErrorOccurred;
    
    // 会话管理方法
    Task<bool> ConnectAsync(int channel);
    Task DisconnectAsync();
    bool IsConnected();
    Task<bool> ChangeSessionAsync(byte sessionType);
    Task<bool> SecurityAccessAsync(byte securityLevel, byte[] key);
    ObdSession GetCurrentSession();
    
    // 诊断命令方法
    Task<ObdResponse> SendRequestAsync(ObdRequest request);
    Task<List<ObdDiagnosticTroubleCode>> ReadDtcAsync();
    Task<bool> ClearDtcAsync();
    Task<Dictionary<string, double>> ReadFreezeFrameAsync(int frameIndex = 0);
    
    // 内存访问方法
    Task<byte[]> ReadMemoryAsync(uint address, int length);
    Task<bool> WriteMemoryAsync(uint address, byte[] data);
    Task<byte[]> ReadDataByIdentifierAsync(byte[] identifier);
    Task<bool> WriteDataByIdentifierAsync(byte[] identifier, byte[] data);
    
    // ECU信息方法
    Task<List<EcuInfo>> GetEcuInfoAsync();
    Task<Dictionary<string, string>> GetVehicleInfoAsync();
    
    // 协议管理方法
    Task<DiagnosticProtocol> DetectProtocolAsync();
    Task<bool> SetProtocolAsync(DiagnosticProtocol protocol);
    DiagnosticProtocol GetCurrentProtocol();
    
    // 服务信息方法
    List<ObdService> GetSupportedServices();
    bool IsServiceSupported(byte serviceId);
    
    // 安全访问方法
    byte[] GenerateSecurityKey(byte securityLevel, byte[] seed);
    bool IsSecurityAccessGranted(byte securityLevel);
}

// ECU信息类
public class EcuInfo
{
    public string EcuName { get; set; }
    public string EcuId { get; set; }
    public string HardwareVersion { get; set; }
    public string SoftwareVersion { get; set; }
    public byte Address { get; set; }
    public List<byte> SupportedServices { get; set; }
}

// 诊断协议枚举
public enum DiagnosticProtocol
{
    ISO15765_4_CAN,   // ISO 15765-4 (CAN)
    ISO14230_4_KWP,   // ISO 14230-4 (KWP2000)
    ISO9141_2,        // ISO 9141-2
    SAEJ1850VPW,      // SAE J1850 VPW
    SAEJ1850PWM,      // SAE J1850 PWM
    CANExtended       // CAN Extended
}
```

### 3.4 IAutomationService

```csharp
/// <summary>
/// 自动化测试服务接口
/// </summary>
public interface IAutomationService
{
    // 事件定义
    event EventHandler<ScriptExecutionStartedEventArgs> ScriptExecutionStarted;
    event EventHandler<ScriptExecutionCompletedEventArgs> ScriptExecutionCompleted;
    event EventHandler<ScriptExecutionProgressEventArgs> ScriptExecutionProgress;
    event EventHandler<ErrorEventArgs> ErrorOccurred;
    
    // 脚本管理方法
    string CreateScript(AutomationScript script);
    bool SaveScript(AutomationScript script);
    AutomationScript LoadScript(string scriptId);
    List<AutomationScript> GetAllScripts();
    bool DeleteScript(string scriptId);
    bool UpdateScript(AutomationScript script);
    List<AutomationScript> SearchScripts(string keyword);
    
    // 脚本执行方法
    string ExecuteScriptAsync(string scriptId, Dictionary<string, object> parameters = null);
    bool StopScriptExecution(string executionId);
    bool PauseScriptExecution(string executionId);
    bool ResumeScriptExecution(string executionId);
    ScriptExecutionStatus GetScriptExecutionStatus(string executionId);
    TestReport GetExecutionResult(string executionId);
    
    // 任务管理方法
    string CreateTestTask(TestTask task);
    bool SaveTestTask(TestTask task);
    TestTask LoadTestTask(string taskId);
    List<TestTask> GetAllTestTasks();
    bool DeleteTestTask(string taskId);
    List<TestTask> SearchTasks(string keyword);
    
    // 任务执行方法
    string ExecuteTestTaskAsync(string taskId);
    bool StopTestTask(string taskId);
    bool PauseTestTask(string taskId);
    bool ResumeTestTask(string taskId);
    TestTaskStatus GetTestTaskStatus(string taskId);
    
    // 报告管理方法
    TestReport GenerateReport(string executionId);
    bool SaveReport(string reportId, string filePath);
    TestReport LoadReport(string reportId);
    List<TestReport> GetReportsByTaskId(string taskId);
    List<TestReport> GetReportsByDateRange(DateTime startDate, DateTime endDate);
    
    // 模板管理方法
    bool SaveTemplate(TestTemplate template);
    TestTemplate LoadTemplate(string templateId);
    List<TestTemplate> GetAllTemplates();
    bool DeleteTemplate(string templateId);
    AutomationScript CreateScriptFromTemplate(string templateId);
    
    // 变量管理
    Dictionary<string, object> GetScriptVariables(string scriptId);
    bool UpdateScriptVariables(string scriptId, Dictionary<string, object> variables);
}

// 事件参数类
public class ScriptExecutionStartedEventArgs : EventArgs
{
    public string ExecutionId { get; set; }
    public string ScriptId { get; set; }
    public DateTime StartTime { get; set; }
}

public class ScriptExecutionCompletedEventArgs : EventArgs
{
    public string ExecutionId { get; set; }
    public string ScriptId { get; set; }
    public bool Success { get; set; }
    public DateTime EndTime { get; set; }
    public TestReport Report { get; set; }
}

public class ScriptExecutionProgressEventArgs : EventArgs
{
    public string ExecutionId { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public double ProgressPercentage { get; set; }
    public string CurrentStepName { get; set; }
}

// 脚本执行状态类
public class ScriptExecutionStatus
{
    public string ExecutionId { get; set; }
    public string ScriptId { get; set; }
    public ExecutionStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int CurrentStep { get; set; }
    public int TotalSteps { get; set; }
    public string CurrentStepName { get; set; }
    public string ErrorMessage { get; set; }
}

// 测试任务状态类
public class TestTaskStatus
{
    public string TaskId { get; set; }
    public TaskStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public int CompletedScripts { get; set; }
    public int TotalScripts { get; set; }
    public string CurrentExecutionId { get; set; }
}
```

### 3.5 ILoggingService

```csharp
/// <summary>
/// 日志服务接口
/// </summary>
public interface ILoggingService
{
    // 日志记录方法
    void LogDebug(string message);
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
    void LogError(string message, Exception exception);
    void LogFatal(string message);
    void LogFatal(string message, Exception exception);
    
    // 日志配置方法
    void Configure(LoggingConfig config);
    LoggingConfig GetCurrentConfig();
    
    // 日志级别控制
    void SetLogLevel(LogLevel level);
    LogLevel GetLogLevel();
    bool IsEnabled(LogLevel level);
    
    // 日志查询方法
    List<LogEntry> QueryLogs(LogQuery query);
    List<LogEntry> GetRecentLogs(int count);
    List<LogEntry> GetLogsByDateRange(DateTime startDate, DateTime endDate);
    List<LogEntry> GetLogsByLevel(LogLevel level);
    
    // 日志导出方法
    bool ExportLogsToFile(string filePath, LogExportFormat format, LogQuery query = null);
    bool ExportAllLogs(string filePath, LogExportFormat format);
    
    // 日志清理方法
    bool ClearLogs();
    bool ClearOldLogs(DateTime olderThan);
    
    // 日志统计方法
    LogStatistics GetLogStatistics();
}

// 日志条目类
public class LogEntry
{
    public string Id { get; set; }
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Message { get; set; }
    public string Source { get; set; }
    public string Exception { get; set; }
    public string ThreadId { get; set; }
    public string ProcessId { get; set; }
}

// 日志查询类
public class LogQuery
{
    public LogLevel? MinimumLevel { get; set; }
    public LogLevel? MaximumLevel { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string MessageContains { get; set; }
    public string SourceContains { get; set; }
    public int? Limit { get; set; }
    public bool IncludeExceptions { get; set; }
}

// 日志统计类
public class LogStatistics
{
    public int TotalEntries { get; set; }
    public int DebugCount { get; set; }
    public int InfoCount { get; set; }
    public int WarningCount { get; set; }
    public int ErrorCount { get; set; }
    public int FatalCount { get; set; }
    public DateTime FirstLogEntry { get; set; }
    public DateTime LastLogEntry { get; set; }
    public Dictionary<string, int> SourceCounts { get; set; }
}

// 日志导出格式枚举
public enum LogExportFormat
{
    Text,
    CSV,
    XML,
    JSON,
    HTML
}
```

### 3.6 ILicenseService

```csharp
/// <summary>
/// 许可证管理服务接口
/// </summary>
public interface ILicenseService
{
    // 事件定义
    event EventHandler<LicenseStatusChangedEventArgs> LicenseStatusChanged;
    event EventHandler<ErrorEventArgs> ErrorOccurred;
    
    // 许可证激活方法
    Task<LicenseActivationResult> ActivateLicenseAsync(string licenseKey);
    Task<LicenseActivationResult> ActivateOfflineAsync(string licenseKey, string activationCode);
    
    // 许可证验证方法
    bool ValidateLicense();
    bool IsLicenseValid();
    LicenseValidationResult ValidateLicenseDetails();
    
    // 许可证信息方法
    LicenseInfo GetLicenseInfo();
    DateTime GetExpiryDate();
    LicenseType GetLicenseType();
    string GetLicenseKey();
    
    // 许可证管理方法
    bool DeactivateLicense();
    Task<bool> RefreshLicenseAsync();
    bool IsTrialLicense();
    int GetRemainingTrialDays();
    
    // 功能限制方法
    bool IsFeatureEnabled(string featureName);
    List<string> GetEnabledFeatures();
    List<string> GetDisabledFeatures();
    
    // 许可证导入导出方法
    bool ExportLicense(string filePath);
    Task<LicenseActivationResult> ImportLicenseAsync(string filePath);
    
    // 系统绑定方法
    string GenerateHardwareFingerprint();
    bool IsLicenseBoundToCurrentSystem();
}

// 许可证激活结果类
public class LicenseActivationResult
{
    public bool Success { get; set; }
    public string ActivationCode { get; set; }
    public string Message { get; set; }
    public LicenseInfo LicenseInfo { get; set; }
}

// 许可证验证结果类
public class LicenseValidationResult
{
    public bool IsValid { get; set; }
    public ValidationStatus Status { get; set; }
    public string Message { get; set; }
    public DateTime ValidUntil { get; set; }
    public List<string> Issues { get; set; }
}

// 验证状态枚举
public enum ValidationStatus
{
    Valid,
    Invalid,
    Expired,
    NotActivated,
    HardwareMismatch,
    Tampered
}

// 许可证状态变更事件参数
public class LicenseStatusChangedEventArgs : EventArgs
{
    public bool IsValid { get; set; }
    public LicenseType LicenseType { get; set; }
    public DateTime ExpiryDate { get; set; }
}
```

## 4. 服务间依赖关系

### 4.1 依赖关系图

```
IAutomationService --> ICanService
IAutomationService --> IDbcService
IAutomationService --> IObdService
IAutomationService --> ILoggingService

IObdService --> ICanService
IObdService --> ILoggingService

ICanService --> ILoggingService
IDbcService --> ILoggingService

所有服务 --> ILicenseService (功能限制检查)
```

### 4.2 依赖注入配置

```csharp
// 依赖注入配置示例
public class ServiceModule : IModule
{
    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        // 注册服务接口和实现
        containerRegistry.RegisterSingleton<ICanService, ZLGUsbCanService>();
        containerRegistry.RegisterSingleton<IDbcService, DbcService>();
        containerRegistry.RegisterSingleton<IObdService, ObdService>();
        containerRegistry.RegisterSingleton<IAutomationService, AutomationService>();
        containerRegistry.RegisterSingleton<ILoggingService, LoggingService>();
        containerRegistry.RegisterSingleton<ILicenseService, LicenseService>();
        
        // 注册服务工厂
        containerRegistry.RegisterSingleton<ICanServiceFactory, CanServiceFactory>();
    }
}
```

## 5. 接口使用示例

### 5.1 自动化服务使用CAN服务示例

```csharp
public class AutomationService : IAutomationService
{
    private readonly ICanService _canService;
    private readonly IDbcService _dbcService;
    private readonly ILoggingService _loggingService;
    
    // 构造函数注入
    public AutomationService(ICanService canService, IDbcService dbcService, ILoggingService loggingService)
    {
        _canService = canService;
        _dbcService = dbcService;
        _loggingService = loggingService;
    }
    
    // 执行发送CAN报文步骤
    private async Task<bool> ExecuteSendCanMessageStep(TestStep step)
    {
        try
        {
            // 从步骤参数中获取配置
            int channel = (int)step.Parameters["Channel"];
            uint messageId = (uint)step.Parameters["MessageId"];
            byte[] data = (byte[])step.Parameters["Data"];
            
            // 创建CAN报文
            var message = new CanMessage
            {
                Id = messageId,
                Data = data,
                DLC = (byte)data.Length,
                Timestamp = DateTime.Now
            };
            
            // 使用CAN服务发送报文
            bool success = await _canService.SendMessageAsync(channel, message);
            
            _loggingService.LogInfo($"发送CAN报文: ID=0x{messageId:X}, 通道={channel}, 结果={success}");
            
            return success;
        }
        catch (Exception ex)
        {
            _loggingService.LogError("执行CAN发送步骤失败", ex);
            throw;
        }
    }
}
```

### 5.2 OBD服务使用CAN服务示例

```csharp
public class ObdService : IObdService
{
    private readonly ICanService _canService;
    private readonly ILoggingService _loggingService;
    private ObdSession _currentSession;
    
    // 构造函数注入
    public ObdService(ICanService canService, ILoggingService loggingService)
    {
        _canService = canService;
        _loggingService = loggingService;
        _currentSession = new ObdSession();
        
        // 注册CAN消息接收事件
        _canService.MessageReceived += OnCanMessageReceived;
    }
    
    // 发送OBD请求
    public async Task<ObdResponse> SendRequestAsync(ObdRequest request)
    {
        try
        {
            // 将OBD请求转换为CAN报文
            var canMessage = ConvertToCanMessage(request);
            
            // 发送CAN报文
            bool success = await _canService.SendMessageAsync(_currentSession.Configuration["Channel"] as int? ?? 0, canMessage);
            
            if (!success)
            {
                throw new Exception("发送OBD请求失败");
            }
            
            // 等待响应（简化版）
            // 实际实现中应该有超时和重试机制
            return await WaitForResponseAsync(request.RequestId);
        }
        catch (Exception ex)
        {
            _loggingService.LogError("发送OBD请求失败", ex);
            throw;
        }
    }
    
    // 处理接收到的CAN消息
    private void OnCanMessageReceived(object sender, CanMessage message)
    {
        // 将CAN报文转换为OBD响应
        var obdResponse = ConvertToObdResponse(message);
        if (obdResponse != null)
        {
            // 触发响应接收事件
            ResponseReceived?.Invoke(this, obdResponse);
        }
    }
}
```

## 6. 性能优化考虑

- 使用异步接口避免阻塞
- 实现服务缓存机制
- 合理设计事件通知机制
- 使用对象池管理频繁创建的对象
- 实现服务调用超时和重试机制