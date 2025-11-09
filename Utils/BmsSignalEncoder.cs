using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Utils
{
    public class BmsSignalEncoder
    {
        public byte[] EncodeSignal(double value, double factor, double offset)
        {
            // 编码信号值为字节数组
            int rawValue = (int)((value - offset) / factor);
            return BitConverter.GetBytes(rawValue);
        }
    }
}