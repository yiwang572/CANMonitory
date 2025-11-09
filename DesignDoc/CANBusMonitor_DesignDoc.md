# BMS总线上位机软件设计文档

## 1. 项目概述

### 1.1 项目名称
CANMonitor - 电池管理系统CAN总线监控与测试工具

### 1.2 用户群体
- 电动汽车BMS工程师
- 电池系统测试人员
- 嵌入式系统开发者
- CAN总线应用工程师

### 1.3 应用目标
提供一个专注于电池管理系统(BMS)的CAN总线监控、分析、诊断和测试工具，支持致远USBCANFD系列硬件设备（USBCANFD-MINI、USBCANFD-100U、USBCANFD-200U等），简化BMS开发和测试流程。

### 1.4 开发环境
- Visual Studio 2015
- .NET Framework 4.5
- Prism框架
- CAN(FD)接口卡二次开发接口函数库 SDK

## 2. 系统架构

### 2.1 整体架构
采用MVVM(Model-View-ViewModel)架构模式，基于Prism框架实现模块化、可扩展的应用程序结构。

```
┌─────────────────────────────────────────────────────────────────┐
│                      表现层 (Views)                              │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────────────────┐ │
│  │ 导航视图     │   │ 工作区视图    │   │ 属性面板视图              │ │
│  └─────────────┘   └─────────────┘   └─────────────────────────┘ │
└───────────────────────────┬─────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────┐
│                     业务逻辑层 (ViewModels)                       │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────────────────┐ │
│  │ 监控模块VM   │   │ 发送模块VM    │   │ 诊断模块VM               │ │
│  ├─────────────┤   ├─────────────┤   ├─────────────────────────┤ │
│  │ DBC解析VM    │   │ 回放模块VM   │   │ 自动化测试VM              │ │
│  └─────────────┘   └─────────────┘   └─────────────────────────┘ │
└───────────────────────────┬─────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────┐
│                      服务层 (Services)                           │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────────────────┐ │
│  │ CAN服务      │   │ DBC服务     │   │ OBD服务                  │ │
│  ├─────────────┤   ├─────────────┤   ├─────────────────────────┤ │
│  │ 日志服务     │   │ 许可证服务    │   │ 自动化服务                │ │
│  └─────────────┘   └─────────────┘   └─────────────────────────┘ │
└───────────────────────────┬─────────────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────────────┐
│                      数据层 (Models)                             │
│  ┌─────────────┐   ┌─────────────┐   ┌─────────────────────────┐ │
│  │ CAN报文模型  │   │ DBC数据模型   │   │ 配置数据模型              │ │
│  └─────────────┘   └─────────────┘   └─────────────────────────┘ │
└─────────────────────────────────────────────────────────────────┘
```

### 2.2 技术栈选择
- **开发语言**: C#
- **UI框架**: WPF
- **MVVM框架**: Prism 6.3.0
- **依赖注入**: Unity .net4.5
- **CAN通信**: 致远USBCANFD SDK（zlgcan.dll），支持CAN和CAN FD协议
- **DBC解析**: 自定义解析器，优化BMS信号解析
- **数据存储**: JSON、XML
- **加密算法**: AES128

## 3. 功能模块设计

### 3.1 核心功能模块

#### 3.1.1 DBC文件加载与解析模块
- 支持标准DBC文件格式解析
- 自动识别BMS相关报文、信号、节点等信息
- 提供BMS信号解码和编码功能（电池电压、电流、温度等）
- 支持多个DBC文件同时加载和切换
- 提供BMS信号预定义模板

#### 3.1.2 CAN报文监控模块
- 实时显示CAN/CAN FD总线报文
- 支持原始数据和DBC解码数据同时显示
- 提供BMS报文过滤、搜索、统计功能
- 支持自定义BMS信号显示界面，可拖拽配置
- 支持关键BMS参数趋势图表显示
- 支持异常报警和阈值监控

