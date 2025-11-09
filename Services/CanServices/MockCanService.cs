using System;
using System.Collections.Generic;
using CANMonitor.Services.CanServices;

namespace CANMonitor.Services.CanServices
{
    /// <summary>
    /// ICanService的模拟实现，用于开发和测试环境
    /// 不依赖于实际的CAN硬件和SDK
    /// </summary>
    public class MockCanService : ICanService
    {
        // 存储模拟的通道连接状态
        private Dictionary<string, bool> _channelConnections = new Dictionary<string, bool>();
        
        // 存储模拟的通道配置
        private Dictionary<string, CanChannelConfig> _channelConfigs = new Dictionary<string, CanChannelConfig>();

        /// <summary>
        /// 构造函数
        /// </summary>
        public MockCanService()
        {
            // 初始化模拟通道
            _channelConnections["CAN1"] = false;
            _channelConnections["CAN2"] = false;
        }

        /// <summary>
        /// 报文接收事件
        /// </summary>
        public event EventHandler<CanMessage> MessageReceived;

        /// <summary>
        /// 连接到CAN通道（模拟实现）
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <param name="baudRate">波特率</param>
        /// <returns>模拟连接成功</returns>
        public bool Connect(string channel, int baudRate)
        {
            if (!_channelConnections.ContainsKey(channel))
            {
                _channelConnections[channel] = false;
            }
            
            _channelConnections[channel] = true;
            _channelConfigs[channel] = new CanChannelConfig { Channel = channel, BaudRate = baudRate };
            
            return true;
        }

        /// <summary>
        /// 断开指定CAN通道（模拟实现）
        /// </summary>
        /// <param name="channel">要断开的通道名称</param>
        public void Disconnect(string channel)
        {
            if (_channelConnections.ContainsKey(channel))
            {
                _channelConnections[channel] = false;
            }
        }

        /// <summary>
        /// 断开所有CAN通道（模拟实现）
        /// </summary>
        public void DisconnectAll()
        {
            foreach (var channel in _channelConnections.Keys)
            {
                _channelConnections[channel] = false;
            }
        }

        /// <summary>
        /// 发送CAN报文（模拟实现）
        /// </summary>
        /// <param name="message">要发送的CAN报文</param>
        /// <returns>模拟发送成功</returns>
        public bool SendMessage(CanMessage message)
        {
            return SendMessage("CAN1", message);
        }

        /// <summary>
        /// 通过指定通道发送CAN报文（模拟实现）
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <param name="message">要发送的CAN报文</param>
        /// <returns>模拟发送成功</returns>
        public bool SendMessage(string channel, CanMessage message)
        {
            return _channelConnections.ContainsKey(channel) && _channelConnections[channel];
        }

        /// <summary>
        /// 获取可用的CAN通道列表（模拟实现）
        /// </summary>
        /// <returns>模拟的可用通道列表</returns>
        public List<string> GetAvailableChannels()
        {
            return new List<string> { "CAN1", "CAN2" };
        }

        /// <summary>
        /// 检查指定通道是否已连接（模拟实现）
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <returns>模拟的连接状态</returns>
        public bool IsChannelConnected(string channel)
        {
            return _channelConnections.ContainsKey(channel) && _channelConnections[channel];
        }

        /// <summary>
        /// 获取指定通道的波特率（模拟实现）
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <returns>模拟的波特率值</returns>
        public int GetChannelBaudRate(string channel)
        {
            if (_channelConfigs.ContainsKey(channel))
            {
                return _channelConfigs[channel].BaudRate;
            }
            return 500000; // 默认波特率
        }
    }
}