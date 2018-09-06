using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_GetMcsuSta : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.GetMcsuSta);
            writer.Write(AxisNo);
        }
        public override ZigBeePackage ByteArrToPackage(byte[] RawData)
        {
            int RealLen = RawData.Length;
            base.ByteArrToPackage(RawData);

            byte axisIndex = RawData[7];    //AxisNo
            byte AxisState = RawData[8];  //AxisState
            Int32 AbsPos = (Int32)((RawData[RealLen - 11]) + (RawData[RealLen - 10] << 8) + (RawData[RealLen - 9] << 16) + (RawData[RealLen - 8] << 24));
            Int16 Acc = (Int16)(RawData[RealLen - 7] + (RawData[RealLen - 6] << 8));
            byte Error = RawData[RealLen - 5];

            this.ReturnObject = new MCSUS_STATE()
            {
                AxisIndex = axisIndex,
                IsInit = (AxisState & 0x01) == 1,
                IsHomed = ((AxisState >> 1) & 0x01) == 1,
                IsBusy = ((AxisState >> 2) & 0x01) == 1,
                IsReversed = ((AxisState >> 3) & 0x01) == 1,
                AbsPosition = AbsPos,
                Acceleration = Acc,
                Error = Error
            };
            return base.ByteArrToPackage(RawData);
        }
        public byte AxisNo { get; set; }

    }
}
