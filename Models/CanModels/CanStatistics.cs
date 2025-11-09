using System;

namespace CANMonitor.Models.CanModels
{
    /// <summary>
    /// CAN统计信息模型类
    /// 用于记录CAN通信的统计数据
    /// </summary>
    public class CanStatistics
    {
        /// <summary>
        /// 总接收帧数
        /// </summary>
        public uint TotalReceivedFrames { get; set; }

        /// <summary>
        /// 总发送帧数
        /// </summary>
        public uint TotalSentFrames { get; set; }

        /// <summary>
        /// 错误帧数量
        /// </summary>
        public uint ErrorFrames { get; set; }

        /// <summary>
        /// 丢帧数量
        /// </summary>
        public uint LostFrames { get; set; }

        /// <summary>
        /// 总线负载率（百分比）
        /// </summary>
        public double BusLoadRate { get; set; }

        /// <summary>
        /// 平均接收速率（帧/秒）
        /// </summary>
        public double AvgReceiveRate { get; set; }

        /// <summary>
        /// 平均发送速率（帧/秒）
        /// </summary>
        public double AvgSendRate { get; set; }

        /// <summary>
        /// 接收缓冲区占用率（百分比）
        /// </summary>
        public double RxBufferUsage { get; set; }

        /// <summary>
        /// 发送缓冲区占用率（百分比）
        /// </summary>
        public double TxBufferUsage { get; set; }

        /// <summary>
        /// 统计开始时间
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 最后更新时间
        /// </summary>
        public DateTime LastUpdateTime { get; set; }

        /// <summary>
        /// 通道号
        /// </summary>
        public int ChannelNumber { get; set; }

        /// <summary>
        /// 默认构造函数
        /// </summary>
        public CanStatistics()
        {
            TotalReceivedFrames = 0;
            TotalSentFrames = 0;
            ErrorFrames = 0;
            LostFrames = 0;
            BusLoadRate = 0.0;
            AvgReceiveRate = 0.0;
            AvgSendRate = 0.0;
            RxBufferUsage = 0.0;
            TxBufferUsage = 0.0;
            StartTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            ChannelNumber = 0;
        }

        /// <summary>
        /// 重置统计信息
        /// </summary>
        public void Reset()
        {
            TotalReceivedFrames = 0;
            TotalSentFrames = 0;
            ErrorFrames = 0;
            LostFrames = 0;
            BusLoadRate = 0.0;
            AvgReceiveRate = 0.0;
            AvgSendRate = 0.0;
            RxBufferUsage = 0.0;
            TxBufferUsage = 0.0;
            StartTime = DateTime.Now;
            LastUpdateTime = DateTime.Now;
            // 保留通道号信息
        }

        /// <summary>
        /// 更新统计信息
        /// </summary>
        /// <param name="receivedCount">新增接收帧数</param>
        /// <param name="sentCount">新增发送帧数</param>
        /// <param name="errorCount">新增错误帧数</param>
        /// <param name="lostCount">新增丢帧数</param>
        public void Update(uint receivedCount, uint sentCount, uint errorCount, uint lostCount)
        {
            TotalReceivedFrames += receivedCount;
            TotalSentFrames += sentCount;
            ErrorFrames += errorCount;
            LostFrames += lostCount;
            LastUpdateTime = DateTime.Now;

            // 计算平均速率（简单实现）
            TimeSpan duration = LastUpdateTime - StartTime;
            if (duration.TotalSeconds > 0)
            {
                AvgReceiveRate = TotalReceivedFrames / duration.TotalSeconds;
                AvgSendRate = TotalSentFrames / duration.TotalSeconds;
            }
        }
    }
}