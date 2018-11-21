using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_SET_T_OUT : ZigBeePackage
    {
        public Irixi_HOST_CMD_SET_T_OUT()
        {
            FrameLength = 0x04; //???
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_SET_T_OUT);
        }
        public override ZigBeePackage GetDataFromRowByteArr(byte[] RawData)
        {
            return base.GetDataFromRowByteArr(RawData);
        }
    }
}
