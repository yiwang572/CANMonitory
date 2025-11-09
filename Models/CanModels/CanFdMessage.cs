using System;
using System.Collections.Generic;

namespace CANMonitor.Models.CanModels
{
    /// <summary>
    /// CAN FD 报文模型类
    /// 用于表示和处理 CAN FD 网络中的报文数据
    /// </summary>
    public class CanFdMessage
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
        /// 波特率切换标志位
        /// </summary>
        public bool Brs { get; set; }

        /// <summary>
        /// 错误状态指示位
        /// </summary>
        public bool Esi { get; set; }

        /// <summary>
        /// 是否为FD格式
        /// </summary>
        public bool IsFdFormat { get; set; }

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
        /// 数据长度（1-64字节）
        /// </summary>
        public int DataLength
        {
            get { return Data?.Length ?? 0; }
            set { /* 只读属性 */ }
        }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CanFdMessage()
        {
            Data = new byte[64];
            Timestamp = DateTime.Now;
            IsExtendedFrame = false;
            Brs = false;
            Esi = false;
            IsFdFormat = true;
            SignalValues = new List<SignalValue>();
            Channel = string.Empty;
        }

        /// <summary>
        /// 带通道名称的构造函数
        /// </summary>
        /// <param name="channel">通道名称</param>
        public CanFdMessage(string channel)
            : this()
        {
            Channel = channel;
        }

        /// <summary>
        /// 从底层CAN FD帧转换为CanFdMessage对象
        /// </summary>
        /// <param name="canFdFrame">底层CAN FD帧结构</param>
        /// <returns>转换后的CanFdMessage对象</returns>
        public static CanFdMessage FromCanFdFrame(object canFdFrame)
        {
            // 这里应该根据具体的底层API实现转换逻辑
            // 由于是模拟实现，返回一个默认对象
            return new CanFdMessage();
        }

        /// <summary>
        /// 转换为底层CAN FD帧格式
        /// </summary>
        /// <returns>底层CAN FD帧结构</returns>
        public object ToCanFdFrame()
        {
            // 这里应该根据具体的底层API实现转换逻辑
            // 由于是模拟实现，返回null
            return null;
        }
    }
}