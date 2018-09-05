using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.MotionCards.IrixiCommand
{
    public class Irixi_RunBlindSearch : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.RunBlndSerach);
            writer.Write(HAxisNo);
            writer.Write(VAxisNo);
            writer.Write(Range);
            writer.Write(Gap);
            writer.Write(SpeedPercent);
            writer.Write(Interval);
        }

        public byte HAxisNo { get; set; }
        public byte VAxisNo { get; set; }
        public UInt32 Range { get; set; }
        public UInt32 Gap {get;set;}
        public byte SpeedPercent { get; set; }
        public UInt16 Interval { get; set; }

    }
}
