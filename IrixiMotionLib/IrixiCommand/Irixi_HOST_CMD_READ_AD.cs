using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_READ_AD : ZigBeePackage
    {
        public Irixi_HOST_CMD_READ_AD()
        {
            FrameLength = 0x05;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_READ_AD);
            writer.Write((byte)ADChannelFlags);
        }
        public override ZigBeePackage GetDataFromRowByteArr(byte[] RawData)
        {
            base.GetDataFromRowByteArr(RawData);
            List<UInt16> ADValues = new List<UInt16>();
            if (RawData != null && RawData.Length >= 7) //固定结构
            {
                this.FrameLength = (short)(RawData[1] + (RawData[2] << 8));
                this.APIIdentifier = RawData[3];
                this.FrameID = (short)(RawData[4] + (RawData[5] << 8));
                this.Cmd = RawData[6];
            }
            for (int i = 0; i < (FrameLength - 4) / 2; i++)
            {
                ADValues.Add((UInt16)(RawData[6 + 2 * i]  + RawData[6 + 2 * i + 1]<<256));
            }
            this.ReturnObject = ADValues;
            return this;
        }
        public EnumADCChannelFlags ADChannelFlags { get; set; }
    }
}
