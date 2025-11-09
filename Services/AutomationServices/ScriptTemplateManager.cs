using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CANMonitor.Models.CommonModels;

namespace CANMonitor.Services.AutomationServices
{
    /// <summary>
    /// 脚本模板管理器
    /// 负责管理和生成无代码脚本模板
    /// </summary>
    public class ScriptTemplateManager
    {
        private readonly List<TestTemplate> _templates;
        private readonly string _templatesFolderPath;

        public ScriptTemplateManager()
        {
            _templates = new List<TestTemplate>();
            _templatesFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "ScriptTemplates");
            InitializeBuiltInTemplates();
            LoadCustomTemplates();
        }

        /// <summary>
        /// 初始化内置模板
        /// </summary>
        private void InitializeBuiltInTemplates()
        {
            // BMS基本通信测试模板
            var communicationTestTemplate = new TestTemplate
            {
                TemplateId = "BMS_BASIC_COMM_TEST",
                Name = "BMS基本通信测试",
                Description = "测试BMS与监控系统的基本通信功能",
                Type = TemplateType.BasicTest,
                Category = "BMS基础测试",
                Parameters = new List<ScriptVariable>
                {
                    new ScriptVariable { Name = "MessageId", Type = VariableType.UInteger, DefaultValue = "0x18FF1101", Description = "BMS状态报文ID" },
                    new ScriptVariable { Name = "Timeout", Type = VariableType.Integer, DefaultValue = "5", Description = "超时时间(秒)" }
                },
                Steps = new List<TestStep>
                {
                    new TestStep
                    {
                        StepId = Guid.NewGuid().ToString(),
                        StepName = "初始化通信",
                        StepType = StepType.Log,
                        Parameters = new Dictionary<string, object> { { "Message", "初始化CAN通信" } },
                        PreConditions = new List<ValidationRule>(),
                        ExpectedResult = "成功初始化通信"
                    },
                    new TestStep
                    {
                        StepId = Guid.NewGuid().ToString(),
                        StepName = "等待BMS状态报文",
                        StepType = StepType.ReceiveCanMessage,
                        Parameters = new Dictionary<string, object>
                        {{
 "ArbitrationId", "{{MessageId}}" },
                            { "TimeoutMs", "{{Timeout}}000" }
                        },
                        PreConditions = new List<ValidationRule>(),
                        ExpectedResult = "成功接收BMS状态报文"
                    },
                    new TestStep
                    {
                        StepId = Guid.NewGuid().ToString(),
                        StepName = "记录测试结果",
                        StepType = StepType.Log,
                        Parameters = new Dictionary<string, object> { { "Message", "基本通信测试完成" } },
                        PreConditions = new List<ValidationRule>(),
                        ExpectedResult = "记录测试结果"
                    }
                }
            };

            // SOC状态监控测试模板
            var socMonitoringTemplate = new TestTemplate
            {
                TemplateId = "BMS_SOC_MONITOR_TEST",
                Name = "BMS SOC状态监控测试",
                Description = "监控BMS的SOC(荷电状态)信号",
                Type = TemplateType.SignalMonitoring,
                Category = "BMS功能测试",
                Parameters = new List<ScriptVariable>
                {
                    new ScriptVariable { Name = "SocMessageId", Type = VariableType.UInteger, DefaultValue = "0x18FF1102", Description = "SOC报文ID" },
                    new ScriptVariable { Name = "MinSocThreshold", Type = VariableType.Float, DefaultValue = "20.0", Description = "SOC最小值阈值" },
                    new ScriptVariable { Name = "MaxSocThreshold", Type = VariableType.Float, DefaultValue = "80.0", Description = "SOC最大值阈值" },
                    new ScriptVariable { Name = "MonitoringDuration", Type = VariableType.Integer, DefaultValue = "60", Description = "监控时长(秒)" }
                },
                Steps = new List<TestStep>
                {
                    new TestStep
                    {
                        StepId = Guid.NewGuid().ToString(),
                        StepName = "开始SOC监控",
                        StepType = StepType.Log,
                        Parameters = new Dictionary<string, object> { { "Message", "开始SOC状态监控" } },
                        PreConditions = new List<ValidationRule>(),
                        ExpectedResult = "开始监控记录"
                    },
                    new TestStep
                    {
                        StepId = Guid.NewGuid().ToString(),
                        StepName = "读取SOC值",
                        StepType = StepType.ReceiveCanMessage,
                        Parameters = new Dictionary<string, object>
                        {{
 "ArbitrationId", "{{SocMessageId}}" },
                            { "SignalName", "SOC" }
                        },
                        PreConditions = new List<ValidationRule>(),
                        ExpectedResult = "成功读取SOC值"
                    },
                    new TestStep
                    {
                        StepId = Guid.NewGuid().ToString(),
                        StepName = "验证SOC范围",
                        StepType = StepType.Validate,
                        Parameters = new Dictionary<string, object>
                        {{
 "MinValue", "{{MinSocThreshold}}" },
                            { "MaxValue", "{{MaxSocThreshold}}" },
                            { "Comparison", ComparisonOperator.Between }
                        },
                        PreConditions = new List<ValidationRule>(),
                        ExpectedResult = "SOC值在有效范围内"
                    }
                }
            };

            // 电池电压测试模板
            var voltageTestTemplate = new TestTemplate
            {
                TemplateId = "BMS_VOLTAGE_TEST",
                Name = "BMS电池电压测试",
                Description = "测试电池单体电压和总电压",
                Type = TemplateType.ParameterTest,
                Category = "BMS功能测试",
                Parameters = new List<ScriptVariable>
                {
                    new ScriptVariable { Name = "VoltageMessageId", Type = VariableType.UInteger, DefaultValue = "0x18FF1103", Description = "电压报文ID" },
                    new ScriptVariable { Name = "CellCount", Type = VariableType.Integer, DefaultValue = "16", Description = "电池单体数量" },
                    new ScriptVariable { Name = "MinCellVoltage", Type = VariableType.Float, DefaultValue = "2.8", Description = "最小单体电压(V)" },
                    new ScriptVariable { Name = "MaxCellVoltage", Type = VariableType.Float, DefaultValue = "4.2", Description = "最大单体电压(V)" }
                },
                Steps = new List<TestStep>
                {
                    new TestStep
                    {
                        StepId = Guid.NewGuid().ToString(),
                        StepName = "读取电池电压",
                        StepType = StepType.ReceiveCanMessage,
                        Parameters = new Dictionary<string, object>
                        {{
 "ArbitrationId", "{{VoltageMessageId}}" },
                            { "TimeoutMs", 3000 },
                            { "CellCount", "{{CellCount}}" }
                        },
                        PreConditions = new List<ValidationRule>(),
                        ExpectedResult = "成功读取电池电压数据"
                    },
                    new TestStep
                    {
                        StepId = Guid.NewGuid().ToString(),
                        StepName = "检查单体电压",
                        StepType = StepType.Validate,
                        Parameters = new Dictionary<string, object>
                        {{
 "MinValue", "{{MinCellVoltage}}" },
                            { "MaxValue", "{{MaxCellVoltage}}" },
                            { "Comparison", ComparisonOperator.Between }
                        },
                        PreConditions = new List<ValidationRule>(),
                        ExpectedResult = "所有电池单体电压在有效范围内"
                    }
                }
            };

            // 温度测试模板
            var temperatureTestTemplate = new TestTemplate
            {
                TemplateId = "BMS_TEMPERATURE_TEST",
                Name = "BMS温度测试",
                Description = "测试电池温度传感器",
                Type = TemplateType.ParameterTest,
                Category = "BMS功能测试",
                Parameters = new List<ScriptVariable>(),
                Steps = new List<TestStep>
                {
                    new TestStep
                    {
                        StepId = Guid.NewGuid().ToString(),
                        StepName = "读取温度数据",
                        StepType = StepType.ReceiveCanMessage,
                        Parameters = new Dictionary<string, object>
                        {{
 "ArbitrationId", 0x18FF1104 },
                            { "TimeoutMs", 3000 },
                            { "SignalName", "Temperatures" }
                        },
                        PreConditions = new List<ValidationRule>(),
                        ExpectedResult = "成功读取温度数据"
                    },
                    new TestStep
                    {
                        StepId = Guid.NewGuid().ToString(),
                        StepName = "检查温度范围",
                        StepType = StepType.Validate,
                        Parameters = new Dictionary<string, object>
                        {{
 "MinValue", -20.0 },
                            { "MaxValue", 60.0 },
                            { "Comparison", ComparisonOperator.Between }
                        },
                        PreConditions = new List<ValidationRule>(),
                        ExpectedResult = "温度在安全工作范围内"
                    }
                }
            };

            _templates.Add(communicationTestTemplate);
            _templates.Add(socMonitoringTemplate);
            _templates.Add(voltageTestTemplate);
            _templates.Add(temperatureTestTemplate);
        }

