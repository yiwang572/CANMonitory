# 自动化测试服务设计文档

## 1. 服务概述

自动化测试服务负责BMS测试脚本的执行、测试流程管理、测试结果收集和报告生成，支持无代码脚本方案，为BMS测试提供自动化解决方案。

## 2. 服务接口

```csharp
public interface IAutomationService
{
    /// <summary>
    /// 执行测试脚本
    /// </summary>
    /// <param name="scriptPath">脚本文件路径</param>
    void ExecuteScript(string scriptPath);
    
    /// <summary>
    /// 停止脚本执行
    /// </summary>
    void StopScript();
    
    /// <summary>
    /// 暂停脚本执行
    /// </summary>
    void PauseScript();
    
    /// <summary>
    /// 继续脚本执行
    /// </summary>
    void ResumeScript();
    
    /// <summary>
    /// 生成测试报告
    /// </summary>
    /// <returns>测试报告对象</returns>
    TestReport GenerateReport();
    
    /// <summary>
    /// 导出测试报告
    /// </summary>
    /// <param name="filePath">导出文件路径</param>
    /// <param name="format">报告格式</param>
    /// <returns>导出是否成功</returns>
    bool ExportReport(string filePath, ReportFormat format);
    
    /// <summary>
    /// 步骤完成事件
    /// </summary>
    event EventHandler<TestStepResult> StepCompleted;
    
    /// <summary>
    /// 获取可用的测试模板列表
    /// </summary>
    /// <returns>测试模板列表</returns>
    List<TestTemplate> GetAvailableTemplates();
    
    /// <summary>
    /// 基于模板创建新脚本
    /// </summary>
    /// <param name="templateId">模板ID</param>
    /// <param name="outputPath">输出路径</param>
    /// <returns>创建是否成功</returns>
    bool CreateScriptFromTemplate(string templateId, string outputPath);
    
    /// <summary>
    /// 获取当前执行状态
    /// </summary>
    AutomationExecutionStatus ExecutionStatus { get; }
    
    /// <summary>
    /// 获取执行进度
    /// </summary>
    int ExecutionProgress { get; }
    
    /// <summary>
    /// 执行无代码脚本
    /// </summary>
    /// <param name="scriptData">脚本数据</param>
    void ExecuteNoCodeScript(NoCodeScriptData scriptData);
    
    /// <summary>
    /// 保存无代码脚本
    /// </summary>
    /// <param name="scriptData">脚本数据</param>
    /// <param name="filePath">保存路径</param>
    /// <returns>保存是否成功</returns>
    bool SaveNoCodeScript(NoCodeScriptData scriptData, string filePath);
    
    /// <summary>
    /// 加载无代码脚本
    /// </summary>
    /// <param name="filePath">脚本文件路径</param>
    /// <returns>脚本数据</returns>
    NoCodeScriptData LoadNoCodeScript(string filePath);
    
    /// <summary>
    /// 获取支持的测试操作列表
    /// </summary>
    /// <returns>测试操作列表</returns>
    List<TestOperation> GetSupportedOperations();
}
```

## 3. 实现类设计

### 3.1 AutomationService

```csharp
public class AutomationService : IAutomationService
{
    // 实现IAutomationService接口的所有方法
    // 提供自动化测试功能
}
```

### 3.2 ScriptExecutor

```csharp
public class ScriptExecutor
{
    public void Execute(string scriptPath);
    public void ExecuteNoCode(NoCodeScriptData scriptData);
    
    // 内部执行方法
    private TestStepResult ExecuteStep(TestStep step);
    private void HandleScriptError(Exception ex, TestStep step);
    private void UpdateProgress(int currentStep, int totalSteps);
}
```

### 3.3 ReportGenerator

```csharp
public class ReportGenerator
{
    public TestReport Generate(List<TestStepResult> results);
    public bool Export(TestReport report, string filePath, ReportFormat format);
    
    // 不同格式的报告生成方法
    private string GenerateTextReport(TestReport report);
    private string GenerateHtmlReport(TestReport report);
    private string GenerateExcelReport(TestReport report);
    private string GeneratePdfReport(TestReport report);
}
```

### 3.4 NoCodeScriptEngine

```csharp
public class NoCodeScriptEngine
{
    public TestStepResult ExecuteOperation(TestOperation operation, Dictionary<string, object> parameters);
    public NoCodeScriptData ValidateScript(NoCodeScriptData scriptData);
    
    // 操作执行方法
    private TestStepResult ExecuteCanMessageSend(Dictionary<string, object> parameters);
    private TestStepResult ExecuteWaitOperation(Dictionary<string, object> parameters);
    private TestStepResult ExecuteConditionCheck(Dictionary<string, object> parameters);
    private TestStepResult ExecuteDiagnosticCommand(Dictionary<string, object> parameters);
}
```

## 4. 数据模型引用

- **TestReport**: 测试报告数据模型
- **TestStep**: 测试步骤数据模型
- **TestStepResult**: 测试步骤结果数据模型
- **TestTemplate**: 测试模板数据模型
- **NoCodeScriptData**: 无代码脚本数据模型
- **TestOperation**: 测试操作数据模型
- **AutomationExecutionStatus**: 自动化执行状态枚举
- **ReportFormat**: 报告格式枚举

## 5. 关键功能实现

### 5.1 脚本执行
- 支持代码脚本执行
- 支持无代码脚本执行
- 支持脚本暂停、继续、停止
- 支持脚本断点调试

### 5.2 无代码脚本支持
- 可视化脚本解析
- 步骤流程执行
- 参数动态绑定
- 条件分支支持

### 5.3 测试模板管理
- 内置标准测试模板库
- 支持用户自定义模板
- 支持模板导入导出
- 支持模板版本管理

### 5.4 测试结果收集
- 实时结果收集
- 详细日志记录
- 异常处理和恢复
- 截图和数据捕获

### 5.5 报告生成
- 多格式报告支持（文本、HTML、Excel、PDF）
- 图表化展示
- 统计分析功能
- 历史对比支持

### 5.6 BMS特定测试功能
- SOC校准测试
- 均衡控制测试
- 保护功能测试
- 通信协议测试

## 6. 性能优化

- 异步执行模型
- 资源池管理
- 执行缓存优化
- 并行测试支持

## 7. 依赖关系

- 依赖ICanService进行CAN通信
- 依赖IObdService进行诊断操作
- 依赖ILoggingService进行日志记录
- 依赖各种测试相关数据模型
- 可选依赖第三方报告生成库

## 8. 安全性

- 脚本执行沙箱
- 权限控制
- 敏感操作验证
- 执行超时保护

## 9. 扩展性

- 支持插件化操作扩展
- 支持自定义报告模板
- 支持新的脚本语言
- 支持远程测试执行