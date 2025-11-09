using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.BmsModels
{
    public class BatteryData
    {
        public int BatteryId { get; set; }
        public string BatteryName { get; set; }
        public double TotalVoltage { get; set; }
        public double TotalCurrent { get; set; }
        public double SOC { get; set; }
        public double Temperature { get; set; }
        public DateTime Timestamp { get; set; }
    }
}