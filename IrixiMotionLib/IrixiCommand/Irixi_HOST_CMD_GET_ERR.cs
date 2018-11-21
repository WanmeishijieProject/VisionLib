using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_GET_ERR : ZigBeePackage
    {
        public  Irixi_HOST_CMD_GET_ERR()
        {
            FrameLength = 0x04;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_GET_ERR);
        }
        public override ZigBeePackage GetDataFromRowByteArr(byte[] RawData)
        {
            base.GetDataFromRowByteArr(RawData);
            ReturnObject = new Tuple<byte, byte>(RawData[7], RawData[8]);
            return this;
        }
    }
}