#### 3.1.3 报文记录与回放模块
- 支持BLF、ASC、CSV格式报文记录，优化BMS数据存储
- 提供定时记录、手动记录、触发记录功能
- 支持按时间比例回放、单步回放
- 回放过程中支持暂停、继续、停止操作
- 支持BMS关键事件标记和跳转
- 支持数据导出为Excel格式进行离线分析

#### 3.1.4 报文发送模块
- 支持手动输入原始报文发送
- 支持基于DBC信号编辑和发送BMS控制命令
- 提供定时发送、循环发送功能
- 支持发送脚本编辑和执行
- 提供BMS常用测试命令模板库
- 支持致远USBCAN（FD）硬件特定发送模式（单次、周期、事件触发）

#### 3.1.5 OBD诊断功能模块
- 支持OBD-II标准诊断协议
- 支持BMS特定诊断协议（符合ISO 15765-4）
- 提供BMS安全访问功能，支持新旧标准
- 实现AES128算法加密解密用于BMS高级安全访问
- 支持常用诊断服务(读取故障码、清除故障码、读取数据流等)
- 支持BMS特定诊断功能（电池信息读取、均衡控制、校准等）

#### 3.1.6 ZCK文件烧写功能模块
- 支持ZCK格式固件文件解析，专为BMS固件设计
- 提供固件烧写进度显示和状态反馈
- 支持烧写前检查和校验
- 提供烧写日志记录
- 支持BMS固件备份和恢复功能
- 支持通过致远USBCAN（FD）实现高速固件下载
- 提供固件版本管理和回滚机制

#### 3.1.7 主题设置模块
- 支持多种预设主题切换
- 提供自定义主题功能
- 支持主题导出和导入
- 主题包括颜色、字体、控件样式等
- 为BMS关键参数提供特殊颜色编码（如超温警告红色显示）

#### 3.1.8 用户权限管理模块
- 支持多用户角色(管理员、普通用户、访客)
- 提供权限设置界面
- 支持用户登录和认证
- 记录用户操作日志

#### 3.1.9 许可证管理模块
- 默认设置三个月试用期
- 支持许可证激活和验证
- 提供许可证信息查看
- 实现许可证过期提醒

#### 3.1.10 自动化测试模块
- 支持脚本语言编写BMS测试用例
- 提供BMS测试步骤编辑和执行
- 支持测试结果记录和报告生成
- 提供BMS标准测试模板库（如SOC校准、均衡测试等）
- 支持与致远USBCAN（FD）硬件协同的自动化测试流程

### 3.2 辅助功能模块

#### 3.2.1 日志记录模块
- 记录系统运行日志
- 记录用户操作日志
- 提供日志查看和导出功能

#### 3.2.2 配置管理模块
- 保存和加载用户配置
- 支持配置导入和导出
- 提供默认配置恢复

#### 3.2.3 多语言支持模块
- 默认中文界面
- 支持英文等多语言切换
- 提供语言包管理

## 4. 类设计

### 4.1 核心服务接口

