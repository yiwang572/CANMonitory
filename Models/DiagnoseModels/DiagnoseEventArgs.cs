using System;
using System.Collections.Generic;

namespace CANMonitor.Models.DiagnoseModels
{
    /// <summary>
    /// 连接状态变化事件参数
    /// </summary>
    public class ConnectionStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected { get; set; }
        
        /// <summary>
        /// 连接错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
    }
    
    /// <summary>
    /// DTC更新事件参数
    /// </summary>
    public class DtcUpdatedEventArgs : EventArgs
    {
        /// <summary>
        /// DTC列表
        /// </summary>
        public List<DtcInfo> DtcList { get; set; }
    }
}