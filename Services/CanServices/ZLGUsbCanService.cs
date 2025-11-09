using System;
using System.Collections.Generic;
using System.Threading;
using System.Runtime.InteropServices;
using CANMonitor.Interfaces;
using CANMonitor.Models.CanModels;
using CANMonitor.Utils;
using CANMonitor.Models;

namespace CANMonitor.Services.CanServices
{
    public class ZLGUsbCanService : ICanService, IDisposable
    {
        // ZLG SDK包装器实例
        private ZLGSdkWrapper _zlgSdkWrapper;
        
        // 设备句柄
        private IntPtr _deviceHandle = IntPtr.Zero;
        
        // 设备类型和索引
        private int _deviceType = ZLGSdkWrapper.DEVICE_TYPE_USBCANFD_200U;
        private int _deviceIndex = 0;
        
        // 通道状态和配置管理
        private Dictionary<string, bool> _connectedChannels = new Dictionary<string, bool>();
        private Dictionary<string, string> _channelBaudRates = new Dictionary<string, string>();
        private Dictionary<string, Queue<CanMessage>> _messageQueues = new Dictionary<string, Queue<CanMessage>>();
        private Dictionary<string, Queue<CanFdMessage>> _messageFdQueues = new Dictionary<string, Queue<CanFdMessage>>();
        private string[] _availableChannels = { "CAN1", "CAN2", "CAN3", "CAN4" };
        
        // 自动接收相关
        private Thread _receiveThread;
        private bool _isAutoReceiveRunning = false;
        
        // 实现ICanService接口的属性和事件
        public bool IsDeviceOpened { get; private set; } = false;
        
        public event EventHandler<CanMessage[]> MessagesReceived;
        public event EventHandler<CanFdMessage[]> MessagesReceivedFD;
        public event EventHandler<CanChannelStatus> ChannelStatusChanged;
        public event EventHandler<CanErrorInfo> ErrorOccurred;
        
        public ZLGUsbCanService()
        {
            _zlgSdkWrapper = new ZLGSdkWrapper();
            
            // 检查DLL是否可用
            if (!_zlgSdkWrapper.IsDllAvailable())
            {
                throw new InvalidOperationException("无法加载zlgcan.dll，请确保DLL文件位于正确位置");
            }
            
            // 初始化时打开设备
            _deviceHandle = _zlgSdkWrapper.ZCAN_OpenDevice(_deviceType, _deviceIndex, 0);
            if (_deviceHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("无法打开CAN设备，请检查设备连接");
            }
            
            IsDeviceOpened = true;
        }
        
        // 打开设备
        public bool OpenDevice(string channel, string baudRate)
        {
            try
            {
                if (_connectedChannels.ContainsKey(channel) && _connectedChannels[channel])
                {
                    Console.WriteLine($"通道 {channel} 已经打开");
                    return true;
                }
                
                // 解析通道号
                int channelIndex = ParseChannelIndex(channel);
                if (channelIndex < 0)
                    return false;
                
                // 创建初始化配置
                ZLGSdkWrapper.CAN_InitConfig initConfig = CreateInitConfig(baudRate);
                
                // 调用SDK初始化通道
                int result = _zlgSdkWrapper.ZCAN_InitCAN(_deviceHandle, channelIndex, ref initConfig);
                if (result != 1)
                {
                    Console.WriteLine($"初始化通道 {channel} 失败，错误码: {result}");
                    return false;
                }
                
                // 启动CAN通道
                result = _zlgSdkWrapper.ZCAN_StartCAN(_deviceHandle, channelIndex);
                if (result != 1)
                {
                    Console.WriteLine($"启动通道 {channel} 失败，错误码: {result}");
                    return false;
                }
                
                // 注册通道状态
                _connectedChannels[channel] = true;
                _channelBaudRates[channel] = baudRate;
                
                // 创建消息队列
                if (!_messageQueues.ContainsKey(channel))
                {
                    _messageQueues[channel] = new Queue<CanMessage>();
                }
                
                Console.WriteLine($"成功打开通道 {channel}，波特率: {baudRate}");
                
                // 触发通道状态变化事件
                var status = GetChannelStatus(channelIndex);
                status.Status = ChannelStatus.Running;
                ChannelStatusChanged?.Invoke(this, status);
                
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"打开设备失败: {ex.Message}");
                return false;
            }
        }
        
