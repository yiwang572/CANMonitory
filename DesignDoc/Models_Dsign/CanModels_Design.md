# CAN数据模型设计文档

## 1. 模型概述

CAN数据模型包含与CAN总线通信相关的核心数据结构，用于表示CAN报文、CAN FD报文、信号、过滤器、设备信息等，是系统中CAN通信的基础数据表示，基于致远电子ZLGCAN接口函数实现，支持多种设备类型和通信模式。

## 2. 主要数据模型

### 2.1 CanMessage

```csharp
/// <summary>
/// CAN报文模型（标准CAN）
/// </summary>
public class CanMessage
{
    /// <summary>
    /// 报文ID
    /// </summary>
    public uint Id { get; set; }
    
    /// <summary>
    /// 报文数据
    /// </summary>
    public byte[] Data { get; set; }
    
    /// <summary>
    /// 时间戳（系统时间）
    /// </summary>
    public ulong Timestamp { get; set; }
    
    /// <summary>
    /// 是否为扩展帧
    /// </summary>
    public bool IsExtendedFrame { get; set; }
    
    /// <summary>
    /// 是否为远程帧
    /// </summary>
    public bool IsRemoteFrame { get; set; }
    
    /// <summary>
    /// 是否为错误帧
    /// </summary>
    public bool IsErrorFrame { get; set; }
    
    /// <summary>
    /// 报文名称（来自DBC）
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 信号值列表
    /// </summary>
    public List<SignalValue> SignalValues { get; set; }
    
    /// <summary>
    /// 数据长度
    /// </summary>
    public byte DataLength { get; set; }
    
    /// <summary>
    /// 通道索引
    /// </summary>
    public uint ChannelIndex { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public CanMessage()
    {
        Data = new byte[8]; // 默认8字节
        SignalValues = new List<SignalValue>();
        Timestamp = 0;
        DataLength = 8;
        ChannelIndex = 0;
    }
    
    /// <summary>
    /// 从ZLGCAN can_frame结构体转换
    /// </summary>
    /// <param name="frame">ZLGCAN can_frame结构体</param>
    /// <param name="timestamp">时间戳</param>
    /// <returns>CanMessage对象</returns>
    public static CanMessage FromCanFrame(can_frame frame, ulong timestamp = 0)
    {
        return new CanMessage
        {
            Id = frame.ID,
            Data = frame.Data,
            DataLength = frame.DLC,
            Timestamp = timestamp,
            IsExtendedFrame = frame.ExternFlag != 0,
            IsRemoteFrame = frame.RemoteFlag != 0,
            IsErrorFrame = frame.RemoteFlag == 0 && frame.ExternFlag == 0 && frame.ID == 0 && frame.DLC == 0
        };
    }
    
    /// <summary>
    /// 转换为ZLGCAN can_frame结构体
    /// </summary>
    /// <returns>ZLGCAN can_frame结构体</returns>
    public can_frame ToCanFrame()
    {
        return new can_frame
        {
            ID = Id,
            Data = Data,
            DLC = DataLength,
            ExternFlag = (byte)(IsExtendedFrame ? 1 : 0),
            RemoteFlag = (byte)(IsRemoteFrame ? 1 : 0),
            Reserved = 0
        };
    }
    
    /// <summary>
    /// 从ZLGCAN can_frame结构体转换
    /// </summary>
    /// <param name="frame">ZLGCAN can_frame结构体</param>
    /// <returns>CanMessage对象</returns>
    public static CanMessage FromCanFrame(can_frame frame)
    {
        return new CanMessage
        {
            Id = frame.ID,
            Data = frame.Data,
            Length = frame.DLC,
            IsExtendedFrame = frame.ExternFlag == 1,
            IsRemoteFrame = frame.RemoteFlag == 1,
            Timestamp = DateTime.Now
        };
    }
}

/// <summary>
/// 设备类型常量（与ZLGCAN保持一致）
/// </summary>
public static class DeviceTypes
{
    // USB-CANFD系列
    public const int USBCANFD_MINI = 43;      // USB-CANFD Mini
    public const int USBCANFD_100U = 42;      // USB-CANFD-100U
    public const int USBCANFD_200U = 40;      // USB-CANFD-200U
    public const int USBCANFD_400U = 41;      // USB-CANFD-400U
    
    // CANFD-NET系列
    public const int CANFDNET_400U = 42;      // CANFD-NET-400U
    public const int CANFDNET_400U_TCP = 43;  // CANFD-NET-400U TCP
    
    // 其他设备类型
    public const int PCIECANFD_100U = 44;     // PCIeCANFD-100U
    
    // USB-CAN系列
    public const int USBCAN_8E_U = 45;        // USB-CAN-8E-U
    public const int USBCAN_4E_U = 46;        // USB-CAN-4E-U
    public const int USBCAN_2E_U = 47;        // USB-CAN-2E-U
    
    // USB-CANFD系列
    public const int USBCAN_8FD_U = 48;       // USB-CAN-8FD-U
    public const int USBCAN_4FD_U = 49;       // USB-CAN-4FD-U
    public const int USBCAN_2FD_U = 50;       // USB-CAN-2FD-U
}
```

