using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.PackageParseMgr.IrixiCommand
{
    public class Irixi_Home : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.Home);
            writer.Write(AxisNo);
        }
        public byte AxisNo { get; set; }

    }
}
