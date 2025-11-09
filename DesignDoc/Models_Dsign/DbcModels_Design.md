# DBC数据模型设计文档

## 1. 模型概述

DBC数据模型包含与DBC文件解析、存储和管理相关的核心数据结构，用于表示DBC文件中的网络、报文、信号等信息，是系统中DBC解析和信号处理的基础数据表示。

## 2. 主要数据模型

### 2.1 DbcFile

```csharp
/// <summary>
/// DBC文件模型
/// </summary>
public class DbcFile
{
    /// <summary>
    /// DBC文件路径
    /// </summary>
    public string FilePath { get; set; }
    
    /// <summary>
    /// DBC文件名
    /// </summary>
    public string FileName { get; set; }
    
    /// <summary>
    /// 文件版本
    /// </summary>
    public string Version { get; set; }
    
    /// <summary>
    /// 生成时间
    /// </summary>
    public DateTime GeneratedTime { get; set; }
    
    /// <summary>
    /// 描述信息
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 网络节点列表
    /// </summary>
    public List<DbcNode> Nodes { get; set; }
    
    /// <summary>
    /// 报文列表
    /// </summary>
    public List<DbcMessage> Messages { get; set; }
    
    /// <summary>
    /// 环境变量列表
    /// </summary>
    public List<DbcEnvironmentVariable> EnvironmentVariables { get; set; }
    
    /// <summary>
    /// 符号表
    /// </summary>
    public Dictionary<uint, DbcMessage> MessageIdMap { get; set; }
    
    /// <summary>
    /// 信号名称映射
    /// </summary>
    public Dictionary<string, DbcSignal> SignalNameMap { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public DbcFile()
    {
        Nodes = new List<DbcNode>();
        Messages = new List<DbcMessage>();
        EnvironmentVariables = new List<DbcEnvironmentVariable>();
        MessageIdMap = new Dictionary<uint, DbcMessage>();
        SignalNameMap = new Dictionary<string, DbcSignal>();
    }
}
```

### 2.2 DbcMessage

```csharp
/// <summary>
/// DBC报文定义模型
/// </summary>
public class DbcMessage
{
    /// <summary>
    /// 报文ID
    /// </summary>
    public uint Id { get; set; }
    
    /// <summary>
    /// 报文名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// DLC（数据长度码）
    /// </summary>
    public byte DLC { get; set; }
    
    /// <summary>
    /// 发送节点
    /// </summary>
    public string Transmitter { get; set; }
    
    /// <summary>
    /// 接收节点列表
    /// </summary>
    public List<string> Receivers { get; set; }
    
    /// <summary>
    /// 信号列表
    /// </summary>
    public List<DbcSignal> Signals { get; set; }
    
    /// <summary>
    /// 周期（毫秒）
    /// </summary>
    public int CycleTime { get; set; }
    
    /// <summary>
    /// 注释
    /// </summary>
    public string Comment { get; set; }
    
    /// <summary>
    /// 属性值
    /// </summary>
    public Dictionary<string, string> AttributeValues { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public DbcMessage()
    {
        Receivers = new List<string>();
        Signals = new List<DbcSignal>();
        AttributeValues = new Dictionary<string, string>();
    }
}
```

### 2.3 DbcSignal

```csharp
/// <summary>
/// DBC信号定义模型
/// </summary>
public class DbcSignal
{
    /// <summary>
    /// 信号名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 起始位
    /// </summary>
    public int StartBit { get; set; }
    
    /// <summary>
    /// 信号长度（位）
    /// </summary>
    public int Length { get; set; }
    
    /// <summary>
    /// 是否为小端序
    /// </summary>
    public bool IsLittleEndian { get; set; }
    
    /// <summary>
    /// 是否为有符号信号
    /// </summary>
    public bool IsSigned { get; set; }
    
    /// <summary>
    /// 缩放因子
    /// </summary>
    public double Factor { get; set; }
    
    /// <summary>
    /// 偏移量
    /// </summary>
    public double Offset { get; set; }
    
    /// <summary>
    /// 最小值
    /// </summary>
    public double Minimum { get; set; }
    
    /// <summary>
    /// 最大值
    /// </summary>
    public double Maximum { get; set; }
    
    /// <summary>
    /// 单位
    /// </summary>
    public string Unit { get; set; }
    
    /// <summary>
    /// 接收节点列表
    /// </summary>
    public List<string> Receivers { get; set; }
    
    /// <summary>
    /// 所属报文
    /// </summary>
    public DbcMessage ParentMessage { get; set; }
    
    /// <summary>
    /// 值表
    /// </summary>
    public Dictionary<long, string> ValueTable { get; set; }
    
    /// <summary>
    /// 注释
    /// </summary>
    public string Comment { get; set; }
    
    /// <summary>
    /// 属性值
    /// </summary>
    public Dictionary<string, string> AttributeValues { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public DbcSignal()
    {
        Receivers = new List<string>();
        ValueTable = new Dictionary<long, string>();
        AttributeValues = new Dictionary<string, string>();
        Factor = 1.0;
        Offset = 0.0;
    }
    
    /// <summary>
    /// 从原始值计算物理值
    /// </summary>
    public double CalculatePhysicalValue(long rawValue)
    {
        return rawValue * Factor + Offset;
    }
    
    /// <summary>
    /// 从物理值计算原始值
    /// </summary>
    public long CalculateRawValue(double physicalValue)
    {
        return (long)Math.Round((physicalValue - Offset) / Factor);
    }
    
    /// <summary>
    /// 获取原始值对应的文本描述
    /// </summary>
    public string GetValueDescription(long rawValue)
    {
        if (ValueTable.ContainsKey(rawValue))
        {
            return ValueTable[rawValue];
        }
        return string.Empty;
    }
}
```

