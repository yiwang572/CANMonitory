# 自动化测试数据模型设计文档

## 1. 模型概述

自动化测试数据模型包含与自动化测试相关的核心数据结构，用于表示测试脚本、测试任务、测试报告等信息，是系统中自动化测试功能的基础数据表示。

## 2. 主要数据模型

### 2.1 AutomationScript

```csharp
/// <summary>
/// 自动化测试脚本模型
/// </summary>
public class AutomationScript
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
    /// 标签
    /// </summary>
    public List<string> Tags { get; set; }
    
    /// <summary>
    /// 步骤列表（用于无代码脚本）
    /// </summary>
    public List<TestStep> Steps { get; set; }
    
    /// <summary>
    /// 变量列表
    /// </summary>
    public List<ScriptVariable> Variables { get; set; }
    
    /// <summary>
    /// 配置参数
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public AutomationScript()
    {
        ScriptId = Guid.NewGuid().ToString();
        CreatedTime = DateTime.Now;
        ModifiedTime = DateTime.Now;
        Tags = new List<string>();
        Steps = new List<TestStep>();
        Variables = new List<ScriptVariable>();
        Configuration = new Dictionary<string, object>();
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
```

### 2.2 TestStep

```csharp
/// <summary>
/// 测试步骤模型
/// </summary>
public class TestStep
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
    /// 步骤描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 步骤参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; }
    
    /// <summary>
    /// 前置条件
    /// </summary>
    public string Precondition { get; set; }
    
    /// <summary>
    /// 期望结果
    /// </summary>
    public string ExpectedResult { get; set; }
    
    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int Timeout { get; set; }
    
    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; }
    
    /// <summary>
    /// 子步骤
    /// </summary>
    public List<TestStep> SubSteps { get; set; }
    
    /// <summary>
    /// 执行顺序
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public TestStep()
    {
        StepId = Guid.NewGuid().ToString();
        Parameters = new Dictionary<string, object>();
        SubSteps = new List<TestStep>();
        Timeout = 3000;
        RetryCount = 0;
    }
}

/// <summary>
/// 步骤类型枚举
/// </summary>
public enum StepType
{
    SendCanMessage,      // 发送CAN报文
    ReceiveCanMessage,   // 接收CAN报文
    Wait,                // 等待
    CheckCondition,      // 条件检查
    SetVariable,         // 设置变量
    GetVariable,         // 获取变量
    Log,                 // 日志记录
    CallScript,          // 调用脚本
    Loop,                // 循环
    IfElse,              // 条件分支
    ObdCommand,          // OBD命令
    StartRecording,      // 开始记录
    StopRecording,       // 停止记录
    Assert,              // 断言
    Delay,               // 延迟
    SignalMonitor        // 信号监控
}
```

### 2.3 TestTask

```csharp
/// <summary>
/// 测试任务模型
/// </summary>
public class TestTask
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public string TaskId { get; set; }
    
    /// <summary>
    /// 任务名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 任务描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 关联的脚本ID列表
    /// </summary>
    public List<string> ScriptIds { get; set; }
    
    /// <summary>
    /// 执行模式
    /// </summary>
    public ExecutionMode Mode { get; set; }
    
    /// <summary>
    /// 任务状态
    /// </summary>
    public TaskStatus Status { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? StartTime { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? EndTime { get; set; }
    
    /// <summary>
    /// 执行参数
    /// </summary>
    public Dictionary<string, object> Parameters { get; set; }
    
    /// <summary>
    /// 优先级
    /// </summary>
    public int Priority { get; set; }
    
    /// <summary>
    /// 创建者
    /// </summary>
    public string Creator { get; set; }
    
    /// <summary>
    /// 备注
    /// </summary>
    public string Remarks { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public TestTask()
    {
        TaskId = Guid.NewGuid().ToString();
        ScriptIds = new List<string>();
        Parameters = new Dictionary<string, object>();
        CreatedTime = DateTime.Now;
        Mode = ExecutionMode.Sequential;
        Status = TaskStatus.Ready;
        Priority = 0;
    }
}

/// <summary>
/// 执行模式枚举
/// </summary>
public enum ExecutionMode
{
    Sequential,  // 顺序执行
    Parallel     // 并行执行
}

/// <summary>
/// 任务状态枚举
/// </summary>
public enum TaskStatus
{
    Ready,      // 准备就绪
    Running,    // 运行中
    Completed,  // 已完成
    Failed,     // 失败
    Paused,     // 已暂停
    Canceled    // 已取消
}
```

### 2.4 TestReport

