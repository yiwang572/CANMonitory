using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CANMonitor.Utils
{
    public class FileHandler
    {
        public string ReadTextFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        public void WriteTextFile(string filePath, string content)
        {
            File.WriteAllText(filePath, content);
        }

        public byte[] ReadBinaryFile(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public void WriteBinaryFile(string filePath, byte[] content)
        {
            File.WriteAllBytes(filePath, content);
        }
    }
}