using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using CANMonitor.Interfaces;
using CANMonitor.Utils;

namespace CANMonitor.Services.CanServices
{
    /// <summary>
    /// CAN服务实现类，支持多通道CAN通信管理
    /// </summary>
    public class CanService : ICanService
    {
        // 存储通道配置和状态的字典
        private Dictionary<string, CanChannelConfig> _channelConfigs = new Dictionary<string, CanChannelConfig>();
        
        // 存储通道连接状态的字典
        private Dictionary<string, bool> _channelConnections = new Dictionary<string, bool>();
        
        // 存储每个通道的接收任务
        private Dictionary<string, Task> _receiveTasks = new Dictionary<string, Task>();
        
        // 存储每个通道的取消令牌
        private Dictionary<string, CancellationTokenSource> _cancellationTokens = new Dictionary<string, CancellationTokenSource>();
        
        // 默认通道（为了向后兼容）
        private string _defaultChannel = "CAN1";
        
        // ZLG USB CAN设备服务
        private ZLGUsbCanService _zlgService;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public CanService()
        {
            _zlgService = new ZLGUsbCanService();
        }

        /// <summary>
        /// 报文接收事件
        /// </summary>
        public event EventHandler<CanMessage> MessageReceived;

        /// <summary>
        /// 连接到CAN通道
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <param name="baudRate">波特率</param>
        /// <returns>连接是否成功</returns>
        public bool Connect(string channel, int baudRate)
        {
            try
            {
                // 配置通道参数
                var config = new CanChannelConfig
                {
                    Channel = channel,
                    BaudRate = baudRate,
                    IsCanFd = false,  // 默认使用标准CAN模式
                    DataBaudRate = 0
                };

                // 保存通道配置
                if (!_channelConfigs.ContainsKey(channel))
                {
                    _channelConfigs.Add(channel, config);
                }
                else
                {
                    _channelConfigs[channel] = config;
                }

                // 连接到ZLG设备
                bool connected = _zlgService.Connect(channel, baudRate);
                _channelConnections[channel] = connected;

                if (connected)
                {
                    // 设置为默认通道
                    _defaultChannel = channel;

                    // 创建并启动接收任务
                    StartReceiveTask(channel);
                }

                return connected;
            }
            catch (Exception ex)
            {
                // 记录错误日志
                Console.WriteLine($"连接通道 {channel} 失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 断开指定CAN通道
        /// </summary>
        /// <param name="channel">要断开的通道名称</param>
        public void Disconnect(string channel)
        {
            try
            {
                if (_channelConnections.ContainsKey(channel) && _channelConnections[channel])
                {
                    // 取消接收任务
                    if (_cancellationTokens.ContainsKey(channel))
                    {
                        _cancellationTokens[channel].Cancel();
                        _cancellationTokens.Remove(channel);
                    }

                    // 等待接收任务完成
                    if (_receiveTasks.ContainsKey(channel))
                    {
                        _receiveTasks[channel].Wait(100);
                        _receiveTasks.Remove(channel);
                    }

                    // 断开设备连接
                    _zlgService.Disconnect(channel);
                    _channelConnections[channel] = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"断开通道 {channel} 失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 断开所有CAN通道
        /// </summary>
        public void DisconnectAll()
        {
            var channels = new List<string>(_channelConnections.Keys);
            foreach (var channel in channels)
            {
                Disconnect(channel);
            }
        }

        /// <summary>
        /// 发送CAN报文（使用默认通道）
        /// </summary>
        /// <param name="message">要发送的CAN报文</param>
        /// <returns>发送是否成功</returns>
        public bool SendMessage(CanMessage message)
        {
            return SendMessage(_defaultChannel, message);
        }

        /// <summary>
        /// 通过指定通道发送CAN报文
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <param name="message">要发送的CAN报文</param>
        /// <returns>发送是否成功</returns>
        public bool SendMessage(string channel, CanMessage message)
        {
            try
            {
                if (!_channelConnections.ContainsKey(channel) || !_channelConnections[channel])
                {
                    return false;
                }

                // 设置报文通道
                message.Channel = channel;

                // 发送报文
                return _zlgService.SendMessage(channel, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送报文失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 获取可用的CAN通道列表
        /// </summary>
        /// <returns>可用通道名称列表</returns>
        public List<string> GetAvailableChannels()
        {
            // 获取ZLG设备支持的通道列表
            return _zlgService.GetAvailableChannels();
        }

        /// <summary>
        /// 检查指定通道是否已连接
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <returns>连接状态</returns>
        public bool IsChannelConnected(string channel)
        {
            return _channelConnections.ContainsKey(channel) && _channelConnections[channel];
        }

        /// <summary>
        /// 获取指定通道的波特率
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <returns>波特率值</returns>
        public int GetChannelBaudRate(string channel)
        {
            if (_channelConfigs.ContainsKey(channel))
            {
                return _channelConfigs[channel].BaudRate;
            }
            return 0;
        }

        /// <summary>
        /// 启动通道接收任务
        /// </summary>
        /// <param name="channel">通道名称</param>
        private void StartReceiveTask(string channel)
        {
            // 取消之前的任务
            if (_cancellationTokens.ContainsKey(channel))
            {
                _cancellationTokens[channel].Cancel();
                _cancellationTokens.Remove(channel);
            }

            // 创建新的取消令牌
            var cts = new CancellationTokenSource();
            _cancellationTokens[channel] = cts;

            // 启动新的接收任务
            _receiveTasks[channel] = Task.Run(() => ReceiveMessages(channel, cts.Token), cts.Token);
        }

        /// <summary>
        /// 接收CAN报文的任务
        /// </summary>
        /// <param name="channel">通道名称</param>
        /// <param name="cancellationToken">取消令牌</param>
        private void ReceiveMessages(string channel, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    // 从ZLG设备接收报文
                    CanMessage message = _zlgService.ReceiveMessage(channel);
                    if (message != null)
                    {
                        // 设置通道信息
                        message.Channel = channel;
                        
                        // 触发接收事件
                        MessageReceived?.Invoke(this, message);
                    }
                    
                    // 短暂休眠，避免CPU占用过高
                    Thread.Sleep(1);
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                    {
                        Console.WriteLine($"接收报文错误: {ex.Message}");
                    }
                }
            }
        }
    }
}