```csharp
/// <summary>
/// 测试报告模型
/// </summary>
public class TestReport
{
    /// <summary>
    /// 报告ID
    /// </summary>
    public string ReportId { get; set; }
    
    /// <summary>
    /// 关联的任务ID
    /// </summary>
    public string TaskId { get; set; }
    
    /// <summary>
    /// 报告名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 测试结果
    /// </summary>
    public TestResult Result { get; set; }
    
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }
    
    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// 测试时长（毫秒）
    /// </summary>
    public long DurationMs { get; set; }
    
    /// <summary>
    /// 总步骤数
    /// </summary>
    public int TotalSteps { get; set; }
    
    /// <summary>
    /// 通过步骤数
    /// </summary>
    public int PassedSteps { get; set; }
    
    /// <summary>
    /// 失败步骤数
    /// </summary>
    public int FailedSteps { get; set; }
    
    /// <summary>
    /// 跳过步骤数
    /// </summary>
    public int SkippedSteps { get; set; }
    
    /// <summary>
    /// 测试详细信息
    /// </summary>
    public List<TestStepResult> StepResults { get; set; }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public List<string> ErrorMessages { get; set; }
    
    /// <summary>
    /// 测试日志
    /// </summary>
    public List<string> TestLogs { get; set; }
    
    /// <summary>
    /// 环境信息
    /// </summary>
    public Dictionary<string, string> EnvironmentInfo { get; set; }
    
    /// <summary>
    /// 生成时间
    /// </summary>
    public DateTime GeneratedTime { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public TestReport()
    {
        ReportId = Guid.NewGuid().ToString();
        StepResults = new List<TestStepResult>();
        ErrorMessages = new List<string>();
        TestLogs = new List<string>();
        EnvironmentInfo = new Dictionary<string, string>();
        GeneratedTime = DateTime.Now;
    }
}

/// <summary>
/// 测试结果枚举
/// </summary>
public enum TestResult
{
    Passed,     // 通过
    Failed,     // 失败
    Partial,    // 部分通过
    Skipped     // 跳过
}

/// <summary>
/// 测试步骤结果模型
/// </summary>
public class TestStepResult
{
    /// <summary>
    /// 步骤ID
    /// </summary>
    public string StepId { get; set; }
    
    /// <summary>
    /// 步骤名称
    /// </summary>
    public string StepName { get; set; }
    
    /// <summary>
    /// 步骤类型
    /// </summary>
    public StepType StepType { get; set; }
    
    /// <summary>
    /// 执行结果
    /// </summary>
    public StepResult Result { get; set; }
    
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
    public long DurationMs { get; set; }
    
    /// <summary>
    /// 实际结果
    /// </summary>
    public string ActualResult { get; set; }
    
    /// <summary>
    /// 错误信息
    /// </summary>
    public string ErrorMessage { get; set; }
    
    /// <summary>
    /// 子步骤结果
    /// </summary>
    public List<TestStepResult> SubStepResults { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public TestStepResult()
    {
        SubStepResults = new List<TestStepResult>();
    }
}

/// <summary>
/// 步骤结果枚举
/// </summary>
public enum StepResult
{
    Passed,     // 通过
    Failed,     // 失败
    Skipped,    // 跳过
    NotExecuted // 未执行
}
```

## 3. 辅助数据模型

### 3.1 ScriptVariable

```csharp
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
    
    /// <summary>
    /// 是否为输入参数
    /// </summary>
    public bool IsInputParameter { get; set; }
    
    /// <summary>
    /// 是否为输出参数
    /// </summary>
    public bool IsOutputParameter { get; set; }
    
    /// <summary>
    /// 是否为常量
    /// </summary>
    public bool IsConstant { get; set; }
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
    DateTime,
    ByteArray,
    Object,
    CanMessage,
    List
}
```

### 3.2 TestTemplate

```csharp
/// <summary>
/// 测试模板模型
/// </summary>
public class TestTemplate
{
    /// <summary>
    /// 模板ID
    /// </summary>
    public string TemplateId { get; set; }
    
    /// <summary>
    /// 模板名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 模板描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 模板类型
    /// </summary>
    public TemplateType Type { get; set; }
    
    /// <summary>
    /// 适用场景
    /// </summary>
    public string ApplicableScenario { get; set; }
    
    /// <summary>
    /// 步骤列表
    /// </summary>
    public List<TestStep> Steps { get; set; }
    
    /// <summary>
    /// 变量列表
    /// </summary>
    public List<ScriptVariable> Variables { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
    
    /// <summary>
    /// 是否为内置模板
    /// </summary>
    public bool IsBuiltIn { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public TestTemplate()
    {
        TemplateId = Guid.NewGuid().ToString();
        Steps = new List<TestStep>();
        Variables = new List<ScriptVariable>();
        CreatedTime = DateTime.Now;
    }
}

/// <summary>
/// 模板类型枚举
/// </summary>
public enum TemplateType
{
    BasicTest,        // 基础测试
    SignalMonitoring, // 信号监控
    FunctionalTest,   // 功能测试
    DiagnosticsTest,  // 诊断测试
    PerformanceTest,  // 性能测试
    Custom            // 自定义
}
```

## 4. 数据模型关系

```
AutomationScript 1--* TestStep
AutomationScript 1--* ScriptVariable
TestTask 1--* AutomationScript
TestTask 1--1 TestReport
TestReport 1--* TestStepResult
TestTemplate 1--* TestStep
TestTemplate 1--* ScriptVariable
```

## 5. 序列化支持

所有数据模型均支持JSON序列化和反序列化，便于数据持久化和网络传输。

```csharp
// 示例：序列化和反序列化
var scriptJson = JsonConvert.SerializeObject(automationScript);
var deserializedScript = JsonConvert.DeserializeObject<AutomationScript>(scriptJson);
```

## 6. 性能优化考虑

- 使用延迟加载处理大型测试报告
- 实现增量更新机制
- 使用对象池管理频繁创建的对象
- 优化测试日志记录

## 7. 扩展性考虑

- 支持自定义测试步骤类型
- 支持插件式的报告生成器
- 可扩展的模板系统
- 支持自定义变量类型和操作