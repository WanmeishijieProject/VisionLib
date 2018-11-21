using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_GET_MCSU_STA : ZigBeePackage
    {
        public Irixi_HOST_CMD_GET_MCSU_STA()
        {
            FrameLength = 0x05;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_GET_MCSU_STA);
            writer.Write(AxisNo);
        }
        public override ZigBeePackage GetDataFromRowByteArr(byte[] RawData)
        {
            int RealLen = RawData.Length;
            base.GetDataFromRowByteArr(RawData);

            byte axisIndex = RawData[7];    //AxisNo
            byte AxisState = RawData[8];  //AxisState
            byte Error = RawData[9];
            Int32 AbsPos = (Int32)((RawData[10]) + (RawData[11] << 8) + (RawData[12] << 16) + (RawData[13] << 24));

            base.GetDataFromRowByteArr(RawData);
            this.ReturnObject = new MCSUS_STATE()
            {
                AxisIndex = axisIndex,
                IsInit = (AxisState & 0x01) == 1,
                IsHomed = ((AxisState >> 1) & 0x01) == 1,
                IsBusy = ((AxisState >> 2) & 0x01) == 1,
                IsReversed = ((AxisState >> 3) & 0x01) == 1,
                AbsPosition = AbsPos,
                Error = Error
            };
            return this;
        }
        public byte AxisNo { get; set; }

    }
}
