# 服务层与数据层接口设计文档

## 1. 接口概述

本文档定义了服务层与数据层之间的交互接口，确保服务层能够通过统一的接口访问和操作数据，实现数据访问的抽象和持久化策略的灵活替换。

## 2. 接口设计原则

- **仓储模式**：使用仓储模式封装数据访问逻辑
- **单一职责**：每个接口专注于特定实体的数据访问
- **事务支持**：提供事务管理能力
- **异步操作**：支持异步数据访问提高性能
- **缓存支持**：提供数据缓存机制
- **查询优化**：支持复杂查询和过滤

## 3. 核心数据访问接口

### 3.1 IRepository<T>

```csharp
/// <summary>
/// 通用仓储接口
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public interface IRepository<T> where T : class, IEntity
{
    // 基本CRUD操作
    Task<T> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<IEnumerable<T>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<T> AddAsync(T entity);
    Task<IEnumerable<T>> AddRangeAsync(IEnumerable<T> entities);
    Task<T> UpdateAsync(T entity);
    Task<IEnumerable<T>> UpdateRangeAsync(IEnumerable<T> entities);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteAsync(T entity);
    Task<int> DeleteRangeAsync(IEnumerable<Guid> ids);
    Task<int> DeleteRangeAsync(IEnumerable<T> entities);
    
    // 查询方法
    IQueryable<T> Query();
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<T> FindFirstAsync(Expression<Func<T, bool>> predicate);
    Task<T> FindSingleAsync(Expression<Func<T, bool>> predicate);
    
    // 统计方法
    Task<int> CountAsync();
    Task<int> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AnyAsync();
    Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
    
    // 批量操作
    Task<int> SaveChangesAsync();
    
    // 缓存操作
    void InvalidateCache(Guid id);
    void InvalidateAllCache();
}

/// <summary>
/// 实体接口
/// </summary>
public interface IEntity
{
    /// <summary>
    /// 实体ID
    /// </summary>
    Guid Id { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    DateTime CreatedTime { get; set; }
    
    /// <summary>
    /// 修改时间
    /// </summary>
    DateTime ModifiedTime { get; set; }
    
    /// <summary>
    /// 是否删除
    /// </summary>
    bool IsDeleted { get; set; }
}
```

### 3.2 ICanMessageRepository

```csharp
/// <summary>
/// CAN报文仓储接口
/// </summary>
public interface ICanMessageRepository : IRepository<CanMessageEntity>
{
    // 特定查询方法
    Task<IEnumerable<CanMessageEntity>> GetMessagesByTimeRangeAsync(DateTime startTime, DateTime endTime);
    Task<IEnumerable<CanMessageEntity>> GetMessagesByIdAsync(uint messageId, int limit = 1000);
    Task<IEnumerable<CanMessageEntity>> GetMessagesByChannelAsync(int channelId, int limit = 1000);
    Task<IEnumerable<CanMessageEntity>> SearchMessagesAsync(string keyword, int limit = 1000);
    
    // 批量操作
    Task<int> BulkInsertAsync(IEnumerable<CanMessageEntity> messages);
    Task<int> BulkDeleteAsync(DateTime olderThan);
    
    // 统计分析
    Task<Dictionary<uint, int>> GetMessageFrequencyAsync(DateTime startTime, DateTime endTime);
    Task<List<MessageStatistics>> GetMessageStatisticsAsync(DateTime startTime, DateTime endTime);
    
    // 导出功能
    Task<bool> ExportToFileAsync(string filePath, ExportFormat format, DateTime startTime, DateTime endTime);
}

/// <summary>
/// CAN报文实体类
/// </summary>
public class CanMessageEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public uint MessageId { get; set; }
    public string MessageName { get; set; }
    public byte[] Data { get; set; }
    public byte DLC { get; set; }
    public bool IsExtendedFrame { get; set; }
    public int ChannelId { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, double> SignalValues { get; set; }
    public CanMessageType MessageType { get; set; }
}

/// <summary>
/// 报文统计类
/// </summary>
public class MessageStatistics
{
    public uint MessageId { get; set; }
    public string MessageName { get; set; }
    public int Count { get; set; }
    public DateTime FirstSeen { get; set; }
    public DateTime LastSeen { get; set; }
    public double AverageIntervalMs { get; set; }
}

/// <summary>
/// 导出格式枚举
/// </summary>
public enum ExportFormat
{
    CSV,
    JSON,
    XML,
    ASC,
    BLF
}
```

