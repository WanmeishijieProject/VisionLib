using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_READ_MEM : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_READ_MEM);
            writer.Write(MemOffset);
            writer.Write(MemLength);
        }

        public override ZigBeePackage ByteArrToPackage(byte[] RawData)
        {
            List<short> ADCRawDataList = new List<short>();
            int PackageID = RawData[7] + (RawData[8] << 8);
            int DataLength = RawData[9] + (RawData[10] << 8);
            int nStartPos = 11;
            for (int i = 0; i < DataLength; i++)
            {
                short value = (short)(RawData[2 * i + nStartPos] + (RawData[2 * i + 1 + nStartPos] << 8));
                ADCRawDataList.Add(value);
            }

            ReturnObject = ADCRawDataList;
            return base.ByteArrToPackage(RawData);
        }

        public UInt32 MemOffset { get; set; }
        public UInt32 MemLength { get; set; }
    }
}
