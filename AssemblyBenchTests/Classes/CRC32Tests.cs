using Microsoft.VisualStudio.TestTools.UnitTesting;
using JPT_TosaTest.Classes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Package;
namespace JPT_TosaTest.Classes.Tests
{
    [TestClass()]
    public class CRC32Tests
    {
        [TestMethod()]
        public void TestCRC32()
        {
            CRC32 crc32 = new CRC32();
            UInt32[] data = Byte2Uint32(new byte[] { 0x7E, 0x05, 0x00, 105, 16, 1, 14, 0 },0,8);
            UInt32 crcValue = crc32.Calculate(data, data.Length);
            UInt32 bt0 = (crcValue & 0xFF);
            UInt32 bt1 = ((crcValue>>8) & 0xFF);
            UInt32 bt2 = ((crcValue>>16) & 0xFF);
            UInt32 bt3 = ((crcValue>>24) & 0xFF);

        }

        private uint[] Byte2Uint32(byte[] data, int offset, int length)
        {
            List<UInt32> result = new List<uint>();
            int len = (length - offset) / 4;

            if ((length - offset) % 4 != 0)
            {
                len = len + 1;
            }
            for (int i = offset; i < len; i++)
            {
                UInt32 ret = 0;
                for (int j = 0; j < 4; j++)
                {
                    int index = 4 * i + j + offset;
                    if (index < data.Length)
                        ret += (UInt32)(data[index] << (j * 8));
                }
                result.Add(ret);
            }
            return result.ToArray();
        }
    }
}