using System;
using System.Collections.Generic;

namespace CANMonitor.Interfaces
{
    /// <summary>
    /// CAN设备信息类
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
        /// 设备序列号
        /// </summary>
        public string SerialNumber { get; set; }
        
        /// <summary>
        /// 固件版本
        /// </summary>
        public string FirmwareVersion { get; set; }
    }
    
    /// <summary>
    /// CAN初始化配置类
    /// </summary>
    public class CanInitConfig
    {
        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; set; }
        
        /// <summary>
        /// 是否为CAN FD模式
        /// </summary>
        public bool IsCanFd { get; set; }
        
        /// <summary>
        /// 数据波特率（CAN FD模式）
        /// </summary>
        public int DataBaudRate { get; set; }
        
        /// <summary>
        /// 定时参数0
        /// </summary>
        public int Timing0 { get; set; }
        
        /// <summary>
        /// 定时参数1
        /// </summary>
        public int Timing1 { get; set; }
        
        /// <summary>
        /// 接收超时时间（毫秒）
        /// </summary>
        public int ReceiveTimeout { get; set; }
    }
    
    /// <summary>
    /// CAN错误信息类
    /// </summary>
    public class CanErrorInfo
    {
        /// <summary>
        /// 错误码
        /// </summary>
        public int ErrorCode { get; set; }
        
        /// <summary>
        /// 错误描述
        /// </summary>
        public string ErrorDescription { get; set; }
        
        /// <summary>
        /// 错误时间
        /// </summary>
        public DateTime ErrorTime { get; set; }
    }
    
    /// <summary>
    /// CAN通道状态类
    /// </summary>
    public class CanChannelStatus
    {
        /// <summary>
        /// 通道索引
        /// </summary>
        public uint ChannelIndex { get; set; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; set; }
        
        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get; set; }
        
        /// <summary>
        /// 接收报文数
        /// </summary>
        public uint ReceiveCount { get; set; }
        
        /// <summary>
        /// 发送报文数
        /// </summary>
        public uint SendCount { get; set; }
        
        /// <summary>
        /// 错误计数
        /// </summary>
        public uint ErrorCount { get; set; }
    }
};

namespace CANMonitor.Services.CanServices
{
    /// <summary>
    /// CAN信号值模型，表示CAN报文中解析出的信号值
    /// </summary>
    public class SignalValue
    {
        /// <summary>
        /// 信号名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 信号数值
        /// </summary>
        public double Value { get; set; }

        /// <summary>
        /// 信号单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 格式化后的信号值（包含单位）
        /// </summary>
        public string FormattedValue { get; set; }
    }

    /// <summary>
    /// CAN通道配置类，用于存储通道连接参数
    /// </summary>
    public class CanChannelConfig
    {
        /// <summary>
        /// 通道名称
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// 是否使用CAN FD模式
        /// </summary>
        public bool IsCanFd { get; set; }

        /// <summary>
        /// 数据波特率（CAN FD模式下使用）
        /// </summary>
        public int DataBaudRate { get; set; }
    }

    /// <summary>
    /// CAN通信服务接口，定义CAN总线通信的基本操作
    /// 支持多通道配置和管理，基于ZLGCAN接口实现
    /// </summary>
    public interface ICanService
    {
        /// <summary>
        /// 打开设备
        /// </summary>
        /// <param name="deviceType">设备类型</param>
        /// <param name="deviceIndex">设备索引</param>
        /// <returns>操作是否成功</returns>
        bool OpenDevice(int deviceType, int deviceIndex);

        /// <summary>
        /// 关闭设备
        /// </summary>
        void CloseDevice();

        /// <summary>
        /// 初始化CAN通道
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="config">初始化配置</param>
        /// <returns>操作是否成功</returns>
        bool InitCAN(uint channelIndex, CanInitConfig config);

        /// <summary>
        /// 启动CAN通道
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <returns>操作是否成功</returns>
        bool StartCAN(uint channelIndex);

        /// <summary>
        /// 停止CAN通道
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        void StopCAN(uint channelIndex);

        /// <summary>
        /// 重置CAN通道
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        void ResetCAN(uint channelIndex);

        /// <summary>
        /// 发送CAN报文
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="message">CAN报文</param>
        /// <returns>发送成功的报文数</returns>
        int SendMessage(uint channelIndex, CanMessage message);

        /// <summary>
        /// 批量发送CAN报文
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="messages">CAN报文列表</param>
        /// <returns>发送成功的报文数</returns>
        int SendMessages(uint channelIndex, List<CanMessage> messages);

        /// <summary>
        /// 发送CAN FD报文
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="message">CAN FD报文</param>
        /// <returns>发送成功的报文数</returns>
        int SendMessageFD(uint channelIndex, CanFdMessage message);

