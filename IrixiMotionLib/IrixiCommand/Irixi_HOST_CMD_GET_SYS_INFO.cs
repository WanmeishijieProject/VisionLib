using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_GET_SYS_INFO : ZigBeePackage
    {
        public Irixi_HOST_CMD_GET_SYS_INFO()
        {
            FrameLength = 0x04;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_GET_SYS_INFO);
        }
        public override ZigBeePackage ByteArrToPackage(byte[] RawData)
        {

            return base.ByteArrToPackage(RawData);
        }
    }
}