        /// <summary>
        /// 加载自定义模板
        /// </summary>
        private void LoadCustomTemplates()
        {
            try
            {
                if (Directory.Exists(_templatesFolderPath))
                {
                    // 预留：从配置文件或数据库加载自定义模板
                }
            }
            catch (Exception ex)
            {
                // 记录异常但不影响内置模板的使用
                Console.WriteLine($"加载自定义模板失败: {ex.Message}");
            }
        }

        /// <summary>
        /// 获取所有模板
        /// </summary>
        /// <returns>模板列表</returns>
        public List<TestTemplate> GetAllTemplates()
        {
            return _templates;
        }

        /// <summary>
        /// 根据ID获取模板
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <returns>模板对象</returns>
        public TestTemplate GetTemplateById(string templateId)
        {
            return _templates.FirstOrDefault(t => t.TemplateId == templateId);
        }

        /// <summary>
        /// 根据类别获取模板
        /// </summary>
        /// <param name="category">模板类别</param>
        /// <returns>模板列表</returns>
        public List<TestTemplate> GetTemplatesByCategory(string category)
        {
            return _templates.Where(t => t.Category == category).ToList();
        }

        /// <summary>
        /// 根据类型获取模板
        /// </summary>
        /// <param name="type">模板类型</param>
        /// <returns>模板列表</returns>
        public List<TestTemplate> GetTemplatesByType(TemplateType type)
        {
            return _templates.Where(t => t.Type == type).ToList();
        }

