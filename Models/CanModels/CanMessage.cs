using System;
using System.Collections.Generic;

namespace CANMonitor.Models.CanModels
{
    /// <summary>
    /// CAN 报文模型类
    /// 用于表示和处理 CAN 网络中的报文数据
    /// </summary>
    public class CanMessage
    {
        /// <summary>
        /// 报文 ID
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// 报文数据内容
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 报文时间戳
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// 是否为扩展帧
        /// </summary>
        public bool IsExtendedFrame { get; set; }

        /// <summary>
        /// 报文名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 包含的信号值列表
        /// </summary>
        public List<SignalValue> SignalValues { get; set; }

        /// <summary>
        /// 通道名称
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CanMessage()
        {
            Data = new byte[8];
            Timestamp = DateTime.Now;
            IsExtendedFrame = false;
            SignalValues = new List<SignalValue>();
            Channel = string.Empty;
        }

        /// <summary>
        /// 带通道名称的构造函数
        /// </summary>
        /// <param name="channel">通道名称</param>
        public CanMessage(string channel)
            : this()
        {
            Channel = channel;
        }
    }

    /// <summary>
    /// 信号值模型类
    /// 用于表示 CAN 报文中的单个信号数据
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
        /// 格式化后的信号值
        /// </summary>
        public string FormattedValue { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public SignalValue()
        {
            Name = string.Empty;
            Unit = string.Empty;
            FormattedValue = string.Empty;
        }
    }
}