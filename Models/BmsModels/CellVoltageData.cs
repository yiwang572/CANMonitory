using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.BmsModels
{
    public class CellVoltageData
    {
        public int CellId { get; set; }
        public double Voltage { get; set; }
        public DateTime Timestamp { get; set; }
    }
}