        // 关闭CAN设备
        public void CloseDevice(string channel)
        {
            try
            {
                if (_connectedChannels.ContainsKey(channel) && _connectedChannels[channel])
                {
                    Console.WriteLine($"正在断开通道 {channel}");
                    
                    // 解析通道号
                    int channelIndex = ParseChannelIndex(channel);
                    if (channelIndex >= 0 && _deviceHandle != IntPtr.Zero)
                    {
                        // 调用SDK停止CAN通道
                        _zlgSdkWrapper.ZCAN_StopCAN(_deviceHandle, channelIndex);
                    }
                    
                    _connectedChannels[channel] = false;
                    
                    // 清空消息队列
                    if (_messageQueues.ContainsKey(channel))
                    {
                        _messageQueues[channel].Clear();
                    }
                    
                    Console.WriteLine($"通道 {channel} 已断开");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"断开连接失败: {ex.Message}");
            }
        }
        
        // 发送CAN报文
        public bool SendMessage(string channel, CanMessage message)
        {
            try
            {
                if (!IsConnected(channel))
                {
                    Console.WriteLine($"通道 {channel} 未连接，无法发送报文");
                    return false;
                }
                
                // 解析通道号
                int channelIndex = ParseChannelIndex(channel);
                if (channelIndex < 0)
                    return false;
                
                // 准备SDK消息结构体
                ZLGSdkWrapper.CAN_Transmit transmitMsg = new ZLGSdkWrapper.CAN_Transmit
                {
                    ID = message.Id,
                    DLC = (byte)(message.Data?.Length ?? 0),
                    TransmitType = 0,  // 正常发送
                    RemoteFlag = 0,    // 数据帧
                    ExternFlag = (message.Id > 0x7FF) ? 1 : 0,  // 判断是标准帧还是扩展帧
                };
                
                // 复制数据
                if (transmitMsg.DLC > 0 && message.Data != null)
                {
                    // 根据DLC复制数据，最多8字节
                    Array.Copy(message.Data, 0, transmitMsg.Data, 0, Math.Min(transmitMsg.DLC, 8));
                }
                
                // 调用SDK发送消息
                int result = _zlgSdkWrapper.ZCAN_Transmit(_deviceHandle, ref transmitMsg, 1);
                
                if (result > 0)
                {
                    Console.WriteLine($"发送报文到通道 {channel}: ID=0x{message.Id:X}, DLC={message.Data?.Length ?? 0} 成功");
                    return true;
                }
                else
                {
                    Console.WriteLine($"发送报文到通道 {channel} 失败，错误码: {_zlgSdkWrapper.ZCAN_GetErrorInfo(_deviceHandle)}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送报文失败: {ex.Message}");
                return false;
            }
        }
        
        // 发送CAN FD报文
        public bool SendMessageFD(string channel, CanFdMessage message)
        {
            try
            {
                if (!IsConnected(channel))
                {
                    Console.WriteLine($"通道 {channel} 未连接，无法发送CAN FD报文");
                    return false;
                }
                
                // 解析通道号
                int channelIndex = ParseChannelIndex(channel);
                if (channelIndex < 0)
                    return false;
                
                // 准备SDK CAN FD消息结构体
                ZLGSdkWrapper.CANFD_Transmit transmitMsg = new ZLGSdkWrapper.CANFD_Transmit
                {
                    ID = message.Id,
                    DLC = (byte)message.Data?.Length ?? 0,
                    Flag = message.IsFdFormat ? 1 : 0,  // CAN FD格式标志
                    ExternFlag = (message.Id > 0x7FF) ? 1 : 0,  // 判断是标准帧还是扩展帧
                };
                
                // 复制数据
                if (transmitMsg.DLC > 0 && message.Data != null)
                {
                    // 根据DLC复制数据，CAN FD最多64字节
                    Array.Copy(message.Data, 0, transmitMsg.Data, 0, Math.Min(transmitMsg.DLC, 64));
                }
                
                // 调用SDK发送CAN FD消息
                int result = _zlgSdkWrapper.ZCAN_TransmitFD(_deviceHandle, ref transmitMsg, 1);
                
                if (result > 0)
                {
                    Console.WriteLine($"发送CAN FD报文到通道 {channel}: ID=0x{message.Id:X}, DLC={message.Data?.Length ?? 0} 成功");
                    return true;
                }
                else
                {
                    Console.WriteLine($"发送CAN FD报文到通道 {channel} 失败，错误码: {_zlgSdkWrapper.ZCAN_GetErrorInfo(_deviceHandle)}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"发送CAN FD报文失败: {ex.Message}");
                return false;
            }
        }
        
        // 批量发送CAN报文
        public bool SendMessages(string channel, CanMessage[] messages)
        {
            try
            {
                if (!IsConnected(channel))
                {
                    Console.WriteLine($"通道 {channel} 未连接，无法批量发送报文");
                    return false;
                }
                
                bool allSuccess = true;
                foreach (var message in messages)
                {
                    if (!SendMessage(channel, message))
                    {
                        allSuccess = false;
                    }
                }
                
                return allSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"批量发送报文失败: {ex.Message}");
                return false;
            }
        }
        
        // 批量发送CAN FD报文
        public bool SendMessagesFD(string channel, CanFdMessage[] messages)
        {
            try
            {
                if (!IsConnected(channel))
                {
                    Console.WriteLine($"通道 {channel} 未连接，无法批量发送CAN FD报文");
                    return false;
                }
                
                bool allSuccess = true;
                foreach (var message in messages)
                {
                    if (!SendMessageFD(channel, message))
                    {
                        allSuccess = false;
                    }
                }
                
                return allSuccess;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"批量发送CAN FD报文失败: {ex.Message}");
                return false;
            }
        }
        
        // 接收CAN报文
        public CanMessage ReceiveMessage(string channel)
        {
            try
            {
                if (!IsConnected(channel) || !_messageQueues.ContainsKey(channel))
                {
                    return null;
                }
                
                // 尝试从队列中获取消息
                if (_messageQueues[channel].Count > 0)
                {
                    return _messageQueues[channel].Dequeue();
                }
                
                // 解析通道号
                int channelIndex = ParseChannelIndex(channel);
                if (channelIndex < 0 || _deviceHandle == IntPtr.Zero)
                    return null;
                
                // 调用SDK接收消息
                ZLGSdkWrapper.CAN_Receive receiveMsg = new ZLGSdkWrapper.CAN_Receive();
                int result = _zlgSdkWrapper.ZCAN_Receive(_deviceHandle, channelIndex, ref receiveMsg, 1, 0);
                
                if (result > 0)
                {
                    // 转换为应用程序消息格式
                    byte[] data = new byte[receiveMsg.DLC];
                    Array.Copy(receiveMsg.Data, data, receiveMsg.DLC);
                    
                    CanMessage message = new CanMessage
                    {
                        Id = receiveMsg.ID,
                        Data = data,
                        Timestamp = DateTime.Now,
                        Channel = channel,
                        IsRemote = receiveMsg.RemoteFlag == 1,
                        IsExtended = receiveMsg.ExternFlag == 1
                    };
                    
                    return message;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"接收报文失败: {ex.Message}");
                return null;
            }
        }
        
        // 接收CAN FD报文
        public CanFdMessage ReceiveMessageFD(string channel)
        {
            try
            {
                if (!IsConnected(channel) || !_messageFdQueues.ContainsKey(channel))
                {
                    return null;
                }
                
                if (_messageFdQueues[channel].Count > 0)
                {
                    return _messageFdQueues[channel].Dequeue();
                }
                
                // 解析通道号
                int channelIndex = ParseChannelIndex(channel);
                if (channelIndex < 0 || _deviceHandle == IntPtr.Zero)
                    return null;
                
                // 调用SDK接收CAN FD消息
                ZLGSdkWrapper.CANFD_Receive receiveMsg = new ZLGSdkWrapper.CANFD_Receive();
                int result = _zlgSdkWrapper.ZCAN_ReceiveFD(_deviceHandle, channelIndex, ref receiveMsg, 1, 0);
                
                if (result > 0 && (receiveMsg.Flag & 0x01) == 1)  // 检查是否为CAN FD格式
                {
                    // 转换为应用程序CAN FD消息格式
                    byte[] data = new byte[receiveMsg.DLC];
                    Array.Copy(receiveMsg.Data, data, receiveMsg.DLC);
                    
                    CanFdMessage message = new CanFdMessage
                    {
                        Id = receiveMsg.ID,
                        Data = data,
                        Timestamp = DateTime.Now,
                        Channel = channel,
                        IsFdFormat = true,
                        IsExtended = receiveMsg.ExternFlag == 1
                    };
                    
                    return message;
                }
                
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"接收CAN FD报文失败: {ex.Message}");
                return null;
            }
        }
        