```csharp
// CAN通信服务接口
public interface ICanService
{
    bool OpenDevice(uint deviceType, uint deviceIndex);
    bool CloseDevice();
    bool InitCAN(uint channelIndex, CanInitConfig config);
    bool StartCAN(uint channelIndex);
    bool StopCAN(uint channelIndex);
    int SendMessage(CanMessage message);
    int SendMessageFD(CanFdMessage message);
    List<CanMessage> ReceiveMessage(uint maxCount = 100);
    List<CanFdMessage> ReceiveMessageFD(uint maxCount = 100);
    bool StartAutoReceive(uint receiveIntervalMs = 10);
    void StopAutoReceive();
    CanErrorInfo ReadChannelErrInfo();
    event EventHandler<List<CanMessage>> MessagesReceived;
    event EventHandler<List<CanFdMessage>> MessagesReceivedFD;
    List<CanDeviceInfo> GetAvailableDevices();
}

// DBC解析服务接口
public interface IDbcService
{
    bool LoadDbcFile(string filePath);
    void UnloadDbcFile();
    List<DbcMessage> GetMessages();
    DbcSignal GetSignalById(string signalId);
    double DecodeSignal(byte[] data, DbcSignal signal);
    byte[] EncodeSignal(double value, DbcSignal signal);
}

// OBD诊断服务接口
public interface IObdService
{
    bool Connect(string interfaceName);
    void Disconnect();
    ObdResponse SendCommand(ObdCommand command);
    bool SecurityAccess(string level, string key, bool useAes = false);
    List<DiagnosticTroubleCode> ReadDTCs();
    bool ClearDTCs();
}

// 许可证服务接口
public interface ILicenseService
{
    bool IsValid { get; }
    DateTime ExpireDate { get; }
    LicenseStatus Status { get; }
    bool ActivateLicense(string licenseKey);
    int GetRemainingDays();
}

// 自动化测试服务接口
public interface IAutomationService
{
    void ExecuteScript(string scriptPath);
    void StopScript();
    TestReport GenerateReport();
    event EventHandler<TestStepResult> StepCompleted;
}

// 日志服务接口
public interface ILoggingService
{
    void Log(LogLevel level, string message);
    void LogError(string message, Exception ex = null);
    void LogInfo(string message);
    string GetLogContent();
}
```

### 4.2 数据模型类

```csharp
// CAN报文模型
public class CanMessage
{
    public uint Id { get; set; }
    public byte[] Data { get; set; }
    public byte DataLength { get; set; }
    public ulong Timestamp { get; set; }
    public bool IsExtendedFrame { get; set; }
    public bool IsRemoteFrame { get; set; }
    public bool IsErrorFrame { get; set; }
    public uint ChannelIndex { get; set; }
    public string Name { get; set; }
    public List<SignalValue> SignalValues { get; set; }
    
    // 与ZLGCAN接口转换方法
    public can_frame ToCanFrame();
    public static CanMessage FromCanFrame(can_frame frame, ulong timestamp = 0);
}

// DBC信号模型
public class DbcSignal
{
    public string Name { get; set; }
    public string MessageName { get; set; }
    public int StartBit { get; set; }
    public int Length { get; set; }
    public bool IsSigned { get; set; }
    public double Factor { get; set; }
    public double Offset { get; set; }
    public double Min { get; set; }
    public double Max { get; set; }
    public string Unit { get; set; }
    public string Receiver { get; set; }
}

// 信号值模型
public class SignalValue
{
    public string Name { get; set; }
    public double Value { get; set; }
    public string Unit { get; set; }
    public string FormattedValue { get; set; }
}

// 用户权限模型
public class UserPermission
{
    public string UserName { get; set; }
    public UserRole Role { get; set; }
    public bool CanAccessDiagnostics { get; set; }
    public bool CanSendMessages { get; set; }
    public bool CanManageUsers { get; set; }
}

// 主题模型
public class Theme
{
    public string Name { get; set; }
    public Color PrimaryColor { get; set; }
    public Color SecondaryColor { get; set; }
    public Color BackgroundColor { get; set; }
    public Color TextColor { get; set; }
    public string FontFamily { get; set; }
    public double FontSize { get; set; }
}
```

## 5. 界面设计

### 5.1 整体布局
采用左侧导航栏+中央工作区+右侧属性面板的三栏式布局。

