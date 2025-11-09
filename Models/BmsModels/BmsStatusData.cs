using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.BmsModels
{
    public class BmsStatusData
    {
        public bool IsCharging { get; set; }
        public bool IsDischarging { get; set; }
        public bool IsFault { get; set; }
        public bool IsBalancing { get; set; }
        public int ErrorCode { get; set; }
        public DateTime Timestamp { get; set; }
    }
}