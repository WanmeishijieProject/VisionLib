using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_SET_CSSTHD : ZigBeePackage
    {
        public Irixi_HOST_CMD_SET_CSSTHD()
        {
            FrameLength = 0x0A;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_SET_CSSTHD);
            writer.Write(CssChannel);
            writer.Write(LowThreshold);
            writer.Write(HightThreshold);
        }

        public byte CssChannel { get; set; }
        public UInt16 LowThreshold { get; set; }
        public UInt16 HightThreshold { get; set; }


    }
}