```
┌─────────────────────────────────────────────────────────────────────────────────┐
│                                菜单栏                                           │
├─────────────┬───────────────────────────────────────┬─────────────────────────┤
│             │                                       │                         │
│             │                                       │                         │
│  导航视图    │             工作区视图                  │     属性面板视图          │
│             │                                       │                         │
│             │                                       │                         │
└─────────────┴───────────────────────────────────────┴─────────────────────────┘
│                                状态栏                                           │
└─────────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 导航视图
包含以下功能模块入口：
- 设备连接
- 报文监控
- 报文发送
- DBC管理
- OBD诊断
- 自动化测试
- 系统设置

### 5.3 工作区视图
根据选择的功能模块显示对应的工作界面，支持多标签页管理。

### 5.4 属性面板视图
显示当前选中对象的属性，支持实时编辑和更新。

### 5.5 设计风格
- **配色方案**: 深蓝主色调(#0E4C92)搭配灰白中性色(#F8F9FA, #343A40)
- **关键数据**: 使用高对比度色彩突出(#FF6B6B, #4ECDC4, #45B7D1)
- **字体**: 微软雅黑，大小适中，层次分明
- **交互**: 拖拽式控件配置，右键菜单快速操作，多标签页管理
- **语言**: 全中文界面，支持英文等多语言切换

## 6. 文件结构

```
CANMonitor/              # 主项目根目录
├── CANMonitor.sln       # 解决方案文件
├── CANMonitor.csproj    # 主项目文件
├── packages.config      # NuGet包配置
├── App.xaml             # 应用程序入口
├── App.xaml.cs          # 应用程序逻辑
├── MainWindow.xaml      # 主窗口界面
├── MainWindow.xaml.cs   # 主窗口逻辑
├── AssemblyInfo.cs      # 程序集信息
├── Views/               # 视图层
│   ├── MainWindow.xaml          # 主窗口
│   ├── CANMonitoringView.xaml   # BMS监控视图
│   ├── MessageSendView.xaml     # 报文发送视图
│   ├── DiagnoseView.xaml        # 诊断视图
│   ├── FirmwareUpdateView.xaml  # 固件更新视图
│   ├── AutomationView.xaml      # 自动化测试视图
│   ├── ThemeSettingsView.xaml   # 主题设置视图
│   └── LicenseView.xaml         # 许可证视图
├── ViewModels/          # 视图模型层
│   ├── MainWindowViewModel.cs
│   ├── CANMonitoringViewModel.cs
│   ├── MessageSendViewModel.cs
│   ├── DiagnoseViewModel.cs
│   ├── FirmwareUpdateViewModel.cs
│   ├── AutomationViewModel.cs
│   └── ThemeSettingsViewModel.cs
├── Models/              # 数据模型层
│   ├── BmsModels/        # BMS相关数据模型
│   │   ├── BatteryData.cs
│   │   ├── CellVoltageData.cs
│   │   ├── TemperatureData.cs
│   │   └── BmsStatusData.cs
│   ├── CanModels/        # CAN相关数据模型
│   │   ├── CanMessage.cs
│   │   ├── CanSignal.cs
│   │   └── CanChannel.cs
│   └── CommonModels/     # 通用数据模型
│       ├── TestStep.cs
│       ├── TestReport.cs
│       ├── User.cs
│       ├── Theme.cs
│       └── LogEntry.cs
├── Services/            # 服务实现层
│   ├── CanServices/      # CAN服务
│   │   ├── CanService.cs
│   │   └── ZLGUsbCanService.cs    # 致远硬件服务实现，支持多通道分别设置开启
│   ├── BmsServices/      # BMS专用服务
│   │   ├── BmsDataProcessor.cs
│   │   └── BmsSignalDecoder.cs
│   ├── DbcServices/      # DBC服务
│   │   └── DbcService.cs
│   ├── DiagnoseServices/ # 诊断服务
│   │   └── ObdService.cs
│   ├── FirmwareServices/ # 固件服务
│   │   └── ZckFirmwareService.cs
│   ├── AutomationServices/ # 自动化服务
│   │   └── AutomationService.cs
│   ├── LoggingServices/  # 日志服务
│   │   └── LoggingService.cs
│   └── LicenseServices/  # 许可证服务
│       └── LicenseService.cs
├── Utils/               # 工具类
│   ├── ZLGSdkWrapper.cs      # 致远SDK封装
│   ├── DbcParser.cs         # DBC解析器
│   ├── BmsSignalEncoder.cs  # BMS信号编码器
│   ├── AesEncryption.cs     # AES加密工具
│   ├── FileHandler.cs       # 文件操作助手
│   └── LogHelper.cs         # 日志助手
├── Resources/           # 资源文件
│   ├── Images/           # 图片资源
│   ├── DbcTemplates/     # DBC模板文件
│   ├── TestTemplates/    # 测试模板
│   ├── Styles/
│   └── Languages/
├── Themes/                  # 主题文件
│   ├── DarkBlueTheme.xaml
│   └── LightTheme.xaml
└── References/          # 第三方引用
    └── zlgcan.dll     # 致远USBCAN（FD）SDK核心库
