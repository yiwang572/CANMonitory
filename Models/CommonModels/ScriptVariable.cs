using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.CommonModels
{
    /// <summary>
    /// 脚本变量模型
    /// </summary>
    public class ScriptVariable
    {
        /// <summary>
        /// 变量名
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 变量类型
        /// </summary>
        public VariableType Type { get; set; }
        
        /// <summary>
        /// 初始值
        /// </summary>
        public object InitialValue { get; set; }
        
        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 是否为输入参数
        /// </summary>
        public bool IsInputParameter { get; set; }
        
        /// <summary>
        /// 是否为输出参数
        /// </summary>
        public bool IsOutputParameter { get; set; }
        
        /// <summary>
        /// 是否为常量
        /// </summary>
        public bool IsConstant { get; set; }
    }

    /// <summary>
    /// 变量类型枚举
    /// </summary>
    public enum VariableType
    {
        Integer,
        Float,
        Boolean,
        String,
        DateTime,
        ByteArray,
        Object,
        CanMessage,
        List
    }
}