        /// <summary>
        /// 根据模板生成测试脚本
        /// </summary>
        /// <param name="templateId">模板ID</param>
        /// <param name="parameters">参数值字典</param>
        /// <returns>生成的自动化脚本</returns>
        public AutomationScript GenerateScript(string templateId, Dictionary<string, object> parameters)
        {
            var template = GetTemplateById(templateId);
            if (template == null)
            {
                throw new ArgumentException($"模板ID '{templateId}' 不存在");
            }

            // 创建新的脚本实例
            var script = new AutomationScript
            {
                ScriptId = Guid.NewGuid().ToString(),
                ScriptName = $"{template.Name} - {DateTime.Now:yyyyMMdd_HHmmss}",
                Description = template.Description,
                Type = ScriptType.NoCode,
                CreatedTime = DateTime.Now,
                LastModifiedTime = DateTime.Now,
                Steps = new List<TestStep>(),
                Variables = new List<ScriptVariable>()
            };

            // 复制模板参数作为脚本变量
            foreach (var param in template.Parameters)
            {
                var variable = new ScriptVariable
                {
                    Name = param.Name,
                    Type = param.Type,
                    Description = param.Description,
                    DefaultValue = param.DefaultValue
                };

                // 如果提供了参数值，覆盖默认值
                if (parameters.TryGetValue(param.Name, out var value))
                {
                    variable.DefaultValue = value.ToString();
                }

                script.Variables.Add(variable);
            }

            // 复制并参数化模板步骤
            foreach (var templateStep in template.Steps)
            {
                var step = new TestStep
                {
                    StepId = Guid.NewGuid().ToString(),
                    StepName = templateStep.StepName,
                    StepType = templateStep.StepType,
                    Parameters = new Dictionary<string, object>(templateStep.Parameters),
                    PreConditions = new List<ValidationRule>(templateStep.PreConditions),
                    ExpectedResult = templateStep.ExpectedResult
                };

                // 替换步骤参数中的占位符
                ReplaceParameters(step.Parameters, parameters);
                script.Steps.Add(step);
            }

            return script;
        }

        /// <summary>
        /// 替换参数占位符
        /// </summary>
        /// <param name="parameters">参数字典</param>
        /// <param name="values">参数值字典</param>
        private void ReplaceParameters(Dictionary<string, object> parameters, Dictionary<string, object> values)
        {
            foreach (var key in parameters.Keys.ToList())
            {
                if (parameters[key] is string strValue)
                {
                    // 检查是否包含参数占位符（如 {MessageId}）
                    foreach (var valueKey in values.Keys)
                    {
                        string placeholder = $"{{{valueKey}}}";
                        if (strValue.Contains(placeholder))
                        {
                            parameters[key] = strValue.Replace(placeholder, values[valueKey]?.ToString() ?? string.Empty);
                        }
                    }
                }
                // 对于非字符串类型参数，如果直接提供了值则替换
                else if (values.TryGetValue(key, out var value))
                {
                    parameters[key] = value;
                }
            }
        }

        /// <summary>
        /// 保存自定义模板
        /// </summary>
        /// <param name="template">模板对象</param>
        public void SaveTemplate(TestTemplate template)
        {
            // 预留：保存自定义模板到配置文件或数据库
            _templates.Add(template);
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="templateId">模板ID</param>
        public void DeleteTemplate(string templateId)
        {
            var template = GetTemplateById(templateId);
            if (template != null)
            {
                _templates.Remove(template);
                // 预留：从配置文件或数据库删除模板
            }
        }
    }
}