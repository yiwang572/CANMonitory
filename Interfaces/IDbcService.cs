using System.Collections.Generic;

namespace CANMonitor.Services.DbcServices
{
    /// <summary>
    /// DBC信号模型，表示DBC文件中的一个信号定义
    /// </summary>
    public class DbcSignal
    {
        /// <summary>
        /// 信号名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 所属报文名称
        /// </summary>
        public string MessageName { get; set; }

        /// <summary>
        /// 起始位
        /// </summary>
        public int StartBit { get; set; }

        /// <summary>
        /// 信号长度（位）
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 是否为有符号数
        /// </summary>
        public bool IsSigned { get; set; }

        /// <summary>
        /// 缩放因子
        /// </summary>
        public double Factor { get; set; }

        /// <summary>
        /// 偏移量
        /// </summary>
        public double Offset { get; set; }

        /// <summary>
        /// 最小值
        /// </summary>
        public double Min { get; set; }

        /// <summary>
        /// 最大值
        /// </summary>
        public double Max { get; set; }

        /// <summary>
        /// 信号单位
        /// </summary>
        public string Unit { get; set; }

        /// <summary>
        /// 接收节点
        /// </summary>
        public string Receiver { get; set; }
    }

    /// <summary>
    /// DBC报文模型，表示DBC文件中的一个报文定义
    /// </summary>
    public class DbcMessage
    {
        /// <summary>
        /// 报文ID
        /// </summary>
        public uint Id { get; set; }

        /// <summary>
        /// 报文名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 报文长度（字节）
        /// </summary>
        public int Length { get; set; }

        /// <summary>
        /// 发送节点
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// 信号列表
        /// </summary>
        public List<DbcSignal> Signals { get; set; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DbcMessage()
        {
            Signals = new List<DbcSignal>();
        }
    }

    /// <summary>
    /// DBC解析服务接口，定义DBC文件解析和信号编解码功能
    /// 优化支持BMS信号解析
    /// </summary>
    public interface IDbcService
    {
        /// <summary>
        /// 加载DBC文件
        /// </summary>
        /// <param name="filePath">DBC文件路径</param>
        /// <returns>加载是否成功</returns>
        bool LoadDbcFile(string filePath);

        /// <summary>
        /// 卸载DBC文件
        /// </summary>
        void UnloadDbcFile();

        /// <summary>
        /// 获取所有报文定义
        /// </summary>
        /// <returns>报文定义列表</returns>
        List<DbcMessage> GetMessages();

        /// <summary>
        /// 根据ID获取信号定义
        /// </summary>
        /// <param name="signalId">信号ID</param>
        /// <returns>信号定义</returns>
        DbcSignal GetSignalById(string signalId);

        /// <summary>
        /// 解码CAN数据中的信号值
        /// </summary>
        /// <param name="data">CAN报文数据</param>
        /// <param name="signal">信号定义</param>
        /// <returns>解码后的信号值</returns>
        double DecodeSignal(byte[] data, DbcSignal signal);

        /// <summary>
        /// 编码信号值为CAN数据
        /// </summary>
        /// <param name="value">信号值</param>
        /// <param name="signal">信号定义</param>
        /// <returns>编码后的数据</returns>
        byte[] EncodeSignal(double value, DbcSignal signal);

        /// <summary>
        /// 根据报文ID获取报文定义
        /// </summary>
        /// <param name="messageId">报文ID</param>
        /// <returns>报文定义</returns>
        DbcMessage GetMessageById(uint messageId);

        /// <summary>
        /// 获取所有BMS相关的信号
        /// </summary>
        /// <returns>BMS信号列表</returns>
        List<DbcSignal> GetBmsSignals();
    }
}