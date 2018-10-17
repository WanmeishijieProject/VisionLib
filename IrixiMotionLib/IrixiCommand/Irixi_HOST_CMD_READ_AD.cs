using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_READ_AD : ZigBeePackage
    {
        public Irixi_HOST_CMD_READ_AD()
        {
            FrameLength = 0x05;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_READ_AD);
            writer.Write(ADChannelFlags);
        }
        public override ZigBeePackage ByteArrToPackage(byte[] RawData)
        {
           return base.ByteArrToPackage(RawData);
   
        }
        public byte ADChannelFlags { get; set; }
    }
}
