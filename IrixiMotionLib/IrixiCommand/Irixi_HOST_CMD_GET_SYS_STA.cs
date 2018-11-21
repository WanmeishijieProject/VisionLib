using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_GET_SYS_STA : ZigBeePackage
    {
        public Irixi_HOST_CMD_GET_SYS_STA()
        {
            FrameLength = 0x04;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_GET_SYS_STA);
            writer.Write(AxisNo);
        }
        public override ZigBeePackage GetDataFromRowByteArr(byte[] RawData)
        {

            return base.GetDataFromRowByteArr(RawData);
        }
        public byte AxisNo { get; set; }

    }
}