### 2.2 CanFdMessage

```csharp
/// <summary>
/// CAN FD报文模型
/// </summary>
public class CanFdMessage
{
    /// <summary>
    /// 报文ID
    /// </summary>
    public uint Id { get; set; }
    
    /// <summary>
    /// 报文数据（最长64字节）
    /// </summary>
    public byte[] Data { get; set; }
    
    /// <summary>
    /// 时间戳（系统时间）
    /// </summary>
    public ulong Timestamp { get; set; }
    
    /// <summary>
    /// 是否为扩展帧
    /// </summary>
    public bool IsExtendedFrame { get; set; }
    
    /// <summary>
    /// 是否为远程帧
    /// </summary>
    public bool IsRemoteFrame { get; set; }
    
    /// <summary>
    /// 是否为CAN FD帧
    /// </summary>
    public bool IsFdFrame { get; set; }
    
    /// <summary>
    /// 比特率切换标志
    /// </summary>
    public bool Brs { get; set; }
    
    /// <summary>
    /// 错误状态指示器
    /// </summary>
    public bool Esi { get; set; }
    
    /// <summary>
    /// 报文名称（来自DBC）
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 信号值列表
    /// </summary>
    public List<SignalValue> SignalValues { get; set; }
    
    /// <summary>
    /// 数据长度
    /// </summary>
    public byte DataLength { get; set; }
    
    /// <summary>
    /// 通道索引
    /// </summary>
    public uint ChannelIndex { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public CanFdMessage()
    {
        Data = new byte[64]; // CAN FD最大64字节
        SignalValues = new List<SignalValue>();
        Timestamp = 0;
        DataLength = 8;
        ChannelIndex = 0;
        IsFdFrame = true;
    }
    
    /// <summary>
    /// 转换为ZLGCAN canfd_frame结构体
    /// </summary>
    /// <returns>ZLGCAN canfd_frame结构体</returns>
    public canfd_frame ToCanFdFrame()
    {
        return new canfd_frame
        {
            ID = Id,
            Data = Data,
            DLC = DataLength,
            ExternFlag = (byte)(IsExtendedFrame ? 1 : 0),
            RemoteFlag = (byte)(IsRemoteFrame ? 1 : 0),
            FD = (byte)(IsFdFrame ? 1 : 0),
            BRS = (byte)(Brs ? 1 : 0),
            ESI = (byte)(Esi ? 1 : 0),
            Reserved = 0
        };
    }
    
    /// <summary>
    /// 从ZLGCAN canfd_frame结构体转换
    /// </summary>
    /// <param name="frame">ZLGCAN canfd_frame结构体</param>
    /// <param name="timestamp">时间戳</param>
    /// <returns>CanFdMessage对象</returns>
    public static CanFdMessage FromCanFdFrame(canfd_frame frame, ulong timestamp = 0)
    {
        return new CanFdMessage
        {
            Id = frame.ID,
            Data = frame.Data,
            DataLength = frame.DLC,
            Timestamp = timestamp,
            IsExtendedFrame = frame.ExternFlag != 0,
            IsRemoteFrame = frame.RemoteFlag != 0,
            IsFdFrame = frame.FD != 0,
            Brs = frame.BRS != 0,
            Esi = frame.ESI != 0
        };
    }
    
    /// <summary>
    /// 转换为ZLGCAN canfd_frame结构体
    /// </summary>
    /// <returns>ZLGCAN canfd_frame结构体</returns>
    public canfd_frame ToCanFdFrame()
    {
        return new canfd_frame
        {
            ID = Id,
            Data = Data,
            DLC = Length,
            ExternFlag = (byte)(IsExtendedFrame ? 1 : 0),
            RemoteFlag = (byte)(IsRemoteFrame ? 1 : 0),
            FD_Frame = (byte)(IsFdFrame ? 1 : 0),
            BRS = (byte)(Brs ? 1 : 0),
            ESI = (byte)(Esi ? 1 : 0),
            Reserved = 0
        };
    }
    
    /// <summary>
    /// 从ZLGCAN canfd_frame结构体转换
    /// </summary>
    /// <param name="frame">ZLGCAN canfd_frame结构体</param>
    /// <returns>CanFdMessage对象</returns>
    public static CanFdMessage FromCanFdFrame(canfd_frame frame)
    {
        return new CanFdMessage
        {
            Id = frame.ID,
            Data = frame.Data,
            Length = frame.DLC,
            IsExtendedFrame = frame.ExternFlag == 1,
            IsRemoteFrame = frame.RemoteFlag == 1,
            IsFdFrame = frame.FD_Frame == 1,
            Brs = frame.BRS == 1,
            Esi = frame.ESI == 1,
            Timestamp = DateTime.Now
        };
    }
}

### 2.3 CanSignal

```csharp
/// <summary>
/// CAN信号模型（简化版，详细信号定义见DbcSignal）
/// </summary>
public class CanSignal
{
    /// <summary>
    /// 信号名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 信号值
    /// </summary>
    public double Value { get; set; }
    
