using Prism.Commands;
using Prism.Mvvm;
using CANMonitor.Interfaces;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CANMonitor.ViewModels
{
    /// <summary>
    /// CAN监控视图模型
    /// 负责CAN报文的实时接收、显示和管理
    /// </summary>
    public class CanMonitorViewModel : BindableBase
    {
        /// <summary>
        /// CAN服务
        /// </summary>
        private readonly ICanService _canService;
        
        /// <summary>
        /// DBC服务
        /// </summary>
        private readonly IDbcService _dbcService;
        
        /// <summary>
        /// 消息列表
        /// </summary>
        private ObservableCollection<CanMessageViewModel> _messages;
        public ObservableCollection<CanMessageViewModel> Messages
        {
            get { return _messages; }
            set { SetProperty(ref _messages, value); }
        }
        
        /// <summary>
        /// 过滤后的消息列表
        /// </summary>
        private ICollectionView _filteredMessages;
        public ICollectionView FilteredMessages
        {
            get { return _filteredMessages; }
            set { SetProperty(ref _filteredMessages, value); }
        }
        
        /// <summary>
        /// 过滤文本
        /// </summary>
        private string _filterText;
        public string FilterText
        {
            get { return _filterText; }
            set { SetProperty(ref _filterText, value); }
        }
        
        /// <summary>
        /// 自动滚动标志
        /// </summary>
        private bool _autoScroll;
        public bool AutoScroll
        {
            get { return _autoScroll; }
            set { SetProperty(ref _autoScroll, value); }
        }
        
        /// <summary>
        /// 消息数量
        /// </summary>
        private int _messageCount;
        public int MessageCount
        {
            get { return _messageCount; }
            set { SetProperty(ref _messageCount, value); }
        }
        
        /// <summary>
        /// 波特率
        /// </summary>
        private string _baudRate;
        public string BaudRate
        {
            get { return _baudRate; }
            set { SetProperty(ref _baudRate, value); }
        }
        
        /// <summary>
        /// 状态
        /// </summary>
        private string _status;
        public string Status
        {
            get { return _status; }
            set { SetProperty(ref _status, value); }
        }
        
        /// <summary>
        /// 开始监控命令
        /// </summary>
        public DelegateCommand StartMonitoringCommand { get; private set; }
        
        /// <summary>
        /// 停止监控命令
        /// </summary>
        public DelegateCommand StopMonitoringCommand { get; private set; }
        
        /// <summary>
        /// 清空列表命令
        /// </summary>
        public DelegateCommand ClearMessagesCommand { get; private set; }
        
        /// <summary>
        /// 导出数据命令
        /// </summary>
        public DelegateCommand ExportDataCommand { get; private set; }
        
        /// <summary>
        /// 应用过滤命令
        /// </summary>
        public DelegateCommand ApplyFilterCommand { get; private set; }
        
        /// <summary>
        /// 监控状态
        /// </summary>
        private bool _isMonitoring;
        
        /// <summary>
        /// 消息数量上限
        /// </summary>
        private const int MaxMessagesCount = 10000;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="canService">CAN服务</param>
        /// <param name="dbcService">DBC服务</param>
        public CanMonitorViewModel(ICanService canService, IDbcService dbcService)
        {
            _canService = canService;
            _dbcService = dbcService;
            
            // 初始化命令
            StartMonitoringCommand = new DelegateCommand(StartMonitoring, CanStartMonitoring).ObservesProperty(() => _canService.IsConnected);
            StopMonitoringCommand = new DelegateCommand(StopMonitoring, CanStopMonitoring).ObservesProperty(() => _isMonitoring);
            ClearMessagesCommand = new DelegateCommand(ClearMessages);
            ExportDataCommand = new DelegateCommand(ExportData);
            ApplyFilterCommand = new DelegateCommand(ApplyFilter);
            
            // 初始化数据
            InitializeData();
            
            // 订阅事件
            _canService.MessageReceived += CanService_MessageReceived;
        }
        
        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            Messages = new ObservableCollection<CanMessageViewModel>();
            FilteredMessages = CollectionViewSource.GetDefaultView(Messages);
            FilteredMessages.Filter = MessageFilter;
            
            FilterText = string.Empty;
            AutoScroll = true;
            MessageCount = 0;
            
            if (_canService.IsConnected)
            {
                BaudRate = _canService.CurrentBaudRate.ToString();
                Status = "已连接";
            }
            else
            {
                BaudRate = "0";
                Status = "未连接";
            }
            
            _isMonitoring = false;
        }
        
        /// <summary>
        /// 开始监控
        /// </summary>
        private void StartMonitoring()
        {
            if (!_canService.IsConnected)
            {
                MessageBox.Show("请先连接设备", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            try
            {
                _canService.StartMonitoring();
                _isMonitoring = true;
                Status = "监控中...";
                BaudRate = _canService.CurrentBaudRate.ToString();
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"启动监控失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 停止监控
        /// </summary>
        private void StopMonitoring()
        {
            try
            {
                _canService.StopMonitoring();
                _isMonitoring = false;
                Status = "已停止";
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"停止监控失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 清空消息列表
        /// </summary>
        private void ClearMessages()
        {
            Messages.Clear();
            MessageCount = 0;
        }
        
        /// <summary>
        /// 导出数据
        /// </summary>
        private void ExportData()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "CSV文件 (*.csv)|*.csv|文本文件 (*.txt)|*.txt|所有文件 (*.*)|*.*",
                Title = "导出CAN数据"
            };
            
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName))
                    {
                        // 写入表头
                        writer.WriteLine("时间,通道,ID,类型,数据长度,数据,解析信息");
                        
                        // 写入数据
                        foreach (var message in Messages)
                        {
                            writer.WriteLine($"{message.Time},{message.Channel},{message.Id},{message.Type},{message.Length},{message.Data},{message.DecodedInfo}");
                        }
                    }
                    
                    MessageBox.Show("数据导出成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"数据导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        /// <summary>
        /// 应用过滤
        /// </summary>
        private void ApplyFilter()
        {
            FilteredMessages.Refresh();
        }
        
        /// <summary>
        /// 消息过滤器
        /// </summary>
        private bool MessageFilter(object item)
        {
            if (string.IsNullOrEmpty(FilterText))
                return true;
            
            var message = item as CanMessageViewModel;
            if (message == null)
                return false;
            
            // 过滤ID、数据或解析信息
            return message.Id.Contains(FilterText) || 
                   message.Data.Contains(FilterText) || 
                   message.DecodedInfo.Contains(FilterText);
        }
        
        /// <summary>
        /// CAN消息接收事件处理
        /// </summary>
        private void CanService_MessageReceived(object sender, CanMessage message)
        {
            // 在UI线程中更新消息列表
            Application.Current.Dispatcher.Invoke(() =>
            {
                // 限制消息数量，防止内存溢出
                if (Messages.Count >= MaxMessagesCount)
                {
                    Messages.RemoveAt(0);
                }
                
                // 创建消息视图模型
                var messageVm = new CanMessageViewModel(message)
                {
                    DecodedInfo = _dbcService.DecodeMessage(message)
                };
                
                Messages.Add(messageVm);
                MessageCount = Messages.Count;
            });
        }
        
        /// <summary>
        /// 是否可以开始监控
        /// </summary>
        private bool CanStartMonitoring()
        {
            return _canService.IsConnected && !_isMonitoring;
        }
        
        /// <summary>
        /// 是否可以停止监控
        /// </summary>
        private bool CanStopMonitoring()
        {
            return _isMonitoring;
        }
    }
    
    /// <summary>
    /// CAN消息视图模型
    /// </summary>
    public class CanMessageViewModel : BindableBase
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string Time { get; private set; }
        
        /// <summary>
        /// 通道
        /// </summary>
        public string Channel { get; private set; }
        
        /// <summary>
        /// ID
        /// </summary>
        public string Id { get; private set; }
        
        /// <summary>
        /// 类型
        /// </summary>
        public string Type { get; private set; }
        
        /// <summary>
        /// 数据长度
        /// </summary>
        public int Length { get; private set; }
        
        /// <summary>
        /// 数据
        /// </summary>
        public string Data { get; private set; }
        
        /// <summary>
        /// 解析信息
        /// </summary>
        private string _decodedInfo;
        public string DecodedInfo
        {
            get { return _decodedInfo; }
            set { SetProperty(ref _decodedInfo, value); }
        }
        
        /// <summary>
        /// 消息颜色
        /// </summary>
        public string MessageColor { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public CanMessageViewModel(CanMessage message)
        {
            Time = System.DateTime.Now.ToString("HH:mm:ss.fff");
            Channel = message.Channel.ToString();
            Id = message.Id.ToString("X8");
            Type = message.IsExtendedFrame ? "扩展" : "标准";
            Length = message.Data.Length;
            Data = ByteArrayToHexString(message.Data);
            DecodedInfo = string.Empty;
            
            // 根据消息类型设置不同颜色
            MessageColor = message.IsExtendedFrame ? "#0066CC" : "#333333";
        }
        
        /// <summary>
        /// 字节数组转十六进制字符串
        /// </summary>
        private string ByteArrayToHexString(byte[] data)
        {
            return string.Join(" ", data.Select(b => b.ToString("X2")));
        }
    }
}