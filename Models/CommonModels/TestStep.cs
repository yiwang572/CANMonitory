using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.CommonModels
{
    /// <summary>
    /// 测试步骤模型
    /// </summary>
    public class TestStep
    {
        /// <summary>
        /// 步骤ID
        /// </summary>
        public string StepId { get; set; }
        
        /// <summary>
        /// 步骤类型
        /// </summary>
        public StepType StepType { get; set; }
        
        /// <summary>
        /// 步骤名称
        /// </summary>
        public string StepName { get; set; }
        
        /// <summary>
        /// 列表索引（用于界面显示）
        /// </summary>
        public int ListIndex { get; set; }
        
        /// <summary>
        /// 步骤描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 步骤参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
        
        /// <summary>
        /// 前置条件
        /// </summary>
        public List<string> PreConditions { get; set; }
        
        /// <summary>
        /// 期望结果
        /// </summary>
        public string ExpectedResult { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public TestStep()
        {
            StepId = Guid.NewGuid().ToString();
            Parameters = new Dictionary<string, object>();
            PreConditions = new List<string>();
        }
    }

    /// <summary>
    /// 步骤类型枚举
    /// </summary>
    public enum StepType
    {
        SendCanMessage,      // 发送CAN报文
        ReceiveCanMessage,   // 接收CAN报文
        Wait,                // 等待
        Assert,              // 断言
        Log,                 // 日志
        Condition,           // 条件判断
        Loop,                // 循环
        VariableSet,         // 设置变量
        VariableGet,         // 获取变量
        CallScript,          // 调用脚本
        StartRecording,      // 开始记录
        StopRecording,       // 停止记录
        Delay,               // 延迟
        SignalMonitor        // 信号监控
    }
}