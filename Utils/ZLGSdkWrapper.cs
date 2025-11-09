using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace CANMonitor.Utils
{
    /// <summary>
    /// ZLG CAN SDK 包装类
    /// 提供对zlgcan.dll原生函数的P/Invoke声明和包装
    /// </summary>
    public class ZLGSdkWrapper
    {
        // 设备类型常量
        public const int DEVICE_TYPE_USBCAN_I = 1;
        public const int DEVICE_TYPE_USBCAN_II = 2;
        public const int DEVICE_TYPE_USBCAN_2C = 3;
        public const int DEVICE_TYPE_USBCAN_2E_U = 4;
        public const int DEVICE_TYPE_USBCANFD_200U = 5;

        // 打开设备
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_OpenDevice(int deviceType, int deviceIndex, int reserved);

        // 关闭设备
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_CloseDevice(int deviceType, int deviceIndex);

        // 初始化CAN通道
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_InitCAN(int deviceType, int deviceIndex, int canIndex, ref CAN_InitConfig config);

        // 启动CAN通道
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_StartCAN(int deviceType, int deviceIndex, int canIndex);

        // 重置CAN通道
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_ResetCAN(int deviceType, int deviceIndex, int canIndex);

        // 发送CAN消息
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_Transmit(int deviceType, int deviceIndex, int canIndex, ref CAN_TransmitMsg[] pSend, int len);

        // 接收CAN消息
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_Receive(int deviceType, int deviceIndex, int canIndex, ref CAN_ReceiveMsg[] pReceive, int len, int waitTime);

        // 获取接收缓冲区中消息数量
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_GetReceiveNum(int deviceType, int deviceIndex, int canIndex);

        // 清空缓冲区
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_ClearBuffer(int deviceType, int deviceIndex, int canIndex);

        // 读取通道状态
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_ReadChannelStatus(int deviceType, int deviceIndex, int canIndex, ref CAN_Status status);

        // 读取通道错误信息
        [DllImport("zlgcan.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int ZCAN_ReadChannelErrInfo(int deviceType, int deviceIndex, int canIndex, ref CAN_ErrorInfo errInfo);

        /// <summary>
        /// CAN初始化配置结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CAN_InitConfig
        {
            public uint AccCode;       // 验收码
            public uint AccMask;       // 屏蔽码
            public uint Reserved;      // 保留位
            public byte Filter;        // 滤波方式
            public byte Timing0;       // 定时器0
            public byte Timing1;       // 定时器1
            public byte Mode;          // 模式
        }

        /// <summary>
        /// CAN发送消息结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CAN_TransmitMsg
        {
            public uint ID;            // 帧ID
            public uint TimeStamp;     // 时间戳
            public byte TimeFlag;      // 时间标志
            public byte SendType;      // 发送类型
            public byte RemoteFlag;    // 远程帧标志
            public byte ExternFlag;    // 扩展帧标志
            public byte DataLen;       // 数据长度
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data;        // 数据
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Reserved;    // 保留
        }

        /// <summary>
        /// CAN接收消息结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CAN_ReceiveMsg
        {
            public uint ID;            // 帧ID
            public uint TimeStamp;     // 时间戳
            public byte TimeFlag;      // 时间标志
            public byte SendType;      // 发送类型
            public byte RemoteFlag;    // 远程帧标志
            public byte ExternFlag;    // 扩展帧标志
            public byte DataLen;       // 数据长度
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public byte[] Data;        // 数据
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public byte[] Reserved;    // 保留
            public byte Channel;       // 通道号
        }

        /// <summary>
        /// CAN状态结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CAN_Status
        {
            public byte ErrInterrupt;  // 错误中断
            public byte regMode;       // 模式寄存器
            public byte regStatus;     // 状态寄存器
            public byte regALCapture;  // 仲裁丢失捕获
            public byte regECCapture;  // 错误计数器捕获
            public byte regEWLimit;    // 错误警告限制
            public byte regRECounter;  // 接收错误计数
            public byte regTECounter;  // 发送错误计数
        }

        /// <summary>
        /// CAN错误信息结构体
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct CAN_ErrorInfo
        {
            public uint ErrCode;       // 错误码
            public uint Passive_ErrData; // 被动错误数据
            public uint ArLost_ErrData; // 仲裁丢失错误数据
        }

        /// <summary>
        /// 初始化SDK
        /// </summary>
        /// <returns>操作是否成功</returns>
        public bool Initialize()
        {
            // ZLG CAN SDK 不需要显式初始化，通过OpenDevice即可使用
            return true;
        }

        /// <summary>
        /// 检查DLL是否存在并可加载
        /// </summary>
        /// <returns>DLL是否可用</returns>
        public bool IsDllAvailable()
        {
            try
            {
                // 尝试调用一个简单的函数来验证DLL是否可用
                // 这里使用ZCAN_OpenDevice并传入无效参数，应该返回错误码
                int result = ZCAN_OpenDevice(-1, -1, 0);
                // 即使返回错误也没关系，只要不抛出异常，说明DLL已成功加载
                return true;
            }
            catch (DllNotFoundException)
            {
                return false;
            }
            catch (BadImageFormatException)
            {
                return false;
            }
        }
    }
}