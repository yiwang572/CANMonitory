using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.CommonModels
{
    /// <summary>
    /// 测试任务模型
    /// </summary>
    public class TestTask
    {
        /// <summary>
        /// 任务ID
        /// </summary>
        public string TaskId { get; set; }
        
        /// <summary>
        /// 任务名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 任务描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 关联的脚本ID列表
        /// </summary>
        public List<string> ScriptIds { get; set; }
        
        /// <summary>
        /// 执行模式
        /// </summary>
        public ExecutionMode Mode { get; set; }
        
        /// <summary>
        /// 任务状态
        /// </summary>
        public TaskStatus Status { get; set; }
        
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedTime { get; set; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? StartTime { get; set; }
        
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? EndTime { get; set; }
        
        /// <summary>
        /// 执行参数
        /// </summary>
        public Dictionary<string, object> Parameters { get; set; }
        
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; }
        
        /// <summary>
        /// 创建者
        /// </summary>
        public string Creator { get; set; }
        
        /// <summary>
        /// 备注
        /// </summary>
        public string Remarks { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public TestTask()
        {
            TaskId = Guid.NewGuid().ToString();
            ScriptIds = new List<string>();
            Parameters = new Dictionary<string, object>();
            CreatedTime = DateTime.Now;
            Mode = ExecutionMode.Sequential;
            Status = TaskStatus.Ready;
            Priority = 0;
        }
    }

    /// <summary>
    /// 执行模式枚举
    /// </summary>
    public enum ExecutionMode
    {
        Sequential,  // 顺序执行
        Parallel     // 并行执行
    }

    /// <summary>
    /// 任务状态枚举
    /// </summary>
    public enum TaskStatus
    {
        Ready,      // 准备就绪
        Running,    // 运行中
        Completed,  // 已完成
        Failed,     // 失败
        Paused,     // 已暂停
        Canceled    // 已取消
    }
}