```

## 7. 开发计划

### 7.1 第一阶段：CANMonitor基础框架搭建（2周）
- 搭建基于Prism 6.3.0的WPF项目
- 实现MVVM架构和依赖注入
- 配置致远USBCAN（FD）SDK集成环境
- 实现主题切换功能和BMS专用颜色编码
- 完成三栏式BMS监控界面布局

### 7.2 第二阶段：BMS CAN通信功能开发（3周）
- 实现致远USBCAN（FD）硬件驱动封装
- 实现CAN/CAN FD通道配置与管理
- 实现BMS报文实时接收和显示
- 实现BMS控制报文发送功能
- 实现BMS关键参数过滤和监控

### 7.3 第三阶段：BMS DBC解析与数据可视化（2周）
- 实现DBC文件加载与解析，优化BMS信号解析
- 实现电池电压、电流、温度等BMS信号解码
- 实现BMS参数趋势图表显示
- 实现BMS信号拖拽配置和自定义监控界面
- 实现异常报警和阈值监控机制

### 7.4 第四阶段：BMS诊断与高级功能开发（3周）
- 实现OBD-II标准诊断协议和BMS特定诊断协议（ISO 15765-4）
- 实现BMS安全访问功能，集成AES128加密
- 实现BMS故障码读取、清除和数据分析
- 实现ZCK文件解析和固件烧写功能
- 实现BMS固件备份和恢复功能
- 实现用户权限管理和许可证系统

### 7.5 第五阶段：BMS自动化测试功能（2周）
- 实现BMS测试脚本编辑和执行引擎
- 实现BMS标准测试模板库（SOC校准、均衡测试等）
- 实现测试结果统计和报告生成
- 实现与致远USBCAN（FD）硬件协同的自动化测试流程

### 7.6 第六阶段：系统集成与测试（1周）
- 系统功能集成和全面测试
- 与实际BMS设备联调测试
- 性能优化和Bug修复
- 用户文档编写和使用指南

## 8. 安全性考虑

### 8.1 数据安全
- 敏感数据加密存储
- 通信数据加密传输
- 定期备份配置数据

### 8.2 访问控制
- 严格的用户权限管理
- 关键操作需要身份验证
- 操作日志记录与审计

### 8.3 软件保护
- 许可证验证机制
- 反调试和反破解措施
- 试用期限制

## 9. 扩展性考虑

### 9.1 模块化设计
- 采用插件架构，支持功能扩展
- 接口标准化，便于替换实现
- 配置驱动，减少硬编码

### 9.2 硬件兼容性
- 支持多种CAN硬件接口
- 提供硬件抽象层
- 可配置的驱动加载机制

### 9.3 未来扩展方向
- 支持更多总线类型（LIN, FlexRay等）
- 云端数据同步与分析
- 远程诊断功能
- 更多自动化测试模板

---

**文档版本**: 1.0
**创建日期**: 2024-01-01
**最后更新**: 2024-01-01
**文档作者**: 系统架构师