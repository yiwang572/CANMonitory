# OBD诊断数据模型设计文档

## 1. 模型概述

OBD诊断数据模型包含与OBD诊断协议相关的核心数据结构，用于表示诊断请求、响应、会话状态等信息，是系统中OBD诊断功能的基础数据表示。

## 2. 主要数据模型

### 2.1 ObdRequest

```csharp
/// <summary>
/// OBD诊断请求模型
/// </summary>
public class ObdRequest
{
    /// <summary>
    /// 请求ID
    /// </summary>
    public string RequestId { get; set; }
    
    /// <summary>
    /// 服务ID
    /// </summary>
    public byte ServiceId { get; set; }
    
    /// <summary>
    /// 请求数据
    /// </summary>
    public byte[] Data { get; set; }
    
    /// <summary>
    /// 请求描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int TimeoutMs { get; set; }
    
    /// <summary>
    /// 是否等待响应
    /// </summary>
    public bool WaitForResponse { get; set; }
    
    /// <summary>
    /// 请求时间
    /// </summary>
    public DateTime RequestTime { get; set; }
    
    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ObdRequest()
    {
        RequestId = Guid.NewGuid().ToString();
        TimeoutMs = 1000;
        WaitForResponse = true;
        RequestTime = DateTime.Now;
    }
    
    /// <summary>
    /// 格式化请求为字符串
    /// </summary>
    public override string ToString()
    {
        return $"0x{ServiceId:X2} {BitConverter.ToString(Data ?? new byte[0]).Replace("-", " ")}";
    }
}
```

### 2.2 ObdResponse

```csharp
/// <summary>
/// OBD诊断响应模型
/// </summary>
public class ObdResponse
{
    /// <summary>
    /// 响应ID
    /// </summary>
    public string ResponseId { get; set; }
    
    /// <summary>
    /// 对应的请求ID
    /// </summary>
    public string RequestId { get; set; }
    
    /// <summary>
    /// 服务ID
    /// </summary>
    public byte ServiceId { get; set; }
    
    /// <summary>
    /// 响应代码
    /// </summary>
    public byte ResponseCode { get; set; }
    
    /// <summary>
    /// 响应数据
    /// </summary>
    public byte[] Data { get; set; }
    
    /// <summary>
    /// 响应时间
    /// </summary>
    public DateTime ResponseTime { get; set; }
    
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool IsSuccessful { get; set; }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ObdResponse()
    {
        ResponseId = Guid.NewGuid().ToString();
        ResponseTime = DateTime.Now;
    }
    
    /// <summary>
    /// 格式化响应为字符串
    /// </summary>
    public override string ToString()
    {
        return $"0x{ServiceId:X2} 0x{ResponseCode:X2} {BitConverter.ToString(Data ?? new byte[0]).Replace("-", " ")}";
    }
}
```

### 2.3 ObdSession

```csharp
/// <summary>
/// OBD诊断会话模型
/// </summary>
public class ObdSession
{
    /// <summary>
    /// 会话ID
    /// </summary>
    public string SessionId { get; set; }
    
    /// <summary>
    /// 当前会话类型
    /// </summary>
    public byte CurrentSessionType { get; set; }
    
    /// <summary>
    /// 安全访问级别
    /// </summary>
    public int SecurityAccessLevel { get; set; }
    
    /// <summary>
    /// 是否安全访问已认证
    /// </summary>
    public bool IsSecurityAccessGranted { get; set; }
    
    /// <summary>
    /// 会话开始时间
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// 最后通信时间
    /// </summary>
    public DateTime LastCommunicateTime { get; set; }
    
    /// <summary>
    /// 活跃标志
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// 请求历史
    /// </summary>
    public List<ObdRequest> RequestHistory { get; set; }
    
    /// <summary>
    /// 响应历史
    /// </summary>
    public List<ObdResponse> ResponseHistory { get; set; }
    
    /// <summary>
    /// 配置参数
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ObdSession()
    {
        SessionId = Guid.NewGuid().ToString();
        CurrentSessionType = 0x01; // 默认会话
        SecurityAccessLevel = 0;
        IsSecurityAccessGranted = false;
        StartTime = DateTime.Now;
        LastCommunicateTime = DateTime.Now;
        IsActive = true;
        RequestHistory = new List<ObdRequest>();
        ResponseHistory = new List<ObdResponse>();
        Configuration = new Dictionary<string, object>();
    }
    
    /// <summary>
    /// 更新会话时间
    /// </summary>
    public void UpdateSessionTime()
    {
        LastCommunicateTime = DateTime.Now;
    }
    
    /// <summary>
    /// 重置安全访问状态
    /// </summary>
    public void ResetSecurityAccess()
    {
        SecurityAccessLevel = 0;
        IsSecurityAccessGranted = false;
    }
}
```

