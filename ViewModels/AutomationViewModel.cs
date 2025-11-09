using Prism.Mvvm;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CANMonitor.Models.CommonModels;
using CANMonitor.Services.AutomationServices;

namespace CANMonitor.ViewModels
{
    public class AutomationViewModel : BindableBase
    {
        // 服务依赖
        private readonly AutomationService _automationService;
        private readonly ScriptTemplateManager _templateManager;

        #region 属性

        // 视图绑定属性
        private string _title = "自动化测试";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        // 模板相关属性
        private ObservableCollection<TestTemplate> _availableTemplates;
        public ObservableCollection<TestTemplate> AvailableTemplates
        {
            get { return _availableTemplates; }
            set { SetProperty(ref _availableTemplates, value); }
        }

        private TestTemplate _selectedTemplate;
        public TestTemplate SelectedTemplate
        {
            get { return _selectedTemplate; }
            set { SetProperty(ref _selectedTemplate, value); }
        }

        // 脚本相关属性
        private ObservableCollection<AutomationScript> _scripts;
        /// <summary>
        /// 所有可用的脚本列表
        /// </summary>
        public ObservableCollection<AutomationScript> Scripts
        {
            get { return _scripts; }
            set { SetProperty(ref _scripts, value); }
        }

        private AutomationScript _currentScript;
        public AutomationScript CurrentScript
        {
            get { return _currentScript; }
            set 
            { 
                SetProperty(ref _currentScript, value);
                // 更新步骤索引
                UpdateStepsListIndex();
            }
        }

        private TestStep _selectedStep;
        public TestStep SelectedStep
        {
            get { return _selectedStep; }
            set { SetProperty(ref _selectedStep, value); }
        }

        // 测试报告和执行相关属性
        private ObservableCollection<TestReport> _testReports;
        public ObservableCollection<TestReport> TestReports
        {
            get { return _testReports; }
            set { SetProperty(ref _testReports, value); }
        }

        private TestReport _currentTestReport;
        public TestReport CurrentTestReport
        {
            get { return _currentTestReport; }
            set { SetProperty(ref _currentTestReport, value); }
        }

        private bool _isTestRunning;
        public bool IsTestRunning
        {
            get { return _isTestRunning; }
            set { SetProperty(ref _isTestRunning, value); }
        }

        private string _executionStatus;
        public string ExecutionStatus
        {
            get { return _executionStatus; }
            set { SetProperty(ref _executionStatus, value); }
        }

        // 任务管理相关属性
        private ObservableCollection<TestTask> _testTasks;
        public ObservableCollection<TestTask> TestTasks
        {
            get { return _testTasks; }
            set { SetProperty(ref _testTasks, value); }
        }

        private TestTask _currentTask;
        public TestTask CurrentTask
        {
            get { return _currentTask; }
            set 
            { 
                SetProperty(ref _currentTask, value);
                // 当当前任务改变时，更新当前任务的脚本名称列表
                OnPropertyChanged(nameof(CurrentTaskScriptNames));
            }
        }

        /// <summary>
        /// 当前任务中的脚本名称列表
        /// </summary>
        public string CurrentTaskScriptNames
        { 
            get 
            { 
                if (CurrentTask == null || CurrentTask.ScriptIds == null || CurrentTask.ScriptIds.Count == 0)
                { 
                    return "无脚本";
                }
                
                // 根据ScriptIds获取实际的脚本名称
                var scriptNames = new List<string>();
                foreach (var scriptId in CurrentTask.ScriptIds)
                { 
                    var script = Scripts.FirstOrDefault(s => s != null && s.ScriptId == scriptId);
                    if (script != null)
                    { 
                        scriptNames.Add(script.ScriptName);
                    }
                    else
                    { 
                        scriptNames.Add(scriptId); // 如果找不到脚本，显示ID
                    }
                }
                return string.Join(", ", scriptNames);
            } 
        }

        // 参数编辑相关属性
        private ObservableCollection<ScriptVariable> _templateParameters;
        public ObservableCollection<ScriptVariable> TemplateParameters
        {
            get { return _templateParameters; }
            set { SetProperty(ref _templateParameters, value); }
        }

        // 可用的步骤类型
        public ObservableCollection<StepType> AvailableStepTypes { get; set; }
        
        // 可用的脚本类型
        public ObservableCollection<ScriptType> AvailableScriptTypes { get; set; }
        #endregion

