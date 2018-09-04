using Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JPT_TosaTest.Classes.PackageParseMgr.IrixiCommand
{
    public class Irixi_GetMemLen : ZigBeePackage
    {
        protected override void WriteData()
        {
            writer.Write((byte)Enumcmd.GetMemLength);
        }
    }
}