    /// <summary>
    /// 信号单位
    /// </summary>
    public string Unit { get; set; }
    
    /// <summary>
    /// 所属报文ID
    /// </summary>
    public uint MessageId { get; set; }
    
    /// <summary>
    /// 所属报文名称
    /// </summary>
    public string MessageName { get; set; }
    
    /// <summary>
    /// 信号描述
    /// </summary>
    public string Description { get; set; }
}
```

### 2.3 SignalValue

```csharp
/// <summary>
/// 信号值模型
/// </summary>
public class SignalValue
{
    /// <summary>
    /// 信号名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 信号值
    /// </summary>
    public double Value { get; set; }
    
    /// <summary>
    /// 信号单位
    /// </summary>
    public string Unit { get; set; }
    
    /// <summary>
    /// 格式化后的信号值
    /// </summary>
    public string FormattedValue { get; set; }
    
    /// <summary>
    /// 信号原始值（未缩放）
    /// </summary>
    public long RawValue { get; set; }
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// 是否为有效信号
    /// </summary>
    public bool IsValid { get; set; }
}
```

### 2.4 CanFilter

```csharp
/// <summary>
/// CAN过滤器模型
/// </summary>
public class CanFilter
{
    /// <summary>
    /// 过滤器ID
    /// </summary>
    public uint FilterId { get; set; }
    
    /// <summary>
    /// 掩码
    /// </summary>
    public uint Mask { get; set; }
    
    /// <summary>
    /// 是否为扩展帧过滤器
    /// </summary>
    public bool IsExtendedFrame { get; set; }
    
    /// <summary>
    /// 过滤模式（接受/拒绝）
    /// </summary>
    public FilterMode Mode { get; set; }
    
    /// <summary>
    /// 过滤器描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 转换为ZLGCAN过滤器配置
    /// </summary>
    /// <returns>包含AccCode和AccMask的元组</returns>
    public (uint AccCode, uint AccMask) ToZlgFilter()
    {
        // 根据过滤模式确定是接受还是拒绝
        if (Mode == FilterMode.Accept)
        {
            // 接受模式：设置AccCode为过滤器ID，AccMask为掩码
            return (FilterId, Mask);
        }
        else
        {
            // 拒绝模式：需要特殊处理，这里简化处理
            return (0, 0);
        }
    }
}

### 2.5 CanInitConfig

