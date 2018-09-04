using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_SetDout : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.SetDout);
            writer.Write(GPIOChannel);
            writer.Write(GPIOState);
        }
        public byte GPIOChannel { get; set; }
        public byte GPIOState { get; set; }


    }
}
