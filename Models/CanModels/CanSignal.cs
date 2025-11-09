using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.CanModels
{
    public class CanSignal
    {
        public string Name { get; set; }
        public int StartBit { get; set; }
        public int Length { get; set; }
        public double Factor { get; set; }
        public double Offset { get; set; }
        public double Min { get; set; }
        public double Max { get; set; }
        public string Unit { get; set; }
        public bool IsSigned { get; set; }
    }
}