```csharp
/// <summary>
/// CAN初始化配置模型
/// </summary>
public class CanInitConfig
{
    /// <summary>
    /// 验收码
    /// </summary>
    public uint AccCode { get; set; }
    
    /// <summary>
    /// 验收掩码
    /// </summary>
    public uint AccMask { get; set; }
    
    /// <summary>
    /// 保留位
    /// </summary>
    public uint Reserved { get; set; }
    
    /// <summary>
    /// 滤波方式 0表示单滤波 1表示双滤波
    /// </summary>
    public byte Filter { get; set; }
    
    /// <summary>
    /// 波特率参数0 (Timing0)
    /// </summary>
    public byte Timing0 { get; set; }
    
    /// <summary>
    /// 波特率参数1 (Timing1)
    /// </summary>
    public byte Timing1 { get; set; }
    
    /// <summary>
    /// 模式 0表示正常模式 1表示只听模式
    /// </summary>
    public byte Mode { get; set; }
    
    /// <summary>
    /// CAN FD模式下的数据段波特率参数0
    /// </summary>
    public byte Timing0Data { get; set; }
    
    /// <summary>
    /// CAN FD模式下的数据段波特率参数1
    /// </summary>
    public byte Timing1Data { get; set; }
    
    /// <summary>
    /// 是否使用CAN FD模式
    /// </summary>
    public bool IsFDMode { get; set; }
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public CanInitConfig()
    {
        AccCode = 0x00000000; // 默认不滤波
        AccMask = 0xFFFFFFFF; // 默认不滤波
        Reserved = 0;
        Filter = 0;
        Timing0 = 0x00;
        Timing1 = 0x1C; // 默认500Kbps
        Mode = 0; // 正常模式
        IsFDMode = false;
    }
    
    /// <summary>
    /// 根据波特率设置Timing0和Timing1
    /// </summary>
    /// <param name="baudRate">波特率</param>
    public void SetBaudRate(int baudRate)
    {
        // 根据不同的波特率设置对应的Timing参数
        switch (baudRate)
        {
            case 10000:
                Timing0 = 0x31;
                Timing1 = 0x1C;
                break;
            case 20000:
                Timing0 = 0x18;
                Timing1 = 0x1C;
                break;
            case 50000:
                Timing0 = 0x09;
                Timing1 = 0x1C;
                break;
            case 100000:
                Timing0 = 0x04;
                Timing1 = 0x1C;
                break;
            case 125000:
                Timing0 = 0x03;
                Timing1 = 0x1C;
                break;
            case 250000:
                Timing0 = 0x01;
                Timing1 = 0x1C;
                break;
            case 500000:
                Timing0 = 0x00;
                Timing1 = 0x1C;
                break;
            case 800000:
                Timing0 = 0x00;
                Timing1 = 0x16;
                break;
            case 1000000:
                Timing0 = 0x00;
                Timing1 = 0x14;
                break;
            default:
                // 默认500Kbps
                Timing0 = 0x00;
                Timing1 = 0x1C;
                break;
        }
    }
    
    /// <summary>
    /// 转换为ZLGCAN初始化配置结构体
    /// </summary>
    /// <returns>ZLGCAN初始化配置结构体</returns>
    public ZCAN_CHANNEL_INIT_CONFIG ToZlgInitConfig()
    {
        return new ZCAN_CHANNEL_INIT_CONFIG
        {
            AccCode = AccCode,
            AccMask = AccMask,
            Reserved = Reserved,
            Filter = Filter,
            Timing0 = Timing0,
            Timing1 = Timing1,
            Mode = Mode
        };
    }
}

### 2.6 CanErrorInfo

```csharp
/// <summary>
/// CAN错误信息模型
/// </summary>
public class CanErrorInfo
{
    /// <summary>
    /// 错误状态寄存器
    /// </summary>
    public uint ErrStatus { get; set; }
    
    /// <summary>
    /// 接收错误计数
    /// </summary>
    public byte RxErrCounter { get; set; }
    
    /// <summary>
    /// 发送错误计数
    /// </summary>
    public byte TxErrCounter { get; set; }
    
    /// <summary>
    /// 错误类型
    /// </summary>
    public ErrorType ErrorType { get; set; }
    
    /// <summary>
    /// 错误描述
    /// </summary>
    public string ErrorDescription { get; set; }
    
    /// <summary>
    /// 发生时间
    /// </summary>
    public DateTime Timestamp { get; set; }
    
    /// <summary>
    /// 设备类型
    /// </summary>
    public int DeviceType { get; set; }
    
    /// <summary>
    /// 设备索引
    /// </summary>
    public int DeviceIndex { get; set; }
    
    /// <summary>
    /// 通道号
    /// </summary>
    public int Channel { get; set; }
    
