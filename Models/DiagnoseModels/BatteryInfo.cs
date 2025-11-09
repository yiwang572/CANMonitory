namespace CANMonitor.Models.DiagnoseModels
{
    /// <summary>
    /// 电池信息
    /// </summary>
    public class BatteryInfo
    {
        /// <summary>
        /// 总电压
        /// </summary>
        public float Voltage { get; set; }
        
        /// <summary>
        /// 电流
        /// </summary>
        public float Current { get; set; }
        
        /// <summary>
        /// 荷电状态
        /// </summary>
        public float SOC { get; set; }
        
        /// <summary>
        /// 健康状态
        /// </summary>
        public float SOH { get; set; }
        
        /// <summary>
        /// 温度
        /// </summary>
        public float Temperature { get; set; }
    }
}