### 3.3 IDbcFileRepository

```csharp
/// <summary>
/// DBC文件仓储接口
/// </summary>
public interface IDbcFileRepository : IRepository<DbcFileEntity>
{
    // 文件操作方法
    Task<DbcFileEntity> GetByFilePathAsync(string filePath);
    Task<DbcFileEntity> GetByFileNameAsync(string fileName);
    Task<IEnumerable<DbcFileEntity>> GetRecentlyUsedAsync(int count = 5);
    
    // 内容查询方法
    Task<DbcMessageEntity> GetMessageByIdAsync(Guid dbcFileId, uint messageId);
    Task<DbcSignalEntity> GetSignalByNameAsync(Guid dbcFileId, string signalName);
    Task<List<DbcMessageEntity>> GetMessagesByDbcFileIdAsync(Guid dbcFileId);
    Task<List<DbcSignalEntity>> GetSignalsByDbcFileIdAsync(Guid dbcFileId);
    Task<List<DbcNodeEntity>> GetNodesByDbcFileIdAsync(Guid dbcFileId);
    
    // 缓存管理
    Task<DbcFileEntity> GetCachedDbcFileAsync(Guid dbcFileId);
    void CacheDbcFile(DbcFileEntity dbcFile);
    void ClearDbcFileCache(Guid dbcFileId);
    
    // 导入导出
    Task<DbcFileEntity> ImportDbcFileAsync(string filePath);
    Task<bool> ExportDbcFileAsync(Guid dbcFileId, string filePath);
}

/// <summary>
/// DBC文件实体类
/// </summary>
public class DbcFileEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string Version { get; set; }
    public string Description { get; set; }
    public DateTime LastAccessedTime { get; set; }
    public long FileSize { get; set; }
    
    // 导航属性
    public virtual List<DbcMessageEntity> Messages { get; set; }
    public virtual List<DbcNodeEntity> Nodes { get; set; }
}

/// <summary>
/// DBC报文实体类
/// </summary>
public class DbcMessageEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public Guid DbcFileId { get; set; }
    public uint MessageId { get; set; }
    public string Name { get; set; }
    public byte DLC { get; set; }
    public string Transmitter { get; set; }
    public int CycleTime { get; set; }
    public string Comment { get; set; }
    
    // 导航属性
    public virtual DbcFileEntity DbcFile { get; set; }
    public virtual List<DbcSignalEntity> Signals { get; set; }
    public virtual List<string> Receivers { get; set; }
}

/// <summary>
/// DBC信号实体类
/// </summary>
public class DbcSignalEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public Guid DbcMessageId { get; set; }
    public string Name { get; set; }
    public int StartBit { get; set; }
    public int Length { get; set; }
    public bool IsLittleEndian { get; set; }
    public bool IsSigned { get; set; }
    public double Factor { get; set; }
    public double Offset { get; set; }
    public double Minimum { get; set; }
    public double Maximum { get; set; }
    public string Unit { get; set; }
    public string Comment { get; set; }
    
    // 导航属性
    public virtual DbcMessageEntity Message { get; set; }
    public virtual List<string> Receivers { get; set; }
    public virtual Dictionary<long, string> ValueTable { get; set; }
}

/// <summary>
/// DBC节点实体类
/// </summary>
public class DbcNodeEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public Guid DbcFileId { get; set; }
    public string Name { get; set; }
    public int Id { get; set; }
    public string Comment { get; set; }
    
    // 导航属性
    public virtual DbcFileEntity DbcFile { get; set; }
}
```

### 3.4 IScriptRepository

