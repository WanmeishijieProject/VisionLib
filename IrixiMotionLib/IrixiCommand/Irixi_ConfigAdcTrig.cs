using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_ConfigAdcTrig : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.ConfigAdcTrigger);
            writer.Write(ADCChannelFlags);
        }

        public byte ADCChannelFlags { get; set; }

    }
}
