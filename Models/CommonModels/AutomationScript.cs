using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.CommonModels
{
    /// <summary>
    /// 自动化测试脚本模型
    /// </summary>
    public class AutomationScript
    {
        /// <summary>
        /// 脚本ID
        /// </summary>
        public string ScriptId { get; set; }
        
        /// <summary>
        /// 脚本名称
        /// </summary>
        public string ScriptName { get; set; }
        
        /// <summary>
        /// 脚本类型
        /// </summary>
        public ScriptType Type { get; set; }
        
        /// <summary>
        /// 脚本描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 脚本内容（代码类型脚本使用）
        /// </summary>
        public string Content { get; set; }
        
        /// <summary>
        /// 步骤列表（无代码类型脚本使用）
        /// </summary>
        public List<TestStep> Steps { get; set; }
        
        /// <summary>
        /// 变量列表
        /// </summary>
        public List<ScriptVariable> Variables { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }
        
        /// <summary>
        /// 更新时间
        /// </summary>
        public DateTime UpdateTime { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public AutomationScript()
        {
            ScriptId = Guid.NewGuid().ToString();
            Steps = new List<TestStep>();
            Variables = new List<ScriptVariable>();
            CreateTime = DateTime.Now;
            UpdateTime = DateTime.Now;
            Type = ScriptType.NoCode;
        }
    }

    /// <summary>
    /// 脚本类型枚举
    /// </summary>
    public enum ScriptType
    {
        Code,     // 代码脚本
        NoCode    // 无代码脚本
    }

    /// <summary>
    /// 验证规则
    /// </summary>
    public class ValidationRule
    {
        public string Target { get; set; } // 验证目标
        public string Property { get; set; } // 验证属性
        public ComparisonOperator Operator { get; set; } // 比较运算符
        public string ExpectedValue { get; set; } // 期望值
        public string ErrorMessage { get; set; } // 错误消息
    }

    /// <summary>
    /// 比较运算符枚举
    /// </summary>
    public enum ComparisonOperator
    {
        Equal,               // 等于
        NotEqual,            // 不等于
        GreaterThan,         // 大于
        GreaterThanOrEqual,  // 大于等于
        LessThan,            // 小于
        LessThanOrEqual,     // 小于等于
        Contains,            // 包含
        StartsWith,          // 以...开始
        EndsWith,            // 以...结束
        RegexMatch           // 正则匹配
    }
}