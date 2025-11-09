using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.CommonModels
{
    /// <summary>
    /// 测试模板模型
    /// </summary>
    public class TestTemplate
    {
        /// <summary>
        /// 模板ID
        /// </summary>
        public string TemplateId { get; set; }
        
        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 模板描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 模板类型
        /// </summary>
        public TemplateType Type { get; set; }
        
        /// <summary>
        /// 适用场景
        /// </summary>
        public string ApplicableScenario { get; set; }
        
        /// <summary>
        /// 步骤列表
        /// </summary>
        public List<TestStep> Steps { get; set; }
        
        /// <summary>
        /// 变量列表
        /// </summary>
        public List<ScriptVariable> Variables { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
        
        /// <summary>
        /// 是否为内置模板
        /// </summary>
        public bool IsBuiltIn { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public TestTemplate()
        {
            TemplateId = Guid.NewGuid().ToString();
            Steps = new List<TestStep>();
            Variables = new List<ScriptVariable>();
            CreatedTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 模板类型枚举
    /// </summary>
    public enum TemplateType
    {
        BasicTest,        // 基础测试
        SignalMonitoring, // 信号监控
        FunctionalTest,   // 功能测试
        DiagnosticsTest,  // 诊断测试
        PerformanceTest,  // 性能测试
        Custom            // 自定义
    }
}
