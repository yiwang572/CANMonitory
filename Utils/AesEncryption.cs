using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CANMonitor.Utils
{
    /// <summary>
    /// AES加密解密工具类
    /// 用于BMS诊断中的安全访问加密通信
    /// </summary>
    public class AesEncryption
    {
        /// <summary>
        /// AES加密方法
        /// </summary>
        /// <param name="plainText">明文数据</param>
        /// <param name="key">加密密钥</param>
        /// <returns>加密后的Base64字符串</returns>
        public string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText) || string.IsNullOrEmpty(key))
                throw new ArgumentException("明文和密钥不能为空");

            try
            {
                // 使用密钥派生函数获取实际的AES密钥和IV
                byte[] keyBytes = DeriveKeyAndIv(key, out byte[] iv);

                using (Aes aes = Aes.Create())
                {
                    // 设置AES参数
                    aes.Key = keyBytes;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    // 创建加密器
                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    // 加密数据
                    using (MemoryStream msEncrypt = new MemoryStream())
                    {
                        using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                        {
                            using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            {
                                swEncrypt.Write(plainText);
                            }
                            return Convert.ToBase64String(msEncrypt.ToArray());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("加密过程中发生错误", ex);
            }
        }

        /// <summary>
        /// AES解密方法
        /// </summary>
        /// <param name="cipherText">加密的Base64字符串</param>
        /// <param name="key">解密密钥</param>
        /// <returns>解密后的明文</returns>
        public string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText) || string.IsNullOrEmpty(key))
                throw new ArgumentException("密文和密钥不能为空");

            try
            {
                // 转换Base64密文为字节数组
                byte[] cipherBytes = Convert.FromBase64String(cipherText);

                // 使用密钥派生函数获取实际的AES密钥和IV
                byte[] keyBytes = DeriveKeyAndIv(key, out byte[] iv);

                using (Aes aes = Aes.Create())
                {
                    // 设置AES参数
                    aes.Key = keyBytes;
                    aes.IV = iv;
                    aes.Mode = CipherMode.CBC;
                    aes.Padding = PaddingMode.PKCS7;

                    // 创建解密器
                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);

                    // 解密数据
                    using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                return srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new CryptographicException("解密过程中发生错误", ex);
            }
        }

        /// <summary>
        /// 从密码生成AES密钥和初始化向量(IV)
        /// 使用SHA256哈希算法派生密钥
        /// </summary>
        /// <param name="password">用户密码或种子值</param>
        /// <param name="iv">输出的初始化向量</param>
        /// <returns>AES密钥</returns>
        private byte[] DeriveKeyAndIv(string password, out byte[] iv)
        {
            // 创建UTF8编码的密码字节数组
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);

            // 使用SHA256计算密码的哈希值
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(passwordBytes);

                // 前16字节用于密钥(AES-128)
                byte[] key = new byte[16];
                Array.Copy(hash, 0, key, 0, 16);

                // 后16字节用于IV(AES块大小为16字节)
                iv = new byte[16];
                Array.Copy(hash, 16, iv, 0, 16);

                return key;
            }
        }

        /// <summary>
        /// 生成随机密钥
        /// 用于创建安全的临时密钥
        /// </summary>
        /// <param name="keyLength">密钥长度(字节)</param>
        /// <returns>随机密钥</returns>
        public string GenerateRandomKey(int keyLength = 32)
        {
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                byte[] keyBytes = new byte[keyLength];
                rng.GetBytes(keyBytes);
                return Convert.ToBase64String(keyBytes);
            }
        }

        /// <summary>
        /// 验证两个消息是否匹配（防止数据篡改）
        /// 通过比较加密哈希值实现
        /// </summary>
        /// <param name="message1">第一个消息</param>
        /// <param name="message2">第二个消息</param>
        /// <param name="key">用于加密的密钥</param>
        /// <returns>两个消息是否匹配</returns>
        public bool VerifyMessagesMatch(string message1, string message2, string key)
        {
            if (string.IsNullOrEmpty(message1) || string.IsNullOrEmpty(message2) || string.IsNullOrEmpty(key))
                return false;

            try
            {
                string hash1 = Encrypt(message1, key);
                string hash2 = Encrypt(message2, key);

                return hash1.Equals(hash2, StringComparison.Ordinal);
            }
            catch
            {
                return false;
            }
        }
    }
}