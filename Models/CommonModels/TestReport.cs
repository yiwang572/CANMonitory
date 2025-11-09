using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.CommonModels
{
    /// <summary>
    /// 测试报告模型
    /// </summary>
    public class TestReport
    {
        /// <summary>
        /// 报告ID
        /// </summary>
        public string ReportId { get; set; }
        
        /// <summary>
        /// 关联的任务ID
        /// </summary>
        public string TaskId { get; set; }
        
        /// <summary>
        /// 报告名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 测试结果
        /// </summary>
        public TestResult Result { get; set; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// 测试时长（毫秒）
        /// </summary>
        public long DurationMs { get; set; }
        
        /// <summary>
        /// 总步骤数
        /// </summary>
        public int TotalSteps { get; set; }
        
        /// <summary>
        /// 通过步骤数
        /// </summary>
        public int PassedSteps { get; set; }
        
        /// <summary>
        /// 失败步骤数
        /// </summary>
        public int FailedSteps { get; set; }
        
        /// <summary>
        /// 跳过步骤数
        /// </summary>
        public int SkippedSteps { get; set; }
        
        /// <summary>
        /// 测试详细信息
        /// </summary>
        public List<TestStepResult> StepResults { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public List<string> ErrorMessages { get; set; }
        
        /// <summary>
        /// 测试日志
        /// </summary>
        public List<string> TestLogs { get; set; }
        
        /// <summary>
        /// 环境信息
        /// </summary>
        public Dictionary<string, string> EnvironmentInfo { get; set; }
        
        /// <summary>
        /// 生成时间
        /// </summary>
        public DateTime GeneratedTime { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public TestReport()
        {
            ReportId = Guid.NewGuid().ToString();
            StepResults = new List<TestStepResult>();
            ErrorMessages = new List<string>();
            TestLogs = new List<string>();
            EnvironmentInfo = new Dictionary<string, string>();
            GeneratedTime = DateTime.Now;
        }
    }

    /// <summary>
    /// 测试结果枚举
    /// </summary>
    public enum TestResult
    {
        Passed,     // 通过
        Failed,     // 失败
        Partial,    // 部分通过
        Skipped     // 跳过
    }

    /// <summary>
    /// 测试步骤结果模型
    /// </summary>
    public class TestStepResult
    {
        /// <summary>
        /// 步骤ID
        /// </summary>
        public string StepId { get; set; }
        
        /// <summary>
        /// 步骤名称
        /// </summary>
        public string StepName { get; set; }
        
        /// <summary>
        /// 步骤类型
        /// </summary>
        public StepType StepType { get; set; }
        
        /// <summary>
        /// 执行结果
        /// </summary>
        public StepResult Result { get; set; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime StartTime { get; set; }
        
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime { get; set; }
        
        /// <summary>
        /// 执行时长（毫秒）
        /// </summary>
        public long DurationMs { get; set; }
        
        /// <summary>
        /// 实际结果
        /// </summary>
        public string ActualResult { get; set; }
        
        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; }
        
        /// <summary>
        /// 子步骤结果
        /// </summary>
        public List<TestStepResult> SubStepResults { get; set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public TestStepResult()
        {
            SubStepResults = new List<TestStepResult>();
        }
    }

    /// <summary>
    /// 步骤结果枚举
    /// </summary>
    public enum StepResult
    {
        Passed,     // 通过
        Failed,     // 失败
        Skipped,    // 跳过
        NotExecuted // 未执行
    }
}