    /// <summary>
    /// 从ZLGCAN错误信息结构体转换
    /// </summary>
    /// <param name="errorInfo">ZLGCAN错误信息结构体</param>
    /// <returns>CanErrorInfo对象</returns>
    public static CanErrorInfo FromZlgErrorInfo(ZCAN_CHANNEL_ERROR_INFO errorInfo)
    {
        var canErrorInfo = new CanErrorInfo
        {
            ErrStatus = errorInfo.ErrStatus,
            RxErrCounter = errorInfo.RxErrCounter,
            TxErrCounter = errorInfo.TxErrCounter,
            Timestamp = DateTime.Now
        };
        
        // 解析错误状态，设置错误类型和描述
        canErrorInfo.ParseErrorStatus();
        
        return canErrorInfo;
    }
    
    /// <summary>
    /// 解析错误状态
    /// </summary>
    private void ParseErrorStatus()
    {
        // 根据ErrStatus解析错误类型
        if ((ErrStatus & 0x0001) != 0) // 接收错误报警
        {
            ErrorType |= ErrorType.ReceiveWarning;
            ErrorDescription += "接收错误报警;";
        }
        if ((ErrStatus & 0x0002) != 0) // 发送错误报警
        {
            ErrorType |= ErrorType.TransmitWarning;
            ErrorDescription += "发送错误报警;";
        }
        if ((ErrStatus & 0x0004) != 0) // 接收错误被动
        {
            ErrorType |= ErrorType.ReceivePassive;
            ErrorDescription += "接收错误被动;";
        }
        if ((ErrStatus & 0x0008) != 0) // 发送错误被动
        {
            ErrorType |= ErrorType.TransmitPassive;
            ErrorDescription += "发送错误被动;";
        }
        if ((ErrStatus & 0x0010) != 0) // 总线关闭
        {
            ErrorType |= ErrorType.BusOff;
            ErrorDescription += "总线关闭;";
        }
        if ((ErrStatus & 0x0020) != 0) // 数据溢出
        {
            ErrorType |= ErrorType.DataOverflow;
            ErrorDescription += "数据溢出;";
        }
        
        // 清除开头多余的分号
        if (ErrorDescription.StartsWith(";"))
        {
            ErrorDescription = ErrorDescription.Substring(1);
        }
    }
}

/// <summary>
/// 错误类型枚举（使用位域）
/// </summary>
[Flags]
public enum ErrorType
{
    None = 0,
    ReceiveWarning = 1,
    TransmitWarning = 2,
    ReceivePassive = 4,
    TransmitPassive = 8,
    BusOff = 16,
    DataOverflow = 32
}

### 2.7 CanChannelStatus

```csharp
/// <summary>
/// CAN通道状态模型
/// </summary>
public class CanChannelStatus
{
    /// <summary>
    /// 设备类型
    /// </summary>
    public int DeviceType { get; set; }
    
    /// <summary>
    /// 设备索引
    /// </summary>
    public int DeviceIndex { get; set; }
    
    /// <summary>
    /// 通道号
    /// </summary>
    public int Channel { get; set; }
    
    /// <summary>
    /// 通道状态
    /// </summary>
    public ChannelStatus Status { get; set; }
    
    /// <summary>
    /// 当前波特率
    /// </summary>
    public int BaudRate { get; set; }
    
    /// <summary>
    /// 是否在CAN FD模式
    /// </summary>
    public bool IsFDMode { get; set; }
    
    /// <summary>
    /// 接收队列中等待处理的报文数量
    /// </summary>
    public int PendingReceiveCount { get; set; }
    
    /// <summary>
    /// 发送队列中等待发送的报文数量
    /// </summary>
    public int PendingSendCount { get; set; }
    
    /// <summary>
    /// 是否有错误
    /// </summary>
    public bool HasError { get; set; }
    
    /// <summary>
    /// 最后一次错误信息
    /// </summary>
    public CanErrorInfo LastErrorInfo { get; set; }
    
    /// <summary>
    /// 状态更新时间
    /// </summary>
    public DateTime LastUpdatedTime { get; set; }
    
    /// <summary>
    /// 重置通道状态
    /// </summary>
    public void Reset()
    {
        Status = ChannelStatus.Disconnected;
        PendingReceiveCount = 0;
        PendingSendCount = 0;
        HasError = false;
        LastErrorInfo = null;
        LastUpdatedTime = DateTime.Now;
    }
}

/// <summary>
/// 过滤模式枚举
/// </summary>
public enum FilterMode
{
    Accept,  // 接受匹配的报文
    Reject   // 拒绝匹配的报文
}
```

