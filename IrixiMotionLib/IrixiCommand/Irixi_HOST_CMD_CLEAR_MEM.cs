using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_CLEAR_MEM : ZigBeePackage
    {
        public Irixi_HOST_CMD_CLEAR_MEM()
        {
            FrameLength = 0x04;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_CLEAR_MEM);
        }
    }
}
