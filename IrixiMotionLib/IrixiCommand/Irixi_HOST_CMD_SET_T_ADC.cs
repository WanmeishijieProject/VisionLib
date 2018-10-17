using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_SET_T_ADC : ZigBeePackage
    {
        public Irixi_HOST_CMD_SET_T_ADC()
        {
            FrameLength = 0x05;
        }
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_SET_T_ADC);
            writer.Write(ADCChannelFlags);
        }
        public byte ADCChannelFlags { get; set; }

    }
}