### 2.5 CanDeviceInfo

```csharp
/// <summary>
/// CAN设备信息模型
/// </summary>
public class CanDeviceInfo
{
    /// <summary>
    /// 设备类型
    /// </summary>
    public int DeviceType { get; set; }
    
    /// <summary>
    /// 设备索引
    /// </summary>
    public int DeviceIndex { get; set; }
    
    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; }
    
    /// <summary>
    /// 设备描述
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// 固件版本
    /// </summary>
    public string FirmwareVersion { get; set; }
    
    /// <summary>
    /// 硬件版本
    /// </summary>
    public string HardwareVersion { get; set; }
    
    /// <summary>
    /// 序列号
    /// </summary>
    public string SerialNumber { get; set; }
    
    /// <summary>
    /// 通道数量
    /// </summary>
    public int ChannelCount { get; set; }
    
    /// <summary>
    /// 是否支持CAN FD
    /// </summary>
    public bool IsCanFDSupported { get; set; }
    
    /// <summary>
    /// 是否为网络设备
    /// </summary>
    public bool IsNetworkDevice { get; set; }
    
    /// <summary>
    /// IP地址（网络设备）
    /// </summary>
    public string IpAddress { get; set; }
    
    /// <summary>
    /// 设备状态
    /// </summary>
    public DeviceStatus Status { get; set; }
    
    /// <summary>
    /// 支持的波特率列表
    /// </summary>
    public List<int> SupportedBaudRates { get; set; }
}

/// <summary>
/// 设备状态枚举
/// </summary>
public enum DeviceStatus
{
    Disconnected,
    Connected,
    Opened,
    Error
}

### 2.6 CanChannelInfo

```csharp
/// <summary>
/// CAN通道信息模型
/// </summary>
public class CanChannelInfo
{
    /// <summary>
    /// 通道名称
    /// </summary>
    public string Name { get; set; }
    
    /// <summary>
    /// 通道索引
    /// </summary>
    public int Index { get; set; }
    
    /// <summary>
    /// 所属设备类型
    /// </summary>
    public int DeviceType { get; set; }
    
    /// <summary>
    /// 所属设备索引
    /// </summary>
    public int DeviceIndex { get; set; }
    
    /// <summary>
    /// 当前波特率
    /// </summary>
    public int BaudRate { get; set; }
    
    /// <summary>
    /// 通道状态
    /// </summary>
    public ChannelStatus Status { get; set; }
    
    /// <summary>
    /// 是否为CAN FD通道
    /// </summary>
    public bool IsCanFDSupported { get; set; }
    
    /// <summary>
    /// CAN FD波特率（数据段）
    /// </summary>
    public int CanFDBaudRate { get; set; }
    
    /// <summary>
    /// 通道初始化配置
    /// </summary>
    public CanInitConfig InitConfig { get; set; }
}

/// <summary>
/// 通道状态枚举
/// </summary>
public enum ChannelStatus
{
    Disconnected,
    Connected,
    Error,
    Initializing
}
```

### 2.6 CanStatistics

```csharp
/// <summary>
/// CAN统计信息模型
/// </summary>
public class CanStatistics
{
    /// <summary>
    /// 总接收帧数
    /// </summary>
    public long TotalReceivedFrames { get; set; }
    
    /// <summary>
    /// 总发送帧数
    /// </summary>
    public long TotalSentFrames { get; set; }
    
    /// <summary>
    /// 错误帧数
    /// </summary>
    public long ErrorFrames { get; set; }
    
    /// <summary>
    /// 丢帧数
    /// </summary>
    public long DroppedFrames { get; set; }
    
    /// <summary>
    /// 总线负载率（百分比）
    /// </summary>
    public double BusLoadPercentage { get; set; }
    
    /// <summary>
    /// 当前波特率
    /// </summary>
    public int CurrentBaudRate { get; set; }
    
    /// <summary>
    /// 最后统计时间
    /// </summary>
    public DateTime LastStatisticsTime { get; set; }
    