### 2.4 ObdService

```csharp
/// <summary>
/// OBD服务定义模型
/// </summary>
public class ObdService
{
    /// <summary>
    /// 服务ID
    /// </summary>
    public byte ServiceId { get; set; }
    
    /// <summary>
    /// 服务名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 服务描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 支持的子功能列表
    /// </summary>
    public List<ObdSubFunction> SubFunctions { get; set; }
    
    /// <summary>
    /// 适用会话类型
    /// </summary>
    public List<byte> ApplicableSessions { get; set; }
    
    /// <summary>
    /// 是否需要安全访问
    /// </summary>
    public bool RequiresSecurityAccess { get; set; }
    
    /// <summary>
    /// 所需安全访问级别
    /// </summary>
    public int RequiredSecurityLevel { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ObdService()
    {
        SubFunctions = new List<ObdSubFunction>();
        ApplicableSessions = new List<byte>();
    }
}

/// <summary>
/// OBD子功能模型
/// </summary>
public class ObdSubFunction
{
    /// <summary>
    /// 子功能ID
    /// </summary>
    public byte SubFunctionId { get; set; }
    
    /// <summary>
    /// 子功能名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 子功能描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 是否支持肯定响应
    /// </summary>
    public bool SupportPositiveResponse { get; set; }
}
```

## 3. 辅助数据模型

### 3.1 ObdDiagnosticTroubleCode

```csharp
/// <summary>
/// OBD故障码模型
/// </summary>
public class ObdDiagnosticTroubleCode
{
    /// <summary>
    /// 故障码
    /// </summary>
    public string Code { get; set; }
    
    /// <summary>
    /// 故障码类型
    /// </summary>
    public DtcType Type { get; set; }
    
    /// <summary>
    /// 故障描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 是否为当前故障
    /// </summary>
    public bool IsCurrent { get; set; }
    
    /// <summary>
    /// 是否为历史故障
    /// </summary>
    public bool IsHistory { get; set; }
    
    /// <summary>
    /// 出现次数
    /// </summary>
    public int Count { get; set; }
    
    /// <summary>
    /// 发生时间
    /// </summary>
    public DateTime OccurrenceTime { get; set; }
    
    /// <summary>
    /// 相关冻结帧数据
    /// </summary>
    public Dictionary<string, double> FreezeFrameData { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ObdDiagnosticTroubleCode()
    {
        FreezeFrameData = new Dictionary<string, double>();
    }
}

/// <summary>
/// 故障码类型枚举
/// </summary>
public enum DtcType
{
    P = 0, // 动力系统
    C = 1, // 底盘系统
    B = 2, // 车身系统
    U = 3  // 网络系统
}
```

### 3.2 ObdScript

