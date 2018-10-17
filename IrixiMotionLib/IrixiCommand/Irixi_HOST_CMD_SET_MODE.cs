using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_SET_MODE : ZigBeePackage
    {
        public Irixi_HOST_CMD_SET_MODE()
        {
            FrameLength = 0x06;
        }

        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_SET_MODE);
            writer.Write(AxisNo);
        }      
        public byte AxisNo { get; set; }
        public byte Mode { get; set; }

    }
}
