using System;

namespace CANMonitor.Interfaces
{
    /// <summary>
    /// 授权管理服务接口
    /// 提供软件授权验证、到期检查、功能限制等服务
    /// </summary>
    public interface ILicenseService
    {
        /// <summary>
        /// 授权状态变更事件
        /// </summary>
        event EventHandler<LicenseStatusChangedEventArgs> LicenseStatusChanged;

        /// <summary>
        /// 获取当前授权状态
        /// </summary>
        LicenseStatus CurrentStatus { get; }

        /// <summary>
        /// 获取授权到期日期
        /// </summary>
        DateTime? ExpirationDate { get; }

        /// <summary>
        /// 获取授权类型
        /// </summary>
        LicenseType LicenseType { get; }
        
        /// <summary>
        /// 获取授权是否有效
        /// </summary>
        bool IsValid { get; }

        /// <summary>
        /// 加载并验证授权
        /// </summary>
        /// <returns>验证是否成功</returns>
        bool ValidateLicense();

        /// <summary>
        /// 激活软件授权
        /// </summary>
        /// <param name="licenseKey">授权密钥</param>
        /// <returns>激活是否成功</returns>
        bool ActivateLicense(string licenseKey);

        /// <summary>
        /// 检查功能是否已授权
        /// </summary>
        /// <param name="featureCode">功能代码</param>
        /// <returns>功能是否已授权</returns>
        bool IsFeatureAuthorized(string featureCode);

        /// <summary>
        /// 获取剩余授权天数
        /// </summary>
        /// <returns>剩余天数，如果是永久授权返回-1</returns>
        int GetRemainingDays();

        /// <summary>
        /// 导出授权信息
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>导出是否成功</returns>
        bool ExportLicenseInfo(string filePath);

        /// <summary>
        /// 刷新授权状态
        /// </summary>
        void RefreshLicenseStatus();
    }

    /// <summary>
    /// 授权状态枚举
    /// </summary>
    public enum LicenseStatus
    {
        /// <summary>
        /// 未激活
        /// </summary>
        Inactive,
        
        /// <summary>
        /// 已激活
        /// </summary>
        Active,
        
        /// <summary>
        /// 已过期
        /// </summary>
        Expired,
        
        /// <summary>
        /// 无效
        /// </summary>
        Invalid,
        
        /// <summary>
        /// 试用版
        /// </summary>
        Trial
    }

    /// <summary>
    /// 授权类型枚举
    /// </summary>
    public enum LicenseType
    {
        /// <summary>
        /// 试用版
        /// </summary>
        Trial,
        
        /// <summary>
        /// 标准版
        /// </summary>
        Standard,
        
        /// <summary>
        /// 专业版
        /// </summary>
        Professional,
        
        /// <summary>
        /// 企业版
        /// </summary>
        Enterprise
    }

    /// <summary>
    /// 授权状态变更事件参数
    /// </summary>
    public class LicenseStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 新的授权状态
        /// </summary>
        public LicenseStatus NewStatus { get; set; }
        
        /// <summary>
        /// 旧的授权状态
        /// </summary>
        public LicenseStatus OldStatus { get; set; }
        
        /// <summary>
        /// 变更原因
        /// </summary>
        public string Reason { get; set; }
    }
}