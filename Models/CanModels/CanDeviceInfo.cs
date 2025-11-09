using System;

namespace CANMonitor.Models.CanModels
{
    /// <summary>
    /// CAN设备信息模型类
    /// 用于表示CAN设备的基本信息
    /// </summary>
    public class CanDeviceInfo
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

        /// <summary>
        /// 硬件版本
        /// </summary>
        public string HardwareVersion { get; set; }

        /// <summary>
        /// 驱动版本
        /// </summary>
        public string DriverVersion { get; set; }

        /// <summary>
        /// 可用通道数
        /// </summary>
        public int AvailableChannels { get; set; }

        /// <summary>
        /// 设备描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 设备状态
        /// </summary>
        public DeviceStatus Status { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CanDeviceInfo()
        {
            DeviceType = string.Empty;
            DeviceIndex = 0;
            DeviceName = string.Empty;
            SerialNumber = string.Empty;
            FirmwareVersion = string.Empty;
            HardwareVersion = string.Empty;
            DriverVersion = string.Empty;
            AvailableChannels = 0;
            Description = string.Empty;
            Status = DeviceStatus.Disconnected;
        }
    }
}