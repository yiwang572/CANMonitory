using System;

namespace CANMonitor.Models.CanModels
{
    /// <summary>
    /// CAN初始化配置类
    /// 用于配置CAN设备的初始化参数
    /// </summary>
    public class CanInitConfig
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
        /// 波特率（标准模式）
        /// </summary>
        public int BaudRate { get; set; }

        /// <summary>
        /// 波特率（FD模式下的数据段波特率）
        /// </summary>
        public int DataBaudRate { get; set; }

        /// <summary>
        /// 是否启用FD模式
        /// </summary>
        public bool IsFdMode { get; set; }

        /// <summary>
        /// 是否启用回环模式
        /// </summary>
        public bool IsLoopbackMode { get; set; }

        /// <summary>
        /// 是否启用静默模式
        /// </summary>
        public bool IsSilentMode { get; set; }

        /// <summary>
        /// 是否启用单通道模式
        /// </summary>
        public bool IsSingleChannelMode { get; set; }

        /// <summary>
        /// 滤波方式
        /// </summary>
        public FilterMode FilterMode { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CanInitConfig()
        {
            DeviceType = "USBCANFD-200U";
            DeviceIndex = 0;
            ChannelNumber = 0;
            BaudRate = 500000;
            DataBaudRate = 2000000;
            IsFdMode = false;
            IsLoopbackMode = false;
            IsSilentMode = false;
            IsSingleChannelMode = false;
            FilterMode = FilterMode.All;
        }

        /// <summary>
        /// 设置波特率（支持10Kbps至1Mbps）
        /// </summary>
        /// <param name="baudRate">波特率值</param>
        /// <returns>设置是否成功</returns>
        public bool SetBaudRate(int baudRate)
        {
            // 验证波特率范围
            if (baudRate < 10000 || baudRate > 1000000)
            {
                return false;
            }

            // 检查是否为常用波特率
            int[] commonBaudRates = {
                10000, 20000, 50000, 100000, 125000, 250000, 
                500000, 800000, 1000000
            };

            bool isValidBaudRate = false;
            foreach (var rate in commonBaudRates)
            {
                if (rate == baudRate)
                {
                    isValidBaudRate = true;
                    break;
                }
            }

            if (!isValidBaudRate)
            {
                return false;
            }

            BaudRate = baudRate;
            return true;
        }

        /// <summary>
        /// 转换为ZLG初始化配置
        /// </summary>
        /// <returns>ZLG库所需的初始化配置结构</returns>
        public object ToZlgInitConfig()
        {
            // 这里应该根据具体的ZLG SDK实现转换逻辑
            // 由于是模拟实现，返回null
            return null;
        }
    }

    /// <summary>
    /// 过滤模式枚举
    /// </summary>
    public enum FilterMode
    {
        /// <summary>
        /// 接收所有帧
        /// </summary>
        All = 0,

        /// <summary>
        /// 只接收标准帧
        /// </summary>
        Standard = 1,

        /// <summary>
        /// 只接收扩展帧
        /// </summary>
        Extended = 2,

        /// <summary>
        /// 自定义过滤
        /// </summary>
        Custom = 3
    }
}