using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using CANMonitor.Interfaces;

namespace CANMonitor.Services.LicenseServices
{
    /// <summary>
    /// 授权管理服务实现类
    /// 提供软件授权验证、到期检查、功能限制等服务
    /// </summary>
    public class LicenseService : ILicenseService
    {
        // 授权状态变更事件
        public event EventHandler<LicenseStatusChangedEventArgs> LicenseStatusChanged;

        // 当前授权状态
        private LicenseStatus _currentStatus;
        
        // 授权到期日期
        private DateTime? _expirationDate;
        
        // 授权类型
        private LicenseType _licenseType;
        
        // 已授权功能列表
        private HashSet<string> _authorizedFeatures;
        
        // 授权文件路径
        private readonly string _licenseFilePath;

        /// <summary>
        /// 获取当前授权状态
        /// </summary>
        public LicenseStatus CurrentStatus => _currentStatus;

        /// <summary>
        /// 获取授权到期日期
        /// </summary>
        public DateTime? ExpirationDate => _expirationDate;

        /// <summary>
        /// 获取授权类型
        /// </summary>
        public LicenseType LicenseType => _licenseType;
        
        /// <summary>
        /// 获取授权是否有效
        /// </summary>
        public bool IsValid => _currentStatus == LicenseStatus.Active || _currentStatus == LicenseStatus.Trial;

        /// <summary>
        /// 构造函数
        /// </summary>
        public LicenseService()
        {
            _currentStatus = LicenseStatus.Inactive;
            _expirationDate = null;
            _licenseType = LicenseType.Trial;
            _authorizedFeatures = new HashSet<string>();
            
            // 设置授权文件路径
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string appFolder = Path.Combine(appDataPath, "CANMonitor");
            
            if (!Directory.Exists(appFolder))
            {
                Directory.CreateDirectory(appFolder);
            }
            
            _licenseFilePath = Path.Combine(appFolder, "license.dat");
        }

        /// <summary>
        /// 加载并验证授权
        /// </summary>
        /// <returns>验证是否成功</returns>
        public bool ValidateLicense()
        {            
            try
            {
                // 检查授权文件是否存在
                if (File.Exists(_licenseFilePath))
                {
                    string licenseData = File.ReadAllText(_licenseFilePath);
                    
                    // 解密授权数据
                    string decryptedData = DecryptLicenseData(licenseData);
                    
                    // 解析授权数据
                    if (ParseLicenseData(decryptedData))
                    {
                        // 检查授权是否过期
                        if (_expirationDate.HasValue && DateTime.Now > _expirationDate.Value)
                        {
                            UpdateStatus(LicenseStatus.Expired, "授权已过期");
                            return false;
                        }
                        
                        UpdateStatus(LicenseStatus.Active, "授权验证成功");
                        return true;
                    }
                }
                
                // 没有有效的授权文件，设置为试用模式
                SetTrialMode();
                return true; // 试用模式视为有效
            }
            catch (Exception ex)
            {
                Console.WriteLine($"授权验证失败: {ex.Message}");
                UpdateStatus(LicenseStatus.Invalid, "授权文件格式错误");
                return false;
            }
        }

        /// <summary>
        /// 激活软件授权
        /// </summary>
        /// <param name="licenseKey">授权密钥</param>
        /// <returns>激活是否成功</returns>
        public bool ActivateLicense(string licenseKey)
        {
            try
            {                
                // 验证授权密钥格式
                if (string.IsNullOrEmpty(licenseKey) || licenseKey.Length != 25)
                {
                    UpdateStatus(_currentStatus, "无效的授权密钥格式");
                    return false;
                }

                // 模拟授权验证过程
                // 在实际应用中，这里应该连接授权服务器验证密钥
                LicenseType type;
                DateTime? expirationDate;
                HashSet<string> features;
                
                // 根据密钥前缀判断授权类型
                if (licenseKey.StartsWith("PRO"))
                {
                    type = LicenseType.Professional;
                    expirationDate = DateTime.Now.AddYears(1);
                    features = GetProfessionalFeatures();
                }
                else if (licenseKey.StartsWith("ENT"))
                {
                    type = LicenseType.Enterprise;
                    expirationDate = null; // 永久授权
                    features = GetEnterpriseFeatures();
                }
                else if (licenseKey.StartsWith("STD"))
                {
                    type = LicenseType.Standard;
                    expirationDate = DateTime.Now.AddMonths(6);
                    features = GetStandardFeatures();
                }
                else
                {
                    UpdateStatus(_currentStatus, "不支持的授权密钥类型");
                    return false;
                }

                // 生成授权数据
                string licenseData = GenerateLicenseData(type, expirationDate, features);
                
                // 加密并保存授权文件
                string encryptedData = EncryptLicenseData(licenseData);
                File.WriteAllText(_licenseFilePath, encryptedData);

                // 更新授权状态
                _licenseType = type;
                _expirationDate = expirationDate;
                _authorizedFeatures = features;
                
                UpdateStatus(LicenseStatus.Active, "软件激活成功");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"软件激活失败: {ex.Message}");
                UpdateStatus(_currentStatus, "激活过程发生错误");
                return false;
            }
        }

