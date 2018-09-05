using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_ReadDin : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.ReadDin);
        }
        public override ZigBeePackage ByteArrToPackage(byte[] RawData)
        {
            ReturnObject = RawData[RawData.Length - 5];
            return base.ByteArrToPackage(RawData);
        }
    }
}
