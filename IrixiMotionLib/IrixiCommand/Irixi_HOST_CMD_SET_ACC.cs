using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_HOST_CMD_SET_ACC : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.HOST_CMD_SET_ACC);
            writer.Write(AxisNo);
            writer.Write(Acc);
        }

        public byte AxisNo { get; set; }
        public UInt16 Acc { get; set; }
    }
}