        /// <summary>
        /// 检查功能是否已授权
        /// </summary>
        /// <param name="featureCode">功能代码</param>
        /// <returns>功能是否已授权</returns>
        public bool IsFeatureAuthorized(string featureCode)
        {
            if (string.IsNullOrEmpty(featureCode))
                return false;

            // 试用版有功能限制
            if (_licenseType == LicenseType.Trial)
            {
                // 试用版仅开放基础功能
                return featureCode == "BASIC_MONITORING" || 
                       featureCode == "LIMITED_RECORDING" ||
                       featureCode == "VIEW_REPORTS";
            }

            return _authorizedFeatures.Contains(featureCode);
        }

        /// <summary>
        /// 获取剩余授权天数
        /// </summary>
        /// <returns>剩余天数，如果是永久授权返回-1</returns>
        public int GetRemainingDays()
        {            
            if (!_expirationDate.HasValue)
                return -1; // 永久授权

            TimeSpan remaining = _expirationDate.Value - DateTime.Now;
            
            if (remaining.TotalDays < 0)
                return 0; // 已过期

            return (int)Math.Ceiling(remaining.TotalDays);
        }

        /// <summary>
        /// 导出授权信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>导出是否成功</returns>
        public bool ExportLicenseInfo(string filePath)
        {            
            try
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("==== BMS监控软件授权信息 ====");
                sb.AppendLine($"授权状态: {_currentStatus}");
                sb.AppendLine($"授权类型: {_licenseType}");
                
                if (_expirationDate.HasValue)
                {
                    sb.AppendLine($"到期日期: {_expirationDate.Value.ToString("yyyy-MM-dd")}");
                    sb.AppendLine($"剩余天数: {GetRemainingDays()}");
                }
                else
                {
                    sb.AppendLine("到期日期: 永久");
                }
                
                sb.AppendLine();
                sb.AppendLine("已授权功能:");
                foreach (string feature in _authorizedFeatures)
                {
                    sb.AppendLine($"- {GetFeatureDescription(feature)}");
                }
                
                File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"导出授权信息失败: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 刷新授权状态
        /// </summary>
        public void RefreshLicenseStatus()
        {            
            LicenseStatus oldStatus = _currentStatus;
            
            // 重新验证授权
            ValidateLicense();
            
            // 检查是否需要通知状态变更
            if (_currentStatus != oldStatus)
            {
                OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                {
                    NewStatus = _currentStatus,
                    OldStatus = oldStatus,
                    Reason = "授权状态刷新"
                });
            }
        }

        /// <summary>
        /// 设置试用模式
        /// </summary>
        private void SetTrialMode()
        {            
            _licenseType = LicenseType.Trial;
            _expirationDate = DateTime.Now.AddDays(30);
            _authorizedFeatures.Clear();
            
            // 试用版仅包含基础功能
            _authorizedFeatures.Add("BASIC_MONITORING");
            _authorizedFeatures.Add("LIMITED_RECORDING");
            _authorizedFeatures.Add("VIEW_REPORTS");
            
            UpdateStatus(LicenseStatus.Trial, "进入试用模式");
        }

        /// <summary>
        /// 更新授权状态
        /// </summary>
        private void UpdateStatus(LicenseStatus newStatus, string reason)
        {            
            LicenseStatus oldStatus = _currentStatus;
            _currentStatus = newStatus;
            
            // 触发状态变更事件
            if (oldStatus != newStatus)
            {
                OnLicenseStatusChanged(new LicenseStatusChangedEventArgs
                {
                    NewStatus = newStatus,
                    OldStatus = oldStatus,
                    Reason = reason
                });
            }
        }

        /// <summary>
        /// 触发授权状态变更事件
        /// </summary>
        private void OnLicenseStatusChanged(LicenseStatusChangedEventArgs e)
        {            
            LicenseStatusChanged?.Invoke(this, e);
        }

