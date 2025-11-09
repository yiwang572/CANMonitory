using CANMonitor.Interfaces;
using CANMonitor.Services;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace CANMonitor.ViewModels
{
    /// <summary>
    /// OBD诊断监控页面的视图模型
    /// 集成故障诊断、数据流监控、冻结帧分析和BMS特殊功能
    /// </summary>
    public class ObdMonitorViewModel : BindableBase
    {
        #region 私有字段
        private readonly IObdService _obdService;
        private readonly IRegionManager _regionManager;
        private readonly DispatcherTimer _refreshTimer;
        private bool _isConnected;
        private string _connectionStatusText;
        private string _connectionStatusColor;
        private ushort _selectedTargetAddress;
        private ObservableCollection<DtcInfo> _dtcList;
        private ObservableCollection<DataStreamParameter> _dataStreamParameters;
        private ObservableCollection<FaultFrameInfo> _freezeFrameList;
        private FaultFrameInfo _selectedFreezeFrame;
        private ObservableCollection<string> _targetAddresses;
        private string _statusMessage;
        private int _activeDtcCount;
        private int _pendingDtcCount;
        private int _permanentDtcCount;
        private bool _canReadDtc;
        private bool _canClearDtc;
        private bool _canRequestSecurityAccess;
        private BatteryInfo _batteryInfo;
        private ObservableCollection<CellVoltageItem> _cellVoltages;
        private ObservableCollection<TemperatureItem> _temperatures;
        private bool _isCellBalancingEnabled;
        private string _selectedDiagnosticView;
        private string _securityAccessKey;
        private bool _hasSecurityAccess;
        private byte _currentSessionType;
        private FaultStatus _faultStatus;
        #endregion

        #region 属性
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnected
        {
            get { return _isConnected; }
            set { SetProperty(ref _isConnected, value); }
        }

        /// <summary>
        /// 连接状态文本
        /// </summary>
        public string ConnectionStatusText
        {
            get { return _connectionStatusText; }
            set { SetProperty(ref _connectionStatusText, value); }
        }

        /// <summary>
        /// 连接状态颜色
        /// </summary>
        public string ConnectionStatusColor
        {
            get { return _connectionStatusColor; }
            set { SetProperty(ref _connectionStatusColor, value); }
        }

        /// <summary>
        /// 选中的目标地址
        /// </summary>
        public ushort SelectedTargetAddress
        {
            get { return _selectedTargetAddress; }
            set { SetProperty(ref _selectedTargetAddress, value); }
        }

        /// <summary>
        /// DTC列表
        /// </summary>
        public ObservableCollection<DtcInfo> DtcList
        {
            get { return _dtcList; }
            set { SetProperty(ref _dtcList, value); }
        }

        /// <summary>
        /// 数据流参数列表
        /// </summary>
        public ObservableCollection<DataStreamParameter> DataStreamParameters
        {
            get { return _dataStreamParameters; }
            set { SetProperty(ref _dataStreamParameters, value); }
        }

        /// <summary>
        /// 冻结帧列表
        /// </summary>
        public ObservableCollection<FaultFrameInfo> FreezeFrameList
        {
            get { return _freezeFrameList; }
            set { SetProperty(ref _freezeFrameList, value); }
        }

        /// <summary>
        /// 选中的冻结帧
        /// </summary>
        public FaultFrameInfo SelectedFreezeFrame
        {
            get { return _selectedFreezeFrame; }
            set { SetProperty(ref _selectedFreezeFrame, value); }
        }

        /// <summary>
        /// 目标地址列表
        /// </summary>
        public ObservableCollection<string> TargetAddresses
        {
            get { return _targetAddresses; }
            set { SetProperty(ref _targetAddresses, value); }
        }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { SetProperty(ref _statusMessage, value); }
        }

        /// <summary>
        /// 活动DTC数量
        /// </summary>
        public int ActiveDtcCount
        {
            get { return _activeDtcCount; }
            set { SetProperty(ref _activeDtcCount, value); }
        }

        /// <summary>
        /// 待处理DTC数量
        /// </summary>
        public int PendingDtcCount
        {
            get { return _pendingDtcCount; }
            set { SetProperty(ref _pendingDtcCount, value); }
        }

        /// <summary>
        /// 永久DTC数量
        /// </summary>
        public int PermanentDtcCount
        {
            get { return _permanentDtcCount; }
            set { SetProperty(ref _permanentDtcCount, value); }
        }

        /// <summary>
        /// 是否可以读取DTC
        /// </summary>
        public bool CanReadDtc
        {
            get { return _canReadDtc; }
            set { SetProperty(ref _canReadDtc, value); }
        }

        /// <summary>
        /// 是否可以清除DTC
        /// </summary>
        public bool CanClearDtc
        {
            get { return _canClearDtc; }
            set { SetProperty(ref _canClearDtc, value); }
        }

        /// <summary>
        /// 是否可以请求安全访问
        /// </summary>
        public bool CanRequestSecurityAccess
        {
            get { return _canRequestSecurityAccess; }
            set { SetProperty(ref _canRequestSecurityAccess, value); }
        }

        /// <summary>
        /// 电池信息
        /// </summary>
        public BatteryInfo BatteryInfo
        {
            get { return _batteryInfo; }
            set { SetProperty(ref _batteryInfo, value); }
        }

        /// <summary>
        /// 单体电池电压列表
        /// </summary>
        public ObservableCollection<CellVoltageItem> CellVoltages
        {
            get { return _cellVoltages; }
            set { SetProperty(ref _cellVoltages, value); }
        }

        /// <summary>
        /// 温度传感器列表
        /// </summary>
        public ObservableCollection<TemperatureItem> Temperatures
        {
            get { return _temperatures; }
            set { SetProperty(ref _temperatures, value); }
        }

        /// <summary>
        /// 是否启用电池均衡
        /// </summary>
        public bool IsCellBalancingEnabled
        {
            get { return _isCellBalancingEnabled; }
            set { SetProperty(ref _isCellBalancingEnabled, value); }
        }

        /// <summary>
        /// 选中的诊断视图
        /// </summary>
        public string SelectedDiagnosticView
        {
            get { return _selectedDiagnosticView; }
            set { SetProperty(ref _selectedDiagnosticView, value); }
        }

        /// <summary>
        /// 安全访问密钥
        /// </summary>
        public string SecurityAccessKey
        {
            get { return _securityAccessKey; }
            set { SetProperty(ref _securityAccessKey, value); }
        }

        /// <summary>
        /// 是否具有安全访问权限
        /// </summary>
        public bool HasSecurityAccess
        {
            get { return _hasSecurityAccess; }
            set { SetProperty(ref _hasSecurityAccess, value); }
        }

        /// <summary>
        /// 当前会话类型
        /// </summary>
        public byte CurrentSessionType
        {
            get { return _currentSessionType; }
            set { SetProperty(ref _currentSessionType, value); }
        }

        /// <summary>
        /// 故障状态
        /// </summary>
        public FaultStatus FaultStatus
        {
            get { return _faultStatus; }
            set { SetProperty(ref _faultStatus, value); }
        }
        #endregion

        #region 命令
        /// <summary>
        /// 连接命令
        /// </summary>
        public DelegateCommand ConnectCommand { get; private set; }

        /// <summary>
        /// 断开连接命令
        /// </summary>
        public DelegateCommand DisconnectCommand { get; private set; }

        /// <summary>
        /// 安全访问命令
        /// </summary>
        public DelegateCommand SecurityAccessCommand { get; private set; }

        /// <summary>
        /// 切换诊断会话命令
        /// </summary>
        public DelegateCommand<byte> SwitchSessionCommand { get; private set; }

        /// <summary>
        /// 读取DTC命令
        /// </summary>
        public DelegateCommand ReadDtcCommand { get; private set; }

        /// <summary>
        /// 清除DTC命令
        /// </summary>
        public DelegateCommand ClearDtcCommand { get; private set; }

        /// <summary>
        /// 读取电池信息命令
        /// </summary>
        public DelegateCommand ReadBatteryInfoCommand { get; private set; }

        /// <summary>
        /// 启动电池均衡命令
        /// </summary>
        public DelegateCommand StartCellBalancingCommand { get; private set; }

        /// <summary>
        /// 停止电池均衡命令
        /// </summary>
        public DelegateCommand StopCellBalancingCommand { get; private set; }

        /// <summary>
        /// 读取单体电压命令
        /// </summary>
        public DelegateCommand ReadCellVoltagesCommand { get; private set; }

        /// <summary>
        /// 读取温度传感器命令
        /// </summary>
        public DelegateCommand ReadTemperaturesCommand { get; private set; }

        /// <summary>
        /// 读取故障状态命令
        /// </summary>
        public DelegateCommand ReadFaultStatusCommand { get; private set; }

        /// <summary>
        /// 切换诊断视图命令
        /// </summary>
        public DelegateCommand<string> SwitchDiagnosticViewCommand { get; private set; }
        #endregion

        /// <summary>
        /// 构造函数
        /// </summary>
        public ObdMonitorViewModel(IObdService obdService, IRegionManager regionManager)
        {
            _obdService = obdService;
            _regionManager = regionManager;
            
            // 初始化属性
            InitializeProperties();
            
            // 初始化命令
            InitializeCommands();
            
            // 初始化定时器
            InitializeTimer();
            
            // 注册事件处理程序
            RegisterEventHandlers();
            
            // 初始化目标地址列表
            InitializeTargetAddresses();
        }

        /// <summary>
        /// 初始化属性
        /// </summary>
        private void InitializeProperties()
        {
            IsConnected = false;
            ConnectionStatusText = "未连接";
            ConnectionStatusColor = "Red";
            SelectedTargetAddress = 0x700; // 默认地址
            DtcList = new ObservableCollection<DtcInfo>();
            DataStreamParameters = new ObservableCollection<DataStreamParameter>();
            FreezeFrameList = new ObservableCollection<FaultFrameInfo>();
            TargetAddresses = new ObservableCollection<string>();
            StatusMessage = "就绪";
            ActiveDtcCount = 0;
            PendingDtcCount = 0;
            PermanentDtcCount = 0;
            CanReadDtc = false;
            CanClearDtc = false;
            CanRequestSecurityAccess = false;
            CellVoltages = new ObservableCollection<CellVoltageItem>();
            Temperatures = new ObservableCollection<TemperatureItem>();
            IsCellBalancingEnabled = false;
            SelectedDiagnosticView = "DTCView";
            SecurityAccessKey = "1";
            HasSecurityAccess = false;
            CurrentSessionType = 0x01; // 默认会话
            
            // 初始化数据流参数
            InitializeDataStreamParameters();
        }

        /// <summary>
        /// 初始化数据流参数
        /// </summary>
        private void InitializeDataStreamParameters()
        {
            DataStreamParameters.Add(new DataStreamParameter { ParameterName = "发动机转速", ParameterValue = "0", Unit = "RPM", MinValue = 0, MaxValue = 8000 });
            DataStreamParameters.Add(new DataStreamParameter { ParameterName = "冷却液温度", ParameterValue = "0", Unit = "°C", MinValue = -40, MaxValue = 130 });
            DataStreamParameters.Add(new DataStreamParameter { ParameterName = "进气温度", ParameterValue = "0", Unit = "°C", MinValue = -40, MaxValue = 85 });
            DataStreamParameters.Add(new DataStreamParameter { ParameterName = "燃油压力", ParameterValue = "0", Unit = "kPa", MinValue = 0, MaxValue = 500 });
            DataStreamParameters.Add(new DataStreamParameter { ParameterName = "节气门位置", ParameterValue = "0", Unit = "%", MinValue = 0, MaxValue = 100 });
            DataStreamParameters.Add(new DataStreamParameter { ParameterName = "车速", ParameterValue = "0", Unit = "km/h", MinValue = 0, MaxValue = 250 });
            DataStreamParameters.Add(new DataStreamParameter { ParameterName = "电池电压", ParameterValue = "0", Unit = "V", MinValue = 10, MaxValue = 16 });
            DataStreamParameters.Add(new DataStreamParameter { ParameterName = "空气质量流量", ParameterValue = "0", Unit = "g/s", MinValue = 0, MaxValue = 500 });
        }

        /// <summary>
        /// 初始化命令
        /// </summary>
        private void InitializeCommands()
        {
            ConnectCommand = new DelegateCommand(OnConnect, CanConnect);
            DisconnectCommand = new DelegateCommand(OnDisconnect, CanDisconnect);
            SecurityAccessCommand = new DelegateCommand(OnSecurityAccess, CanSecurityAccess);
            SwitchSessionCommand = new DelegateCommand<byte>(OnSwitchSession, CanSwitchSession);
            ReadDtcCommand = new DelegateCommand(OnReadDtc, CanReadDtc);
            ClearDtcCommand = new DelegateCommand(OnClearDtc, CanClearDtc);
            ReadBatteryInfoCommand = new DelegateCommand(OnReadBatteryInfo, CanReadBatteryInfo);
            StartCellBalancingCommand = new DelegateCommand(OnStartCellBalancing, CanStartCellBalancing);
            StopCellBalancingCommand = new DelegateCommand(OnStopCellBalancing, CanStopCellBalancing);
            ReadCellVoltagesCommand = new DelegateCommand(OnReadCellVoltages, CanReadCellVoltages);
            ReadTemperaturesCommand = new DelegateCommand(OnReadTemperatures, CanReadTemperatures);
            ReadFaultStatusCommand = new DelegateCommand(OnReadFaultStatus, CanReadFaultStatus);
            SwitchDiagnosticViewCommand = new DelegateCommand<string>(OnSwitchDiagnosticView);
        }

        /// <summary>
        /// 初始化定时器
        /// </summary>
        private void InitializeTimer()
        {
            _refreshTimer = new DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromSeconds(1);
            _refreshTimer.Tick += OnRefreshTimerTick;
        }

        /// <summary>
        /// 注册事件处理程序
        /// </summary>
        private void RegisterEventHandlers()
        {
            _obdService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _obdService.DtcUpdated += OnDtcUpdated;
        }

        /// <summary>
        /// 初始化目标地址列表
        /// </summary>
        private void InitializeTargetAddresses()
        {
            TargetAddresses.Add("0x700 - BMS主控制器");
            TargetAddresses.Add("0x701 - BMS从控制器1");
            TargetAddresses.Add("0x702 - BMS从控制器2");
            TargetAddresses.Add("0x703 - 电池信息采集器");
            TargetAddresses.Add("0x704 - 电机控制器");
        }

        /// <summary>
        /// 更新命令可执行状态
        /// </summary>
        private void UpdateCanExecute()
        {
            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            SecurityAccessCommand.RaiseCanExecuteChanged();
            SwitchSessionCommand.RaiseCanExecuteChanged();
            ReadDtcCommand.RaiseCanExecuteChanged();
            ClearDtcCommand.RaiseCanExecuteChanged();
            ReadBatteryInfoCommand.RaiseCanExecuteChanged();
            StartCellBalancingCommand.RaiseCanExecuteChanged();
            StopCellBalancingCommand.RaiseCanExecuteChanged();
            ReadCellVoltagesCommand.RaiseCanExecuteChanged();
            ReadTemperaturesCommand.RaiseCanExecuteChanged();
            ReadFaultStatusCommand.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 连接命令执行方法
        /// </summary>
        private void OnConnect()
        {
            try
            {
                StatusMessage = "正在连接到目标设备...";
                bool success = _obdService.Connect(SelectedTargetAddress);
                
                if (success)
                {
                    StatusMessage = "连接成功";
                    _refreshTimer.Start();
                }
                else
                {
                    StatusMessage = "连接失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "连接异常: " + ex.Message;
                ShowMessage(ex.Message, "连接异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以连接
        /// </summary>
        private bool CanConnect()
        {
            return !IsConnected;
        }

        /// <summary>
        /// 断开连接命令执行方法
        /// </summary>
        private void OnDisconnect()
        {
            try
            {
                StatusMessage = "正在断开连接...";
                _obdService.Disconnect();
                _refreshTimer.Stop();
                StatusMessage = "已断开连接";
                ResetState();
            }
            catch (Exception ex)
            {
                StatusMessage = "断开连接异常: " + ex.Message;
                ShowMessage(ex.Message, "断开连接异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以断开连接
        /// </summary>
        private bool CanDisconnect()
        {
            return IsConnected;
        }

        /// <summary>
        /// 安全访问命令执行方法
        /// </summary>
        private void OnSecurityAccess()
        {
            try
            {
                if (byte.TryParse(SecurityAccessKey, out byte securityLevel))
                {
                    StatusMessage = "正在请求安全访问...";
                    bool success = _obdService.RequestSecurityAccess(securityLevel);
                    
                    if (success)
                    {
                        HasSecurityAccess = true;
                        StatusMessage = "安全访问成功";
                        ShowMessage("安全访问成功", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        StatusMessage = "安全访问失败";
                        ShowMessage("安全访问失败，请检查密钥是否正确", "操作失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    ShowMessage("无效的安全级别，请输入有效的数字", "输入错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "安全访问异常: " + ex.Message;
                ShowMessage(ex.Message, "安全访问异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                UpdateCanExecute();
            }
        }

        /// <summary>
        /// 判断是否可以请求安全访问
        /// </summary>
        private bool CanSecurityAccess()
        {
            return IsConnected && !HasSecurityAccess;
        }

        /// <summary>
        /// 切换会话命令执行方法
        /// </summary>
        private void OnSwitchSession(byte sessionType)
        {
            try
            {
                StatusMessage = $"正在切换到会话类型 {sessionType:X2}...";
                bool success = _obdService.SetDiagnosticSession(sessionType);
                
                if (success)
                {
                    CurrentSessionType = sessionType;
                    StatusMessage = $"已切换到会话类型 {sessionType:X2}";
                }
                else
                {
                    StatusMessage = $"切换到会话类型 {sessionType:X2} 失败";
                    ShowMessage("会话切换失败，可能需要先获取安全访问权限", "操作失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "会话切换异常: " + ex.Message;
                ShowMessage(ex.Message, "会话切换异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以切换会话
        /// </summary>
        private bool CanSwitchSession(byte sessionType)
        {
            return IsConnected;
        }

        /// <summary>
        /// 读取DTC命令执行方法
        /// </summary>
        private void OnReadDtc()
        {
            try
            {
                StatusMessage = "正在读取故障码...";
                var dtcList = _obdService.ReadDTCs(1); // 读取所有DTC
                
                DtcList.Clear();
                foreach (var dtc in dtcList)
                {
                    DtcList.Add(dtc);
                }
                
                // 更新DTC统计
                UpdateDtcStatistics();
                
                StatusMessage = "故障码读取完成";
            }
            catch (Exception ex)
            {
                StatusMessage = "读取故障码异常: " + ex.Message;
                ShowMessage(ex.Message, "读取故障码异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以读取DTC
        /// </summary>
        private bool CanReadDtc()
        {
            return IsConnected;
        }

        /// <summary>
        /// 清除DTC命令执行方法
        /// </summary>
        private void OnClearDtc()
        {
            try
            {
                if (MessageBox.Show("确定要清除所有故障码吗？", "确认清除", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    StatusMessage = "正在清除故障码...";
                    bool success = _obdService.ClearDTCs();
                    
                    if (success)
                    {
                        DtcList.Clear();
                        UpdateDtcStatistics();
                        StatusMessage = "故障码清除成功";
                        ShowMessage("故障码已成功清除", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        StatusMessage = "故障码清除失败";
                        ShowMessage("故障码清除失败，可能需要先获取安全访问权限", "操作失败", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "清除故障码异常: " + ex.Message;
                ShowMessage(ex.Message, "清除故障码异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以清除DTC
        /// </summary>
        private bool CanClearDtc()
        {
            return IsConnected && HasSecurityAccess && DtcList.Count > 0;
        }

        /// <summary>
        /// 读取电池信息命令执行方法
        /// </summary>
        private void OnReadBatteryInfo()
        {
            try
            {
                StatusMessage = "正在读取电池信息...";
                BatteryInfo = _obdService.ReadBatteryInfo();
                
                if (BatteryInfo != null)
                {
                    StatusMessage = "电池信息读取完成";
                }
                else
                {
                    StatusMessage = "电池信息读取失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "读取电池信息异常: " + ex.Message;
                ShowMessage(ex.Message, "读取电池信息异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以读取电池信息
        /// </summary>
        private bool CanReadBatteryInfo()
        {
            return IsConnected;
        }

        /// <summary>
        /// 启动电池均衡命令执行方法
        /// </summary>
        private void OnStartCellBalancing()
        {
            try
            {
                StatusMessage = "正在启动电池均衡...";
                bool success = _obdService.StartCellBalancing();
                
                if (success)
                {
                    IsCellBalancingEnabled = true;
                    StatusMessage = "电池均衡已启动";
                    ShowMessage("电池均衡功能已成功启动", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "电池均衡启动失败";
                    ShowMessage("电池均衡启动失败，可能需要先获取安全访问权限", "操作失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "启动电池均衡异常: " + ex.Message;
                ShowMessage(ex.Message, "启动电池均衡异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以启动电池均衡
        /// </summary>
        private bool CanStartCellBalancing()
        {
            return IsConnected && HasSecurityAccess && !IsCellBalancingEnabled;
        }

        /// <summary>
        /// 停止电池均衡命令执行方法
        /// </summary>
        private void OnStopCellBalancing()
        {
            try
            {
                StatusMessage = "正在停止电池均衡...";
                bool success = _obdService.StopCellBalancing();
                
                if (success)
                {
                    IsCellBalancingEnabled = false;
                    StatusMessage = "电池均衡已停止";
                    ShowMessage("电池均衡功能已成功停止", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "电池均衡停止失败";
                    ShowMessage("电池均衡停止失败，可能需要先获取安全访问权限", "操作失败", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "停止电池均衡异常: " + ex.Message;
                ShowMessage(ex.Message, "停止电池均衡异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以停止电池均衡
        /// </summary>
        private bool CanStopCellBalancing()
        {
            return IsConnected && HasSecurityAccess && IsCellBalancingEnabled;
        }

        /// <summary>
        /// 读取单体电压命令执行方法
        /// </summary>
        private void OnReadCellVoltages()
        {
            try
            {
                StatusMessage = "正在读取单体电压...";
                var voltages = _obdService.ReadCellVoltages();
                
                CellVoltages.Clear();
                for (int i = 0; i < voltages.Count; i++)
                {
                    CellVoltages.Add(new CellVoltageItem
                    {
                        CellNumber = i + 1,
                        Voltage = voltages[i]
                    });
                }
                
                StatusMessage = "单体电压读取完成";
            }
            catch (Exception ex)
            {
                StatusMessage = "读取单体电压异常: " + ex.Message;
                ShowMessage(ex.Message, "读取单体电压异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以读取单体电压
        /// </summary>
        private bool CanReadCellVoltages()
        {
            return IsConnected;
        }

        /// <summary>
        /// 读取温度传感器命令执行方法
        /// </summary>
        private void OnReadTemperatures()
        {
            try
            {
                StatusMessage = "正在读取温度传感器...";
                var temperatures = _obdService.ReadTemperatureSensors();
                
                Temperatures.Clear();
                string[] locations = { "电池模组1", "电池模组2", "电池模组3", "电池模组4", "环境", "控制板" };
                
                for (int i = 0; i < temperatures.Count; i++)
                {
                    Temperatures.Add(new TemperatureItem
                    {
                        SensorNumber = i + 1,
                        Location = i < locations.Length ? locations[i] : $"传感器{i + 1}",
                        Temperature = temperatures[i]
                    });
                }
                
                StatusMessage = "温度传感器读取完成";
            }
            catch (Exception ex)
            {
                StatusMessage = "读取温度传感器异常: " + ex.Message;
                ShowMessage(ex.Message, "读取温度传感器异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以读取温度传感器
        /// </summary>
        private bool CanReadTemperatures()
        {
            return IsConnected;
        }

        /// <summary>
        /// 读取故障状态命令执行方法
        /// </summary>
        private void OnReadFaultStatus()
        {
            try
            {
                StatusMessage = "正在读取故障状态...";
                FaultStatus = _obdService.ReadFaultStatus();
                
                if (FaultStatus != null)
                {
                    StatusMessage = "故障状态读取完成";
                }
                else
                {
                    StatusMessage = "故障状态读取失败";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "读取故障状态异常: " + ex.Message;
                ShowMessage(ex.Message, "读取故障状态异常", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 判断是否可以读取故障状态
        /// </summary>
        private bool CanReadFaultStatus()
        {
            return IsConnected;
        }

        /// <summary>
        /// 切换诊断视图命令执行方法
        /// </summary>
        private void OnSwitchDiagnosticView(string viewName)
        {
            SelectedDiagnosticView = viewName;
        }

        /// <summary>
        /// 连接状态变更事件处理
        /// </summary>
        private void OnConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            IsConnected = e.IsConnected;
            ConnectionStatusText = IsConnected ? "已连接" : "未连接";
            ConnectionStatusColor = IsConnected ? "Green" : "Red";
            
            if (IsConnected)
            {
                StatusMessage = "连接成功";
                HasSecurityAccess = false;
            }
            else
            {
                StatusMessage = "连接断开";
                ResetState();
            }
            
            UpdateCanExecute();
        }

        /// <summary>
        /// DTC更新事件处理
        /// </summary>
        private void OnDtcUpdated(object sender, DtcUpdatedEventArgs e)
        {
            if (e.DtcList != null)
            {
                DtcList.Clear();
                foreach (var dtc in e.DtcList)
                {
                    DtcList.Add(dtc);
                }
                
                UpdateDtcStatistics();
            }
        }

        /// <summary>
        /// 刷新定时器Tick事件处理
        /// </summary>
        private void OnRefreshTimerTick(object sender, EventArgs e)
        {
            // 定期更新电池信息
            if (IsConnected && SelectedDiagnosticView == "BMSView")
            {
                // 异步更新电池信息，避免UI卡顿
                Task.Run(() =>
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        BatteryInfo = _obdService.ReadBatteryInfo();
                    });
                });
            }
        }

        /// <summary>
        /// 更新DTC统计信息
        /// </summary>
        private void UpdateDtcStatistics()
        {
            ActiveDtcCount = DtcList.Count(dtc => dtc.Status == "Active");
            PendingDtcCount = DtcList.Count(dtc => dtc.Status == "Pending");
            PermanentDtcCount = DtcList.Count(dtc => dtc.Status == "Permanent");
            UpdateCanExecute();
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        private void ResetState()
        {
            HasSecurityAccess = false;
            IsCellBalancingEnabled = false;
            CurrentSessionType = 0x01;
            DtcList.Clear();
            UpdateDtcStatistics();
            BatteryInfo = null;
            CellVoltages.Clear();
            Temperatures.Clear();
            FaultStatus = null;
        }

        /// <summary>
        /// 显示消息
        /// </summary>
        private void ShowMessage(string message, string title, MessageBoxButton buttons, MessageBoxImage icon)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, title, buttons, icon);
            });
        }
    }

    #region 辅助类
    /// <summary>
    /// 数据流参数类
    /// </summary>
    public class DataStreamParameter
    {
        public string ParameterName { get; set; }
        public string ParameterValue { get; set; }
        public string Unit { get; set; }
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
    }

    /// <summary>
    /// 冻结帧信息类
    /// </summary>
    public class FaultFrameInfo
    {
        public string DtcCode { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, string> Parameters { get; set; }
        public byte FrameNumber { get; set; }
    }

    /// <summary>
    /// 单体电池电压项
    /// </summary>
    public class CellVoltageItem
    {
        public int CellNumber { get; set; }
        public float Voltage { get; set; }
    }

    /// <summary>
    /// 温度传感器项
    /// </summary>
    public class TemperatureItem
    {
        public int SensorNumber { get; set; }
        public string Location { get; set; }
        public float Temperature { get; set; }
    }
    #endregion
}