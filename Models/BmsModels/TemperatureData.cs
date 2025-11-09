using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.BmsModels
{
    public class TemperatureData
    {
        public int SensorId { get; set; }
        public double Temperature { get; set; }
        public DateTime Timestamp { get; set; }
    }
}