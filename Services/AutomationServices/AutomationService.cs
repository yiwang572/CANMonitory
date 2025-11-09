using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CANMonitor.Models.CommonModels;
using System.Threading;

namespace CANMonitor.Services.AutomationServices
{
    /// <summary>
    /// 自动化测试服务
    /// 负责执行无代码和代码类型的测试脚本
    /// </summary>
    public class AutomationService
    {
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;

        /// <summary>
        /// 执行自动化脚本
        /// </summary>
        /// <param name="script">要执行的脚本</param>
        /// <returns>测试报告</returns>
        public async Task<TestReport> ExecuteScriptAsync(AutomationScript script)
        {
            var report = new TestReport
            {
                TaskId = Guid.NewGuid().ToString(), // 临时任务ID
                Name = $"执行报告 - {script.ScriptName}",
                StartTime = DateTime.Now,
                Result = TestResult.Passed
            };

            try
            {
                _isRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                if (script.Type == ScriptType.NoCode && script.Steps != null)
                {
                    // 执行无代码脚本
                    await ExecuteNoCodeStepsAsync(script.Steps, report, _cancellationTokenSource.Token);
                }
                else if (script.Type == ScriptType.Code && !string.IsNullOrEmpty(script.Content))
                {
                    // 执行代码脚本（预留）
                    // ExecuteCodeScript(script.Content, report);
                }

                // 计算测试统计信息
                report.EndTime = DateTime.Now;
                report.DurationMs = (long)(report.EndTime - report.StartTime).TotalMilliseconds;
                report.TotalSteps = report.StepResults.Count;
                report.PassedSteps = report.StepResults.Count(r => r.Result == StepResult.Passed);
                report.FailedSteps = report.StepResults.Count(r => r.Result == StepResult.Failed);
                report.SkippedSteps = report.StepResults.Count(r => r.Result == StepResult.Skipped);

                // 更新整体测试结果
                if (report.FailedSteps > 0)
                {
                    report.Result = report.PassedSteps > 0 ? TestResult.Partial : TestResult.Failed;
                }
                else if (report.SkippedSteps > 0 && report.PassedSteps == 0)
                {
                    report.Result = TestResult.Skipped;
                }

                return report;
            }
            catch (Exception ex)
            {
                report.ErrorMessages.Add(ex.Message);
                report.Result = TestResult.Failed;
                report.EndTime = DateTime.Now;
                report.DurationMs = (long)(report.EndTime - report.StartTime).TotalMilliseconds;
                return report;
            }
            finally
            {
                _isRunning = false;
            }
        }

        /// <summary>
        /// 执行测试任务
        /// </summary>
        /// <param name="task">测试任务</param>
        /// <param name="scripts">相关脚本列表</param>
        /// <returns>测试报告</returns>
        public async Task<TestReport> ExecuteTaskAsync(TestTask task, List<AutomationScript> scripts)
        {
            task.Status = TaskStatus.Running;
            task.StartTime = DateTime.Now;

            var report = new TestReport
            {
                TaskId = task.TaskId,
                Name = $"任务报告 - {task.Name}",
                StartTime = DateTime.Now,
                EnvironmentInfo = new Dictionary<string, string> { { "TaskId", task.TaskId } }
            };

            try
            {
                if (task.Mode == ExecutionMode.Sequential)
                {
                    // 顺序执行所有脚本
                    foreach (var scriptId in task.ScriptIds)
                    {
                        var script = scripts.FirstOrDefault(s => s.ScriptId == scriptId);
                        if (script != null)
                        {
                            var scriptReport = await ExecuteScriptAsync(script);
                            // 合并报告
                            report.StepResults.AddRange(scriptReport.StepResults);
                            report.ErrorMessages.AddRange(scriptReport.ErrorMessages);
                            report.TestLogs.AddRange(scriptReport.TestLogs);
                        }
                    }
                }
                else if (task.Mode == ExecutionMode.Parallel)
                {
                    // 并行执行脚本（预留）
                    // var tasks = task.ScriptIds.Select(id => {
                    //     var script = scripts.FirstOrDefault(s => s.ScriptId == id);
                    //     return script != null ? ExecuteScriptAsync(script) : Task.FromResult<TestReport>(null);
                    // }).Where(t => t != null);
                    // var results = await Task.WhenAll(tasks);
                    // 合并结果
                }

                task.Status = TaskStatus.Completed;
                task.EndTime = DateTime.Now;
                return report;
            }
            catch (Exception ex)
            {
                task.Status = TaskStatus.Failed;
                task.EndTime = DateTime.Now;
                report.ErrorMessages.Add(ex.Message);
                report.Result = TestResult.Failed;
                return report;
            }
        }