```csharp
/// <summary>
/// 脚本仓储接口
/// </summary>
public interface IScriptRepository : IRepository<ScriptEntity>
{
    // 脚本查询方法
    Task<IEnumerable<ScriptEntity>> GetByTypeAsync(ScriptType type);
    Task<IEnumerable<ScriptEntity>> GetByCreatorAsync(string creator);
    Task<IEnumerable<ScriptEntity>> GetByTagsAsync(IEnumerable<string> tags);
    Task<IEnumerable<ScriptEntity>> SearchScriptsAsync(string keyword);
    Task<IEnumerable<ScriptEntity>> GetRecentlyModifiedAsync(int count = 10);
    
    // 版本控制
    Task<ScriptVersionEntity> GetScriptVersionAsync(Guid scriptId, string version);
    Task<IEnumerable<ScriptVersionEntity>> GetScriptVersionsAsync(Guid scriptId);
    Task<ScriptVersionEntity> SaveScriptVersionAsync(ScriptVersionEntity version);
    
    // 脚本内容管理
    Task<string> GetScriptContentAsync(Guid scriptId);
    Task<bool> UpdateScriptContentAsync(Guid scriptId, string content);
    
    // 模板管理
    Task<IEnumerable<ScriptEntity>> GetTemplatesAsync();
    Task<bool> IsTemplateAsync(Guid scriptId);
    
    // 导入导出
    Task<ScriptEntity> ImportScriptAsync(string filePath);
    Task<bool> ExportScriptAsync(Guid scriptId, string filePath);
}

/// <summary>
/// 脚本实体类
/// </summary>
public class ScriptEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public string Name { get; set; }
    public string Description { get; set; }
    public string Content { get; set; }
    public ScriptType Type { get; set; }
    public string Creator { get; set; }
    public string Version { get; set; }
    public bool IsTemplate { get; set; }
    public List<string> Tags { get; set; }
    public int ExecutionCount { get; set; }
    public DateTime LastExecutionTime { get; set; }
    
    // 导航属性
    public virtual List<ScriptVersionEntity> Versions { get; set; }
    public virtual List<ScriptParameterEntity> Parameters { get; set; }
}

/// <summary>
/// 脚本版本实体类
/// </summary>
public class ScriptVersionEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public Guid ScriptId { get; set; }
    public string Version { get; set; }
    public string Content { get; set; }
    public string ChangeLog { get; set; }
    public string CreatedBy { get; set; }
    
    // 导航属性
    public virtual ScriptEntity Script { get; set; }
}

/// <summary>
/// 脚本参数实体类
/// </summary>
public class ScriptParameterEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public Guid ScriptId { get; set; }
    public string Name { get; set; }
    public VariableType Type { get; set; }
    public string DefaultValue { get; set; }
    public string Description { get; set; }
    public bool IsRequired { get; set; }
    
    // 导航属性
    public virtual ScriptEntity Script { get; set; }
}
```

### 3.5 ITestRepository