        /// <summary>
        /// 批量发送CAN FD报文
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="messages">CAN FD报文列表</param>
        /// <returns>发送成功的报文数</returns>
        int SendMessagesFD(uint channelIndex, List<CanFdMessage> messages);

        /// <summary>
        /// 接收CAN报文
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="maxCount">最大接收数量</param>
        /// <returns>接收到的CAN报文列表</returns>
        List<CanMessage> ReceiveMessage(uint channelIndex, int maxCount = 100);

        /// <summary>
        /// 接收CAN FD报文
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="maxCount">最大接收数量</param>
        /// <returns>接收到的CAN FD报文列表</returns>
        List<CanFdMessage> ReceiveMessageFD(uint channelIndex, int maxCount = 100);

        /// <summary>
        /// 开始自动接收
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="interval">接收间隔（毫秒）</param>
        void StartAutoReceive(uint channelIndex, int interval = 50);

        /// <summary>
        /// 停止自动接收
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        void StopAutoReceive(uint channelIndex);

        /// <summary>
        /// 获取通道状态
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <returns>通道状态</returns>
        CanChannelStatus GetChannelStatus(uint channelIndex);

        /// <summary>
        /// 获取通道错误信息
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <returns>错误信息</returns>
        CanErrorInfo GetChannelErrorInfo(uint channelIndex);

        /// <summary>
        /// 设置报文过滤器
        /// </summary>
        /// <param name="channelIndex">通道索引</param>
        /// <param name="filterType">过滤器类型</param>
        /// <param name="filterConfig">过滤器配置</param>
        /// <returns>操作是否成功</returns>
        bool SetMessageFilter(uint channelIndex, int filterType, object filterConfig);

        /// <summary>
        /// 获取可用设备列表
        /// </summary>
        /// <returns>设备信息列表</returns>
        List<CanDeviceInfo> GetAvailableDevices();

        /// <summary>
        /// 检查设备是否已打开
        /// </summary>
        bool IsDeviceOpened { get; }

        /// <summary>
        /// CAN报文接收事件
        /// </summary>
        event EventHandler<List<CanMessage>> MessagesReceived;

        /// <summary>
        /// CAN FD报文接收事件
        /// </summary>
        event EventHandler<List<CanFdMessage>> MessagesReceivedFD;

        /// <summary>
        /// 通道状态变化事件
        /// </summary>
        event EventHandler<CanChannelStatus> ChannelStatusChanged;

        /// <summary>
        /// 错误事件
        /// </summary>
        event EventHandler<CanErrorInfo> ErrorOccurred;
    }

    /// <summary>
    /// CAN报文模型，表示一条CAN总线上的报文
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
        /// 数据长度
        /// </summary>
        public byte DataLength { get; set; }

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
        /// 通道索引
        /// </summary>
        public uint ChannelIndex { get; set; }

        /// <summary>
        /// 报文名称（通过DBC解析获得）
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 解析后的信号值列表
        /// </summary>
        public List<SignalValue> SignalValues { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public CanMessage()
        {
            Data = new byte[8];
            SignalValues = new List<SignalValue>();
            Timestamp = 0;
            DataLength = 8;
            ChannelIndex = 0;
        }
        
        /// <summary>
        /// 转换为ZLGCAN can_frame结构体
        /// </summary>
        /// <returns>ZLGCAN can_frame结构体</returns>
        public object ToCanFrame();
        
        /// <summary>
        /// 从ZLGCAN can_frame结构体转换
        /// </summary>
        /// <param name="frame">ZLGCAN can_frame结构体</param>
        /// <param name="timestamp">时间戳</param>
        /// <returns>CanMessage对象</returns>
        public static CanMessage FromCanFrame(object frame, ulong timestamp = 0);
    }
    
    /// <summary>
    /// CAN FD报文模型，表示一条CAN FD总线上的报文
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
        /// 数据长度
        /// </summary>
        public byte DataLength { get; set; }
        
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
        /// 通道索引
        /// </summary>
        public uint ChannelIndex { get; set; }
        
        /// <summary>
        /// 报文名称（通过DBC解析获得）
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 解析后的信号值列表
        /// </summary>
        public List<SignalValue> SignalValues { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public CanFdMessage()
        {
            Data = new byte[64];
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
        public object ToCanFdFrame();
        
        /// <summary>
        /// 从ZLGCAN canfd_frame结构体转换
        /// </summary>
        /// <param name="frame">ZLGCAN canfd_frame结构体</param>
        /// <param name="timestamp">时间戳</param>
        /// <returns>CanFdMessage对象</returns>
        public static CanFdMessage FromCanFdFrame(object frame, ulong timestamp = 0);
    }
}