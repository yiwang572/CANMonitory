using System;
using CANMonitor.Interfaces;
using CANMonitor.Models.CanModels;

namespace CANMonitor.Services.CanServices
{
    /// <summary>
    /// CAN服务工厂类
    /// 用于创建不同类型的CAN服务实例
    /// </summary>
    public class CanServiceFactory
    {
        /// <summary>
        /// 创建CAN服务实例
        /// </summary>
        /// <param name="deviceType">设备类型</param>
        /// <returns>CAN服务实例</returns>
        public static ICanService CreateCanService(string deviceType = "ZLG")
        {
            switch (deviceType.ToUpper())
            {
                case "ZLG":
                case "USBCANFD":
                case "USBCANFD-200U":
                    return new ZLGUsbCanService();
                
                // 可以在这里添加更多的设备类型支持
                // case "PEAK":
                //     return new PeakCanService();
                // case "VECTOR":
                //     return new VectorCanService();
                
                default:
                    throw new ArgumentException($"不支持的设备类型: {deviceType}");
            }
        }
        
        /// <summary>
        /// 根据初始化配置创建CAN服务实例
        /// </summary>
        /// <param name="config">CAN初始化配置</param>
        /// <returns>CAN服务实例</returns>
        public static ICanService CreateCanService(CanInitConfig config)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }
            
            // 根据配置中的设备类型创建相应的服务实例
            return CreateCanService(config.DeviceType);
        }
    }
}