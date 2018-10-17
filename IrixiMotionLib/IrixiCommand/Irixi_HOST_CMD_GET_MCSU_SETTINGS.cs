using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_GET_MCSU_SETTINGS : ZigBeePackage
    {
        public Irixi_HOST_CMD_GET_MCSU_SETTINGS()
        {
            FrameLength = 0x05;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_GET_MCSU_SETTINGS);
            writer.Write(AxisNo);
        }

        public byte AxisNo { get; set; }

    }
}
