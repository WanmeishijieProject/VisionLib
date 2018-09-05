using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_GetSysSta : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.GetSysSta);
            writer.Write(AxisNo);
        }
        public override ZigBeePackage ByteArrToPackage(byte[] RawData)
        {

            return base.ByteArrToPackage(RawData);
        }
        public byte AxisNo { get; set; }

    }
}