```csharp
/// <summary>
/// OBD诊断脚本模型
/// </summary>
public class ObdScript
{
    /// <summary>
    /// 脚本ID
    /// </summary>
    public string ScriptId { get; set; }
    
    /// <summary>
    /// 脚本名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 脚本描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 脚本内容
    /// </summary>
    public string Content { get; set; }
    
    /// <summary>
    /// 脚本类型
    /// </summary>
    public ScriptType Type { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
    
    /// <summary>
    /// 修改时间
    /// </summary>
    public DateTime ModifiedTime { get; set; }
    
    /// <summary>
    /// 创建者
    /// </summary>
    public string Creator { get; set; }
    
    /// <summary>
    /// 版本
    /// </summary>
    public string Version { get; set; }
    
    /// <summary>
    /// 步骤列表（用于无代码脚本）
    /// </summary>
    public List<ScriptStep> Steps { get; set; }
    
    /// <summary>
    /// 变量列表
    /// </summary>
    public List<ScriptVariable> Variables { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ObdScript()
    {
        ScriptId = Guid.NewGuid().ToString();
        CreatedTime = DateTime.Now;
        ModifiedTime = DateTime.Now;
        Steps = new List<ScriptStep>();
        Variables = new List<ScriptVariable>();
        Type = ScriptType.Code;
    }
}

/// <summary>
/// 脚本类型枚举
/// </summary>
public enum ScriptType
{
    Code,       // 代码脚本
    NoCode      // 无代码脚本
}

/// <summary>
/// 脚本步骤模型
/// </summary>
public class ScriptStep
{
    /// <summary>
    /// 步骤ID
    /// </summary>
    public string StepId { get; set; }
    
    /// <summary>
    /// 步骤类型
    /// </summary>
    public StepType Type { get; set; }
    
    /// <summary>
    /// 步骤名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 步骤参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; }
    
    /// <summary>
    /// 条件表达式
    /// </summary>
    public string Condition { get; set; }
    
    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int Timeout { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ScriptStep()
    {
        StepId = Guid.NewGuid().ToString();
        Parameters = new Dictionary<string, object>();
        Timeout = 1000;
    }
}

/// <summary>
/// 步骤类型枚举
/// </summary>
public enum StepType
{
    RequestResponse,  // 请求响应
    SessionControl,   // 会话控制
    SecurityAccess,   // 安全访问
    ReadMemory,       // 读取内存
    WriteMemory,      // 写入内存
    Wait,             // 等待
    Log,              // 日志
    Variable,         // 变量操作
    Loop,             // 循环
    Condition         // 条件判断
}

/// <summary>
/// 脚本变量模型
/// </summary>
public class ScriptVariable
{
    /// <summary>
    /// 变量名
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 变量类型
    /// </summary>
    public VariableType Type { get; set; }
    
    /// <summary>
    /// 初始值
    /// </summary>
    public object InitialValue { get; set; }
    
    /// <summary>
    /// 描述
    /// </summary>
    public string Description { get; set; }
}

/// <summary>
/// 变量类型枚举
/// </summary>
public enum VariableType
{
    Integer,
    Float,
    Boolean,
    String,
    ByteArray
}
```

### 3.3 ObdScriptExecutionResult

```csharp
/// <summary>
/// OBD脚本执行结果模型
/// </summary>
public class ObdScriptExecutionResult
{
    /// <summary>
    /// 执行ID
    /// </summary>
    public string ExecutionId { get; set; }
    
    /// <summary>
    /// 脚本ID
    /// </summary>
    public string ScriptId { get; set; }
    
    /// <summary>
    /// 执行状态
    /// </summary>
    public ExecutionStatus Status { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// 执行时长（毫秒）
    /// </summary>
    public long ExecutionTimeMs { get; set; }
    
    /// <summary>
    /// 执行的步骤数量
    /// </summary>
    public int StepsExecuted { get; set; }
    
    /// <summary>
    /// 成功的步骤数量
    /// </summary>
    public int SuccessfulSteps { get; set; }
    
    /// <summary>
    /// 失败的步骤数量
    /// </summary>
    public int FailedSteps { get; set; }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; }
    
    /// <summary>
    /// 执行日志
    /// </summary>
    public List<string> ExecutionLog { get; set; }
    
    /// <summary>
    /// 变量结果
    /// </summary>
    public Dictionary<string, object> VariableResults { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public ObdScriptExecutionResult()
    {
        ExecutionId = Guid.NewGuid().ToString();
        StartTime = DateTime.Now;
        ExecutionLog = new List<string>();
        VariableResults = new Dictionary<string, object>();
    }
}

/// <summary>
/// 执行状态枚举
/// </summary>
public enum ExecutionStatus
{
    Ready,
    Running,
    Completed,
    Failed,
    Paused,
    Canceled
}
```

## 4. 数据模型关系

```
ObdRequest 1--1 ObdResponse
ObdSession 1--* ObdRequest
ObdSession 1--* ObdResponse
ObdScript 1--* ScriptStep
ObdScript 1--* ScriptVariable
ObdScriptExecutionResult -- ObdScript
```

## 5. 序列化支持

所有数据模型均支持JSON序列化和反序列化，便于数据持久化和网络传输。

```csharp
// 示例：序列化和反序列化
var scriptJson = JsonConvert.SerializeObject(obdScript);
var deserializedScript = JsonConvert.DeserializeObject<ObdScript>(scriptJson);
```

## 6. 性能优化考虑

- 使用异步处理请求响应
- 实现请求缓存减少重复通信
- 使用对象池管理频繁创建的对象
- 优化大数据传输处理

## 7. 扩展性考虑

- 支持多种诊断协议扩展
- 无代码脚本模型支持自定义步骤类型
- 可扩展的变量类型系统
- 支持插件式的脚本解析器