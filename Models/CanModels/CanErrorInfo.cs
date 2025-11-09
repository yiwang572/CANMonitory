using System;

namespace CANMonitor.Models.CanModels
{
    /// <summary>
    /// CAN错误信息模型类
    /// 用于表示CAN通信过程中的错误信息
    /// </summary>
    public class CanErrorInfo
    {
        /// <summary>
        /// 错误状态码
        /// </summary>
        public int ErrStatus { get; set; }

        /// <summary>
        /// 接收错误计数器
        /// </summary>
        public int RxErrCounter { get; set; }

        /// <summary>
        /// 发送错误计数器
        /// </summary>
        public int TxErrCounter { get; set; }

        /// <summary>
        /// 通道号
        /// </summary>
        public int ChannelNumber { get; set; }

        /// <summary>
        /// 错误发生时间
        /// </summary>
        public DateTime ErrorTime { get; set; }

        /// <summary>
        /// 错误类型
        /// </summary>
        public ErrorType ErrorType { get; set; }

        /// <summary>
        /// 错误描述
        /// </summary>
        public string ErrorDescription { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CanErrorInfo()
        {
            ErrStatus = 0;
            RxErrCounter = 0;
            TxErrCounter = 0;
            ChannelNumber = 0;
            ErrorTime = DateTime.Now;
            ErrorType = ErrorType.None;
            ErrorDescription = string.Empty;
        }

        /// <summary>
        /// 从ZLG错误信息转换为CanErrorInfo对象
        /// </summary>
        /// <param name="zlgErrorInfo">ZLG SDK错误信息结构</param>
        /// <returns>转换后的CanErrorInfo对象</returns>
        public static CanErrorInfo FromZlgErrorInfo(object zlgErrorInfo)
        {
            // 这里应该根据具体的ZLG SDK实现转换逻辑
            // 由于是模拟实现，返回一个默认对象
            return new CanErrorInfo();
        }
    }

    /// <summary>
    /// 错误类型枚举
    /// </summary>
    public enum ErrorType
    {
        /// <summary>
        /// 无错误
        /// </summary>
        None = 0,

        /// <summary>
        /// 位错误
        /// </summary>
        BitError = 1,

        /// <summary>
        /// 格式错误
        /// </summary>
        FormatError = 2,

        /// <summary>
        /// CRC错误
        /// </summary>
        CrcError = 3,

        /// <summary>
        /// 应答错误
        /// </summary>
        AckError = 4,

        /// <summary>
        /// 总线关闭
        /// </summary>
        BusOff = 5,

        /// <summary>
        /// 超时错误
        /// </summary>
        Timeout = 6,

        /// <summary>
        /// 其他错误
        /// </summary>
        Other = 7
    }
}