    /// <summary>
    /// 重置统计信息
    /// </summary>
    public void Reset()
    {
        TotalReceivedFrames = 0;
        TotalSentFrames = 0;
        ErrorFrames = 0;
        DroppedFrames = 0;
        BusLoadPercentage = 0;
        LastStatisticsTime = DateTime.Now;
    }
}
```

### 2.7 BatchMessageContainer

```csharp
/// <summary>
/// 批量消息容器模型
/// </summary>
public class BatchMessageContainer
{
    /// <summary>
    /// 标准CAN消息列表
    /// </summary>
    public List<CanMessage> CanMessages { get; set; }
    
    /// <summary>
    /// CAN FD消息列表
    /// </summary>
    public List<CanFdMessage> CanFdMessages { get; set; }
    
    /// <summary>
    /// 消息总数
    /// </summary>
    public int TotalCount => CanMessages.Count + CanFdMessages.Count;
    
    /// <summary>
    /// 构造函数
    /// </summary>
    public BatchMessageContainer()
    {
        CanMessages = new List<CanMessage>();
        CanFdMessages = new List<CanFdMessage>();
    }
    
    /// <summary>
    /// 添加标准CAN消息
    /// </summary>
    /// <param name="message">CAN消息</param>
    public void AddCanMessage(CanMessage message)
    {
        CanMessages.Add(message);
    }
    
    /// <summary>
    /// 添加CAN FD消息
    /// </summary>
    /// <param name="message">CAN FD消息</param>
    public void AddCanFdMessage(CanFdMessage message)
    {
        CanFdMessages.Add(message);
    }
    
    /// <summary>
    /// 清空所有消息
    /// </summary>
    public void Clear()
    {
        CanMessages.Clear();
        CanFdMessages.Clear();
    }
}
```

### 2.8 AutoTransmitConfig

```csharp
/// <summary>
/// 自动发送配置模型
/// </summary>
public class AutoTransmitConfig
{
    /// <summary>
    /// 发送对象ID
    /// </summary>
    public int ObjectId { get; set; }
    
    /// <summary>
    /// 发送周期（毫秒）
    /// </summary>
    public uint Cycle { get; set; }
    
    /// <summary>
    /// 发送次数，0表示无限循环
    /// </summary>
    public uint Count { get; set; }
    
    /// <summary>
    /// 发送消息
    /// </summary>
    public CanMessage Message { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// 转换为ZLGCAN自动发送对象
    /// </summary>
    /// <returns>ZLGCAN自动发送对象</returns>
    public ZCAN_AUTO_TRANSMIT_OBJ ToZlgAutoTransmitObj()
    {
        var frame = Message.ToCanFrame();
        return new ZCAN_AUTO_TRANSMIT_OBJ
        {
            object_id = (byte)ObjectId,
            Cycle = Cycle,
            Count = Count,
            Frame = frame,
            Enable = (byte)(Enabled ? 1 : 0)
        };
    }
}

/// <summary>
/// CAN FD自动发送配置模型
/// </summary>
public class CanFdAutoTransmitConfig
{
    /// <summary>
    /// 发送对象ID
    /// </summary>
    public int ObjectId { get; set; }
    
    /// <summary>
    /// 发送周期（毫秒）
    /// </summary>
    public uint Cycle { get; set; }
    
    /// <summary>
    /// 发送次数，0表示无限循环
    /// </summary>
    public uint Count { get; set; }
    
    /// <summary>
    /// 发送消息
    /// </summary>
    public CanFdMessage Message { get; set; }
    
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }
    
    /// <summary>
    /// 转换为ZLGCAN CAN FD自动发送对象
    /// </summary>
    /// <returns>ZLGCAN CAN FD自动发送对象</returns>
    public ZCANFD_AUTO_TRANSMIT_OBJ ToZlgCanFdAutoTransmitObj()
    {
        var frame = Message.ToCanFdFrame();
        return new ZCANFD_AUTO_TRANSMIT_OBJ
        {
            object_id = (byte)ObjectId,
            Cycle = Cycle,
            Count = Count,
            Frame = frame,
            Enable = (byte)(Enabled ? 1 : 0)
        };
    }
}

