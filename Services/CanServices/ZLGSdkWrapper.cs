using System;
using System.Runtime.InteropServices;
using System.IO;

namespace CANMonitor.Services.CanServices
{
    /// <summary>
    /// ZLG CAN SDK包装器，负责与zlgcan.dll交互
    /// </summary>
    public class ZLGSdkWrapper
    {
        // 设备类型定义
        public const int DEVICE_TYPE_USBCANFD_200U = 40;
        
        // DLL路径
        private const string DllPath = "zlgcan.dll";
        
        #region CAN消息结构体
        
        /// <summary>
        /// CAN初始化配置结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CAN_InitConfig
        {
            public uint AccCode;      // 验收码
            public uint AccMask;      // 屏蔽码
            public uint Reserved;     // 保留
            public byte Filter;       // 过滤模式
            public byte Timing0;      // 定时器0
            public byte Timing1;      // 定时器1
            public byte Mode;         // 模式
        }
        
        /// <summary>
        /// CAN发送消息结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CAN_Transmit
        {
            public uint ID;               // 帧ID
            public uint TimeStamp;        // 时间戳
            public byte TimeFlag;         // 时间标志
            public byte SendType;         // 发送类型
            public byte RemoteFlag;       // 远程帧标志
            public byte ExternFlag;       // 扩展帧标志
            public byte DataLen;          // 数据长度
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data;           // 数据
        }
        
        /// <summary>
        /// CAN接收消息结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CAN_Receive
        {
            public uint ID;               // 帧ID
            public uint TimeStamp;        // 时间戳
            public byte TimeFlag;         // 时间标志
            public byte ReceiveType;      // 接收类型
            public byte RemoteFlag;       // 远程帧标志
            public byte ExternFlag;       // 扩展帧标志
            public byte DataLen;          // 数据长度
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data;           // 数据
        }
        
        /// <summary>
        /// CAN FD发送消息结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CANFD_Transmit
        {
            public uint ID;               // 帧ID
            public byte DLC;              // 数据长度码
            public byte Flag;             // 标志位，bit0表示是否是CAN FD帧
            public byte ExternFlag;       // 扩展帧标志
            public byte Reserved;         // 保留
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] Data;           // 数据
        }
        