        // 构造函数
        public AutomationViewModel()
        {
            // 初始化服务（实际项目中应使用依赖注入）
            _templateManager = new ScriptTemplateManager();
            _automationService = new AutomationService();

            // 初始化集合
            AvailableTemplates = new ObservableCollection<TestTemplate>();
            Scripts = new ObservableCollection<AutomationScript>();
            TestReports = new ObservableCollection<TestReport>();
            TestTasks = new ObservableCollection<TestTask>();
            TemplateParameters = new ObservableCollection<ScriptVariable>();

            // 初始化步骤类型和脚本类型列表
            AvailableStepTypes = new ObservableCollection<StepType>(Enum.GetValues(typeof(StepType)).Cast<StepType>());
            AvailableScriptTypes = new ObservableCollection<ScriptType>(Enum.GetValues(typeof(ScriptType)).Cast<ScriptType>());

            // 初始化命令
            LoadTemplatesCommand = new DelegateCommand(LoadTemplates);
            SelectTemplateCommand = new DelegateCommand(OnTemplateSelected);
            GenerateScriptCommand = new DelegateCommand(GenerateScript);
            AddStepCommand = new DelegateCommand(AddStep);
            RemoveStepCommand = new DelegateCommand(RemoveStep);
            MoveStepUpCommand = new DelegateCommand(MoveStepUp);
            MoveStepDownCommand = new DelegateCommand(MoveStepDown);
            EditStepCommand = new DelegateCommand(EditStep);
            RunScriptCommand = new DelegateCommand(RunScript, CanRunScript);
            StopScriptCommand = new DelegateCommand(StopScript);
            SaveScriptCommand = new DelegateCommand(SaveScript);
            LoadScriptCommand = new DelegateCommand(LoadScript);
            CreateTaskCommand = new DelegateCommand(CreateTask);
            RunTaskCommand = new DelegateCommand(RunTask);
            // 新增命令初始化
            AddScriptToTaskCommand = new DelegateCommand(AddScriptToTask);
            RemoveScriptFromTaskCommand = new DelegateCommand(RemoveScriptFromTask);
            AddPreConditionCommand = new DelegateCommand(AddPreCondition);

            // 加载初始数据
            LoadTemplates();
            ExecutionStatus = "就绪";
        }

        /// <summary>
        /// 更新步骤的列表索引
        /// </summary>
        private void UpdateStepsListIndex()
        {            
            if (CurrentScript != null && CurrentScript.Steps != null)
            { 
                for (int i = 0; i < CurrentScript.Steps.Count; i++)
                { 
                    CurrentScript.Steps[i].ListIndex = i + 1;
                }
                OnPropertyChanged(nameof(CurrentScript));
            }
        }

        // 新增命令定义
        public DelegateCommand AddScriptToTaskCommand { get; private set; }
        public DelegateCommand RemoveScriptFromTaskCommand { get; private set; }
        public DelegateCommand AddPreConditionCommand { get; private set; }

        // 命令定义
        public DelegateCommand LoadTemplatesCommand { get; private set; }
        public DelegateCommand SelectTemplateCommand { get; private set; }
        public DelegateCommand GenerateScriptCommand { get; private set; }
        public DelegateCommand AddStepCommand { get; private set; }
        public DelegateCommand RemoveStepCommand { get; private set; }
        public DelegateCommand MoveStepUpCommand { get; private set; }
        public DelegateCommand MoveStepDownCommand { get; private set; }
        public DelegateCommand EditStepCommand { get; private set; }
        public DelegateCommand RunScriptCommand { get; private set; }
        public DelegateCommand StopScriptCommand { get; private set; }
        public DelegateCommand SaveScriptCommand { get; private set; }
        public DelegateCommand LoadScriptCommand { get; private set; }
        public DelegateCommand CreateTaskCommand { get; private set; }
        public DelegateCommand RunTaskCommand { get; private set; }

        // 私有字段
        private CancellationTokenSource _cancellationTokenSource;

        #region 模板管理方法

