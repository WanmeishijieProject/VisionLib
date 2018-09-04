using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.PackageParseMgr.IrixiCommand
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
        UInt32 Range { get; set; }
        UInt32 Gap {get;set;}
        byte SpeedPercent { get; set; }
        UInt16 Interval { get; set; }

    }
}
