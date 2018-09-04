using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_Stop : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.Stop);
            writer.Write(AxisNo);
        }

        public byte AxisNo { get; set; }

    }
}