        // 启动自动接收
        public void StartAutoReceive(string channel)
        {
            if (!IsConnected(channel) || _isAutoReceiveRunning)
                return;
            
            _isAutoReceiveRunning = true;
            _receiveThread = new Thread(() => AutoReceiveLoop(channel))
            {
                IsBackground = true
            };
            _receiveThread.Start();
        }
        
        // 停止自动接收
        public void StopAutoReceive()
        {
            _isAutoReceiveRunning = false;
            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                _receiveThread.Join(1000);
            }
        }
        
        // 自动接收循环
        private void AutoReceiveLoop(string channel)
        {
            // 解析通道号
            int channelIndex = ParseChannelIndex(channel);
            if (channelIndex < 0 || _deviceHandle == IntPtr.Zero)
                return;
            
            while (_isAutoReceiveRunning)
            {
                try
                {
                    // 批量接收CAN报文
                    const int BUFFER_SIZE = 100;
                    ZLGSdkWrapper.CAN_Receive[] receiveBuffer = new ZLGSdkWrapper.CAN_Receive[BUFFER_SIZE];
                    
                    int result = _zlgSdkWrapper.ZCAN_Receive(_deviceHandle, channelIndex, receiveBuffer, BUFFER_SIZE, 100);
                    
                    if (result > 0)
                    {
                        List<CanMessage> receivedMessages = new List<CanMessage>();
                        
                        // 处理接收到的消息
                        for (int i = 0; i < result; i++)
                        {
                            byte[] data = new byte[receiveBuffer[i].DLC];
                            Array.Copy(receiveBuffer[i].Data, data, receiveBuffer[i].DLC);
                            
                            CanMessage message = new CanMessage
                            {
                                Id = receiveBuffer[i].ID,
                                Data = data,
                                Timestamp = DateTime.Now,
                                Channel = channel,
                                IsRemote = receiveBuffer[i].RemoteFlag == 1,
                                IsExtended = receiveBuffer[i].ExternFlag == 1
                            };
                            
                            receivedMessages.Add(message);
                        }
                        
                        // 触发消息接收事件
                        MessagesReceived?.Invoke(this, receivedMessages.ToArray());
                    }
                    
                    // 也尝试接收CAN FD报文
                    ZLGSdkWrapper.CANFD_Receive[] receiveFDBuffer = new ZLGSdkWrapper.CANFD_Receive[BUFFER_SIZE];
                    result = _zlgSdkWrapper.ZCAN_ReceiveFD(_deviceHandle, channelIndex, receiveFDBuffer, BUFFER_SIZE, 10);
                    
                    if (result > 0)
                    {
                        List<CanFdMessage> receivedFDMessages = new List<CanFdMessage>();
                        
                        // 处理接收到的CAN FD消息
                        for (int i = 0; i < result; i++)
                        {
                            if ((receiveFDBuffer[i].Flag & 0x01) == 1)  // 检查是否为CAN FD格式
                            {
                                byte[] data = new byte[receiveFDBuffer[i].DLC];
                                Array.Copy(receiveFDBuffer[i].Data, data, receiveFDBuffer[i].DLC);
                                
                                CanFdMessage message = new CanFdMessage
                                {
                                    Id = receiveFDBuffer[i].ID,
                                    Data = data,
                                    Timestamp = DateTime.Now,
                                    Channel = channel,
                                    IsFdFormat = true,
                                    IsExtended = receiveFDBuffer[i].ExternFlag == 1
                                };
                                
                                receivedFDMessages.Add(message);
                            }
                        }
                        
                        // 触发CAN FD消息接收事件
                        if (receivedFDMessages.Count > 0)
                        {
                            MessagesReceivedFD?.Invoke(this, receivedFDMessages.ToArray());
                        }
                    }
                    
                    Thread.Sleep(10);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"自动接收循环错误: {ex.Message}");
                    // 触发错误事件
                    ErrorOccurred?.Invoke(this, new CanErrorInfo
                    {
                        ChannelNumber = channelIndex,
                        ErrorType = ErrorType.ReceiveError,
                        ErrorDescription = ex.Message,
                        RxErrCounter = 0,
                        TxErrCounter = 0
                    });
                }
            }
        }
        
        // 获取通道状态
        public CanChannelStatus GetChannelStatus(int channelNumber)
        {
            var status = new CanChannelStatus
            {
                ChannelNumber = channelNumber,
                ChannelName = $"CAN{channelNumber + 1}",
                Status = IsConnected($"CAN{channelNumber + 1}") ? ChannelStatus.Running : ChannelStatus.Closed
            };
            
            if (status.Status == ChannelStatus.Running && _channelBaudRates.ContainsKey(status.ChannelName))
            {
                status.BaudRate = _channelBaudRates[status.ChannelName];
            }
            
            // 调用SDK获取通道状态信息
            if (_deviceHandle != IntPtr.Zero)
            {
                ZLGSdkWrapper.CAN_Status canStatus = new ZLGSdkWrapper.CAN_Status();
                int result = _zlgSdkWrapper.ZCAN_GetReceiveNum(_deviceHandle, channelNumber);
                if (result >= 0)
                {
                    status.ReceivedMessageCount = result;
                }
                
                result = _zlgSdkWrapper.ZCAN_GetTransmitNum(_deviceHandle, channelNumber);
                if (result >= 0)
                {
                    status.TransmittedMessageCount = result;
                }
            }
            
            return status;
        }
        
        // 获取通道错误信息
        public CanErrorInfo GetChannelErrorInfo(int channelNumber)
        {
            var errorInfo = new CanErrorInfo
            {
                ChannelNumber = channelNumber,
                ErrorType = ErrorType.None,
                ErrorDescription = "无错误",
                RxErrCounter = 0,
                TxErrCounter = 0
            };
            
            // 调用SDK获取错误信息
            if (_deviceHandle != IntPtr.Zero)
            {
                int errorCode = _zlgSdkWrapper.ZCAN_GetErrorInfo(_deviceHandle);
                
                // 根据错误码设置错误类型和描述
                if (errorCode != 0)
                {
                    errorInfo.ErrorType = ErrorType.Other;
                    errorInfo.ErrorDescription = $"错误码: {errorCode}";
                }
                
                // 获取错误计数器
                ZLGSdkWrapper.CAN_ErrorFrame errorFrame = new ZLGSdkWrapper.CAN_ErrorFrame();
                int result = _zlgSdkWrapper.ZCAN_GetLastErrorFrame(_deviceHandle, channelNumber, ref errorFrame);
                if (result == 1)
                {
                    // 错误帧信息可以用于更详细的错误诊断
                }
            }
            
            return errorInfo;
        }
        
        // 设置消息过滤器
        public bool SetMessageFilter(int channelNumber, object filter)
        {
            try
            {
                if (_deviceHandle == IntPtr.Zero)
                    return false;
                
                // 这里假设filter是一个包含AccCode和AccMask的对象
                ZLGSdkWrapper.Filter_Config filterConfig = new ZLGSdkWrapper.Filter_Config
                {
                    FilterIndex = 0,
                    FilterMode = 0,  // 接收所有类型
                    FilterType = 0,  // 列表过滤
                    ExtFrame = 0,    // 标准帧
                    Start = 0,
                    End = 0
                };
                
                // 应用过滤器配置
                int result = _zlgSdkWrapper.ZCAN_SetFilter(_deviceHandle, channelNumber, ref filterConfig);
                
                Console.WriteLine($"设置通道 {channelNumber} 的过滤器，结果: {result}");
                return result == 1;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"设置过滤器失败: {ex.Message}");
                return false;
            }
        }
        
        // 获取可用设备列表
        public List<CanDeviceInfo> GetAvailableDevices()
        {
            List<CanDeviceInfo> devices = new List<CanDeviceInfo>();
            
            // 调用SDK获取设备数量
            int deviceCount = _zlgSdkWrapper.ZCAN_GetDeviceCount(_deviceType, 0);
            
            for (int i = 0; i < deviceCount; i++)
            {
                // 检查设备是否可用
                IntPtr tempHandle = _zlgSdkWrapper.ZCAN_OpenDevice(_deviceType, i, 0);
                if (tempHandle != IntPtr.Zero)
                {
                    // 设备可用，添加到列表
                    devices.Add(new CanDeviceInfo
                    {
                        DeviceType = "USBCANFD-200U",
                        DeviceIndex = i,
                        DeviceName = $"USBCANFD-200U ({i})",
                        AvailableChannels = 4,
                        Status = DeviceStatus.Connected
                    });
                    
                    // 关闭临时句柄
                    _zlgSdkWrapper.ZCAN_CloseDevice(tempHandle);
                }
            }
            
            // 如果没有找到设备，返回模拟的设备信息
            if (devices.Count == 0)
            {
                devices.Add(new CanDeviceInfo
                {
                    DeviceType = "USBCANFD-200U",
                    DeviceIndex = 0,
                    DeviceName = "USBCANFD-200U",
                    AvailableChannels = 4,
                    Status = DeviceStatus.Disconnected
                });
            }
            
            return devices;
        }
        
        // 初始化CAN通道
        public bool InitCAN(CanInitConfig config)
        {
            try
            {
                string channel = $"CAN{config.ChannelNumber + 1}";
                return OpenDevice(channel, config.BaudRate);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化CAN失败: {ex.Message}");
                return false;
            }
        }
        
        // 启动CAN通道
        public bool StartCAN(int channelNumber)
        {
            string channel = $"CAN{channelNumber + 1}";
            if (!IsConnected(channel))
            {
                Console.WriteLine($"通道 {channel} 未初始化，无法启动");
                return false;
            }
            
            // 调用SDK启动CAN通道
            int result = _zlgSdkWrapper.ZCAN_StartCAN(_deviceHandle, channelNumber);
            if (result != 1)
            {
                Console.WriteLine($"启动通道 {channel} 失败，错误码: {result}");
                return false;
            }
            
            Console.WriteLine($"启动通道 {channel}");
            
            // 触发通道状态变化事件
            var status = GetChannelStatus(channelNumber);
            status.Status = ChannelStatus.Running;
            ChannelStatusChanged?.Invoke(this, status);
            
            return true;
        }
        
        // 停止CAN通道
        public void StopCAN(int channelNumber)
        {
            string channel = $"CAN{channelNumber + 1}";
            Console.WriteLine($"停止通道 {channel}");
            
            // 调用SDK停止CAN通道
            _zlgSdkWrapper.ZCAN_StopCAN(_deviceHandle, channelNumber);
            
            // 更新通道状态
            if (_connectedChannels.ContainsKey(channel))
            {
                _connectedChannels[channel] = false;
            }
            
            // 触发通道状态变化事件
            var status = GetChannelStatus(channelNumber);
            status.Status = ChannelStatus.Initialized;
            ChannelStatusChanged?.Invoke(this, status);
        }
        
        // 重置CAN通道
        public void ResetCAN(int channelNumber)
        {
            string channel = $"CAN{channelNumber + 1}";
            Console.WriteLine($"重置通道 {channel}");
            
            // 调用SDK重置CAN通道
            _zlgSdkWrapper.ZCAN_ResetCAN(_deviceHandle, channelNumber);
            
            if (_connectedChannels.ContainsKey(channel))
            {
                _connectedChannels[channel] = false;
                CloseDevice(channel);
            }
        }
        
        // 获取可用的CAN通道列表
        public List<string> GetAvailableChannels()
        {
            // 返回可用通道列表
            return new List<string>(_availableChannels);
        }
        
        // 将消息添加到接收队列（用于测试）
        public void EnqueueMessage(string channel, CanMessage message)
        {
            if (_messageQueues.ContainsKey(channel))
            {
                _messageQueues[channel].Enqueue(message);
            }
        }
        
        // 检查通道是否已连接
        private bool IsConnected(string channel)
        {
            return _connectedChannels.ContainsKey(channel) && _connectedChannels[channel];
        }
        
        // 实现IDisposable接口
        public void Dispose()
        {
            StopAutoReceive();
            
            // 关闭所有打开的通道
            foreach (var channel in new List<string>(_connectedChannels.Keys))
            {
                CloseDevice(channel);
            }
            
            // 关闭设备
            if (_deviceHandle != IntPtr.Zero)
            {
                _zlgSdkWrapper.ZCAN_CloseDevice(_deviceHandle);
                _deviceHandle = IntPtr.Zero;
                IsDeviceOpened = false;
            }
        }
    }
}