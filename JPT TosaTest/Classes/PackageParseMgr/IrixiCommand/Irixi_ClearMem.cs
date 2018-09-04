using Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.PackageParseMgr.IrixiCommand
{
    public class Irixi_ClearMem : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.ClearMem);
        }

    }
}
