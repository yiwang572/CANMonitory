using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CANMonitor.Models.BmsModels;

namespace CANMonitor.Services.BmsServices
{
    public class BmsSignalDecoder
    {
        public CellVoltageData DecodeCellVoltage(byte[] rawData, int cellId)
        {
            // 解码单体电压数据
            return new CellVoltageData
            {
                CellId = cellId,
                Voltage = 0,
                Timestamp = DateTime.Now
            };
        }

        public TemperatureData DecodeTemperature(byte[] rawData, int sensorId)
        {
            // 解码温度数据
            return new TemperatureData
            {
                SensorId = sensorId,
                Temperature = 0,
                Timestamp = DateTime.Now
            };
        }
    }
}