        /// <summary>
        /// 生成授权数据
        /// </summary>
        private string GenerateLicenseData(LicenseType type, DateTime? expiration, HashSet<string> features)
        {            
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Type={type}");
            sb.AppendLine($"Expiration={expiration?.ToString("yyyy-MM-dd") ?? "Permanent"}");
            sb.AppendLine($"Generated={DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}");
            sb.AppendLine("Features=" + string.Join(",", features));
            
            return sb.ToString();
        }

        /// <summary>
        /// 解析授权数据
        /// </summary>
        private bool ParseLicenseData(string data)
        {            
            try
            {
                string[] lines = data.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach (string line in lines)
                {
                    int equalsIndex = line.IndexOf('=');
                    if (equalsIndex > 0)
                    {
                        string key = line.Substring(0, equalsIndex);
                        string value = line.Substring(equalsIndex + 1);
                        
                        switch (key)
                        {
                            case "Type":
                                _licenseType = (LicenseType)Enum.Parse(typeof(LicenseType), value);
                                break;
                                
                            case "Expiration":
                                if (value != "Permanent")
                                {
                                    _expirationDate = DateTime.Parse(value);
                                }
                                else
                                {
                                    _expirationDate = null;
                                }
                                break;
                                
                            case "Features":
                                _authorizedFeatures = new HashSet<string>(value.Split(','));
                                break;
                        }
                    }
                }
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 加密授权数据
        /// </summary>
        private string EncryptLicenseData(string data)
        {            
            // 在实际应用中，这里应该使用更安全的加密方法
            // 这里使用简单的Base64编码作为示例
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 解密授权数据
        /// </summary>
        private string DecryptLicenseData(string encryptedData)
        {            
            // 在实际应用中，这里应该使用更安全的解密方法
            // 这里使用简单的Base64解码作为示例
            byte[] bytes = Convert.FromBase64String(encryptedData);
            return Encoding.UTF8.GetString(bytes);
        }

        /// <summary>
        /// 获取标准版功能列表
        /// </summary>
        private HashSet<string> GetStandardFeatures()
        {            
            return new HashSet<string>
            {
                "BASIC_MONITORING",
                "FULL_RECORDING",
                "VIEW_REPORTS",
                "DBC_SUPPORT",
                "CAN_ANALYSIS"
            };
        }

        /// <summary>
        /// 获取专业版功能列表
        /// </summary>
        private HashSet<string> GetProfessionalFeatures()
        {            
            HashSet<string> features = GetStandardFeatures();
            features.UnionWith(new[]
            {
                "ADVANCED_ANALYTICS",
                "REALTIME_PLOTTING",
                "CUSTOM_DASHBOARDS",
                "EXPERT_DIAGNOSTICS",
                "DATA_EXPORT"
            });
            return features;
        }

        /// <summary>
        /// 获取企业版功能列表
        /// </summary>
        private HashSet<string> GetEnterpriseFeatures()
        {            
            HashSet<string> features = GetProfessionalFeatures();
            features.UnionWith(new[]
            {
                "MULTI_CHANNEL_SUPPORT",
                "REMOTE_MONITORING",
                "API_ACCESS",
                "CUSTOM_PROTOCOLS",
                "PRIORITY_SUPPORT"
            });
            return features;
        }

        /// <summary>
        /// 获取功能描述
        /// </summary>
        private string GetFeatureDescription(string featureCode)
        {            
            switch (featureCode)
            {
                case "BASIC_MONITORING": return "基础监控功能";
                case "FULL_RECORDING": return "完整数据记录";
                case "VIEW_REPORTS": return "报表查看";
                case "DBC_SUPPORT": return "DBC文件支持";
                case "CAN_ANALYSIS": return "CAN分析工具";
                case "ADVANCED_ANALYTICS": return "高级数据分析";
                case "REALTIME_PLOTTING": return "实时数据绘图";
                case "CUSTOM_DASHBOARDS": return "自定义仪表盘";
                case "EXPERT_DIAGNOSTICS": return "专家诊断功能";
                case "DATA_EXPORT": return "数据导出";
                case "MULTI_CHANNEL_SUPPORT": return "多通道支持";
                case "REMOTE_MONITORING": return "远程监控";
                case "API_ACCESS": return "API访问";
                case "CUSTOM_PROTOCOLS": return "自定义协议";
                case "PRIORITY_SUPPORT": return "优先技术支持";
                case "LIMITED_RECORDING": return "有限的数据记录";
                default: return featureCode;
            }
        }
    }
}