### 2.4 DbcNode

```csharp
/// <summary>
/// DBC节点模型
/// </summary>
public class DbcNode
{
    /// <summary>
    /// 节点名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 节点ID
    /// </summary>
    public int Id { get; set; }
    
    /// <summary>
    /// 发送的报文列表
    /// </summary>
    public List<DbcMessage> TransmittedMessages { get; set; }
    
    /// <summary>
    /// 接收的报文列表
    /// </summary>
    public List<DbcMessage> ReceivedMessages { get; set; }
    
    /// <summary>
    /// 注释
    /// </summary>
    public string Comment { get; set; }
    
    /// <summary>
    /// 属性值
    /// </summary>
    public Dictionary<string, string> AttributeValues { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public DbcNode()
    {
        TransmittedMessages = new List<DbcMessage>();
        ReceivedMessages = new List<DbcMessage>();
        AttributeValues = new Dictionary<string, string>();
    }
}
```

## 3. 辅助数据模型

### 3.1 DbcEnvironmentVariable

```csharp
/// <summary>
/// DBC环境变量模型
/// </summary>
public class DbcEnvironmentVariable
{
    /// <summary>
    /// 变量名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 变量类型
    /// </summary>
    public EnvVarType Type { get; set; }
    
    /// <summary>
    /// 最小值
    /// </summary>
    public double Minimum { get; set; }
    
    /// <summary>
    /// 最大值
    /// </summary>
    public double Maximum { get; set; }
    
    /// <summary>
    /// 初始值
    /// </summary>
    public double InitialValue { get; set; }
    
    /// <summary>
    /// 单位
    /// </summary>
    public string Unit { get; set; }
    
    /// <summary>
    /// 访问节点列表
    /// </summary>
    public List<string> AccessNodes { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public DbcEnvironmentVariable()
    {
        AccessNodes = new List<string>();
    }
}

/// <summary>
/// 环境变量类型枚举
/// </summary>
public enum EnvVarType
{
    Integer,
    Float,
    String,
    Data
}
```

### 3.2 DbcAttribute

```csharp
/// <summary>
/// DBC属性模型
/// </summary>
public class DbcAttribute
{
    /// <summary>
    /// 属性名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 属性类型
    /// </summary>
    public AttributeType Type { get; set; }
    
    /// <summary>
    /// 默认值
    /// </summary>
    public string DefaultValue { get; set; }
    
    /// <summary>
    /// 最小取值
    /// </summary>
    public string MinimumValue { get; set; }
    
    /// <summary>
    /// 最大取值
    /// </summary>
    public string MaximumValue { get; set; }
    
    /// <summary>
    /// 适用对象类型
    /// </summary>
    public List<AttributeObjectType> ObjectTypes { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public DbcAttribute()
    {
        ObjectTypes = new List<AttributeObjectType>();
    }
}

/// <summary>
/// 属性类型枚举
/// </summary>
public enum AttributeType
{
    Integer,
    Float,
    String,
    Enum
}

/// <summary>
/// 属性对象类型枚举
/// </summary>
public enum AttributeObjectType
{
    Network,
    Node,
    Message,
    Signal,
    EnvironmentVariable
}
```

### 3.3 DbcParserResult

```csharp
/// <summary>
/// DBC解析结果模型
/// </summary>
public class DbcParserResult
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 解析后的DBC文件对象
    /// </summary>
    public DbcFile DbcFile { get; set; }
    
    /// <summary>
    /// 错误消息
    /// </summary>
    public string ErrorMessage { get; set; }
    
    /// <summary>
    /// 警告列表
    /// </summary>
    public List<string> Warnings { get; set; }
    
    /// <summary>
    /// 解析时间（毫秒）
    /// </summary>
    public long ParseTimeMs { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public DbcParserResult()
    {
        Warnings = new List<string>();
    }
}
```

## 4. 数据模型关系

```
DbcFile 1--* DbcNode
DbcFile 1--* DbcMessage
DbcFile 1--* DbcEnvironmentVariable
DbcMessage 1--* DbcSignal
DbcNode --* DbcMessage (发送/接收关系)
DbcSignal -- DbcMessage (所属关系)
```

## 5. 序列化支持

所有数据模型均支持JSON序列化和反序列化，便于数据持久化和网络传输。

```csharp
// 示例：序列化和反序列化
var dbcJson = JsonConvert.SerializeObject(dbcFile);
var deserializedDbc = JsonConvert.DeserializeObject<DbcFile>(dbcJson);
```

## 6. 性能优化考虑

- 使用字典加速ID和名称查找
- 延迟加载复杂对象
- 预计算常用数据
- 使用内存缓存减少重复解析

## 7. 扩展性考虑

- 支持自定义属性扩展
- 支持不同版本DBC格式
- 可扩展性良好的解析器设计
- 支持增量更新解析结果