        /// <summary>
        /// CAN FD接收消息结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CANFD_Receive
        {
            public uint ID;               // 帧ID
            public uint TimeStamp;        // 时间戳
            public byte TimeFlag;         // 时间标志
            public byte Flag;             // 标志位，bit0表示是否是CAN FD帧
            public byte ExternFlag;       // 扩展帧标志
            public byte DLC;              // 数据长度码
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] Data;           // 数据
        }
        
        /// <summary>
        /// CAN状态结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CAN_Status
        {
            public byte ErrInterrupt;     // 错误中断
            public byte RegMode;          // 寄存器模式
            public byte RegStatus;        // 寄存器状态
            public byte RegALCapture;     // 寄存器AL捕获
            public byte RegECCapture;     // 寄存器EC捕获
            public byte RegEWLimit;       // 寄存器EW限制
            public byte RegRECounter;     // 寄存器RE计数器
            public byte RegTECounter;     // 寄存器TE计数器
            public uint Reserved;         // 保留
        }
        
        /// <summary>
        /// CAN错误帧结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CAN_ErrorFrame
        {
            public uint ErrType;          // 错误类型
            public uint ID;               // 帧ID
            public byte ExternFlag;       // 扩展帧标志
            public byte RemoteFlag;       // 远程帧标志
            public byte DataLen;          // 数据长度
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data;           // 数据
        }
        
        /// <summary>
        /// 过滤器配置结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Filter_Config
        {
            public byte FilterIndex;      // 过滤器索引
            public byte FilterMode;       // 过滤器模式
            public byte FilterType;       // 过滤器类型
            public byte ExtFrame;         // 扩展帧标志
            public uint Start;            // 起始ID
            public uint End;              // 结束ID
        }
        
        #endregion
        
        #region DLL导入函数
        
        // 打开设备
        [DllImport(DllPath, EntryPoint = "ZCAN_OpenDevice", CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr ZCAN_OpenDevice(int deviceType, int deviceIndex, int reserved);
        
        // 关闭设备
        [DllImport(DllPath, EntryPoint = "ZCAN_CloseDevice", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_CloseDevice(IntPtr deviceHandle);
        
        // 初始化CAN通道
        [DllImport(DllPath, EntryPoint = "ZCAN_InitCAN", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_InitCAN(IntPtr deviceHandle, int canIndex, ref CAN_InitConfig initConfig);
        
        // 启动CAN通道
        [DllImport(DllPath, EntryPoint = "ZCAN_StartCAN", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_StartCAN(IntPtr deviceHandle, int canIndex);
        
        // 停止CAN通道
        [DllImport(DllPath, EntryPoint = "ZCAN_StopCAN", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_StopCAN(IntPtr deviceHandle, int canIndex);
        
        // 重置CAN通道
        [DllImport(DllPath, EntryPoint = "ZCAN_ResetCAN", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_ResetCAN(IntPtr deviceHandle, int canIndex);
        
        // 发送CAN消息
        [DllImport(DllPath, EntryPoint = "ZCAN_Transmit", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_Transmit(IntPtr deviceHandle, ref CAN_Transmit sendMsg, int len);
        
        // 接收CAN消息
        [DllImport(DllPath, EntryPoint = "ZCAN_Receive", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_Receive(IntPtr deviceHandle, int canIndex, ref CAN_Receive receiveMsg, int len, int waitTime);
        
        // 批量接收CAN消息
        [DllImport(DllPath, EntryPoint = "ZCAN_Receive", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_Receive(IntPtr deviceHandle, int canIndex, [MarshalAs(UnmanagedType.LPArray)] CAN_Receive[] receiveBuffer, int len, int waitTime);
        
        // 发送CAN FD消息
        [DllImport(DllPath, EntryPoint = "ZCAN_TransmitFD", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_TransmitFD(IntPtr deviceHandle, ref CANFD_Transmit sendMsg, int len);
        
        // 接收CAN FD消息
        [DllImport(DllPath, EntryPoint = "ZCAN_ReceiveFD", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_ReceiveFD(IntPtr deviceHandle, int canIndex, ref CANFD_Receive receiveMsg, int len, int waitTime);
        
        // 批量接收CAN FD消息
        [DllImport(DllPath, EntryPoint = "ZCAN_ReceiveFD", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_ReceiveFD(IntPtr deviceHandle, int canIndex, [MarshalAs(UnmanagedType.LPArray)] CANFD_Receive[] receiveBuffer, int len, int waitTime);
        
        // 获取设备错误信息
        [DllImport(DllPath, EntryPoint = "ZCAN_GetErrorInfo", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_GetErrorInfo(IntPtr deviceHandle);
        
        // 获取通道错误信息
        [DllImport(DllPath, EntryPoint = "ZCAN_GetErrorInfo", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_GetErrorInfo(IntPtr deviceHandle, int canIndex);
        
        // 获取通道状态
        [DllImport(DllPath, EntryPoint = "ZCAN_GetCANStatus", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_GetCANStatus(IntPtr deviceHandle, int canIndex, ref CAN_Status canStatus);
        
        // 获取接收缓冲中的帧数
        [DllImport(DllPath, EntryPoint = "ZCAN_GetReceiveNum", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_GetReceiveNum(IntPtr deviceHandle, int canIndex);
        
        // 获取发送缓冲中的帧数
        [DllImport(DllPath, EntryPoint = "ZCAN_GetTransmitNum", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_GetTransmitNum(IntPtr deviceHandle, int canIndex);
        
        // 获取最后一个错误帧
        [DllImport(DllPath, EntryPoint = "ZCAN_GetLastErrorFrame", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_GetLastErrorFrame(IntPtr deviceHandle, int canIndex, ref CAN_ErrorFrame errorFrame);
        
        // 设置过滤器
        [DllImport(DllPath, EntryPoint = "ZCAN_SetFilter", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_SetFilter(IntPtr deviceHandle, int canIndex, ref Filter_Config filter);
        
        // 获取设备数量
        [DllImport(DllPath, EntryPoint = "ZCAN_GetDeviceCount", CallingConvention = CallingConvention.StdCall)]
        public static extern int ZCAN_GetDeviceCount(int deviceType, int reserved);
        
        #endregion
        
        #region 构造函数和辅助方法
        
        public ZLGSdkWrapper()
        {
            // 构造函数中初始化
            InitializeDllSearchPath();
        }
        
        /// <summary>
        /// 初始化DLL搜索路径，确保能正确加载zlgcan.dll
        /// </summary>
        private void InitializeDllSearchPath()
        {
            try
            {
                // 获取应用程序目录
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                
                // 添加References/zlgcan_x86目录到DLL搜索路径
                string x86DllPath = Path.Combine(appDir, "References", "zlgcan_x86");
                if (Directory.Exists(x86DllPath))
                {
                    // 将DLL目录添加到系统PATH环境变量中
                    string currentPath = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
                    if (!currentPath.Contains(x86DllPath))
                    {
                        Environment.SetEnvironmentVariable("PATH", x86DllPath + ";" + currentPath, EnvironmentVariableTarget.Process);
                    }
                    
                    // 也添加kerneldlls子目录
                    string kernelDllPath = Path.Combine(x86DllPath, "kerneldlls");
                    if (Directory.Exists(kernelDllPath) && !currentPath.Contains(kernelDllPath))
                    {
                        Environment.SetEnvironmentVariable("PATH", kernelDllPath + ";" + Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process), EnvironmentVariableTarget.Process);
                    }
                }
                
                // 复制DLL到输出目录作为备选方案
                CopyDllToOutputDirectory();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"初始化DLL搜索路径失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 复制DLL到输出目录，确保程序能够找到DLL
        /// </summary>
        private void CopyDllToOutputDirectory()
        {
            try
            {
                string appDir = AppDomain.CurrentDomain.BaseDirectory;
                string dllSourcePath = Path.Combine(appDir, "References", "zlgcan_x86", "zlgcan.dll");
                string dllDestPath = Path.Combine(appDir, "zlgcan.dll");
                
                if (File.Exists(dllSourcePath) && (!File.Exists(dllDestPath) || IsFileOutOfDate(dllSourcePath, dllDestPath)))
                {
                    File.Copy(dllSourcePath, dllDestPath, true);
                    Console.WriteLine($"已复制zlgcan.dll到输出目录: {dllDestPath}");
                }
                
                // 也复制kerneldlls目录下的文件
                string kernelSourceDir = Path.Combine(appDir, "References", "zlgcan_x86", "kerneldlls");
                string kernelDestDir = Path.Combine(appDir, "kerneldlls");
                
                if (Directory.Exists(kernelSourceDir))
                {
                    Directory.CreateDirectory(kernelDestDir);
                    foreach (string file in Directory.GetFiles(kernelSourceDir))
                    {
                        string destFile = Path.Combine(kernelDestDir, Path.GetFileName(file));
                        if (!File.Exists(destFile) || IsFileOutOfDate(file, destFile))
                        {
                            File.Copy(file, destFile, true);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"复制DLL失败: {ex.Message}");
            }
        }
        
        /// <summary>
        /// 检查文件是否过期
        /// </summary>
        private bool IsFileOutOfDate(string sourceFile, string destFile)
        {
            if (!File.Exists(destFile))
                return true;
            
            DateTime sourceDate = File.GetLastWriteTime(sourceFile);
            DateTime destDate = File.GetLastWriteTime(destFile);
            
            return sourceDate > destDate;
        }
        
        /// <summary>
        /// 检查DLL是否可用
        /// </summary>
        /// <returns>是否可用</returns>
        public bool IsDllAvailable()
        {
            try
            {
                // 尝试调用一个简单的函数来检查DLL是否可用
                int result = ZCAN_GetDeviceCount(DEVICE_TYPE_USBCANFD_200U, 0);
                // 即使返回0（表示没有设备），只要不抛出异常，DLL就是可用的
                return true;
            }
            catch (DllNotFoundException)
            {
                Console.WriteLine("无法找到zlgcan.dll，请确保DLL文件位于正确位置");
                return false;
            }
            catch (BadImageFormatException)
            {
                Console.WriteLine("zlgcan.dll的格式不匹配，请确保使用正确的x86或x64版本");
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"检查zlgcan.dll可用性时出错: {ex.Message}");
                return false;
            }
        }
        
        #endregion
    }
}