```csharp
/// <summary>
/// 测试仓储接口
/// </summary>
public interface ITestRepository : IRepository<TestTaskEntity>
{
    // 任务查询方法
    Task<IEnumerable<TestTaskEntity>> GetByStatusAsync(TaskStatus status);
    Task<IEnumerable<TestTaskEntity>> GetByCreatorAsync(string creator);
    Task<IEnumerable<TestTaskEntity>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<IEnumerable<TestTaskEntity>> GetRunningTasksAsync();
    
    // 报告管理
    Task<TestReportEntity> GetReportByIdAsync(Guid reportId);
    Task<IEnumerable<TestReportEntity>> GetReportsByTaskIdAsync(Guid taskId);
    Task<IEnumerable<TestReportEntity>> GetReportsByResultAsync(TestResult result);
    Task<IEnumerable<TestReportEntity>> GetReportsByDateRangeAsync(DateTime startDate, DateTime endDate);
    
    // 执行历史
    Task<TestExecutionEntity> GetExecutionByIdAsync(Guid executionId);
    Task<IEnumerable<TestExecutionEntity>> GetExecutionsByTaskIdAsync(Guid taskId);
    Task<IEnumerable<TestExecutionEntity>> GetExecutionsByScriptIdAsync(Guid scriptId);
    
    // 统计分析
    Task<TestStatistics> GetTestStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<List<DailyTestSummary>> GetDailyTestSummariesAsync(DateTime startDate, DateTime endDate);
    
    // 报告导出
    Task<bool> ExportReportAsync(Guid reportId, string filePath, ReportFormat format);
}

/// <summary>
/// 测试任务实体类
/// </summary>
public class TestTaskEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public string Name { get; set; }
    public string Description { get; set; }
    public TaskStatus Status { get; set; }
    public ExecutionMode Mode { get; set; }
    public string Creator { get; set; }
    public int Priority { get; set; }
    public DateTime? ScheduledTime { get; set; }
    
    // 导航属性
    public virtual List<TestTaskScriptEntity> TaskScripts { get; set; }
    public virtual List<TestExecutionEntity> Executions { get; set; }
    public virtual Dictionary<string, string> Parameters { get; set; }
}

/// <summary>
/// 任务脚本关联实体类
/// </summary>
public class TestTaskScriptEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public Guid TaskId { get; set; }
    public Guid ScriptId { get; set; }
    public int ExecutionOrder { get; set; }
    
    // 导航属性
    public virtual TestTaskEntity Task { get; set; }
    public virtual ScriptEntity Script { get; set; }
}

/// <summary>
/// 测试报告实体类
/// </summary>
public class TestReportEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public Guid TaskId { get; set; }
    public Guid ExecutionId { get; set; }
    public string Name { get; set; }
    public TestResult Result { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public long DurationMs { get; set; }
    public int TotalSteps { get; set; }
    public int PassedSteps { get; set; }
    public int FailedSteps { get; set; }
    public int SkippedSteps { get; set; }
    
    // 导航属性
    public virtual TestTaskEntity Task { get; set; }
    public virtual TestExecutionEntity Execution { get; set; }
    public virtual List<TestStepResultEntity> StepResults { get; set; }
    public virtual List<string> ErrorMessages { get; set; }
    public virtual Dictionary<string, string> EnvironmentInfo { get; set; }
}

/// <summary>
/// 测试执行实体类
/// </summary>
public class TestExecutionEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public Guid TaskId { get; set; }
    public string ExecutedBy { get; set; }
    public ExecutionStatus Status { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public long DurationMs { get; set; }
    public string Logs { get; set; }
    
    // 导航属性
    public virtual TestTaskEntity Task { get; set; }
    public virtual TestReportEntity Report { get; set; }
}

/// <summary>
/// 测试统计类
/// </summary>
public class TestStatistics
{
    public int TotalTasks { get; set; }
    public int TotalExecutions { get; set; }
    public int PassedExecutions { get; set; }
    public int FailedExecutions { get; set; }
    public int RunningExecutions { get; set; }
    public double PassRate { get; set; }
    public double AverageExecutionTimeMs { get; set; }
    public Dictionary<string, int> TaskTypeCounts { get; set; }
}

/// <summary>
/// 每日测试摘要类
/// </summary>
public class DailyTestSummary
{
    public DateTime Date { get; set; }
    public int Executions { get; set; }
    public int Passed { get; set; }
    public int Failed { get; set; }
    public double DailyPassRate { get; set; }
    public long TotalDurationMs { get; set; }
}

/// <summary>
/// 报告格式枚举
/// </summary>
public enum ReportFormat
{
    PDF,
    HTML,
    XML,
    JSON,
    CSV
}
```

### 3.6 IConfigurationRepository

