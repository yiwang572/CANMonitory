using System;

namespace CANMonitor.Models.DiagnoseModels
{
    /// <summary>
    /// DTC（故障诊断码）信息类
    /// 用于存储和显示车辆故障码相关信息
    /// </summary>
    public class DtcInfo
    {
        /// <summary>
        /// DTC故障码
        /// </summary>
        public string DtcCode { get; set; }

        /// <summary>
        /// 故障描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 故障状态
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// 严重程度
        /// </summary>
        public string Severity { get; set; }

        /// <summary>
        /// 是否有冻结帧数据
        /// </summary>
        public bool HasFreezeFrame { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DtcInfo()
        {
            CreateTime = DateTime.Now;
            Status = "活跃";
            Severity = "中等";
            HasFreezeFrame = false;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dtcCode">DTC故障码</param>
        /// <param name="description">故障描述</param>
        public DtcInfo(string dtcCode, string description)
            : this()
        {
            DtcCode = dtcCode;
            Description = description;
        }
    }
}