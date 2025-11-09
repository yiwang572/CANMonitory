using CANMonitor.Interfaces;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace CANMonitor.ViewModels
{
    /// <summary>
    /// 诊断功能视图模型
    /// </summary>
    public class DiagnoseViewModel : BindableBase
    {
        private readonly IObdService _obdService;
        private readonly ILogService _logService;
        private bool _isConnected;
        private bool _hasSecurityAccess;
        private string _connectionStatus = "未连接";
        private Brush _connectionStatusColor = Brushes.Red;
        private DiagnosticTarget _selectedTarget;
        private ObservableCollection<DiagnosticTarget> _availableTargets;
        private string _sessionInfo;
        private int _totalDtcCount;
        private int _activeDtcCount;
        private int _pendingDtcCount;
        private ObservableCollection<DtcInfo> _dtcList = new ObservableCollection<DtcInfo>();
        private string _batteryVoltage = "--";
        private string _batteryCurrent = "--";
        private string _batterySOC = "--";
        private string _batterySOH = "--";
        private string _batteryTemperature = "--";

        /// <summary>
        /// 可用诊断目标列表
        /// </summary>
        public ObservableCollection<DiagnosticTarget> AvailableTargets
        {
            get => _availableTargets;
            set => SetProperty(ref _availableTargets, value);
        }

        /// <summary>
        /// 选中的诊断目标
        /// </summary>
        public DiagnosticTarget SelectedTarget
        {
            get => _selectedTarget;
            set => SetProperty(ref _selectedTarget, value);
        }

        /// <summary>
        /// 是否已选择目标
        /// </summary>
        public bool IsTargetSelected => SelectedTarget != null;

        /// <summary>
        /// 连接状态
        /// </summary>
        public string ConnectionStatus
        {
            get => _connectionStatus;
            set => SetProperty(ref _connectionStatus, value);
        }

        /// <summary>
        /// 连接状态颜色
        /// </summary>
        public Brush ConnectionStatusColor
        {
            get => _connectionStatusColor;
            set => SetProperty(ref _connectionStatusColor, value);
        }

        /// <summary>
        /// 会话信息
        /// </summary>
        public string SessionInfo
        {
            get => _sessionInfo;
            set => SetProperty(ref _sessionInfo, value);
        }

        /// <summary>
        /// DTC列表
        /// </summary>
        public ObservableCollection<DtcInfo> DtcList
        {
            get => _dtcList;
            set => SetProperty(ref _dtcList, value);
        }

        /// <summary>
        /// 总DTC数量
        /// </summary>
        public int TotalDtcCount
        {
            get => _totalDtcCount;
            set => SetProperty(ref _totalDtcCount, value);
        }

        /// <summary>
        /// 活跃DTC数量
        /// </summary>
        public int ActiveDtcCount
        {
            get => _activeDtcCount;
            set => SetProperty(ref _activeDtcCount, value);
        }

        /// <summary>
        /// 待处理DTC数量
        /// </summary>
        public int PendingDtcCount
        {
            get => _pendingDtcCount;
            set => SetProperty(ref _pendingDtcCount, value);
        }

        /// <summary>
        /// 电池电压
        /// </summary>
        public string BatteryVoltage
        {
            get => _batteryVoltage;
            set => SetProperty(ref _batteryVoltage, value);
        }

        /// <summary>
        /// 电池电流
        /// </summary>
        public string BatteryCurrent
        {
            get => _batteryCurrent;
            set => SetProperty(ref _batteryCurrent, value);
        }

        /// <summary>
        /// 电池SOC
        /// </summary>
        public string BatterySOC
        {
            get => _batterySOC;
            set => SetProperty(ref _batterySOC, value);
        }

        /// <summary>
        /// 电池SOH
        /// </summary>
        public string BatterySOH
        {
            get => _batterySOH;
            set => SetProperty(ref _batterySOH, value);
        }

        /// <summary>
        /// 电池温度
        /// </summary>
        public string BatteryTemperature
        {
            get => _batteryTemperature;
            set => SetProperty(ref _batteryTemperature, value);
        }

        // 命令定义
        public DelegateCommand ConnectCommand { get; }
        public DelegateCommand DisconnectCommand { get; }
        public DelegateCommand SecurityAccessCommand { get; }
        public DelegateCommand DefaultSessionCommand { get; }
        public DelegateCommand ProgrammingSessionCommand { get; }
        public DelegateCommand ExtendedDiagnosticSessionCommand { get; }
        public DelegateCommand SafetySystemDiagnosticSessionCommand { get; }
        public DelegateCommand ECUResetCommand { get; }
        public DelegateCommand ReadEcuInfoCommand { get; }
        public DelegateCommand ReadAllDTCsCommand { get; }
        public DelegateCommand ReadTestFailedDTCsCommand { get; }
        public DelegateCommand ReadPendingDTCsCommand { get; }
        public DelegateCommand ClearAllDTCsCommand { get; }
        public DelegateCommand ReadBatteryInfoCommand { get; }
        public DelegateCommand StartBalancingCommand { get; }
        public DelegateCommand StopBalancingCommand { get; }
        public DelegateCommand ReadCellVoltagesCommand { get; }
        public DelegateCommand ReadTempSensorsCommand { get; }
        public DelegateCommand ReadFaultStatusCommand { get; }

        /// <summary>
        /// 构造函数
        /// </summary>
        public DiagnoseViewModel(IObdService obdService, ILogService logService)
        {
            _obdService = obdService;
            _logService = logService;
            _isConnected = false;
            _hasSecurityAccess = false;

            // 初始化诊断目标
            AvailableTargets = new ObservableCollection<DiagnosticTarget>
            {
                new DiagnosticTarget { Name = "BMS", Address = 0x700 },
                new DiagnosticTarget { Name = "MCU", Address = 0x710 },
                new DiagnosticTarget { Name = "VCU", Address = 0x720 },
                new DiagnosticTarget { Name = "PEU", Address = 0x730 }
            };

            // 默认选择第一个目标
            if (AvailableTargets.Any())
                SelectedTarget = AvailableTargets[0];

            // 初始化命令
            ConnectCommand = new DelegateCommand(OnConnect, CanConnect);
            DisconnectCommand = new DelegateCommand(OnDisconnect, CanDisconnect);
            SecurityAccessCommand = new DelegateCommand(OnSecurityAccess, CanExecuteDiagnosticCommand);
            DefaultSessionCommand = new DelegateCommand(() => SwitchSession(0x01), CanExecuteDiagnosticCommand);
            ProgrammingSessionCommand = new DelegateCommand(() => SwitchSession(0x02), CanExecuteDiagnosticCommand);
            ExtendedDiagnosticSessionCommand = new DelegateCommand(() => SwitchSession(0x03), CanExecuteDiagnosticCommand);
            SafetySystemDiagnosticSessionCommand = new DelegateCommand(() => SwitchSession(0x04), CanExecuteDiagnosticCommand);
            ECUResetCommand = new DelegateCommand(OnECUReset, CanExecuteDiagnosticCommand);
            ReadEcuInfoCommand = new DelegateCommand(OnReadEcuInfo, CanExecuteDiagnosticCommand);
            ReadAllDTCsCommand = new DelegateCommand(OnReadAllDTCs, CanExecuteDiagnosticCommand);
            ReadTestFailedDTCsCommand = new DelegateCommand(OnReadTestFailedDTCs, CanExecuteDiagnosticCommand);
            ReadPendingDTCsCommand = new DelegateCommand(OnReadPendingDTCs, CanExecuteDiagnosticCommand);
            ClearAllDTCsCommand = new DelegateCommand(OnClearAllDTCs, CanExecuteDiagnosticCommand);
            ReadBatteryInfoCommand = new DelegateCommand(OnReadBatteryInfo, CanExecuteDiagnosticCommand);
            StartBalancingCommand = new DelegateCommand(OnStartBalancing, CanExecuteDiagnosticCommand);
            StopBalancingCommand = new DelegateCommand(OnStopBalancing, CanExecuteDiagnosticCommand);
            ReadCellVoltagesCommand = new DelegateCommand(OnReadCellVoltages, CanExecuteDiagnosticCommand);
            ReadTempSensorsCommand = new DelegateCommand(OnReadTempSensors, CanExecuteDiagnosticCommand);
            ReadFaultStatusCommand = new DelegateCommand(OnReadFaultStatus, CanExecuteDiagnosticCommand);

            // 订阅事件
            _obdService.ConnectionStatusChanged += OnConnectionStatusChanged;
            _obdService.DtcUpdated += OnDtcUpdated;
        }

        /// <summary>
        /// 连接命令执行
        /// </summary>
        private void OnConnect()
        {
            try
            {
                if (SelectedTarget != null)
                {
                    _logService.Info($"正在连接到 {SelectedTarget.Name}...");
                    bool success = _obdService.Connect(SelectedTarget.Address);
                    
                    if (success)
                    {
                        _logService.Info($"成功连接到 {SelectedTarget.Name}");
                        _isConnected = true;
                        UpdateCommandCanExecute();
                    }
                    else
                    {
                        _logService.Error($"连接到 {SelectedTarget.Name} 失败");
                    }
                }
            }
            catch (Exception ex)
            {
                _logService.Error("连接过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 断开连接命令执行
        /// </summary>
        private void OnDisconnect()
        {
            try
            {
                _logService.Info("正在断开连接...");
                _obdService.Disconnect();
                _isConnected = false;
                _hasSecurityAccess = false;
                UpdateCommandCanExecute();
            }
            catch (Exception ex)
            {
                _logService.Error("断开连接过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 安全访问命令执行
        /// </summary>
        private void OnSecurityAccess()
        {
            try
            {
                _logService.Info("执行安全访问...");
                bool success = _obdService.RequestSecurityAccess(0x01);
                
                if (success)
                {
                    _logService.Info("安全访问成功");
                    _hasSecurityAccess = true;
                    SessionInfo += "\n安全访问: 成功";
                }
                else
                {
                    _logService.Error("安全访问失败");
                    SessionInfo += "\n安全访问: 失败";
                }
            }
            catch (Exception ex)
            {
                _logService.Error("安全访问过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 切换诊断会话
        /// </summary>
        private void SwitchSession(byte sessionId)
        {
            try
            {
                string sessionName = GetSessionName(sessionId);
                _logService.Info($"切换到 {sessionName} 会话...");
                bool success = _obdService.SetDiagnosticSession(sessionId);
                
                if (success)
                {
                    _logService.Info($"成功切换到 {sessionName} 会话");
                    SessionInfo = $"当前会话: {sessionName}\n切换时间: {DateTime.Now}";
                }
                else
                {
                    _logService.Error($"切换到 {sessionName} 会话失败");
                    SessionInfo += $"\n切换到 {sessionName} 会话: 失败";
                }
            }
            catch (Exception ex)
            {
                _logService.Error("切换会话过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 获取会话名称
        /// </summary>
        private string GetSessionName(byte sessionId)
        {
            switch (sessionId)
            {
                case 0x01: return "默认会话";
                case 0x02: return "编程会话";
                case 0x03: return "扩展诊断会话";
                case 0x04: return "安全系统诊断会话";
                default: return "未知会话";
            }
        }

        /// <summary>
        /// ECU重置命令执行
        /// </summary>
        private void OnECUReset()
        {
            try
            {
                _logService.Info("执行ECU重置...");
                bool success = _obdService.ECUReset(0x01); // 硬重置
                
                if (success)
                {
                    _logService.Info("ECU重置成功");
                    SessionInfo += "\nECU重置: 成功";
                }
                else
                {
                    _logService.Error("ECU重置失败");
                    SessionInfo += "\nECU重置: 失败";
                }
            }
            catch (Exception ex)
            {
                _logService.Error("ECU重置过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 读取ECU信息命令执行
        /// </summary>
        private void OnReadEcuInfo()
        {
            try
            {
                _logService.Info("读取ECU信息...");
                var ecuInfoData = _obdService.ReadEcuIdentification(0x01); // 读取供应商ID
                
                if (ecuInfoData != null && ecuInfoData.Length > 0)
                {
                    _logService.Info("ECU信息读取成功");
                    string hexInfo = BitConverter.ToString(ecuInfoData);
                    SessionInfo += $"\n\nECU信息:\n供应商ID: {hexInfo}";
                }
                else
                {
                    _logService.Error("ECU信息读取失败");
                    SessionInfo += "\nECU信息读取: 失败";
                }
            }
            catch (Exception ex)
            {
                _logService.Error("读取ECU信息过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 读取所有DTC命令执行
        /// </summary>
        private void OnReadAllDTCs()
        {
            ReadDTCs(0x01);
        }

        /// <summary>
        /// 读取测试失败DTC命令执行
        /// </summary>
        private void OnReadTestFailedDTCs()
        {
            ReadDTCs(0x02);
        }

        /// <summary>
        /// 读取待处理DTC命令执行
        /// </summary>
        private void OnReadPendingDTCs()
        {
            ReadDTCs(0x03);
        }

        /// <summary>
        /// 读取DTC
        /// </summary>
        private void ReadDTCs(byte mode)
        {
            try
            {
                _logService.Info("读取DTC...");
                var dtcs = _obdService.ReadDTCs(mode);
                UpdateDtcList(dtcs);
            }
            catch (Exception ex)
            {
                _logService.Error("读取DTC过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 清除所有DTC命令执行
        /// </summary>
        private void OnClearAllDTCs()
        {
            try
            {
                _logService.Info("清除所有DTC...");
                bool success = _obdService.ClearDTCs();
                
                if (success)
                {
                    _logService.Info("DTC清除成功");
                    DtcList.Clear();
                    TotalDtcCount = 0;
                    ActiveDtcCount = 0;
                    PendingDtcCount = 0;
                }
                else
                {
                    _logService.Error("DTC清除失败");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("清除DTC过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 读取电池信息命令执行
        /// </summary>
        private void OnReadBatteryInfo()
        {
            try
            {
                _logService.Info("读取电池信息...");
                var batteryInfo = _obdService.ReadBatteryInfo();
                
                if (batteryInfo != null)
                {
                    _logService.Info("电池信息读取成功");
                    BatteryVoltage = $"{batteryInfo.Voltage} V";
                    BatteryCurrent = $"{batteryInfo.Current} A";
                    BatterySOC = $"{batteryInfo.SOC}%";
                    BatterySOH = $"{batteryInfo.SOH}%";
                    BatteryTemperature = $"{batteryInfo.Temperature} °C";
                }
                else
                {
                    _logService.Error("电池信息读取失败");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("读取电池信息过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 开始均衡命令执行
        /// </summary>
        private void OnStartBalancing()
        {
            try
            {
                _logService.Info("开始电池均衡...");
                bool success = _obdService.StartCellBalancing();
                
                if (success)
                {
                    _logService.Info("电池均衡已启动");
                }
                else
                {
                    _logService.Error("电池均衡启动失败");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("启动电池均衡过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 停止均衡命令执行
        /// </summary>
        private void OnStopBalancing()
        {
            try
            {
                _logService.Info("停止电池均衡...");
                bool success = _obdService.StopCellBalancing();
                
                if (success)
                {
                    _logService.Info("电池均衡已停止");
                }
                else
                {
                    _logService.Error("电池均衡停止失败");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("停止电池均衡过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 读取单体电压命令执行
        /// </summary>
        private void OnReadCellVoltages()
        {
            try
            {
                _logService.Info("读取单体电压...");
                var cellVoltages = _obdService.ReadCellVoltages();
                
                if (cellVoltages != null && cellVoltages.Any())
                {
                    _logService.Info("单体电压读取成功");
                    // 显示单体电压信息
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("单体电压信息:");
                    for (int i = 0; i < cellVoltages.Count; i++)
                    {
                        sb.AppendLine($"单体 {i + 1}: {cellVoltages[i]} V");
                    }
                    sb.AppendLine($"最高电压: {cellVoltages.Max()} V");
                    sb.AppendLine($"最低电压: {cellVoltages.Min()} V");
                    sb.AppendLine($"平均电压: {cellVoltages.Average()} V");
                    
                    MessageBox.Show(sb.ToString(), "单体电压", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logService.Error("单体电压读取失败");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("读取单体电压过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 读取温度传感器命令执行
        /// </summary>
        private void OnReadTempSensors()
        {
            try
            {
                _logService.Info("读取温度传感器...");
                var temperatures = _obdService.ReadTemperatureSensors();
                
                if (temperatures != null && temperatures.Any())
                {
                    _logService.Info("温度传感器读取成功");
                    // 显示温度信息
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("温度传感器信息:");
                    for (int i = 0; i < temperatures.Count; i++)
                    {
                        sb.AppendLine($"温度 {i + 1}: {temperatures[i]} °C");
                    }
                    sb.AppendLine($"最高温度: {temperatures.Max()} °C");
                    sb.AppendLine($"最低温度: {temperatures.Min()} °C");
                    sb.AppendLine($"平均温度: {temperatures.Average()} °C");
                    
                    MessageBox.Show(sb.ToString(), "温度传感器", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logService.Error("温度传感器读取失败");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("读取温度传感器过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 读取故障状态命令执行
        /// </summary>
        private void OnReadFaultStatus()
        {
            try
            {
                _logService.Info("读取故障状态...");
                var faultStatus = _obdService.ReadFaultStatus();
                
                if (faultStatus != null)
                {
                    _logService.Info("故障状态读取成功");
                    // 显示故障状态信息
                    StringBuilder sb = new StringBuilder();
                    sb.AppendLine("故障状态:");
                    sb.AppendLine($"充电故障: {(faultStatus.ChargingFault ? "是" : "否")}");
                    sb.AppendLine($"放电故障: {(faultStatus.DischargingFault ? "是" : "否")}");
                    sb.AppendLine($"温度故障: {(faultStatus.TemperatureFault ? "是" : "否")}");
                    sb.AppendLine($"电压故障: {(faultStatus.VoltageFault ? "是" : "否")}");
                    sb.AppendLine($"通讯故障: {(faultStatus.CommunicationFault ? "是" : "否")}");
                    
                    MessageBox.Show(sb.ToString(), "故障状态", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _logService.Error("故障状态读取失败");
                }
            }
            catch (Exception ex)
            {
                _logService.Error("读取故障状态过程中发生异常", ex);
            }
        }

        /// <summary>
        /// 更新DTC列表
        /// </summary>
        private void UpdateDtcList(List<DtcInfo> dtcs)
        {
            DtcList.Clear();
            
            if (dtcs != null && dtcs.Any())
            {
                _logService.Info($"成功读取 {dtcs.Count} 个DTC");
                
                foreach (var dtc in dtcs)
                {
                    DtcList.Add(dtc);
                }
                
                TotalDtcCount = dtcs.Count;
                ActiveDtcCount = dtcs.Count(d => d.Status.Contains("活跃"));
                PendingDtcCount = dtcs.Count(d => d.Status.Contains("待处理"));
            }
            else
            {
                _logService.Info("未读取到DTC");
                TotalDtcCount = 0;
                ActiveDtcCount = 0;
                PendingDtcCount = 0;
            }
        }

        /// <summary>
        /// 连接状态变更处理
        /// </summary>
        private void OnConnectionStatusChanged(object sender, ConnectionStatusChangedEventArgs e)
        {
            _isConnected = e.IsConnected;
            
            if (e.IsConnected)
            {
                ConnectionStatus = "已连接";
                ConnectionStatusColor = Brushes.Green;
                SessionInfo = "连接已建立\n" + DateTime.Now;
            }
            else
            {
                ConnectionStatus = "未连接";
                ConnectionStatusColor = Brushes.Red;
                SessionInfo = "连接已断开\n" + DateTime.Now;
                if (!string.IsNullOrEmpty(e.ErrorMessage))
                {
                    SessionInfo += $"\n错误: {e.ErrorMessage}";
                }
            }
        }

        /// <summary>
        /// DTC更新处理
        /// </summary>
        private void OnDtcUpdated(object sender, DtcUpdatedEventArgs e)
        {
            UpdateDtcList(e.DtcList);
        }

        /// <summary>
        /// 更新命令可执行状态
        /// </summary>
        private void UpdateCommandCanExecute()
        {
            ConnectCommand.RaiseCanExecuteChanged();
            DisconnectCommand.RaiseCanExecuteChanged();
            SecurityAccessCommand.RaiseCanExecuteChanged();
            DefaultSessionCommand.RaiseCanExecuteChanged();
            ProgrammingSessionCommand.RaiseCanExecuteChanged();
            ExtendedDiagnosticSessionCommand.RaiseCanExecuteChanged();
            SafetySystemDiagnosticSessionCommand.RaiseCanExecuteChanged();
            ECUResetCommand.RaiseCanExecuteChanged();
            ReadEcuInfoCommand.RaiseCanExecuteChanged();
            ReadAllDTCsCommand.RaiseCanExecuteChanged();
            ReadTestFailedDTCsCommand.RaiseCanExecuteChanged();
            ReadPendingDTCsCommand.RaiseCanExecuteChanged();
            ClearAllDTCsCommand.RaiseCanExecuteChanged();
            ReadBatteryInfoCommand.RaiseCanExecuteChanged();
            StartBalancingCommand.RaiseCanExecuteChanged();
            StopBalancingCommand.RaiseCanExecuteChanged();
            ReadCellVoltagesCommand.RaiseCanExecuteChanged();
            ReadTempSensorsCommand.RaiseCanExecuteChanged();
            ReadFaultStatusCommand.RaiseCanExecuteChanged();
        }

        // 命令执行条件判断
        private bool CanConnect() 
        { 
            return IsTargetSelected && !_isConnected;
        }
        private bool CanDisconnect() 
        { 
            return _isConnected;
        }
        private bool CanExecuteDiagnosticCommand() 
        { 
            return _isConnected;
        }
    }

    /// <summary>
    /// 诊断目标类
    /// </summary>
    public class DiagnosticTarget
    {
        /// <summary>
        /// 目标名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 目标地址
        /// </summary>
        public ushort Address { get; set; }
        
        /// <summary>
        /// 重写ToString方法，用于显示在界面上
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
    }
}