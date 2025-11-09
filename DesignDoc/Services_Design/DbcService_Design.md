# DBC服务设计文档

## 1. 服务概述

DBC服务负责DBC文件的加载、解析和管理，提供CAN信号的解码和编码功能，为上层应用提供统一的DBC数据访问接口，特别优化了BMS信号的解析。

## 2. 服务接口

```csharp
public interface IDbcService
{
    /// <summary>
    /// 加载DBC文件
    /// </summary>
    /// <param name="filePath">DBC文件路径</param>
    /// <returns>加载是否成功</returns>
    bool LoadDbcFile(string filePath);
    
    /// <summary>
    /// 卸载当前DBC文件
    /// </summary>
    void UnloadDbcFile();
    
    /// <summary>
    /// 获取所有报文
    /// </summary>
    /// <returns>报文列表</returns>
    List<DbcMessage> GetMessages();
    
    /// <summary>
    /// 根据ID获取信号
    /// </summary>
    /// <param name="signalId">信号ID</param>
    /// <returns>信号对象</returns>
    DbcSignal GetSignalById(string signalId);
    
    /// <summary>
    /// 解码信号值
    /// </summary>
    /// <param name="data">原始数据</param>
    /// <param name="signal">信号定义</param>
    /// <returns>解码后的信号值</returns>
    double DecodeSignal(byte[] data, DbcSignal signal);
    
    /// <summary>
    /// 编码信号值
    /// </summary>
    /// <param name="value">信号值</param>
    /// <param name="signal">信号定义</param>
    /// <returns>编码后的原始数据</returns>
    byte[] EncodeSignal(double value, DbcSignal signal);
    
    /// <summary>
    /// 解码CAN报文中的所有信号
    /// </summary>
    /// <param name="message">CAN报文</param>
    /// <returns>信号值列表</returns>
    List<SignalValue> DecodeMessage(CanMessage message);
    
    /// <summary>
    /// 根据名称查找报文
    /// </summary>
    /// <param name="messageName">报文名称</param>
    /// <returns>报文对象</returns>
    DbcMessage GetMessageByName(string messageName);
    
    /// <summary>
    /// 根据报文ID查找报文
    /// </summary>
    /// <param name="messageId">报文ID</param>
    /// <returns>报文对象</returns>
    DbcMessage GetMessageById(uint messageId);
    
    /// <summary>
    /// 获取所有信号
    /// </summary>
    /// <returns>信号列表</returns>
    List<DbcSignal> GetAllSignals();
    
    /// <summary>
    /// 根据信号名称查找信号
    /// </summary>
    /// <param name="signalName">信号名称</param>
    /// <returns>信号列表</returns>
    List<DbcSignal> GetSignalsByName(string signalName);
    
    /// <summary>
    /// 获取当前加载的DBC文件路径
    /// </summary>
    string CurrentDbcFilePath { get; }
    
    /// <summary>
    /// 获取DBC文件信息
    /// </summary>
    DbcFileInfo GetDbcFileInfo();
    
    /// <summary>
    /// 保存信号配置
    /// </summary>
    /// <param name="signals">信号配置列表</param>
    void SaveSignalConfigurations(List<SignalConfiguration> signals);
    
    /// <summary>
    /// 加载信号配置
    /// </summary>
    /// <returns>信号配置列表</returns>
    List<SignalConfiguration> LoadSignalConfigurations();
}
```

## 3. 实现类设计

### 3.1 DbcService

```csharp
public class DbcService : IDbcService
{
    // 实现IDbcService接口的所有方法
    // 使用自定义DBC解析器实现DBC文件处理
}
```

### 3.2 DbcParser

```csharp
public class DbcParser
{
    public DbcFile Parse(string filePath);
    
    // 内部辅助方法
    private void ParseVersion(string line);
    private void ParseNewSymbol(string line);
    private void ParseBitTiming(string line);
    private void ParseNodes(string line);
    private void ParseMessages(string line);
    private void ParseSignals(string line);
    private void ParseSignalValues(string line);
}
```

## 4. 数据模型引用

- **DbcMessage**: DBC报文数据模型
- **DbcSignal**: DBC信号数据模型
- **SignalValue**: 信号值数据模型
- **CanMessage**: CAN报文数据模型
- **DbcFileInfo**: DBC文件信息数据模型
- **SignalConfiguration**: 信号配置数据模型

## 5. 关键功能实现

### 5.1 DBC文件解析
- 支持标准DBC文件格式解析
- 支持多版本DBC文件兼容
- 支持BMS特定信号扩展属性解析

### 5.2 信号解码
- 支持Intel和Motorola字节序
- 支持有符号和无符号信号
- 支持信号缩放（因子和偏移）
- 支持位字段信号提取

### 5.3 信号编码
- 支持信号值到原始数据的转换
- 支持信号边界检查
- 支持信号值验证

### 5.4 BMS信号优化
- 电池电压信号特殊处理
- 温度信号优化解析
- 电流信号校准支持
- SOC/SOH信号处理

### 5.5 信号配置管理
- 支持用户自定义信号配置
- 支持配置导入导出
- 支持配置模板管理

### 5.6 性能优化
- 信号解码缓存机制
- 批量处理优化
- 预编译表达式提升性能

## 6. 扩展功能

### 6.1 多DBC文件管理
- 支持多个DBC文件同时加载
- 支持DBC文件切换
- 支持DBC文件合并

### 6.2 信号搜索和过滤
- 支持按名称、ID、节点等条件搜索
- 支持信号分组和分类
- 支持信号快速查找

### 6.3 信号监控配置
- 支持监控信号自定义
- 支持监控视图配置
- 支持报警阈值设置

## 7. 依赖关系

- 依赖CanMessage数据模型
- 依赖DbcMessage、DbcSignal等相关数据模型
- 依赖ILoggingService进行日志记录
- 依赖FileHandler进行文件操作

## 8. 安全性

- 文件加载安全检查
- 内存使用限制
- 异常处理和恢复机制