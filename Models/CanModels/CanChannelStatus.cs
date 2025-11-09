using System;

namespace CANMonitor.Models.CanModels
{
    /// <summary>
    /// CAN通道状态模型类
    /// 用于表示CAN通道的当前状态信息
    /// </summary>
    public class CanChannelStatus
    {
        /// <summary>
        /// 设备类型
        /// </summary>
        public string DeviceType { get; set; }

        /// <summary>
        /// 设备索引
        /// </summary>
        public int DeviceIndex { get; set; }

        /// <summary>
        /// 通道号
        /// </summary>
        public int ChannelNumber { get; set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string ChannelName { get; set; }

        /// <summary>
        /// 通道状态
        /// </summary>
        public ChannelStatus Status { get; set; }

        /// <summary>
        /// 波特率
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// 数据段波特率（FD模式）
        /// </summary>
        public int DataBaudRate { get; set; }

        /// <summary>
        /// 是否处于FD模式
        /// </summary>
        public bool IsFdMode { get; set; }

        /// <summary>
        /// 是否处于回环模式
        /// </summary>
        public bool IsLoopbackMode { get; set; }

        /// <summary>
        /// 是否处于静默模式
        /// </summary>
        public bool IsSilentMode { get; set; }

        /// <summary>
        /// 接收错误计数器
        /// </summary>
        public int RxErrCounter { get; set; }

        /// <summary>
        /// 发送错误计数器
        /// </summary>
        public int TxErrCounter { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        public DeviceStatus DeviceStatus { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CanChannelStatus()
        {
            DeviceType = string.Empty;
            DeviceIndex = 0;
            ChannelNumber = 0;
            ChannelName = string.Empty;
            Status = ChannelStatus.Closed;
            BaudRate = 0;
            DataBaudRate = 0;
            IsFdMode = false;
            IsLoopbackMode = false;
            IsSilentMode = false;
            RxErrCounter = 0;
            TxErrCounter = 0;
            DeviceStatus = DeviceStatus.Disconnected;
        }

        /// <summary>
        /// 重置通道状态
        /// </summary>
        public void Reset()
        {
            Status = ChannelStatus.Closed;
            RxErrCounter = 0;
            TxErrCounter = 0;
            // 保留设备和通道的基本信息
        }
    }

    /// <summary>
    /// 通道状态枚举
    /// </summary>
    public enum ChannelStatus
    {
        /// <summary>
        /// 关闭
        /// </summary>
        Closed = 0,

        /// <summary>
        /// 已初始化
        /// </summary>
        Initialized = 1,

        /// <summary>
        /// 运行中
        /// </summary>
        Running = 2,

        /// <summary>
        /// 错误状态
        /// </summary>
        Error = 3,

        /// <summary>
        /// 总线关闭
        /// </summary>
        BusOff = 4
    }

    /// <summary>
    /// 设备状态枚举
    /// </summary>
    public enum DeviceStatus
    {
        /// <summary>
        /// 断开连接
        /// </summary>
        Disconnected = 0,

        /// <summary>
        /// 已连接
        /// </summary>
        Connected = 1,

        /// <summary>
        /// 初始化中
        /// </summary>
        Initializing = 2,

        /// <summary>
        /// 错误状态
        /// </summary>
        Error = 3
    }
}