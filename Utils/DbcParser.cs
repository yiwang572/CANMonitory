using CANMonitor.Services.DbcServices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace CANMonitor.Utils
{
    /// <summary>
    /// DBC文件解析器类
    /// 负责解析DBC文件格式，提取报文和信号定义
    /// </summary>
    public class DbcParser
    {
        // 报文定义的正则表达式
        private readonly Regex _messageRegex = new Regex(@"BO_\s+(\d+)\s+(\w+):\s+(\d+)\s+(\w+)", RegexOptions.Compiled);
        
        // 信号定义的正则表达式
        private readonly Regex _signalRegex = new Regex(@"SG_\s+(\w+)\s+:\s+(\d+)|(\d+)-(\d+)\s+@(\d+)([+-])\s+\(([^)]+)\)\s+\[([^\]]+)\]\s+""([^""]*)""\s+(\w+)\s*(\w*)", RegexOptions.Compiled);
        
        // 信号属性的正则表达式
        private readonly Regex _signalCommentRegex = new Regex(@"CM_\s+SG_\s+(\d+)\s+(\w+)\s+""([^""]*)""", RegexOptions.Compiled);
        
        // 报文属性的正则表达式
        private readonly Regex _messageCommentRegex = new Regex(@"CM_\s+BO_\s+(\d+)\s+""([^""]*)""", RegexOptions.Compiled);
        
        // 节点定义的正则表达式
        private readonly Regex _nodeRegex = new Regex(@"BU_:\s+([\w\s]+)", RegexOptions.Compiled);
        
        /// <summary>
        /// 解析DBC文件
        /// </summary>
        /// <param name="filePath">DBC文件路径</param>
        /// <returns>解析后的报文列表</returns>
        public List<DbcMessage> Parse(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("DBC文件不存在", filePath);
            }
            
            List<DbcMessage> messages = new List<DbcMessage>();
            Dictionary<uint, string> messageComments = new Dictionary<uint, string>();
            Dictionary<string, string> signalComments = new Dictionary<string, string>();
            
            string[] lines = File.ReadAllLines(filePath);
            
            // 首先解析所有的注释和属性
            ParseComments(lines, messageComments, signalComments);
            
            // 解析报文和信号定义
            DbcMessage currentMessage = null;
            
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                
                // 跳过空行和注释行
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("//"))
                    continue;
                
                // 匹配报文定义
                Match messageMatch = _messageRegex.Match(trimmedLine);
                if (messageMatch.Success)
                {
                    // 如果已经有当前报文，先添加到列表中
                    if (currentMessage != null)
                    {
                        messages.Add(currentMessage);
                    }
                    
                    // 创建新的报文
                    uint messageId = uint.Parse(messageMatch.Groups[1].Value);
                    string messageName = messageMatch.Groups[2].Value;
                    int messageLength = int.Parse(messageMatch.Groups[3].Value);
                    string transmitter = messageMatch.Groups[4].Value;
                    
                    currentMessage = new DbcMessage
                    {
                        Id = messageId,
                        Name = messageName,
                        Length = messageLength,
                        Sender = transmitter
                    };
                    
                    continue;
                }
                
                // 匹配信号定义
                Match signalMatch = _signalRegex.Match(trimmedLine);
                if (signalMatch.Success && currentMessage != null)
                {
                    string signalName = signalMatch.Groups[1].Value;
                    
                    // 解析信号位置信息
                    int startBit;
                    if (!string.IsNullOrEmpty(signalMatch.Groups[2].Value))
                    {
                        // 单比特信号
                        startBit = int.Parse(signalMatch.Groups[2].Value);
                    }
                    else
                    {
                        // 多比特信号
                        startBit = int.Parse(signalMatch.Groups[3].Value);
                    }
                    
                    int length = signalMatch.Groups[4].Success ? int.Parse(signalMatch.Groups[4].Value) - startBit + 1 : 1;
                    bool isSigned = signalMatch.Groups[6].Value == "-";
                    
                    // 解析缩放因子和偏移量
                    string factorOffset = signalMatch.Groups[7].Value;
                    string[] factorOffsetParts = factorOffset.Split(',');
                    double factor = double.Parse(factorOffsetParts[0]);
                    double offset = factorOffsetParts.Length > 1 ? double.Parse(factorOffsetParts[1]) : 0;
                    
                    // 解析最小值和最大值
                    string minMax = signalMatch.Groups[8].Value;
                    string[] minMaxParts = minMax.Split(',');
                    double min = double.Parse(minMaxParts[0]);
                    double max = minMaxParts.Length > 1 ? double.Parse(minMaxParts[1]) : 0;
                    
                    // 解析单位
                    string unit = signalMatch.Groups[9].Value;
                    
                    // 解析接收节点
                    string receiver = signalMatch.Groups[10].Value;
                    
                    DbcSignal signal = new DbcSignal
                    {
                        Name = signalName,
                        MessageName = currentMessage.Name,
                        StartBit = startBit,
                        Length = length,
                        IsSigned = isSigned,
                        Factor = factor,
                        Offset = offset,
                        Min = min,
                        Max = max,
                        Unit = unit,
                        Receiver = receiver
                    };
                    
                    // 添加信号到当前报文中
                    currentMessage.Signals.Add(signal);
                }
            }
            
            // 添加最后一个报文
            if (currentMessage != null)
            {
                messages.Add(currentMessage);
            }
            
            return messages;
        }
        
        /// <summary>
        /// 解析DBC文件中的注释信息
        /// </summary>
        /// <param name="lines">文件行</param>
        /// <param name="messageComments">报文注释字典</param>
        /// <param name="signalComments">信号注释字典</param>
        private void ParseComments(string[] lines, Dictionary<uint, string> messageComments, Dictionary<string, string> signalComments)
        {
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                
                // 匹配报文注释
                Match messageCommentMatch = _messageCommentRegex.Match(trimmedLine);
                if (messageCommentMatch.Success)
                {
                    uint messageId = uint.Parse(messageCommentMatch.Groups[1].Value);
                    string comment = messageCommentMatch.Groups[2].Value;
                    messageComments[messageId] = comment;
                }
                
                // 匹配信号注释
                Match signalCommentMatch = _signalCommentRegex.Match(trimmedLine);
                if (signalCommentMatch.Success)
                {
                    uint messageId = uint.Parse(signalCommentMatch.Groups[1].Value);
                    string signalName = signalCommentMatch.Groups[2].Value;
                    string comment = signalCommentMatch.Groups[3].Value;
                    string key = $"{messageId}.{signalName}";
                    signalComments[key] = comment;
                }
            }
        }
        
        /// <summary>
        /// 模拟DBC解析（用于测试，返回预设的BMS相关报文定义）
        /// </summary>
        /// <returns>模拟的BMS报文定义列表</returns>
        public List<DbcMessage> GetMockBmsMessages()
        {
            var messages = new List<DbcMessage>();
            
            // 创建SOC/SOH报文
            var socMessage = new DbcMessage
            {
                Id = 0x1806E8F4,
                Name = "BMS_SOC_INFO",
                Length = 8,
                Sender = "BMS"
            };
            
            // 添加SOC信号
            socMessage.Signals.Add(new DbcSignal
            {
                Name = "SOC",
                MessageName = "BMS_SOC_INFO",
                StartBit = 0,
                Length = 8,
                IsSigned = false,
                Factor = 1.0,
                Offset = 0.0,
                Min = 0.0,
                Max = 100.0,
                Unit = "%",
                Receiver = "VCU"
            });
            
            // 添加SOH信号
            socMessage.Signals.Add(new DbcSignal
            {
                Name = "SOH",
                MessageName = "BMS_SOC_INFO",
                StartBit = 8,
                Length = 8,
                IsSigned = false,
                Factor = 1.0,
                Offset = 0.0,
                Min = 0.0,
                Max = 255.0,
                Unit = "%",
                Receiver = "VCU"
            });
            
            // 创建电池单体电压报文
            var voltageMessage = new DbcMessage
            {
                Id = 0x1806E6F4,
                Name = "BMS_CELL_VOLTAGES",
                Length = 8,
                Sender = "BMS"
            };
            
            // 添加电池电压信号
            for (int i = 0; i < 4; i++)
            {
                voltageMessage.Signals.Add(new DbcSignal
                {
                    Name = $"CellVoltage_{i+1}",
                    MessageName = "BMS_CELL_VOLTAGES",
                    StartBit = 16 + i * 8,
                    Length = 8,
                    IsSigned = false,
                    Factor = 0.01,
                    Offset = 0.0,
                    Min = 0.0,
                    Max = 5.0,
                    Unit = "V",
                    Receiver = "VCU"
                });
            }
            
            // 创建温度信息报文
            var tempMessage = new DbcMessage
            {
                Id = 0x1806E7F4,
                Name = "BMS_TEMPERATURE",
                Length = 8,
                Sender = "BMS"
            };
            
            // 添加温度信号
            for (int i = 0; i < 4; i++)
            {
                tempMessage.Signals.Add(new DbcSignal
                {
                    Name = $"TempSensor_{i+1}",
                    MessageName = "BMS_TEMPERATURE",
                    StartBit = 16 + i * 8,
                    Length = 8,
                    IsSigned = true,
                    Factor = 1.0,
                    Offset = 0.0,
                    Min = -40.0,
                    Max = 125.0,
                    Unit = "°C",
                    Receiver = "VCU"
                });
            }
            
            // 创建BMS状态报文
            var statusMessage = new DbcMessage
            {
                Id = 0x1806E5F4,
                Name = "BMS_STATUS",
                Length = 8,
                Sender = "BMS"
            };
            
            statusMessage.Signals.Add(new DbcSignal
            {
                Name = "BMS_Status",
                MessageName = "BMS_STATUS",
                StartBit = 0,
                Length = 8,
                IsSigned = false,
                Factor = 1.0,
                Offset = 0.0,
                Min = 0.0,
                Max = 255.0,
                Unit = "",
                Receiver = "VCU"
            });
            
            // 添加报文到列表
            messages.Add(socMessage);
            messages.Add(voltageMessage);
            messages.Add(tempMessage);
            messages.Add(statusMessage);
            
            return messages;
        }
    }
}