        /// <summary>
        /// 执行无代码步骤
        /// </summary>
        private async Task ExecuteNoCodeStepsAsync(List<TestStep> steps, TestReport report, CancellationToken token)
        {
            foreach (var step in steps)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                var stepResult = new TestStepResult
                {
                    StepId = step.StepId,
                    StepName = step.StepName,
                    StepType = step.StepType,
                    StartTime = DateTime.Now,
                    Result = StepResult.Passed
                };

                try
                {
                    // 根据步骤类型执行不同的操作
                    switch (step.StepType)
                    {
                        case StepType.SendCanMessage:
                            await ExecuteSendCanMessage(step, stepResult, token);
                            break;
                        case StepType.ReceiveCanMessage:
                            await ExecuteReceiveCanMessage(step, stepResult, token);
                            break;
                        case StepType.Delay:
                            await ExecuteDelay(step, stepResult, token);
                            break;
                        case StepType.Log:
                            ExecuteLog(step, stepResult);
                            break;
                        // 其他步骤类型的实现...
                        default:
                            stepResult.ActualResult = $"未实现的步骤类型: {step.StepType}";
                            stepResult.Result = StepResult.Skipped;
                            break;
                    }
                }
                catch (Exception ex)
                {
                    stepResult.ErrorMessage = ex.Message;
                    stepResult.Result = StepResult.Failed;
                }
                finally
                {
                    stepResult.EndTime = DateTime.Now;
                    stepResult.DurationMs = (long)(stepResult.EndTime - stepResult.StartTime).TotalMilliseconds;
                    report.StepResults.Add(stepResult);
                }
            }
        }

        // 各种步骤类型的具体实现方法
        private async Task ExecuteSendCanMessage(TestStep step, TestStepResult result, CancellationToken token)
        {
            // 模拟发送CAN报文
            await Task.Delay(100, token);
            result.ActualResult = "CAN报文发送成功";
            report.TestLogs.Add($"[{DateTime.Now}] 发送CAN报文: {step.StepName}");
        }

        private async Task ExecuteReceiveCanMessage(TestStep step, TestStepResult result, CancellationToken token)
        {
            // 模拟接收CAN报文
            await Task.Delay(500, token);
            result.ActualResult = "CAN报文接收成功";
            report.TestLogs.Add($"[{DateTime.Now}] 接收CAN报文: {step.StepName}");
        }

        private async Task ExecuteDelay(TestStep step, TestStepResult result, CancellationToken token)
        {
            int delayMs = 1000; // 默认1秒
            if (step.Parameters.TryGetValue("DelayMs", out var delayObj) && delayObj is int d)
            {
                delayMs = d;
            }
            
            await Task.Delay(delayMs, token);
            result.ActualResult = $"延迟 {delayMs} 毫秒";
        }

        private void ExecuteLog(TestStep step, TestStepResult result)
        {
            string message = step.Parameters.TryGetValue("Message", out var msgObj) ? msgObj.ToString() : "日志记录";
            result.ActualResult = message;
            report.TestLogs.Add($"[{DateTime.Now}] {message}");
        }

        /// <summary>
        /// 停止当前正在执行的测试
        /// </summary>
        public void StopExecution()
        {
            if (_isRunning && _cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _isRunning = false;
            }
        }

        /// <summary>
        /// 暂停当前执行的测试
        /// </summary>
        public void PauseExecution()
        {
            // 预留实现
        }

        /// <summary>
        /// 恢复暂停的测试
        /// </summary>
        public void ResumeExecution()
        {
            // 预留实现
        }
    }
}