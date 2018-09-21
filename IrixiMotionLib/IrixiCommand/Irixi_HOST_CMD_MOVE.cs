using Package; 
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_MOVE  : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_MOVE);
            writer.Write(AxisNo);
            writer.Write(Distance);
            writer.Write(SpeedPercent);
        }
        public override ZigBeePackage ByteArrToPackage(byte[] RawData)
        {
            return base.ByteArrToPackage(RawData);
        }
        public byte AxisNo { get; set; }
        public Int32 Distance { get; set; }
        public byte SpeedPercent { get; set; }

    }
}
