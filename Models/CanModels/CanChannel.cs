using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Models.CanModels
{
    public class CanChannel
    {
        public int ChannelId { get; set; }
        public string Name { get; set; }
        public int BaudRate { get; set; }
        public bool IsActive { get; set; }
    }
}