```csharp
/// <summary>
/// 配置仓储接口
/// </summary>
public interface IConfigurationRepository : IRepository<ConfigurationEntity>
{
    // 配置管理方法
    Task<ConfigurationEntity> GetByKeyAsync(string key);
    Task<Dictionary<string, string>> GetAllConfigurationsAsync();
    Task<bool> SetConfigurationAsync(string key, string value);
    Task<bool> SetConfigurationsAsync(Dictionary<string, string> configurations);
    Task<bool> RemoveConfigurationAsync(string key);
    
    // 用户配置
    Task<UserConfigurationEntity> GetUserConfigurationAsync(string userId);
    Task<bool> SaveUserConfigurationAsync(UserConfigurationEntity configuration);
    
    // 系统设置
    Task<SystemSettingsEntity> GetSystemSettingsAsync();
    Task<bool> SaveSystemSettingsAsync(SystemSettingsEntity settings);
    Task<bool> ResetSystemSettingsAsync();
    
    // 导入导出
    Task<bool> ExportConfigurationsAsync(string filePath);
    Task<bool> ImportConfigurationsAsync(string filePath);
}

/// <summary>
/// 配置实体类
/// </summary>
public class ConfigurationEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public string Key { get; set; }
    public string Value { get; set; }
    public string Description { get; set; }
    public bool IsEncrypted { get; set; }
    public ConfigurationScope Scope { get; set; }
}

/// <summary>
/// 用户配置实体类
/// </summary>
public class UserConfigurationEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Language { get; set; }
    public string Theme { get; set; }
    public Dictionary<string, string> UserPreferences { get; set; }
}

/// <summary>
/// 系统设置实体类
/// </summary>
public class SystemSettingsEntity : IEntity
{
    public Guid Id { get; set; }
    public DateTime CreatedTime { get; set; }
    public DateTime ModifiedTime { get; set; }
    public bool IsDeleted { get; set; }
    
    // 业务属性
    public string ApplicationVersion { get; set; }
    public string DefaultLanguage { get; set; }
    public string DefaultTheme { get; set; }
    public int MaxLogFileSize { get; set; }
    public int MaxRecentFiles { get; set; }
    public bool AutoCheckUpdates { get; set; }
    public bool EnableTelemetry { get; set; }
    public Dictionary<string, string> AdvancedSettings { get; set; }
}

/// <summary>
/// 配置作用域枚举
/// </summary>
public enum ConfigurationScope
{
    System,
    User,
    Application,
    Module
}
```

## 4. 数据访问实现策略

### 4.1 仓储实现基类

```csharp
/// <summary>
/// 仓储实现基类
/// </summary>
/// <typeparam name="T">实体类型</typeparam>
public abstract class RepositoryBase<T> : IRepository<T> where T : class, IEntity
{
    protected readonly IDbContext _dbContext;
    protected readonly IMemoryCache _cache;
    protected readonly ILoggingService _loggingService;
    
    public RepositoryBase(IDbContext dbContext, IMemoryCache cache, ILoggingService loggingService)
    {
        _dbContext = dbContext;
        _cache = cache;
        _loggingService = loggingService;
    }
    
    // 实现通用仓储方法...
    
    // 缓存键生成
    protected virtual string GenerateCacheKey(Guid id)
    {
        return $"{typeof(T).Name}:{id}";
    }
    
    // 异步缓存操作
    protected async Task<T> GetFromCacheAsync(Guid id)
    {
        var cacheKey = GenerateCacheKey(id);
        return await _cache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.SlidingExpiration = TimeSpan.FromMinutes(5);
            return await _dbContext.Set<T>().FindAsync(id);
        });
    }
}
```

### 4.2 数据库上下文接口

```csharp
/// <summary>
/// 数据库上下文接口
/// </summary>
public interface IDbContext : IDisposable
{
    // 实体集访问
    DbSet<T> Set<T>() where T : class;
    
    // 事务管理
    IDbContextTransaction BeginTransaction();
    Task<IDbContextTransaction> BeginTransactionAsync();
    void CommitTransaction();
    void RollbackTransaction();
    
    // 保存更改
    int SaveChanges();
    Task<int> SaveChangesAsync();
    
    // 数据库操作
    bool DatabaseExists();
    Task<bool> DatabaseExistsAsync();
    void CreateDatabase();
    Task CreateDatabaseAsync();
    void DeleteDatabase();
    Task DeleteDatabaseAsync();
    
    // 执行SQL
    int ExecuteSqlCommand(string sql, params object[] parameters);
    Task<int> ExecuteSqlCommandAsync(string sql, params object[] parameters);
    IQueryable<T> FromSql<T>(string sql, params object[] parameters) where T : class;
}
```

## 5. 接口使用示例

### 5.1 服务层使用仓储示例

