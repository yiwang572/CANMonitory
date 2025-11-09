using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CANMonitor.Interfaces;
using System.Windows;
using Microsoft.Practices.Unity;
using System.IO;

namespace CANMonitor.ViewModels
{
    /// <summary>
    /// 首页视图模型
    /// 提供系统概览、快速操作和状态监控
    /// </summary>
    public class HomeViewModel : BindableBase, INotifyPropertyChanged
    {
        /// <summary>
        /// 区域管理器
        /// </summary>
        private readonly IRegionManager _regionManager;
        
        /// <summary>
        /// CAN服务
        /// </summary>
        private readonly ICanService _canService;
        
        /// <summary>
        /// DBC服务
        /// </summary>
        private readonly IDbcService _dbcService;
        
        /// <summary>
        /// 授权服务
        /// </summary>
        private readonly ILicenseService _licenseService;
        
        /// <summary>
        /// 连接状态
        /// </summary>
        private string _connectionStatus;
        public string ConnectionStatus
        {
            get { return _connectionStatus; }
            set { SetProperty(ref _connectionStatus, value); }
        }
        
        /// <summary>
        /// DBC文件名
        /// </summary>
        private string _dbcFileName;
        public string DbcFileName
        {
            get { return _dbcFileName; }
            set { SetProperty(ref _dbcFileName, value); }
        }
        
        /// <summary>
        /// 授权状态
        /// </summary>
        private string _licenseStatus;
        public string LicenseStatus
        {
            get { return _licenseStatus; }
            set { SetProperty(ref _licenseStatus, value); }
        }
        
        /// <summary>
        /// 最近活动列表
        /// </summary>
        private ObservableCollection<ActivityLog> _recentActivities;
        public ObservableCollection<ActivityLog> RecentActivities
        {
            get { return _recentActivities; }
            set { SetProperty(ref _recentActivities, value); }
        }
        
        /// <summary>
        /// 连接设备命令
        /// </summary>
        public DelegateCommand ConnectDeviceCommand { get; private set; }
        
        /// <summary>
        /// 加载DBC文件命令
        /// </summary>
        public DelegateCommand LoadDbcCommand { get; private set; }
        
        /// <summary>
        /// 开始监控命令
        /// </summary>
        public DelegateCommand StartMonitoringCommand { get; private set; }
        
        /// <summary>
        /// 打开系统设置命令
        /// </summary>
        public DelegateCommand OpenSettingsCommand { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="regionManager">区域管理器</param>
        /// <param name="canService">CAN服务</param>
        /// <param name="dbcService">DBC服务</param>
        /// <param name="licenseService">授权服务</param>
        public HomeViewModel(IRegionManager regionManager, ICanService canService, IDbcService dbcService, ILicenseService licenseService)
        {
            _regionManager = regionManager;
            _canService = canService;
            _dbcService = dbcService;
            _licenseService = licenseService;
            
            // 初始化命令
            ConnectDeviceCommand = new DelegateCommand(ConnectDevice);
            LoadDbcCommand = new DelegateCommand(LoadDbc);
            StartMonitoringCommand = new DelegateCommand(StartMonitoring);
            OpenSettingsCommand = new DelegateCommand(OpenSettings);
            
            // 初始化数据
            InitializeData();
            
            // 订阅事件
            _canService.ConnectionStateChanged += CanService_ConnectionStateChanged;
            _dbcService.DbcLoaded += DbcService_DbcLoaded;
        }
        
        /// <summary>
        /// 初始化数据
        /// </summary>
        private void InitializeData()
        {
            ConnectionStatus = "未连接";
            DbcFileName = "未加载";
            LicenseStatus = _licenseService.CurrentStatus.ToString();
            
            // 初始化最近活动
            RecentActivities = new ObservableCollection<ActivityLog>
            {
                new ActivityLog { Time = System.DateTime.Now.ToString("HH:mm:ss"), Description = "系统启动" },
                new ActivityLog { Time = System.DateTime.Now.AddMinutes(-1).ToString("HH:mm:ss"), Description = "加载配置完成" }
            };
        }
        
        /// <summary>
        /// 连接设备
        /// </summary>
        private void ConnectDevice()
        {
            // 导航到设备连接页面
            _regionManager.RequestNavigate("MainContentRegion", "DeviceConnectionView");
            AddActivity("打开设备连接页面");
        }
        
        /// <summary>
        /// 加载DBC文件
        /// </summary>
        private void LoadDbc()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "DBC文件 (*.dbc)|*.dbc|所有文件 (*.*)|*.*",
                Title = "选择DBC文件"
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    _dbcService.LoadDbcFile(openFileDialog.FileName);
                    AddActivity($"加载DBC文件: {Path.GetFileName(openFileDialog.FileName)}");
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"加载DBC文件失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
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
            
            // 导航到CAN监控页面
            _regionManager.RequestNavigate("MainContentRegion", "CanMonitorView");
            AddActivity("开始CAN监控");
        }
        
        /// <summary>
        /// 打开系统设置
        /// </summary>
        private void OpenSettings()
        {
            _regionManager.RequestNavigate("MainContentRegion", "SettingsView");
            AddActivity("打开系统设置");
        }
        
        /// <summary>
        /// CAN连接状态变更事件处理
        /// </summary>
        private void CanService_ConnectionStateChanged(object sender, bool isConnected)
        {
            ConnectionStatus = isConnected ? "已连接" : "未连接";
        }
        
        /// <summary>
        /// DBC文件加载完成事件处理
        /// </summary>
        private void DbcService_DbcLoaded(object sender, string fileName)
        {
            DbcFileName = Path.GetFileName(fileName);
        }
        
        /// <summary>
        /// 添加活动日志
        /// </summary>
        private void AddActivity(string description)
        {
            RecentActivities.Insert(0, new ActivityLog { Time = System.DateTime.Now.ToString("HH:mm:ss"), Description = description });
            // 保持日志数量在10条以内
            if (RecentActivities.Count > 10)
            {
                RecentActivities.RemoveAt(RecentActivities.Count - 1);
            }
        }
    }
    
    /// <summary>
    /// 活动日志类
    /// </summary>
    public class ActivityLog
    {
        /// <summary>
        /// 时间
        /// </summary>
        public string Time { get; set; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
    }
}