```csharp
/// <summary>
/// CAN统计信息模型
/// </summary>
public class CanStatistics
{
    /// <summary>
    /// 总接收帧数
    /// </summary>
    public long TotalReceivedFrames { get; set; }
    
    /// <summary>
    /// 总发送帧数
    /// </summary>
    public long TotalSentFrames { get; set; }
    
    /// <summary>
    /// 错误帧数
    /// </summary>
    public long ErrorFrames { get; set; }
    
    /// <summary>
    /// 丢帧数
    /// </summary>
    public long DroppedFrames { get; set; }
    
    /// <summary>
    /// 总线负载率（百分比）
    /// </summary>
    public double BusLoadPercentage { get; set; }
    
    /// <summary>
    /// 当前波特率
    /// </summary>
    public int CurrentBaudRate { get; set; }
    
    /// <summary>
    /// 最后统计时间
    /// </summary>
    public DateTime LastStatisticsTime { get; set; }
    
    /// <summary>
    /// 重置统计信息
    /// </summary>
    public void Reset()
    {
        TotalReceivedFrames = 0;
        TotalSentFrames = 0;
        ErrorFrames = 0;
        DroppedFrames = 0;
        BusLoadPercentage = 0;
        LastStatisticsTime = DateTime.Now;
    }
}
```

## 3. 辅助数据模型

### 3.1 MessageRecordConfig

```csharp
/// <summary>
/// 报文记录配置模型
/// </summary>
public class MessageRecordConfig
{
    /// <summary>
    /// 记录文件路径
    /// </summary>
    public string FilePath { get; set; }
    
    /// <summary>
    /// 记录格式
    /// </summary>
    public RecordFormat Format { get; set; }
    
    /// <summary>
    /// 记录模式
    /// </summary>
    public RecordMode Mode { get; set; }
    
    /// <summary>
    /// 记录过滤器
    /// </summary>
    public List<CanFilter> Filters { get; set; }
    
    /// <summary>
    /// 最大文件大小（MB）
    /// </summary>
    public int MaxFileSize { get; set; }
    
    /// <summary>
    /// 是否包含时间戳
    /// </summary>
    public bool IncludeTimestamp { get; set; }
}

/// <summary>
/// 记录格式枚举
/// </summary>
public enum RecordFormat
{
    BLF,
    ASC,
    CSV,
    RAW
}

/// <summary>
/// 记录模式枚举
/// </summary>
public enum RecordMode
{
    Continuous,
    Triggered,
    Scheduled
}
```

### 3.2 MessageSendTask

```csharp
/// <summary>
/// 报文发送任务模型
/// </summary>
public class MessageSendTask
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public string TaskId { get; set; }
    
    /// <summary>
    /// CAN报文
    /// </summary>
    public CanMessage Message { get; set; }
    
    /// <summary>
    /// 发送模式
    /// </summary>
    public SendMode Mode { get; set; }
    
    /// <summary>
    /// 发送间隔（毫秒）
    /// </summary>
    public int Interval { get; set; }
    
    /// <summary>
    /// 重复次数
    /// </summary>
    public int RepeatCount { get; set; }
    
    /// <summary>
    /// 当前已发送次数
    /// </summary>
    public int CurrentCount { get; set; }
    
    /// <summary>
    /// 任务状态
    /// </summary>
    public TaskStatus Status { get; set; }
    
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreatedTime { get; set; }
}

/// <summary>
/// 发送模式枚举
/// </summary>
public enum SendMode
{
    Single,    // 单次发送
    Periodic,  // 周期发送
    Burst      // 突发发送
}

/// <summary>
/// 任务状态枚举
/// </summary>
public enum TaskStatus
{
    Ready,
    Running,
    Paused,
    Completed,
    Error
}
```

## 4. 数据模型关系

```
CanMessage 1--* SignalValue
CanChannel 1--1 CanStatistics
MessageSendTask 1--1 CanMessage
CanFilter -- CanMessage (过滤关系)
```

## 5. 序列化支持

所有数据模型均支持JSON序列化和反序列化，便于数据持久化和网络传输。

```csharp
// 示例：序列化和反序列化
var jsonSerializer = new JsonSerializer();
var messageJson = JsonConvert.SerializeObject(canMessage);
var deserializedMessage = JsonConvert.DeserializeObject<CanMessage>(messageJson);
```

## 6. 性能优化考虑

- CanMessage使用预分配的byte数组减少内存分配
- SignalValue使用值类型存储原始数据
- 大数据量场景下考虑使用对象池减少GC压力
- 避免频繁的字符串操作，使用StringBuilder

## 7. 扩展性考虑

- 设计为可继承的类结构，便于功能扩展
- 支持自定义属性扩展
- 支持插件式的数据处理扩展