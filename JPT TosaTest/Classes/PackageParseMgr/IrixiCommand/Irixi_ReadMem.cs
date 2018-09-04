using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.PackageParseMgr.IrixiCommand
{
    public class Irixi_ReadMem : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.ReadMem);
            writer.Write(MemOffset);
            writer.Write(MemLength);
        }
        public UInt32 MemOffset { get; set; }
        public UInt32 MemLength { get; set; }
    }
}
