# 许可证服务设计文档

## 1. 服务概述

许可证服务负责软件许可证的管理、验证和激活，提供试用期控制、许可证状态监控和授权管理功能，为软件提供合法使用保障。

## 2. 服务接口

```csharp
public interface ILicenseService
{
    /// <summary>
    /// 许可证是否有效
    /// </summary>
    bool IsValid { get; }
    
    /// <summary>
    /// 许可证过期日期
    /// </summary>
    DateTime ExpireDate { get; }
    
    /// <summary>
    /// 许可证状态
    /// </summary>
    LicenseStatus Status { get; }
    
    /// <summary>
    /// 激活许可证
    /// </summary>
    /// <param name="licenseKey">许可证密钥</param>
    /// <returns>激活是否成功</returns>
    bool ActivateLicense(string licenseKey);
    
    /// <summary>
    /// 获取剩余使用天数
    /// </summary>
    /// <returns>剩余天数</returns>
    int GetRemainingDays();
    
    /// <summary>
    /// 验证许可证
    /// </summary>
    /// <returns>验证是否成功</returns>
    bool ValidateLicense();
    
    /// <summary>
    /// 获取许可证信息
    /// </summary>
    /// <returns>许可证信息</returns>
    LicenseInfo GetLicenseInfo();
    
    /// <summary>
    /// 导出许可证信息
    /// </summary>
    /// <param name="filePath">导出文件路径</param>
    /// <returns>导出是否成功</returns>
    bool ExportLicenseInfo(string filePath);
    
    /// <summary>
    /// 导入许可证文件
    /// </summary>
    /// <param name="filePath">许可证文件路径</param>
    /// <returns>导入是否成功</returns>
    bool ImportLicenseFile(string filePath);
    
    /// <summary>
    /// 获取试用期状态
    /// </summary>
    TrialStatus GetTrialStatus();
    
    /// <summary>
    /// 重置试用期（仅用于测试）
    /// </summary>
    void ResetTrialPeriod();
    
    /// <summary>
    /// 检查许可证更新
    /// </summary>
    /// <returns>是否有更新</returns>
    bool CheckForUpdates();
    
    /// <summary>
    /// 许可证状态变化事件
    /// </summary>
    event EventHandler<LicenseStatusChangedEventArgs> StatusChanged;
}
```

## 3. 实现类设计

### 3.1 LicenseService

```csharp
public class LicenseService : ILicenseService
{
    // 实现ILicenseService接口的所有方法
    // 提供许可证管理和验证功能
}
```

### 3.2 LicenseValidator

```csharp
public class LicenseValidator
{
    public bool Validate(string licenseKey);
    public bool ValidateSignature(byte[] licenseData, byte[] signature);
    
    // 内部验证方法
    private bool CheckExpiration(DateTime expireDate);
    private bool CheckHardwareId(string hardwareId);
    private bool VerifySignature(byte[] data, byte[] signature, byte[] publicKey);
}
```

### 3.3 LicenseGenerator

```csharp
public class LicenseGenerator
{
    public string GenerateTrialLicense(int days);
    public string GenerateLicense(string customerInfo, DateTime expireDate, LicenseType type);
    
    // 内部生成方法
    private byte[] GenerateHardwareId();
    private byte[] GenerateSignature(byte[] data, byte[] privateKey);
    private string EncodeLicense(byte[] licenseData);
}
```

## 4. 数据模型引用

- **LicenseInfo**: 许可证信息数据模型
- **LicenseStatus**: 许可证状态枚举
- **TrialStatus**: 试用期状态枚举
- **LicenseType**: 许可证类型枚举
- **LicenseStatusChangedEventArgs**: 许可证状态变化事件参数数据模型

## 5. 关键功能实现

### 5.1 许可证激活
- 支持在线激活
- 支持离线激活
- 支持许可证密钥验证
- 支持激活状态持久化

### 5.2 许可证验证
- 支持本地验证
- 支持硬件绑定验证
- 支持时间有效性验证
- 支持数字签名验证

### 5.3 试用期管理
- 默认三个月试用期
- 支持试用期倒计时
- 支持试用期状态监控
- 支持试用期结束提醒

### 5.4 许可证信息管理
- 支持许可证信息存储
- 支持许可证信息加密
- 支持许可证信息查询
- 支持许可证信息导出

### 5.5 反调试保护
- 支持简单的反调试检测
- 支持关键代码混淆
- 支持运行时完整性检查

### 5.6 错误处理
- 详细的错误日志记录
- 用户友好的错误提示
- 自动重试机制

## 6. 安全措施

### 6.1 加密保护
- 使用AES128加密许可证数据
- 使用非对称加密验证许可证
- 敏感信息安全存储

### 6.2 反破解措施
- 软件完整性检查
- 运行环境检测
- 敏感函数保护
- 多方位验证机制

### 6.3 隐私保护
- 最小化收集用户信息
- 用户信息加密存储
- 符合数据保护法规

## 7. 依赖关系

- 依赖ILoggingService进行日志记录
- 依赖AesEncryption进行加密操作
- 依赖LicenseInfo等许可证相关数据模型
- 可选依赖网络服务进行在线验证

## 8. 扩展性

- 支持多种许可证类型
- 支持许可证升级机制
- 支持订阅式许可证
- 支持许可证转移功能

## 9. 配置项

- 试用期长度
- 许可证存储路径
- 验证服务器地址
- 日志级别