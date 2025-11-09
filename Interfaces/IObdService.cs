using System;
using System.Collections.Generic;
using CANMonitor.Models.DiagnoseModels;

namespace CANMonitor.Interfaces
{
    /// <summary>
    /// OBD诊断服务接口
    /// 提供车辆诊断相关功能
    /// </summary>
    public interface IObdService
    {
        /// <summary>
        /// 连接状态变化事件
        /// </summary>
        event EventHandler<ConnectionStatusChangedEventArgs> ConnectionStatusChanged;
        
        /// <summary>
        /// DTC更新事件
        /// </summary>
        event EventHandler<DtcUpdatedEventArgs> DtcUpdated;
        
        /// <summary>
        /// 连接到诊断目标
        /// </summary>
        /// <param name="targetAddress">目标地址</param>
        /// <returns>连接是否成功</returns>
        bool Connect(ushort targetAddress);
        
        /// <summary>
        /// 断开诊断连接
        /// </summary>
        void Disconnect();
        
        /// <summary>
        /// 设置诊断会话
        /// </summary>
        /// <param name="sessionId">会话ID</param>
        /// <returns>是否成功</returns>
        bool SetDiagnosticSession(byte sessionId);
        
        /// <summary>
        /// 请求安全访问
        /// </summary>
        /// <param name="securityLevel">安全级别</param>
        /// <returns>是否成功</returns>
        bool RequestSecurityAccess(byte securityLevel);
        
        /// <summary>
        /// ECU重置
        /// </summary>
        /// <param name="resetType">重置类型</param>
        /// <returns>是否成功</returns>
        bool ECUReset(byte resetType);
        
        /// <summary>
        /// 读取ECU标识信息
        /// </summary>
        /// <param name="infoType">信息类型</param>
        /// <returns>标识数据</returns>
        byte[] ReadEcuIdentification(byte infoType);
        
        /// <summary>
        /// 读取故障码
        /// </summary>
        /// <param name="mode">读取模式</param>
        /// <returns>DTC列表</returns>
        List<DtcInfo> ReadDTCs(byte mode);
        
        /// <summary>
        /// 清除所有故障码
        /// </summary>
        /// <returns>是否成功</returns>
        bool ClearDTCs();
        
        /// <summary>
        /// 读取电池信息
        /// </summary>
        /// <returns>电池信息</returns>
        BatteryInfo ReadBatteryInfo();
        
        /// <summary>
        /// 开始电池均衡
        /// </summary>
        /// <returns>是否成功</returns>
        bool StartCellBalancing();
        
        /// <summary>
        /// 停止电池均衡
        /// </summary>
        /// <returns>是否成功</returns>
        bool StopCellBalancing();
        
        /// <summary>
        /// 读取单体电压
        /// </summary>
        /// <returns>单体电压列表</returns>
        List<float> ReadCellVoltages();
        
        /// <summary>
        /// 读取温度传感器
        /// </summary>
        /// <returns>温度传感器值列表</returns>
        List<float> ReadTemperatureSensors();
        
        /// <summary>
        /// 读取故障状态
        /// </summary>
        /// <returns>故障状态信息</returns>
        FaultStatus ReadFaultStatus();
    }
}