```csharp
public class AutomationService : IAutomationService
{
    private readonly IScriptRepository _scriptRepository;
    private readonly ITestRepository _testRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILoggingService _loggingService;
    
    // 构造函数注入
    public AutomationService(
        IScriptRepository scriptRepository,
        ITestRepository testRepository,
        IUnitOfWork unitOfWork,
        ILoggingService loggingService)
    {
        _scriptRepository = scriptRepository;
        _testRepository = testRepository;
        _unitOfWork = unitOfWork;
        _loggingService = loggingService;
    }
    
    // 创建并保存测试脚本
    public async Task<string> CreateAndSaveScriptAsync(AutomationScript script)
    {
        try
        {
            // 开始事务
            using (var transaction = _unitOfWork.BeginTransaction())
            {
                try
                {
                    // 转换为实体
                    var scriptEntity = ConvertToEntity(script);
                    
                    // 保存脚本
                    var savedScript = await _scriptRepository.AddAsync(scriptEntity);
                    
                    // 如果有参数，保存参数
                    if (script.Variables != null && script.Variables.Any())
                    {
                        foreach (var variable in script.Variables)
                        {
                            var parameterEntity = new ScriptParameterEntity
                            {
                                ScriptId = savedScript.Id,
                                Name = variable.Name,
                                Type = variable.Type,
                                DefaultValue = variable.InitialValue?.ToString(),
                                Description = variable.Description,
                                IsRequired = variable.IsInputParameter
                            };
                            
                            await _unitOfWork.Set<ScriptParameterEntity>().AddAsync(parameterEntity);
                        }
                    }
                    
                    // 保存更改
                    await _unitOfWork.SaveChangesAsync();
                    
                    // 提交事务
                    transaction.Commit();
                    
                    return savedScript.Id.ToString();
                }
                catch (Exception ex)
                {
                    // 回滚事务
                    transaction.Rollback();
                    _loggingService.LogError("创建脚本失败", ex);
                    throw;
                }
            }
        }
        catch (Exception ex)
        {
            _loggingService.LogError("创建并保存脚本时发生错误", ex);
            throw new ServiceException("创建脚本失败", ex);
        }
    }
    
    // 转换模型到实体
    private ScriptEntity ConvertToEntity(AutomationScript script)
    {
        return new ScriptEntity
        {
            Id = string.IsNullOrEmpty(script.ScriptId) ? Guid.NewGuid() : Guid.Parse(script.ScriptId),
            Name = script.Name,
            Description = script.Description,
            Content = script.Content,
            Type = script.Type,
            Creator = script.Creator,
            Version = script.Version,
            Tags = script.Tags,
            CreatedTime = script.CreatedTime,
            ModifiedTime = script.ModifiedTime
        };
    }
}
```

### 5.2 单元工作模式示例

```csharp
/// <summary>
/// 单元工作接口
/// </summary>
public interface IUnitOfWork : IDisposable
{
    // 获取仓储
    IRepository<T> GetRepository<T>() where T : class, IEntity;
    IScriptRepository ScriptRepository { get; }
    ITestRepository TestRepository { get; }
    ICanMessageRepository CanMessageRepository { get; }
    IDbcFileRepository DbcFileRepository { get; }
    IConfigurationRepository ConfigurationRepository { get; }
    
    // 事务管理
    IDbContextTransaction BeginTransaction();
    Task<IDbContextTransaction> BeginTransactionAsync();
    void Commit();
    void Rollback();
    
    // 保存更改
    int SaveChanges();
    Task<int> SaveChangesAsync();
    
    // 实体集访问
    DbSet<T> Set<T>() where T : class;
}

// 使用示例
public async Task ProcessTestResultsAsync(List<TestResult> results)
{
    using (var unitOfWork = _unitOfWorkFactory.Create())
    {
        using (var transaction = await unitOfWork.BeginTransactionAsync())
        {
            try
            {
                foreach (var result in results)
                {
                    var resultEntity = ConvertToEntity(result);
                    await unitOfWork.TestRepository.AddAsync(resultEntity);
                }
                
                await unitOfWork.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _loggingService.LogError("处理测试结果失败", ex);
                throw;
            }
        }
    }
}
```

## 6. 性能优化考虑

- 实现二级缓存机制（内存缓存 + 分布式缓存）
- 使用异步方法避免阻塞
- 实现批量操作减少数据库往返
- 使用延迟加载和预先加载优化导航属性访问
- 实现查询优化和索引策略
- 使用连接池管理数据库连接
- 实现数据分片策略处理大数据量