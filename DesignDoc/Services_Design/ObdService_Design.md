# OBD服务设计文档

## 1. 服务概述

OBD服务负责实现OBD-II标准诊断协议和BMS特定诊断协议，提供车辆诊断功能，支持安全访问、故障码读取、数据流监控等操作，为上层应用提供统一的诊断接口。

## 2. 服务接口

```csharp
public interface IObdService
{
    /// <summary>
    /// 连接到诊断接口
    /// </summary>
    /// <param name="interfaceName">接口名称</param>
    /// <returns>连接是否成功</returns>
    bool Connect(string interfaceName);
    
    /// <summary>
    /// 断开诊断连接
    /// </summary>
    void Disconnect();
    
    /// <summary>
    /// 发送诊断命令
    /// </summary>
    /// <param name="command">诊断命令</param>
    /// <returns>诊断响应</returns>
    ObdResponse SendCommand(ObdCommand command);
    
    /// <summary>
    /// 执行安全访问
    /// </summary>
    /// <param name="level">安全等级</param>
    /// <param name="key">访问密钥</param>
    /// <param name="useAes">是否使用AES加密</param>
    /// <returns>安全访问是否成功</returns>
    bool SecurityAccess(string level, string key, bool useAes = false);
    
    /// <summary>
    /// 读取故障码
    /// </summary>
    /// <returns>故障码列表</returns>
    List<DiagnosticTroubleCode> ReadDTCs();
    
    /// <summary>
    /// 清除故障码
    /// </summary>
    /// <returns>清除是否成功</returns>
    bool ClearDTCs();
    
    /// <summary>
    /// 读取数据流
    /// </summary>
    /// <param name="pid">参数ID</param>
    /// <returns>数据流值</returns>
    DataStreamValue ReadDataStream(string pid);
    
    /// <summary>
    /// 批量读取数据流
    /// </summary>
    /// <param name="pids">参数ID列表</param>
    /// <returns>数据流值列表</returns>
    List<DataStreamValue> ReadDataStreams(List<string> pids);
    
    /// <summary>
    /// 读取BMS特定信息
    /// </summary>
    /// <param name="infoType">信息类型</param>
    /// <returns>BMS信息</returns>
    BmsDiagnosticInfo ReadBmsInfo(string infoType);
    
    /// <summary>
    /// 执行BMS特定功能
    /// </summary>
    /// <param name="functionId">功能ID</param>
    /// <param name="parameters">功能参数</param>
    /// <returns>执行结果</returns>
    BmsFunctionResult ExecuteBmsFunction(string functionId, Dictionary<string, object> parameters);
    
    /// <summary>
    /// 获取支持的诊断服务列表
    /// </summary>
    /// <returns>服务列表</returns>
    List<DiagnosticService> GetSupportedServices();
    
    /// <summary>
    /// 获取当前连接状态
    /// </summary>
    bool IsConnected { get; }
    
    /// <summary>
    /// 获取当前安全等级
    /// </summary>
    string CurrentSecurityLevel { get; }
    
    /// <summary>
    /// 诊断响应事件
    /// </summary>
    event EventHandler<DiagnosticResponseEventArgs> DiagnosticResponseReceived;
}
```

## 3. 实现类设计

### 3.1 ObdService

```csharp
public class ObdService : IObdService
{
    // 实现IObdService接口的所有方法
    // 基于ISO 15765-4协议实现诊断功能
}
```

### 3.2 ObdProtocolHandler

```csharp
public class ObdProtocolHandler
{
    public byte[] FormatCommand(string serviceId, string data);
    public ObdResponse ParseResponse(byte[] rawResponse);
    
    // 不同协议的处理方法
    private byte[] HandleIso15765_4Protocol(string serviceId, string data);
    private byte[] HandleKWP2000Protocol(string serviceId, string data);
    private byte[] HandleJ1850VPWProtocol(string serviceId, string data);
    private byte[] HandleJ1850PWMProtocol(string serviceId, string data);
}
```

### 3.3 SecurityAccessHandler

```csharp
public class SecurityAccessHandler
{
    public bool PerformSecurityAccess(string level, string key, bool useAes);
    private byte[] GenerateKeySeed(string level);
    private byte[] CalculateKey(byte[] seed, string key, bool useAes);
    private byte[] EncryptWithAes(byte[] data, string key);
}
```

## 4. 数据模型引用

- **ObdCommand**: OBD命令数据模型
- **ObdResponse**: OBD响应数据模型
- **DiagnosticTroubleCode**: 故障码数据模型
- **DataStreamValue**: 数据流值数据模型
- **BmsDiagnosticInfo**: BMS诊断信息数据模型
- **BmsFunctionResult**: BMS功能执行结果数据模型
- **DiagnosticService**: 诊断服务数据模型
- **DiagnosticResponseEventArgs**: 诊断响应事件参数数据模型

## 5. 关键功能实现

### 5.1 协议支持
- 支持OBD-II标准协议
- 支持ISO 15765-4 (CAN) 协议
- 支持KWP2000协议
- 支持J1850 VPW/PWM协议
- 支持BMS特定扩展协议

### 5.2 安全访问
- 支持多级安全访问
- 实现AES128加密解密
- 支持安全密钥管理
- 支持安全会话维护

### 5.3 故障码管理
- 支持读取所有类型故障码
- 支持清除故障码
- 支持故障码详细信息解析
- 支持历史故障码查询

### 5.4 数据流监控
- 支持实时数据流读取
- 支持多参数同时监控
- 支持数据流单位转换
- 支持数据流自定义显示

### 5.5 BMS特定功能
- 电池信息读取（电压、温度、SOC等）
- 均衡控制功能
- 电池校准功能
- 电池保护设置

### 5.6 诊断脚本
- 支持诊断脚本执行
- 支持脚本步骤管理
- 支持脚本结果记录
- 支持脚本导入导出

## 6. 性能优化

- 命令缓存机制
- 并行数据请求
- 响应超时处理
- 错误重试机制

## 7. 依赖关系

- 依赖ICanService进行底层通信
- 依赖ILoggingService进行日志记录
- 依赖AesEncryption进行安全加密
- 依赖各种诊断相关数据模型

## 8. 安全性

- 安全访问密钥保护
- 通信数据加密
- 敏感操作权限控制
- 异常访问监控

## 9. 扩展性

- 支持自定义诊断协议扩展
- 支持新的BMS功能添加
- 支持诊断服务插件化