        /// <summary>
        /// 加载可用的模板列表
        /// </summary>
        private void LoadTemplates()
        {
            try
            {
                AvailableTemplates.Clear();
                var templates = _templateManager.GetAllTemplates();
                foreach (var template in templates)
                {
                    AvailableTemplates.Add(template);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载模板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 选择模板后处理
        /// </summary>
        private void OnTemplateSelected()
        {
            if (SelectedTemplate == null) return;

            TemplateParameters.Clear();
            if (SelectedTemplate.Parameters != null)
            {
                foreach (var param in SelectedTemplate.Parameters)
                {
                    TemplateParameters.Add(new ScriptVariable
                    {
                        Name = param.Name,
                        Type = param.Type,
                        DefaultValue = param.DefaultValue,
                        Description = param.Description
                    });
                }
                ExecutionStatus = $"已加载模板: {SelectedTemplate.TemplateName}";
            }
        }

        #endregion

        #region 脚本管理方法

        /// <summary>
        /// 根据模板生成脚本
        /// </summary>
        private void GenerateScript()
        {            
            if (SelectedTemplate == null)
            {
                MessageBox.Show("请先选择一个模板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 收集参数值
                var parameters = new Dictionary<string, object>();
                foreach (var param in TemplateParameters)
                {
                    parameters[param.Name] = param.DefaultValue;
                }

                // 生成脚本
                var script = _templateManager.GenerateScript(SelectedTemplate.TemplateId, parameters);
                CurrentScript = script;
                Scripts.Add(script);

                // 更新UI
                ExecutionStatus = $"已生成脚本: {script.ScriptName}";
                MessageBox.Show("脚本生成成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"生成脚本失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 添加步骤
        /// </summary>
        private void AddStep()
        {            
            if (CurrentScript == null)
            { 
                MessageBox.Show("请先创建或选择一个脚本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 初始化步骤列表（如果为null）
            if (CurrentScript.Steps == null)
            { 
                CurrentScript.Steps = new List<TestStep>();
            }

            // 这里可以打开步骤编辑对话框
            // 示例：创建一个日志步骤
            var newStep = new TestStep
            { 
                StepId = Guid.NewGuid().ToString(),
                StepName = "新建步骤",
                StepType = StepType.Log,
                Parameters = new Dictionary<string, object> { { "Message", "新步骤" } },
                PreConditions = new List<ValidationRule>(),
                ExpectedResult = ""
            };

            CurrentScript.Steps.Add(newStep);
            // 更新所有步骤的索引
            UpdateStepsListIndex();
            SelectedStep = newStep;
            ExecutionStatus = "已添加新步骤";
        }

        /// <summary>
        /// 移除步骤
        /// </summary>
        private void RemoveStep()
        {
            if (CurrentScript == null || SelectedStep == null || CurrentScript.Steps == null)
            {
                MessageBox.Show("请先选择一个步骤", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("确定要移除该步骤吗？", "确认", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string stepName = SelectedStep.StepName;
                int stepIndex = CurrentScript.Steps.IndexOf(SelectedStep);
                CurrentScript.Steps.Remove(SelectedStep);
                
                // 更新所有步骤的索引
                UpdateStepsListIndex();
                
                // 选择新的步骤
                if (CurrentScript.Steps.Count > 0)
                { 
                    SelectedStep = CurrentScript.Steps.Count > stepIndex ? CurrentScript.Steps[stepIndex] : CurrentScript.Steps[CurrentScript.Steps.Count - 1];
                }
                else
                { 
                    SelectedStep = null;
                }
                
                ExecutionStatus = $"已移除步骤: {stepName}";
            }
        }

        /// <summary>
        /// 上移步骤
        /// </summary>
        private void MoveStepUp()
        {            
            if (CurrentScript == null || SelectedStep == null || CurrentScript.Steps == null) return;

            int index = CurrentScript.Steps.IndexOf(SelectedStep);
            if (index > 0)
            { 
                CurrentScript.Steps.RemoveAt(index);
                CurrentScript.Steps.Insert(index - 1, SelectedStep);
                // 更新索引
                UpdateStepsListIndex();
                ExecutionStatus = "已调整步骤顺序";
            }
        }

        /// <summary>
        /// 下移步骤
        /// </summary>
        private void MoveStepDown()
        {            
            if (CurrentScript == null || SelectedStep == null || CurrentScript.Steps == null) return;

            int index = CurrentScript.Steps.IndexOf(SelectedStep);
            if (index < CurrentScript.Steps.Count - 1)
            { 
                CurrentScript.Steps.RemoveAt(index);
                CurrentScript.Steps.Insert(index + 1, SelectedStep);
                // 更新索引
                UpdateStepsListIndex();
                ExecutionStatus = "已调整步骤顺序";
            }
        }

        /// <summary>
        /// 添加脚本到任务
        /// </summary>
        private void AddScriptToTask()
        {
            if (CurrentTask == null || CurrentScript == null)
            {
                MessageBox.Show("请选择任务和脚本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // 检查并初始化ScriptIds列表
            if (CurrentTask.ScriptIds == null)
            {
                CurrentTask.ScriptIds = new List<string>();
            }
            
            // 防止重复添加
            if (!CurrentTask.ScriptIds.Contains(CurrentScript.ScriptId))
            {
                CurrentTask.ScriptIds.Add(CurrentScript.ScriptId);
                // 更新UI显示
                OnPropertyChanged(nameof(CurrentTaskScriptNames));
                ExecutionStatus = $"已添加脚本到任务: {CurrentScript.ScriptName}";
            }
            else
            {
                ExecutionStatus = "脚本已存在于任务中";
            }
        }
        
        /// <summary>
        /// 从任务中移除脚本
        /// </summary>
        private void RemoveScriptFromTask()
        {
            if (CurrentTask == null || CurrentScript == null || CurrentTask.ScriptIds == null)
            {
                MessageBox.Show("请选择任务和脚本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            if (CurrentTask.ScriptIds.Contains(CurrentScript.ScriptId))
            {
                CurrentTask.ScriptIds.Remove(CurrentScript.ScriptId);
                OnPropertyChanged(nameof(CurrentTaskScriptNames));
                ExecutionStatus = $"已从任务中移除脚本: {CurrentScript.ScriptName}";
                MessageBox.Show("脚本已成功从任务中移除", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                ExecutionStatus = "脚本不在当前任务中";
                MessageBox.Show("脚本不在当前任务中", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        
        /// <summary>
        /// 添加前置条件
        /// </summary>
        private void AddPreCondition()
        {            
            if (SelectedStep == null)
            { 
                MessageBox.Show("请先选择一个步骤", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // 初始化前置条件列表（如果为null）
            if (SelectedStep.PreConditions == null)
            { 
                SelectedStep.PreConditions = new List<ValidationRule>();
            }
            
            // 创建一个新的验证规则
            var newRule = new ValidationRule
            { 
                RuleId = Guid.NewGuid().ToString(),
                ComparisonOperator = ComparisonOperator.Equal,
                ExpectedValue = "",
                ErrorMessage = "前置条件未满足"
            };
            
            SelectedStep.PreConditions.Add(newRule);
            ExecutionStatus = "已添加前置条件";
        }

        /// <summary>
        /// 编辑步骤
        /// </summary>
        private void EditStep()
        {            
            if (CurrentScript == null || SelectedStep == null)
            {                
                MessageBox.Show("请选择要编辑的步骤", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // 这里可以打开编辑步骤的对话框或执行其他编辑逻辑
            // 示例：更新步骤的最后修改时间
            SelectedStep.LastModifiedTime = DateTime.Now;
            CurrentScript.LastModifiedTime = DateTime.Now;
            
            ExecutionStatus = $"已编辑步骤: {SelectedStep.StepName}";
            // 更新步骤索引
            UpdateStepsListIndex();
        }

        #endregion

        #region 测试执行方法

        /// <summary>
        /// 检查是否可以运行脚本
        /// </summary>
        /// <returns>是否可以运行</returns>
        private bool CanRunScript()
        {
            return CurrentScript != null && CurrentScript.Steps.Count > 0 && !IsTestRunning;
        }

        /// <summary>
        /// 运行脚本
        /// </summary>
        private async void RunScript()
        {            
            if (!CanRunScript()) return;

            try
            {
                IsTestRunning = true;
                ExecutionStatus = "测试运行中...";
                RunScriptCommand.RaiseCanExecuteChanged();
                
                _cancellationTokenSource = new CancellationTokenSource();
                
                // 异步执行脚本
                var report = await _automationService.ExecuteScriptAsync(CurrentScript, _cancellationTokenSource.Token);
                
                // 更新测试报告
                CurrentTestReport = report;
                TestReports.Add(report);
                
                // 更新状态
                ExecutionStatus = report.Result == TestResult.Passed ? "测试通过" : "测试失败";
                
                // 显示结果
                string resultMessage = $"测试{report.Result}！\n执行时间: {report.DurationMs}ms\n通过步骤: {report.PassedStepsCount}/{report.TotalStepsCount}";
                MessageBox.Show(resultMessage, "测试完成", MessageBoxButton.OK, 
                    report.Result == TestResult.Passed ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (OperationCanceledException)
            {
                ExecutionStatus = "测试已取消";
                MessageBox.Show("测试已取消", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ExecutionStatus = "测试出错";
                MessageBox.Show($"测试执行出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsTestRunning = false;
                RunScriptCommand.RaiseCanExecuteChanged();
                _cancellationTokenSource?.Dispose();
            }
        }

        /// <summary>
        /// 停止脚本执行
        /// </summary>
        private void StopScript()
        {            
            if (_cancellationTokenSource != null && !_cancellationTokenSource.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                ExecutionStatus = "正在取消测试...";
            }
        }

        #endregion

        #region 脚本持久化方法

        /// <summary>
        /// 保存脚本
        /// </summary>
        private void SaveScript()
        {
            if (CurrentScript == null)
            {
                MessageBox.Show("请先创建或选择一个脚本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // 这里可以实现脚本的序列化保存
                // 示例：更新最后修改时间
                CurrentScript.LastModifiedTime = DateTime.Now;
                
                ExecutionStatus = $"已保存脚本: {CurrentScript.ScriptName}";
                MessageBox.Show("脚本保存成功", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存脚本失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 加载脚本
        /// </summary>
        private void LoadScript()
        {            
            try
            {
                // 这里可以实现脚本的加载逻辑
                // 示例：加载第一个脚本
                if (Scripts.Count > 0)
                {
                    CurrentScript = Scripts[0];
                    ExecutionStatus = $"已加载脚本: {CurrentScript.ScriptName}";
                }
                else
                {
                    MessageBox.Show("没有可用的脚本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载脚本失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 任务管理方法

        /// <summary>
        /// 创建测试任务
        /// </summary>
        private void CreateTask()
        {            
            try
            {
                var newTask = new TestTask
                {
                    TaskId = Guid.NewGuid().ToString(),
                    TaskName = $"新任务 - {DateTime.Now:yyyyMMdd_HHmmss}",
                    Description = "自动创建的测试任务",
                    CreatedTime = DateTime.Now,
                    ExecutionMode = ExecutionMode.Sequential,
                    ScriptIds = new List<string>(),
                    Status = TaskStatus.Ready
                };

                // 如果有当前脚本，添加到任务中
                if (CurrentScript != null)
                {
                    newTask.ScriptIds.Add(CurrentScript.ScriptId);
                }

                TestTasks.Add(newTask);
                CurrentTask = newTask;
                
                ExecutionStatus = $"已创建任务: {newTask.TaskName}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"创建任务失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 运行测试任务
        /// </summary>
        private async void RunTask()
        {            
            if (CurrentTask == null || CurrentTask.ScriptIds == null || CurrentTask.ScriptIds.Count == 0)
            {
                MessageBox.Show("请先创建任务并添加脚本", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                IsTestRunning = true;
                ExecutionStatus = "任务执行中...";
                CurrentTask.Status = TaskStatus.Running;
                
                _cancellationTokenSource = new CancellationTokenSource();
                
                // 异步执行任务
                var reports = await _automationService.ExecuteTaskAsync(CurrentTask, _cancellationTokenSource.Token);
                
                // 更新测试报告
                foreach (var report in reports)
                {
                    TestReports.Add(report);
                }
                
                // 更新任务状态
                bool allPassed = reports.All(r => r.Result == TestResult.Passed);
                CurrentTask.Status = allPassed ? TaskStatus.Completed : TaskStatus.Failed;
                
                // 更新状态
                ExecutionStatus = allPassed ? "任务执行成功" : "任务执行失败";
                
                // 显示结果
                string resultMessage = $"任务{allPassed ? "成功" : "失败"}！\n执行脚本数: {reports.Count}\n所有测试{allPassed ? "通过" : "未全部通过"}";
                MessageBox.Show(resultMessage, "任务完成", MessageBoxButton.OK, 
                    allPassed ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
            catch (OperationCanceledException)
            {
                ExecutionStatus = "任务已取消";
                if (CurrentTask != null)
                {
                    CurrentTask.Status = TaskStatus.Cancelled;
                }
                MessageBox.Show("任务已取消", "信息", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ExecutionStatus = "任务执行出错";
                if (CurrentTask != null)
                {
                    CurrentTask.Status = TaskStatus.Failed;
                }
                MessageBox.Show($"任务执行出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsTestRunning = false;
                _cancellationTokenSource?.Dispose();
            }
        }

        #endregion
    